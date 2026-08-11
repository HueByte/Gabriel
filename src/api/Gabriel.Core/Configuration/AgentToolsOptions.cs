namespace Gabriel.Core.Configuration;

// Options for the filesystem + shell agent tools. Bound from "AgentTools:*"
// in configuration. The single most important knob is HostRoot - every host-
// mode filesystem op canonicalizes under it, and anything resolving outside
// is rejected.
public sealed class AgentToolsOptions : IConfigSection<AgentToolsOptions>
{
    public static string SectionName => "AgentTools";

    // Absolute directory the host-mode filesystem tools are pinned to. Relative
    // paths resolve against it; absolute paths must canonicalize under it.
    // Leave null/empty to disable host mode entirely (project-sandbox mode
    // still works). Operator must opt in deliberately - there's no auto-default
    // to the process cwd because that's usually the API binary directory, not
    // a useful workspace.
    public string? HostRoot { get; set; }

    // Hard cap on bytes scanned when previewing a file (file_info head/tail,
    // future grep). Bigger files report size but skip preview.
    public long MaxPreviewBytes { get; set; } = 10 * 1024 * 1024; // 10 MiB

    // Max directory entries any single listing operation will return, even if
    // the user / model asked for more. Acts as the safety stop for runaway
    // recursion in list_dir / find.
    public int MaxListEntries { get; set; } = 1000;

    // Default entries to show in a non-paginated listing.
    public int DefaultListEntries { get; set; } = 200;

    // Default head/tail line count shown by file_info previews.
    public int DefaultPreviewLines { get; set; } = 6;

    // Shell executor (shell_execute). Off by default: enabling it hands the
    // model a real command line on the host, which is an explicit operator
    // decision, not a default.
    public ShellToolOptions Shell { get; set; } = new();
}

public sealed class ShellToolOptions
{
    public bool Enabled { get; set; }

    // Working directory commands run in. Falls back to AgentTools:HostRoot
    // when empty; if neither is set the tool refuses to run even when
    // enabled. Note this pins the STARTING directory only - a shell can cd
    // anywhere, which is exactly why Enabled defaults to false.
    public string? WorkingRoot { get; set; }

    // Per-command wall clock. The model can request less via timeout_seconds
    // but never more. Clamped to 600s.
    public int TimeoutSeconds { get; set; } = 60;

    // Combined stdout/stderr cap per stream; longer output is truncated with
    // a marker so one noisy build log can't flood the context window.
    public int MaxOutputChars { get; set; } = 20000;

    // Operator-supplied regexes (case-insensitive) appended to the built-in
    // deny list. A match rejects the command before anything executes.
    public List<string> DenyPatterns { get; set; } = new();
}
