using Raylib_cs;
using System.Numerics;
using Wriggle.Physics;


namespace Wriggle.Weapons;

public class Grenade : Weapon
{

    public GrenadeBody grenadeBodyprefab;
    

    public Grenade(GrenadeBody grenadeBody)
    {
        grenadeBodyprefab = grenadeBody;
    }

    public override void Fire(Vector2 position, Vector2 direction, float timeCooked, float force)
    {
        // Create a new grenade body and add it to the physics world
        GrenadeBody grenade = new GrenadeBody(position, 1f, 10f);
        grenade.fuseTime = MathF.Max(0f, grenade.fuseTime - timeCooked);
        grenade.Velocity = Vector2.Normalize(direction) * grenade.maxForce * force;
    }
    public override void Update(float dt)
    {
        // Update the grenade's fuse timer and check for explosion
        if (grenadeBodyprefab != null && !grenadeBodyprefab.hasExploded)
        {
            grenadeBodyprefab.fuseTime -= dt;
            if (grenadeBodyprefab.fuseTime <= 0f)
            {
                grenadeBodyprefab.Explode();
            }
        }
    }
}
public class GrenadeBody : Ball 
{
    private float explosionRadius = 50f;
    private float explosionForce = 500f;
    private bool hasExploded = false;
    public float fuseTime = 3f;
    public float maxForce = 1000f;
    private Timer fuseTimer;

    public GrenadeBody(Vector2 startPosition, float radius, float mass)
        : base(startPosition, radius, mass)
    {
        color = Color.Green;
    }

    private bool IsOnGround()
    {
        // Simple check for ground collision (you can improve this based on your terrain)
        return Position.Y >= 400f; // Assuming ground is at Y=400
    }

    private void Explode()
    {
        hasExploded = true;
        // Apply explosion force to nearby bodies
        foreach (var body in PhysicsWorld.Instance.GetBodies())
        {
            if (body != this && Vector2.Distance(body.Position, Position) <= explosionRadius)
            {
                Vector2 direction = Vector2.Normalize(body.Position - Position);
                body.ApplyForce(direction * explosionForce);
            }
        }
        // Optionally, you can remove the grenade from the physics world after exploding
        PhysicsWorld.Instance.RemoveBody(this);
    }
}