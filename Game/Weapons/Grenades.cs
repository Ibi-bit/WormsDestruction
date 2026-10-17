using Raylib_cs;
using System.Numerics;
using Wriggle.Characters;
using Wriggle.Physics;


namespace Wriggle.Weapons;

public class Grenade : Weapon
{
    private GrenadeBody? grenadeInstance;
    public event Action<Vector2, float>? Exploded;

    public override void Fire(PhysicsWorld world, Vector2 position, Vector2 direction, float timeCooked, float force, Body? owner = null)
    {
        if (grenadeInstance is not null)
            return;

        grenadeInstance = new GrenadeBody(position, 10f, 1f);
        grenadeInstance.IgnoredBody = owner;
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
        Exploded?.Invoke(grenade.Position, grenade.explosionRadius);
        world.RemoveBody(grenade);
        grenadeInstance = null;
    }
}

public class GrenadeBody : Ball
{
    private bool hasLeftIgnoredBody;
    public Body? IgnoredBody { get; set; }
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

    public override void Integrate(float dt, Vector2 gravity)
    {
        base.Integrate(dt, gravity);
        if (IgnoredBody is not null && !Overlaps(IgnoredBody))
            hasLeftIgnoredBody = true;
    }

    public override bool ShouldCollideWith(Body other) => other != IgnoredBody || hasLeftIgnoredBody;

    private bool Overlaps(Body other)
    {
        return Vector2.DistanceSquared(Position, other.Position)
            < MathF.Pow(Radius + other.Radius, 2f);
    }

    public void Explode(PhysicsWorld world, IReadOnlyList<Character> characters)
    {
        if (hasExploded)
            return;
        hasExploded = true;
        ExplosionAreaEffect explosionEffect = new(Position, explosionRadius, explosionForce, explosionDamage);
        explosionEffect.ApplyEffect(world);
        foreach (Character character in characters)
            character.TakeDamage(explosionEffect.GetDamageAt(character.Body.Position));
    }
}
