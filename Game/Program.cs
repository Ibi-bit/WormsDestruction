using Wriggle.Core;

namespace Wriggle;

internal static class Program
{
    private static void Main()
    {
        GameConfig config = GameConfig.Load();
        using Game game = new(config);
        game.Run();
    }
}