using Gabriel.Core.Configuration;
using Gabriel.Core.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace Gabriel.Infrastructure.Memory;

// In-process open-source embeddings: a sentence-transformer ONNX model (default
// all-MiniLM-L6-v2, Apache-2.0) run through ONNX Runtime with proper BERT
// WordPiece tokenization. No API key, no network, no vendor - the model files
// live on disk (scripts/download-embedding-model.ps1 fetches them).
//
// Conversion pipeline per input (the "proper converter"):
//   text → WordPiece ids with [CLS]/[SEP] → truncate to MaxTokens keeping
//   [SEP] → pad the batch rectangular with an attention mask → run the model
//   (input_ids / attention_mask / token_type_ids, whichever the model
//   declares) → attention-masked MEAN POOLING over the last hidden state →
//   L2 normalize. The math lives in EmbeddingPipeline so it's testable
//   without model files. Models that already emit a pooled [batch, dim]
//   sentence embedding are used as-is (normalized), so swapping in a
//   different open-source model is a config change, not a code change.
//
// Model + tokenizer load lazily on first use; InferenceSession.Run is
// thread-safe, so one singleton session serves concurrent embed calls.
public sealed class LocalOnnxEmbeddingProvider : IEmbeddingProvider, IDisposable
{
    private readonly LocalEmbeddingOptions _options;
    private readonly ILogger<LocalOnnxEmbeddingProvider> _logger;
    private readonly string _modelPath;
    private readonly string _vocabPath;
    private readonly Lazy<InferenceSession> _session;
    private readonly Lazy<BertTokenizer> _tokenizer;

    public LocalOnnxEmbeddingProvider(IOptions<EmbeddingOptions> options, ILogger<LocalOnnxEmbeddingProvider> logger)
    {
        _options = options.Value.Local;
        _logger = logger;

        if (!TryResolveModelFiles(_options, out _modelPath, out _vocabPath))
        {
            // DI normally checks this before registering us; belt-and-braces
            // for direct construction.
            throw new FileNotFoundException(
                $"Local embedding model not found under '{_options.ModelDirectory}' " +
                $"(need {_options.ModelFile} + {_options.VocabFile}). " +
                "Run scripts/download-embedding-model.ps1 or switch Embeddings:Provider.");
        }

        _session = new Lazy<InferenceSession>(() =>
        {
            var session = new InferenceSession(_modelPath);
            _logger.LogInformation(
                "Local embedding model loaded | model={ModelPath} inputs={Inputs}",
                _modelPath, string.Join(",", session.InputMetadata.Keys));
            return session;
        });
        _tokenizer = new Lazy<BertTokenizer>(() =>
        {
            using var vocab = File.OpenRead(_vocabPath);
            return BertTokenizer.Create(vocab);
        });
    }

    public string Name => $"Local:{new DirectoryInfo(Path.GetDirectoryName(_modelPath)!).Name}";

