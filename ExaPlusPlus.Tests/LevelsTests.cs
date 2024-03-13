namespace ExaPlusPlus.Tests;

public class LevelsTests
{
    [Test]
    public void ShouldWornOnLevel3()
    {
        var input = File.ReadAllText(@"Levels/03_trash_world_news_tutorial.expp");

        var expected = File.ReadAllText(@"Levels/03_trash_world_news_tutorial.exppc");

        var result = CompileHelper.Compile(input);

        Assert.AreEqual(expected.Trim(), result.Trim());
    }
}