using My2DGame.Core;
using Raylib_cs;

namespace My2DGame.Scenes;

/// <summary>Visible starting point for the next game's implementation.</summary>
public sealed class GameScene : Scene
{
    public override string Name => "Game";

    public override void Draw()
    {
        const string title = "New game starts here";
        const string subtitle = "Edit Scenes/GameScene.cs to begin.";
        const int titleSize = 36;
        const int subtitleSize = 18;

        int centerX = Raylib.GetScreenWidth() / 2;
        int centerY = Raylib.GetScreenHeight() / 2;

        Raylib.DrawText(title, centerX - Raylib.MeasureText(title, titleSize) / 2, centerY - 32, titleSize, Color.White);
        Raylib.DrawText(subtitle, centerX - Raylib.MeasureText(subtitle, subtitleSize) / 2, centerY + 20, subtitleSize, Color.LightGray);
    }
}
