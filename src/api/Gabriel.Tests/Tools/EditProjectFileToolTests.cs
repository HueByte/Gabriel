using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Services;
using Gabriel.Engine.Tools;
using Gabriel.Engine.Tools.Projects;
using Xunit;

namespace Gabriel.Tests.Tools;

public class EditProjectFileToolTests
{
    private static readonly Guid ProjectId = Guid.NewGuid();

    private static (EditProjectFileTool Edit, WriteProjectFileTool Write, InMemoryFiles Files) Create(params (string Name, string Text)[] files)
    {
        var store = new InMemoryFiles();
        foreach (var (name, text) in files) store.Put(name, text);
        var context = new ToolExecutionContext();
        context.Set(Guid.NewGuid(), Guid.NewGuid(), ProjectId);
        return (new EditProjectFileTool(store, context), new WriteProjectFileTool(store, context), store);
    }

    [Fact]
    public async Task Replaces_a_unique_exact_match_using_snake_case_args()
    {
        var (edit, _, files) = Create(("notes.md", "# Notes\nfeed daily\n"));

        var result = await edit.ExecuteAsync("""{"file":"NOTES.md","old_text":"feed daily","new_text":"feed twice daily"}""", default);

        Assert.StartsWith("Edited notes.md", result);
        Assert.Equal("# Notes\nfeed twice daily\n", files.Text("notes.md"));
    }

    [Fact]
    public async Task Refuses_an_ambiguous_match_unless_replace_all()
    {
        var (edit, _, files) = Create(("a.txt", "x x x"));

        var refused = await edit.ExecuteAsync("""{"file":"a.txt","old_text":"x","new_text":"y"}""", default);
        Assert.Contains("occurs 3 times", refused);
        Assert.Equal("x x x", files.Text("a.txt"));

        await edit.ExecuteAsync("""{"file":"a.txt","old_text":"x","new_text":"y","replace_all":true}""", default);
        Assert.Equal("y y y", files.Text("a.txt"));
    }

    [Fact]
    public async Task Reports_a_missing_match_without_writing()
    {
        var (edit, _, files) = Create(("a.txt", "hello"));

        var result = await edit.ExecuteAsync("""{"file":"a.txt","old_text":"bye","new_text":"x"}""", default);

        Assert.Contains("not found", result);
        Assert.Equal(0, files.Writes);
    }

    [Fact]
    public async Task Write_refuses_to_clobber_unless_overwrite()
    {
        var (_, write, files) = Create(("plan.md", "user's plan"));

        var refused = await write.ExecuteAsync("""{"name":"plan.md","content":"agent text"}""", default);
        Assert.StartsWith("Error:", refused);
        Assert.Equal("user's plan", files.Text("plan.md"));

        var ok = await write.ExecuteAsync("""{"name":"plan.md","content":"agent text","overwrite":true}""", default);
        Assert.StartsWith("Wrote plan.md", ok);
        Assert.Equal("agent text", files.Text("plan.md"));
    }

    // Just enough of IProjectFileService for the write/edit tools; the real
    // implementation's path hardening is DiskProjectFileService's concern.
    private sealed class InMemoryFiles : IProjectFileService
    {
        private readonly Dictionary<string, (ProjectFile File, string Text)> _files = new(StringComparer.OrdinalIgnoreCase);
        public int Writes { get; private set; }

        public void Put(string name, string text)
            => _files[name] = (ProjectFile.Create(ProjectId, name, name, text.Length, "text/plain"), text);

        public string Text(string name) => _files[name].Text;

        public Task<IReadOnlyList<ProjectFile>> ListAsync(Guid projectId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ProjectFile>>(_files.Values.Select(v => v.File).ToList());

        public Task<string?> ReadTextAsync(Guid projectId, Guid fileId, int maxBytes, CancellationToken ct = default)
            => Task.FromResult<string?>(_files.Values.First(v => v.File.Id == fileId).Text);

        public Task<ProjectFile> WriteTextAsync(Guid projectId, string filename, string content, bool overwrite, CancellationToken ct = default)
        {
            if (_files.ContainsKey(filename) && !overwrite)
                throw new DomainException($"'{filename}' already exists.");
            Writes++;
            Put(filename, content);
            return Task.FromResult(_files[filename].File);
        }

        public Task<ProjectFile> GetAsync(Guid projectId, Guid fileId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<(ProjectFile File, Stream Content)> OpenAsync(Guid projectId, Guid fileId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<ProjectFile> UploadAsync(Guid projectId, string filename, string? contentType, Stream content, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(Guid projectId, Guid fileId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<string> GetProjectDirectoryAsync(Guid projectId, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
