using Raylib_cs;
using Wriggle.Core;
using Wriggle.World;
using Wriggle.Physics;
using Wriggle.Characters;
using Wriggle.Players;
using Wriggle.VisualEffects;
using Wriggle.Weapons;
using System.Numerics;
using System.Collections.Generic;

namespace Wriggle.Scenes;

/// <summary>Visible starting point for the next game's implementation.</summary>
public sealed class GameScene : Scene
{
    public override string Name => "Game";
    private readonly string path = Path.Combine(AppContext.BaseDirectory, "Content", "Maps", "Basic.png");
    private readonly GameWorld world;
    private Camera2D camera;
    private readonly List<Character> characters;
    private readonly List<ExplosionEffect> explosionEffects = [];
    private bool isCarveMode = true;

    public GameScene()
    {
        Terrain terrain = Terrain.FromImage(path);
        PhysicsWorld physicsWorld = new(terrain);
        world = new GameWorld(physicsWorld);

        Player player = new(1);
        Wriggler wriggler = new(new Vector2(100f, 200f), 20f, 10f, 1f, Color.Blue);
        if (wriggler.SelectedWeapon is Grenade grenade)
            grenade.Exploded += CreateExplosionEffect;
        player.Wrigglers.Add(wriggler);
        world.AddPlayer(player);

        characters = [wriggler];
        world.PhysicsWorld.AddBody(wriggler.Body);

        camera = new Camera2D
        {
            Target = new Vector2(world.Terrain.W / 2f, world.Terrain.H / 2f),
            Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
            Rotation = 0f,
            Zoom = 1f
        };
    }

    public override void Update(float dt)
    {
        Wriggler wriggler = world.TurnController.CurrentPlayer.CurrentWriggler;
        camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
        Vector2 aimMouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        Vector2 aimDirection = aimMouseWorld - wriggler.Body.Position;
        float move =
            (Raylib.IsKeyDown(KeyboardKey.D) ? 1f : 0f) -
            (Raylib.IsKeyDown(KeyboardKey.A) ? 1f : 0f);

        bool jumpPressed = Raylib.IsKeyPressed(KeyboardKey.Space);
        bool firePressed = Raylib.IsKeyPressed(KeyboardKey.F);
        wriggler.Update(world.PhysicsWorld, characters, move, jumpPressed, firePressed, aimDirection, dt);

        world.PhysicsWorld.Step(dt);
        world.UpdateWeapons(characters, dt);
        UpdateExplosionEffects(dt);

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
                world.Terrain.Carve((int)mouseWorld.X, (int)mouseWorld.Y, 10);
                
            }
            else
            {
                Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                world.Terrain.Paint((int)mouseWorld.X, (int)mouseWorld.Y, 10, Color.Red);
            }
        }
        if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            const float bodyRadius = 5f;
            if (world.Terrain.CircleCollisionNormal(mouseWorld, bodyRadius) == Vector2.Zero)
                world.PhysicsWorld.AddBody(new CircleBody(mouseWorld, 1f, bodyRadius));
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.White);

        camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
        Raylib.BeginMode2D(camera);
        world.Terrain.Draw();
        world.PhysicsWorld.Draw();
        foreach (ExplosionEffect effect in explosionEffects)
            effect.Draw();
        Raylib.EndMode2D();
    }

    private void CreateExplosionEffect(Vector2 position, float radius)
    {
        explosionEffects.Add(new ExplosionEffect(position, radius));
    }

    private void UpdateExplosionEffects(float dt)
    {
        for (int i = explosionEffects.Count - 1; i >= 0; i--)
        {
            ExplosionEffect effect = explosionEffects[i];
            effect.Update(dt);
            if (!effect.IsFinished)
                continue;

            effect.Dispose();
            explosionEffects.RemoveAt(i);
        }
    }

    public override void OnExit()
    {
        foreach (ExplosionEffect effect in explosionEffects)
            effect.Dispose();
        explosionEffects.Clear();
    }

    public override void DrawDebugUI()
    {
        ImGuiNET.ImGui.Text($"Terrain: {world.Terrain.W}x{world.Terrain.H}");
        ImGuiNET.ImGui.Text($"Path: {path}");
        ImGuiNET.ImGui.Text($"Zoom: {camera.Zoom:P0} (mouse wheel)");
        ImGuiNET.ImGui.Text($"Bodies: {world.PhysicsWorld.BodyCount}");
        ImGuiNET.ImGui.Text($"Players: {world.Players.Count}");
        ImGuiNET.ImGui.Text($"Round: {world.RoundController.CurrentRound}");
        ImGuiNET.ImGui.Text($"Turn: {world.TurnController.CurrentTurnIndex}");
        if(ImGuiNET.ImGui.Button($"Toggle Carve Mode {(isCarveMode ? "ON" : "OFF")}"))
        {
            isCarveMode = !isCarveMode;
        }
        Wriggler wriggler = world.TurnController.CurrentPlayer.CurrentWriggler;
        ImGuiNET.ImGui.Text($"Wriggler State:{wriggler.State}");
        ImGuiNET.ImGui.Text($"Health: {wriggler.Health:0}");
    }
}