    public int Dimensions => _options.Dimensions;

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var batch = await EmbedBatchAsync([text], ct);
        return batch[0];
    }

    public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        if (texts.Count == 0) return Task.FromResult<IReadOnlyList<float[]>>(Array.Empty<float[]>());
        // Inference is CPU-bound; hop to the pool so callers (the agent turn,
        // the backfill) aren't blocked on the calling thread.
        return Task.Run<IReadOnlyList<float[]>>(() => EmbedBatch(texts, ct), ct);
    }

    private IReadOnlyList<float[]> EmbedBatch(IReadOnlyList<string> texts, CancellationToken ct)
    {
        var tokenizer = _tokenizer.Value;
        var sequences = new List<IReadOnlyList<int>>(texts.Count);
        foreach (var text in texts)
        {
            ct.ThrowIfCancellationRequested();
            // EncodeToIds adds [CLS]/[SEP]; empty input still encodes to the
            // two special tokens, which embeds fine.
            var ids = tokenizer.EncodeToIds(text ?? string.Empty);
            sequences.Add(EmbeddingPipeline.Truncate(ids, Math.Max(2, _options.MaxTokens)));
        }

        var seqLen = EmbeddingPipeline.PadBatch(
            sequences, tokenizer.PaddingTokenId, out var inputIds, out var attentionMask);
        var shape = new[] { texts.Count, seqLen };

        var session = _session.Value;
        var inputs = new List<NamedOnnxValue>(3);
        AddIfDeclared(session, inputs, "input_ids", inputIds, shape);
        AddIfDeclared(session, inputs, "attention_mask", attentionMask, shape);
        AddIfDeclared(session, inputs, "token_type_ids", new long[inputIds.Length], shape);

        ct.ThrowIfCancellationRequested();
        using var outputs = session.Run(inputs);
        var output = outputs[0].AsTensor<float>();

        var results = new float[texts.Count][];
        if (output.Dimensions.Length == 3)
        {
            // [batch, seq, hidden] token embeddings → masked mean pool.
            var dim = output.Dimensions[2];
            VerifyDimensions(dim);
            var flat = output.ToArray();
            var perRow = seqLen * dim;
            for (var row = 0; row < texts.Count; row++)
            {
                var pooled = EmbeddingPipeline.MeanPool(
                    flat.AsSpan(row * perRow, perRow),
                    attentionMask.AsSpan(row * seqLen, seqLen),
                    dim);
                EmbeddingPipeline.NormalizeInPlace(pooled);
                results[row] = pooled;
            }
        }
        else if (output.Dimensions.Length == 2)
        {
            // Model already pooled to [batch, dim] (sentence_embedding output).
            var dim = output.Dimensions[1];
            VerifyDimensions(dim);
            var flat = output.ToArray();
            for (var row = 0; row < texts.Count; row++)
            {
                var vector = flat.AsSpan(row * dim, dim).ToArray();
                EmbeddingPipeline.NormalizeInPlace(vector);
                results[row] = vector;
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Unexpected embedding model output rank {output.Dimensions.Length}; expected [batch,seq,hidden] or [batch,dim].");
        }

        return results;
    }

    private static void AddIfDeclared(
        InferenceSession session, List<NamedOnnxValue> inputs, string name, long[] data, int[] shape)
    {
        if (session.InputMetadata.ContainsKey(name))
        {
            inputs.Add(NamedOnnxValue.CreateFromTensor(name, new DenseTensor<long>(data, shape)));
        }
    }

    private void VerifyDimensions(int actual)
    {
        if (actual != _options.Dimensions)
        {
            throw new InvalidOperationException(
                $"Embedding model produces {actual}-dim vectors but Embeddings:Local:Dimensions={_options.Dimensions}. " +
                "Fix the config to match the model - the Qdrant collection is created from this value.");
        }
    }

    // Resolution: absolute directory used as-is; relative probed from the
    // working directory and AppContext.BaseDirectory, each walking up to four
    // parents - covers `dotnet run` from src/api, dev.ps1 from the repo root,
    // and /app in the container. Static so DI can check availability before
    // choosing this provider over the Mock fallback.
    public static bool TryResolveModelFiles(LocalEmbeddingOptions options, out string modelPath, out string vocabPath)
    {
        modelPath = string.Empty;
        vocabPath = string.Empty;
        if (string.IsNullOrWhiteSpace(options.ModelDirectory)) return false;

        foreach (var dir in CandidateDirectories(options.ModelDirectory))
        {
            var model = Path.Combine(dir, options.ModelFile);
            var vocab = Path.Combine(dir, options.VocabFile);
            if (File.Exists(model) && File.Exists(vocab))
            {
                modelPath = Path.GetFullPath(model);
                vocabPath = Path.GetFullPath(vocab);
                return true;
            }
        }
        return false;
    }

    private static IEnumerable<string> CandidateDirectories(string configured)
    {
        if (Path.IsPathRooted(configured))
        {
            yield return configured;
            yield break;
        }

        // Eight levels covers every real launch point: repo root (dev.ps1),
        // src/api (dotnet run), /app (container), and test bin directories
        // (src/api/Gabriel.Tests/bin/Debug/net10.0 is six below the root).
        foreach (var root in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            var current = root;
            for (var depth = 0; depth < 8 && current is not null; depth++)
            {
                yield return Path.Combine(current, configured);
                current = Path.GetDirectoryName(current);
            }
        }
    }

    public void Dispose()
    {
        if (_session.IsValueCreated) _session.Value.Dispose();
    }
}
