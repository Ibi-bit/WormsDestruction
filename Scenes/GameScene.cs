using My2DGame.Core;
using Raylib_cs;
using My2DGame.World;
using System.Numerics;

namespace My2DGame.Scenes;

/// <summary>Visible starting point for the next game's implementation.</summary>
public sealed class GameScene : Scene
{
    public override string Name => "Game";
    public string path = "Content/Maps/Basic.png";
    private readonly Terrain terrain = Terrain.FromImage("Content/Maps/Basic.png");
    private Camera2D camera;

    public GameScene()
    {
        camera = new Camera2D
        {
            Target = new Vector2(terrain.W / 2f, terrain.H / 2f),
            Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
            Rotation = 0f,
            Zoom = 1f
        };
    }

    public override void Update(float dt)
    {
        camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);

        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0f)
        {
            Vector2 mouse = Raylib.GetMousePosition();
            Vector2 worldBeforeZoom = Raylib.GetScreenToWorld2D(mouse, camera);

            camera.Zoom = Math.Clamp(camera.Zoom * MathF.Pow(1.2f, wheel), 0.25f, 8f);

            Vector2 worldAfterZoom = Raylib.GetScreenToWorld2D(mouse, camera);
            camera.Target += worldBeforeZoom - worldAfterZoom;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            terrain.Carve((int)mouseWorld.X, (int)mouseWorld.Y, 10);
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.White);

        camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
        Raylib.BeginMode2D(camera);
        terrain.Draw();
        Raylib.EndMode2D();
    }

    public override void DrawDebugUI()
    {
        ImGuiNET.ImGui.Text($"Terrain: {terrain.W}x{terrain.H}");
        ImGuiNET.ImGui.Text($"Path: {path}");
        ImGuiNET.ImGui.Text($"Zoom: {camera.Zoom:P0} (mouse wheel)");
    }
}
