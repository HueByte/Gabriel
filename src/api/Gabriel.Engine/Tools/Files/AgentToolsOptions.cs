namespace Gabriel.Engine.Tools.Files;

// Options for the filesystem + shell agent tools. Bound from "AgentTools:*"
// in configuration. The single most important knob is HostRoot - every host-
// mode filesystem op canonicalizes under it, and anything resolving outside
// is rejected.
public sealed class AgentToolsOptions
{
    public const string SectionName = "AgentTools";

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
}
