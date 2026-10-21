using Gabriel.Engine.Providers;
using Xunit;

namespace Gabriel.Tests.Services;

public class ProviderTextTests
{
    [Theory]
    [InlineData("""{"done": true}""")]
    [InlineData("""Here you go: {"done": true} hope that helps""")]
    [InlineData("```json\n{\"done\": true}\n```")]
    public void Extracts_the_first_object_from_common_wrappers(string reply)
    {
        using var doc = ProviderText.ExtractJsonObject(reply);

        Assert.NotNull(doc);
        Assert.True(doc.RootElement.GetProperty("done").GetBoolean());
    }

    [Fact]
    public void Braces_inside_strings_do_not_end_the_object()
    {
        using var doc = ProviderText.ExtractJsonObject("""{"feedback": "use {placeholders} and \"quotes\"", "done": false}""");

        Assert.NotNull(doc);
        Assert.Equal("use {placeholders} and \"quotes\"", doc.RootElement.GetProperty("feedback").GetString());
    }

    [Fact]
    public void Skips_a_broken_object_and_finds_a_later_valid_one()
    {
        using var doc = ProviderText.ExtractJsonObject("""{not json} then {"done": false}""");

        Assert.NotNull(doc);
        Assert.False(doc.RootElement.GetProperty("done").GetBoolean());
    }

    [Theory]
    [InlineData("")]
    [InlineData("no json here")]
    [InlineData("{unterminated")]
    public void Returns_null_when_there_is_no_object(string reply)
        => Assert.Null(ProviderText.ExtractJsonObject(reply));
}
