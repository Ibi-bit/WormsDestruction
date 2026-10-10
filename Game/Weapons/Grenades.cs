using Raylib_cs;
using System.Numerics;
using Wriggle.Characters;
using Wriggle.Physics;


namespace Wriggle.Weapons;

public class Grenade : Weapon
{
    private GrenadeBody? grenadeInstance;

    public override void Fire(PhysicsWorld world, Vector2 position, Vector2 direction, float timeCooked, float force)
    {
        if (grenadeInstance is not null)
            return;

        grenadeInstance = new GrenadeBody(position, 10f, 1f);
        grenadeInstance.fuseTime = MathF.Max(0f, grenadeInstance.fuseTime - MathF.Max(0f, timeCooked));
        Vector2 launchDirection = direction.LengthSquared() > 0.0001f
            ? Vector2.Normalize(direction)
            : Vector2.UnitX;
        grenadeInstance.Velocity = launchDirection * grenadeInstance.maxForce * MathF.Max(0f, force);
        world.AddBody(grenadeInstance);
    }

    public override void Update(PhysicsWorld world, IReadOnlyList<Character> characters, float dt)
    {
        if (grenadeInstance is not { hasExploded: false } grenade)
            return;

        grenade.fuseTime -= dt;
        if (grenade.fuseTime > 0f)
            return;

        grenade.Explode(world, characters);
        world.RemoveBody(grenade);
        grenadeInstance = null;
    }
}

public class GrenadeBody : Ball
{
    public float explosionRadius = 50f;
    public float explosionForce = 500f;
    public float explosionDamage = 50f;
    public bool hasExploded = false;
    public float fuseTime = 3f;
    public float maxForce = 1000f;

    public GrenadeBody(Vector2 startPosition, float radius, float mass)
        : base(startPosition, radius, mass)
    {
        color = Color.Green;
    }

    public void Explode(PhysicsWorld world, IReadOnlyList<Character> characters)
    {
        if (hasExploded)
            return;

        hasExploded = true;
        ExplosionAreaEffect explosionEffect = new(Position, explosionRadius, explosionForce, explosionDamage);
        explosionEffect.ApplyEffect(world, characters);
    }
}
