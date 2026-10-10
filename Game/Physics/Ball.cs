using Raylib_cs;
using System.Numerics;
using Wriggle.World;
using ImGuiNET;
namespace Wriggle.Physics;
public class Ball : Body
{
    public float radius
    {
        get => Radius;
        set => Radius = MathF.Max(1f, value);
    }
    public Vector2 position
    {
        get => Position;
        set => Position = value;
    }
    public Vector2 velocity
    {
        get => Velocity;
        set => Velocity = value;
    }
    public float mass
    {
        get => Mass;
        set => Mass = MathF.Max(0.0001f, value);
    }
    public Vector2 acceleration = Vector2.Zero;
    public Color color = Color.Red;
    public Rectangle broadPhaseBox =>
        new Rectangle(position.X - radius, position.Y - radius, radius * 2, radius * 2);


    public Ball(Vector2 startPosition, float radius = 24f, float mass = 1f)
        : base(startPosition, MathF.Max(0.0001f, mass), MathF.Max(1f, radius), 0.65f)
    {
        position = startPosition;
        this.mass = MathF.Max(0.0001f, mass);
        acceleration = Vector2.Zero;
    }

    public override void Integrate(float dt, Vector2 gravity)
    {
        velocity += (acceleration + gravity) * dt;
        position += velocity * dt;
    }

    public override void CompleteStep()
    {
        acceleration = Vector2.Zero;
    }

    public override void Draw()
    {
        Raylib.DrawCircle((int)position.X, (int)position.Y, radius, color);
    }

    public void ApplyForce(Vector2 force)
    {
        Vector2 f = force / mass;
        acceleration += f;
    }

    public void ApplyImpulse(Vector2 impulse)
    {
        velocity += impulse / mass;
    }

    public static Vector2 ResoveCollision(
        Vector2 normal,
        Vector2 velocity1,
        Vector2 velocity2,
        float mass1,
        float mass2,
        float restitution
    )
    {
        float relativeVelocity = Vector2.Dot(velocity2 - velocity1, normal);
        float impulseMagnitude = -(1 + restitution) * relativeVelocity / (1 / mass1 + 1 / mass2);
        return impulseMagnitude * normal;
    }

    public void StaticCollision(Vector2 normal, float penetration, float restitution)
    {
        Vector2 impulse = ResoveCollision(
            normal,
            velocity,
            Vector2.Zero,
            mass,
            float.PositiveInfinity,
            restitution
        );
        ApplyImpulse(-impulse);

        float correction = MathF.Max(penetration - 0.01f, 0f) * 0.85f;
        position -= normal * correction;
    }
    public override Vector2 TerrainCollisionNormal(Terrain terrain)
    {
        return terrain.CircleCollisionNormal(position, radius);
    }
    public void DynamicCollision(Ball other, float restitution)
    {
        ResolveDynamicCollision(other, restitution);
    }

    public void BorderCollision(Rectangle bounds, float restitution)
    {
        if (position.X - radius < bounds.X)
        {
            position = new Vector2(bounds.X + radius, position.Y);
            velocity = new Vector2(-velocity.X * restitution, velocity.Y);
        }
        else if (position.X + radius > bounds.X + bounds.Width)
        {
            position = new Vector2(bounds.X + bounds.Width - radius, position.Y);
            velocity = new Vector2(-velocity.X * restitution, velocity.Y);
        }

        if (position.Y - radius < bounds.Y)
        {
            position = new Vector2(position.X, bounds.Y + radius);
            velocity = new Vector2(velocity.X, -velocity.Y * restitution);
        }
        else if (position.Y + radius > bounds.Y + bounds.Height)
        {
            position = new Vector2(position.X, bounds.Y + bounds.Height - radius);
            velocity = new Vector2(velocity.X, -velocity.Y * restitution);
        }
    }


}
