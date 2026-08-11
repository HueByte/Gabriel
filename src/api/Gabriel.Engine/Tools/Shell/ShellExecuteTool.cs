using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Gabriel.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Shell;

// The task-loop workhorse (Claude Code's Bash equivalent): runs one shell
// command and returns exit code + stdout + stderr as the observation.
//
// Threat model, stated honestly: the deny list below is a tripwire for the
// obviously catastrophic (recursive root deletes, disk formatting, firewall/
// service tampering), NOT a security boundary - a shell that can run programs
// can do anything the API process can. The real controls are (1) Enabled
// defaults to false, (2) the operator chooses the working root, (3) every
// command + result is logged. Enabling this tool means trusting the model
// with the host account's shell.
//
// One command per call, no persistent session (deviation from Claude Code's
// stateful shell - stateless keeps the API horizontally scalable and restart-
// safe; chain with `&&` or absolute paths instead of relying on cwd).
public sealed class ShellExecuteTool : ITool
{
    private static readonly string[] BuiltinDenyPatterns =
    [
        @"rm\s+(-\w+\s+)*(/|/\*)\s*$",          // rm -rf / and friends
        @"rm\s+(-\w+\s+)*~",                    // recursive delete of home
        @"Remove-Item\s+.*-Recurse.*\s(C|D):\\(\s|$|\*)", // PS drive-root wipe
        @"\bmkfs(\.|\s)",                       // format a filesystem
        @"\bdd\s+.*of=/dev/",                   // raw-write a device
        @"\bdiskpart\b",
        @"\bformat(\.com)?\s+[a-z]:",
        @"\b(shutdown|reboot|halt|poweroff)\b",
        @":\(\)\s*\{.*\};\s*:",                 // classic fork bomb
        @"\breg(\.exe)?\s+delete\b",
        @"\bsc(\.exe)?\s+(delete|stop)\b",
        @"\bnetsh\s+advfirewall\b",
        @"\bgit\s+push\s+.*--force",            // history rewrite on remotes
    ];

    private readonly ShellToolOptions _options;
    private readonly string? _fallbackRoot;
    private readonly ILogger<ShellExecuteTool> _logger;

    public ShellExecuteTool(IOptions<AgentToolsOptions> options, ILogger<ShellExecuteTool> logger)
    {
        _options = options.Value.Shell;
        _fallbackRoot = options.Value.HostRoot;
        _logger = logger;
    }

    public string Name => "shell_execute";

    public string Description =>
        "Run a single shell command on the host (PowerShell on Windows, sh on " +
        "Linux) and get back its exit code, stdout, and stderr. Commands run " +
        "in the configured workspace directory with a timeout. There is NO " +
        "persistent session between calls - chain steps with '&&' or use " +
        "absolute paths. Use for builds, tests, git status/diff/log, and " +
        "package tooling. Prefer the dedicated file tools (list_dir, grep, " +
        "find, file_info) over shell equivalents.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "command": { "type": "string", "description": "the command line to execute" },
            "timeout_seconds": { "type": "integer", "description": "optional wall-clock limit; capped by server config" }
          },
          "required": ["command"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            return "Error: shell execution is disabled on this server (AgentTools:Shell:Enabled=false).";
        }

        var workingRoot = !string.IsNullOrWhiteSpace(_options.WorkingRoot)
            ? _options.WorkingRoot
            : _fallbackRoot;
        if (string.IsNullOrWhiteSpace(workingRoot) || !Directory.Exists(workingRoot))
        {
            return "Error: no valid working directory configured (AgentTools:Shell:WorkingRoot or AgentTools:HostRoot).";
        }

        ShellArgs args;
        try
        {
            args = JsonSerializer.Deserialize<ShellArgs>(argumentsJson, JsonOpts)
                ?? throw new InvalidOperationException("null args");
        }
        catch (Exception ex)
        {
            return $"Error: invalid arguments JSON — {ex.Message}";
        }

        if (string.IsNullOrWhiteSpace(args.Command))
        {
            return "Error: command must be non-empty.";
        }

        foreach (var pattern in BuiltinDenyPatterns.Concat(_options.DenyPatterns))
        {
            try
            {
                if (Regex.IsMatch(args.Command, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                {
                    _logger.LogWarning("shell_execute REJECTED by deny pattern | pattern={Pattern} command={Command}",
                        pattern, args.Command);
                    return $"Error: command rejected by safety filter (matched deny pattern).";
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // A pathological operator pattern shouldn't block execution.
            }
        }

        var timeoutSeconds = Math.Clamp(
            args.TimeoutSeconds is { } requested ? Math.Min(requested, _options.TimeoutSeconds) : _options.TimeoutSeconds,
            1, 600);

        // ArgumentList (not a hand-built Arguments string) so the runtime does
        // the platform quoting and the command reaches the shell verbatim as
        // one argument - $variables, quotes, and pipes all survive intact.
        var psi = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "powershell.exe" : "/bin/sh",
            WorkingDirectory = workingRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (OperatingSystem.IsWindows())
        {
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-NonInteractive");
            psi.ArgumentList.Add("-Command");
            psi.ArgumentList.Add(args.Command);
        }
        else
        {
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add(args.Command);
        }

        _logger.LogInformation("shell_execute START | cwd={Cwd} timeout={Timeout}s command={Command}",
            workingRoot, timeoutSeconds, args.Command);

        using var process = new Process { StartInfo = psi };
        var sw = Stopwatch.StartNew();
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return $"Error: could not start shell — {ex.Message}";
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
        var timedOut = false;
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Already exited between the timeout and the kill - fine.
            }
        }

        var stdout = Truncate(await SafeRead(stdoutTask));
        var stderr = Truncate(await SafeRead(stderrTask));
        sw.Stop();

        if (timedOut)
        {
            _logger.LogWarning("shell_execute TIMEOUT after {Timeout}s | command={Command}", timeoutSeconds, args.Command);
            return $"Error: command timed out after {timeoutSeconds}s and was killed.\n" + FormatOutput(null, stdout, stderr);
        }

        _logger.LogInformation("shell_execute DONE | exit={ExitCode} elapsedMs={ElapsedMs}",
            process.ExitCode, sw.ElapsedMilliseconds);
        return FormatOutput(process.ExitCode, stdout, stderr);
    }

    private static string FormatOutput(int? exitCode, string stdout, string stderr)
    {
        var sb = new StringBuilder();
        if (exitCode is { } code) sb.Append("Exit code: ").Append(code).AppendLine();
        sb.AppendLine("--- stdout ---");
        sb.AppendLine(string.IsNullOrEmpty(stdout) ? "(empty)" : stdout);
        if (!string.IsNullOrEmpty(stderr))
        {
            sb.AppendLine("--- stderr ---");
            sb.AppendLine(stderr);
        }
        return sb.ToString().TrimEnd();
    }

    private string Truncate(string text)
    {
        var limit = Math.Max(1000, _options.MaxOutputChars);
        return text.Length <= limit
            ? text
            : text[..limit] + $"\n… (truncated, {text.Length - limit} more chars)";
    }

    private static async Task<string> SafeRead(Task<string> task)
    {
        try
        {
            return await task;
        }
        catch
        {
            // Stream torn down by the kill - partial output is gone; the
            // timeout message already tells the model what happened.
            return string.Empty;
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record ShellArgs(
        string Command,
        [property: System.Text.Json.Serialization.JsonPropertyName("timeout_seconds")] int? TimeoutSeconds);
}
