using System.Numerics;
using Raylib_cs;
using Wriggle.World;
using ImGuiNET;

namespace Wriggle.Physics;

public class PhysicsWorld
{
    private float gravity = 600f;
    private const int CollisionSolverIterations = 8;
    private readonly Terrain terrain;
    private readonly List<Body> bodies = new();
    public int BodyCount => bodies.Count;

    private readonly record struct BroadPhaseEntry(Rectangle Bounds, Body Body);

    public PhysicsWorld(Terrain terrain)
    {
        this.terrain = terrain;
    }

    public void AddBody(Body body)
    {
        bodies.Add(body);
    }
    public void RemoveBody(Body body)
    {
        bodies.Remove(body);
    }
    public void ApplyRadialImpulse(Vector2 center, float radius, float force)
    {
        if (radius <= 0f || force <= 0f)
            return;

        foreach (Body body in bodies)
        {
            Vector2 toBody = body.Position - center;
            float distance = toBody.Length();
            if (distance >= radius || body.Mass <= 0f)
                continue;

            Vector2 impulseDirection = distance > 0.0001f
                ? toBody / distance
                : -Vector2.UnitY;
            float impulseMagnitude = force * (1f - distance / radius);
            body.Velocity += impulseDirection * impulseMagnitude / body.Mass;
        }
    }
    private bool MoveBodyOnTerrain(Body body, Vector2 displacement)
    {
        Vector2 previousPosition = body.Position;
        body.Position += displacement;

        Vector2 normal = body.TerrainCollisionNormal(terrain);
        if (normal.LengthSquared() <= 0.000001f)
            return false;

        bool walkable = body.IsWalkableSurface(normal);
        if (!walkable && body.Velocity.X * normal.X < 0f)
            body.Velocity.X = 0f;

        body.Position = previousPosition;
        if (!walkable && displacement.X * normal.X < 0f)
            displacement.X = 0f;

        float intoSurface = Vector2.Dot(displacement, normal);
        if (intoSurface < 0f)
            displacement -= intoSurface * normal;

        body.Position += displacement;
        body.OnCollision(normal, MathF.Min(body.Restitution, terrain.Restitution));
        if (walkable && intoSurface < 0f)
            body.isGrounded = true;

        return true;
    }

    public void Step(float dt)
    {
        if (dt <= 0f) return;

        Vector2 gravityVector = new(0f, gravity);
        foreach (Body body in bodies)
        {
            float estimatedTravel = body.Velocity.Length() * dt + MathF.Abs(gravity) * dt * dt;
            int substepCount = Math.Clamp((int)MathF.Ceiling(estimatedTravel), 1, 512);
            float substepDt = dt / substepCount;
            body.isGrounded = false;

            for (int i = 0; i < substepCount; i++)
            {
                Vector2 previousPosition = body.Position;
                body.Integrate(substepDt, gravityVector);
                Vector2 displacement = body.Position - previousPosition;
                body.Position = previousPosition;
                MoveBodyOnTerrain(body, displacement);
            }
        }

        for (int i = 0; i < CollisionSolverIterations; i++)
        {
            BroadPhaseCollisionDetection();

            foreach (Body body in bodies)
                ResolveTerrainPenetration(body);
        }

        foreach (Body body in bodies)
            body.CompleteStep();
    }

    private void BroadPhaseCollisionDetection()
    {
        List<BroadPhaseEntry> entries = new(bodies.Count);
        foreach (Body body in bodies)
            entries.Add(new BroadPhaseEntry(body.BroadPhaseBounds, body));

        entries.Sort(static (a, b) => a.Bounds.X.CompareTo(b.Bounds.X));

        List<BroadPhaseEntry> activeEntries = new();
        for (int i = 0; i < entries.Count; i++)
        {
            BroadPhaseEntry current = entries[i];

            for (int j = activeEntries.Count - 1; j >= 0; j--)
            {
                BroadPhaseEntry active = activeEntries[j];
                if (active.Bounds.X + active.Bounds.Width < current.Bounds.X)
                    activeEntries.RemoveAt(j);
            }

            foreach (BroadPhaseEntry active in activeEntries)
            {
                if (BoundsOverlap(current.Bounds, active.Bounds))
                {
                    float restitution = MathF.Min(current.Body.Restitution, active.Body.Restitution);
                    current.Body.ResolveDynamicCollision(active.Body, restitution);
                }
            }

            activeEntries.Add(current);
        }
    }

    private static bool BoundsOverlap(Rectangle a, Rectangle b)
    {
        return a.X < b.X + b.Width
            && a.X + a.Width > b.X
            && a.Y < b.Y + b.Height
            && a.Y + a.Height > b.Y;
    }

    private void ResolveTerrainPenetration(Body body)
    {
        int maxCorrections = Math.Clamp(Math.Max(terrain.W, terrain.H), 8, 2048);
        Vector2 collisionNormal = Vector2.Zero;

        for (int i = 0; i < maxCorrections; i++)
        {
            Vector2 normal = body.TerrainCollisionNormal(terrain);
            if (normal.LengthSquared() <= 0.000001f)
                break;

            if (collisionNormal == Vector2.Zero)
                collisionNormal = normal;

            body.Position += normal;
        }

        if (collisionNormal != Vector2.Zero)
        {
            body.OnCollision(collisionNormal, MathF.Min(body.Restitution, terrain.Restitution));
            if (body.IsWalkableSurface(collisionNormal))
                body.isGrounded = true;
        }
    }
    public void CarveTerrain(Vector2 center, float radius)
    {
        terrain.Carve((int)center.X, (int)center.Y, (int)radius);
    }

    public void Draw()
    {
        foreach (Body body in bodies)
        {
            body.Draw();
        }
    }

    public void DebugMenu()
    {
        ImGui.Begin("Physics World");
        ImGui.Text($"Gravity: {gravity}");
        ImGui.InputFloat("Gravity", ref gravity, 10f, 50f, "%.1f");
        ImGui.Text($"Bodies: {bodies.Count}");
        ImGui.End();
    }

}
