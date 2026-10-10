using System.Numerics;
using Wriggle.Characters;

namespace Wriggle.Physics;

public abstract class AreaEffect
{
    public Vector2 Position { get; }

    protected AreaEffect(Vector2 position)
    {
        Position = position;
    }

    public abstract void ApplyEffect(PhysicsWorld world, IReadOnlyList<Character> characters);
}

public class ExplosionAreaEffect : AreaEffect
{
    public float Radius { get; }
    public float Force { get; }
    public float Damage { get; }

    public ExplosionAreaEffect(Vector2 position, float radius, float force, float damage)
        : base(position)
    {
        Radius = radius;
        Force = force;
        Damage = damage;
    }

    public override void ApplyEffect(PhysicsWorld world, IReadOnlyList<Character> characters)
    {
        world.ApplyRadialImpulse(Position, Radius, Force);
        world.CarveTerrain(Position, Radius);

        if (Radius <= 0f)
            return;

        foreach (Character character in characters)
        {
            float distance = Vector2.Distance(Position, character.Body.Position);
            if (distance < Radius)
                character.TakeDamage(Damage * (1f - distance / Radius));
        }
    }
}
