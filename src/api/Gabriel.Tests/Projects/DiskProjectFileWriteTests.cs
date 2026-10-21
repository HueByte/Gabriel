using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Infrastructure.Persistence;
using Gabriel.Infrastructure.Persistence.Repositories;
using Gabriel.Infrastructure.Projects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Projects;

// The agent write path against the real service: SQLite (in-memory) for the
// metadata rows, a temp directory for the bytes. Exercises what the tool
// fakes can't - collision handling, atomic overwrite, size bookkeeping, and
// the shared sanitizing/allowlist guards.
public sealed class DiskProjectFileWriteTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly string _root = Path.Combine(Path.GetTempPath(), "gabriel-tests-" + Guid.NewGuid().ToString("N"));
    private readonly AppDbContext _db;
    private readonly DiskProjectFileService _files;
    private readonly Guid _projectId;

    public DiskProjectFileWriteTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var project = Project.Create(userId, "Test project");
        _db.Projects.Add(project);
        _db.SaveChanges();
        _projectId = project.Id;

        _files = new DiskProjectFileService(
            _db,
            new ProjectRepository(_db),
            new FixedUser(userId),
            Options.Create(new ProjectFilesOptions { Root = _root }),
            NullLogger<DiskProjectFileService>.Instance);
    }

    private string DiskText(ProjectFile f)
        => File.ReadAllText(Path.Combine(_root, _projectId.ToString("N"), f.RelativePath));

    [Fact]
    public async Task Creates_a_text_file_with_metadata()
    {
        var file = await _files.WriteTextAsync(_projectId, "report.md", "# Report\nżółć", overwrite: false);

        Assert.Equal("report.md", file.Name);
        Assert.Equal("# Report\nżółć", DiskText(file));
        Assert.Equal(new FileInfo(Path.Combine(_root, _projectId.ToString("N"), "report.md")).Length, file.SizeBytes);
        Assert.Single(await _files.ListAsync(_projectId));
    }

    [Fact]
    public async Task Existing_file_is_kept_unless_overwrite()
    {
        await _files.WriteTextAsync(_projectId, "notes.md", "original", overwrite: false);

        await Assert.ThrowsAsync<DomainException>(() => _files.WriteTextAsync(_projectId, "NOTES.md", "clobber", overwrite: false));

        var replaced = await _files.WriteTextAsync(_projectId, "NOTES.md", "replacement text", overwrite: true);
        Assert.Equal("notes.md", replaced.Name);
        Assert.Equal("replacement text", DiskText(replaced));
        Assert.Equal(16, replaced.SizeBytes);
        Assert.Single(await _files.ListAsync(_projectId));
        // No temp files left behind by the write-then-move swap.
        Assert.Single(Directory.GetFiles(Path.Combine(_root, _projectId.ToString("N"))));
    }

    [Theory]
    [InlineData("tool.exe")]
    [InlineData("image.png")]
    public async Task Refuses_disallowed_or_binary_types(string name)
        => await Assert.ThrowsAsync<DomainException>(() => _files.WriteTextAsync(_projectId, name, "x", overwrite: false));

    [Fact]
    public async Task Traversal_in_the_name_stays_inside_the_project()
    {
        var file = await _files.WriteTextAsync(_projectId, "../../escape.md", "x", overwrite: false);

        var written = Path.GetFullPath(Path.Combine(_root, _projectId.ToString("N"), file.RelativePath));
        Assert.StartsWith(Path.GetFullPath(Path.Combine(_root, _projectId.ToString("N"))), written);
        Assert.False(File.Exists(Path.Combine(_root, "escape.md")));
    }

    [Fact]
    public async Task Another_users_project_is_not_writable()
    {
        var stranger = new DiskProjectFileService(
            _db, new ProjectRepository(_db), new FixedUser(Guid.NewGuid()),
            Options.Create(new ProjectFilesOptions { Root = _root }), NullLogger<DiskProjectFileService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => stranger.WriteTextAsync(_projectId, "x.md", "x", overwrite: false));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private sealed class FixedUser(Guid id) : ICurrentUser
    {
        public Guid? UserId => id;
        public bool IsAuthenticated => true;
        public string? Email => null;
    }
}
