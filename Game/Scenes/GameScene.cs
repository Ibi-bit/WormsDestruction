using Raylib_cs;
using Wriggle.Core;
using Wriggle.World;
using Wriggle.Physics;
using Wriggle.Characters;
using System.Numerics;
using System.Collections.Generic;

namespace Wriggle.Scenes;

/// <summary>Visible starting point for the next game's implementation.</summary>
public sealed class GameScene : Scene
{
    public override string Name => "Game";
    private readonly string path = Path.Combine(AppContext.BaseDirectory, "Content", "Maps", "Basic.png");
    private readonly Terrain terrain;
    private Camera2D camera;
    private Wriggler wriggler;
    private PhysicsWorld physicsWorld;
    private readonly List<Character> characters = [];
    private bool isCarveMode = true;


    public GameScene()
    {
        terrain = Terrain.FromImage(path);
        camera = new Camera2D
        {
            Target = new Vector2(terrain.W / 2f, terrain.H / 2f),
            Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
            Rotation = 0f,
            Zoom = 1f
        };
        physicsWorld = new PhysicsWorld(terrain);
        wriggler = new Wriggler(new Vector2(100f, 200f), 20f, 10f, 1f, Color.Blue);
        characters.Add(wriggler);
        physicsWorld.AddBody(wriggler.Body);
    }
    

    public override void Update(float dt)
    {
        float move =
            (Raylib.IsKeyDown(KeyboardKey.D) ? 1f : 0f) -
            (Raylib.IsKeyDown(KeyboardKey.A) ? 1f : 0f);

        bool jumpPressed = Raylib.IsKeyPressed(KeyboardKey.Space);
        bool firePressed = Raylib.IsKeyPressed(KeyboardKey.F);
        wriggler.Update(physicsWorld, characters, move, jumpPressed, firePressed, dt);

        physicsWorld.Step(dt);
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
            if (isCarveMode)
            {
                Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                terrain.Carve((int)mouseWorld.X, (int)mouseWorld.Y, 10);
                

            }
            else
            {
                Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                terrain.Paint((int)mouseWorld.X, (int)mouseWorld.Y, 10, Color.Red);
            }
        }
        if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            const float bodyRadius = 5f;
            if (terrain.CircleCollisionNormal(mouseWorld, bodyRadius) == Vector2.Zero)
                physicsWorld.AddBody(new CircleBody(mouseWorld, 1f, bodyRadius));
        }

    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.White);

        camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
        Raylib.BeginMode2D(camera);
        terrain.Draw();
        physicsWorld.Draw();
        wriggler.Draw();
        Raylib.EndMode2D();
    }

    public override void DrawDebugUI()
    {
        ImGuiNET.ImGui.Text($"Terrain: {terrain.W}x{terrain.H}");
        ImGuiNET.ImGui.Text($"Path: {path}");
        ImGuiNET.ImGui.Text($"Zoom: {camera.Zoom:P0} (mouse wheel)");
        ImGuiNET.ImGui.Text($"Bodies: {physicsWorld.BodyCount}");
        if(ImGuiNET.ImGui.Button($"Toggle Carve Mode {(isCarveMode ? "ON" : "OFF")}"))
        {
            isCarveMode = !isCarveMode;
        }
        ImGuiNET.ImGui.Text($"Wriggler State:{wriggler.State}");
        ImGuiNET.ImGui.Text($"Health: {wriggler.Health:0}");
    }
}
