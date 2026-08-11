using Gabriel.Core.Configuration;
using Gabriel.Engine.Tools.Shell;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Tools;

public class ShellExecuteToolTests
{
    private static ShellExecuteTool Create(bool enabled = true, string? workingRoot = null)
    {
        var options = new AgentToolsOptions
        {
            Shell = new ShellToolOptions
            {
                Enabled = enabled,
                WorkingRoot = workingRoot ?? Path.GetTempPath(),
                TimeoutSeconds = 30,
            },
        };
        return new ShellExecuteTool(Options.Create(options), NullLogger<ShellExecuteTool>.Instance);
    }

    [Fact]
    public async Task Disabled_by_default_returns_soft_error()
    {
        var result = await Create(enabled: false).ExecuteAsync(
            """{"command":"echo hi"}""", CancellationToken.None);
        Assert.StartsWith("Error", result);
        Assert.Contains("disabled", result);
    }

    [Theory]
    [InlineData("shutdown -h now")]
    [InlineData("sudo reboot")]
    [InlineData("mkfs.ext4 /dev/sda1")]
    [InlineData("dd if=/dev/zero of=/dev/sda")]
    [InlineData("git push origin main --force")]
    public async Task Destructive_commands_are_rejected(string command)
    {
        var result = await Create().ExecuteAsync(
            $$"""{"command":{{System.Text.Json.JsonSerializer.Serialize(command)}}}""",
            CancellationToken.None);
        Assert.StartsWith("Error", result);
        Assert.Contains("safety filter", result);
    }

    [Fact]
    public async Task Echo_runs_and_reports_exit_code_and_stdout()
    {
        var result = await Create().ExecuteAsync(
            """{"command":"echo gabriel-shell-ok"}""", CancellationToken.None);
        Assert.Contains("Exit code: 0", result);
        Assert.Contains("gabriel-shell-ok", result);
    }

    [Fact]
    public async Task Missing_working_root_refuses_to_run()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var result = await Create(workingRoot: missing).ExecuteAsync(
            """{"command":"echo hi"}""", CancellationToken.None);
        Assert.StartsWith("Error", result);
        Assert.Contains("working directory", result);
    }
}
