using System.Numerics;
using System.Reflection;
using Wriggle.Physics;
using Wriggle.World;
using Xunit;

namespace Wriggle.Tests.Specs;

// Covers specs/physics.allium. Solid terrain is written through the private solid mask because painting uploads a Raylib texture.
public class PhysicsSpecTests
{
    private static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    private static bool HasMember(Type type, string name) =>
        type.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length > 0;

    private static Terrain TerrainWithFloor(int size, int floorY)
    {
        var terrain = new Terrain(size, size);
        var solid = (bool[])typeof(Terrain).GetField("solid", Private)!.GetValue(terrain)!;
        for (int y = floorY; y < size; y++)
            for (int x = 0; x < size; x++)
                solid[y * size + x] = true;
        return terrain;
    }

    [Fact]
    [Trait("Obligation", "value-equality.Vector")]
    public void Vector_HasStructuralEquality()
    {
        Assert.Equal(new Vector2(1, 2), new Vector2(1, 2));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.Vector")]
    public void Vector_HasXYAndLengthSquared()
    {
        Assert.True(HasMember(typeof(Vector2), "X"));
        Assert.True(HasMember(typeof(Vector2), "Y"));
        Assert.True(HasMember(typeof(Vector2), "LengthSquared"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.Body")]
    public void Body_HasSpecFields()
    {
        Assert.True(HasMember(typeof(Body), "Position"));
        Assert.True(HasMember(typeof(Body), "Velocity"));
        Assert.True(HasMember(typeof(Body), "Mass"));
        Assert.True(HasMember(typeof(Body), "Restitution"));
        Assert.True(HasMember(typeof(Body), "isGrounded"));
        Assert.True(HasMember(typeof(Body), "MaxWalkableSlopeRatio"));
        Assert.True(HasMember(typeof(Body), "InverseMass"));
    }

    [Fact]
    [Trait("Obligation", "entity-fields.PhysicsWorld")]
    public void PhysicsWorld_HasBodies()
    {
        Assert.True(HasMember(typeof(PhysicsWorld), "bodies"));
    }

    [Fact]
    [Trait("Obligation", "config-default.max_walkable_slope_default")]
    public void CapsuleBody_DefaultWalkableSlopeIsFour()
    {
        var body = new CapsuleBody(Vector2.Zero, 1f, 20f, 10f);
        Assert.Equal(4f, body.MaxWalkableSlopeRatio);
    }

    [Fact]
    [Trait("Obligation", "rule-success.PhysicsStepped")]
    public void PhysicsStepped_AppliesGravityAndMovesBodies()
    {
        var physics = new PhysicsWorld(new Terrain(20, 20));
        var body = new CircleBody(new Vector2(5, 5), 1f, 1f);
        physics.AddBody(body);

        physics.Step(0.1f);

        Assert.Equal(60f, body.Velocity.Y, 0.5f);
        Assert.True(body.Position.Y > 5f);
        Assert.False(body.isGrounded);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.PhysicsStepped.1")]
    public void PhysicsStepped_NoOpWhenDeltaNotPositive()
    {
        var physics = new PhysicsWorld(new Terrain(20, 20));
        var body = new CircleBody(new Vector2(5, 5), 1f, 1f);
        physics.AddBody(body);

        physics.Step(0f);

        Assert.Equal(Vector2.Zero, body.Velocity);
        Assert.Equal(new Vector2(5, 5), body.Position);
    }

    [Fact]
    [Trait("Obligation", "rule-success.TerrainContactResolved")]
    public void TerrainContact_BodyComesToRestOnFloorAndIsGrounded()
    {
        var physics = new PhysicsWorld(TerrainWithFloor(20, 10));
        var body = new CircleBody(new Vector2(5, 5), 1f, 1f);
        physics.AddBody(body);

        for (int i = 0; i < 180; i++)
            physics.Step(1f / 60f);

        Assert.True(body.isGrounded);
        Assert.InRange(body.Position.Y, 8f, 9.5f);
    }

    [Fact]
    [Trait("Obligation", "rule-success.BodiesCollided")]
    public void BodiesCollided_TransfersMomentumBetweenTouchingBodies()
    {
        var a = new CircleBody(new Vector2(0, 0), 1f, 1f);
        var b = new CircleBody(new Vector2(1.5f, 0), 1f, 1f);
        a.Velocity = new Vector2(10, 0);

        a.ResolveDynamicCollision(b, 0.5f);

        Assert.True(a.Velocity.X < 10f);
        Assert.True(b.Velocity.X > 0f);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.BodiesCollided.1")]
    public void BodiesCollided_NoOpWhenBothBodiesImmovable()
    {
        var a = new CircleBody(new Vector2(0, 0), 0f, 1f);
        var b = new CircleBody(new Vector2(1.5f, 0), 0f, 1f);
        a.Velocity = new Vector2(10, 0);

        a.ResolveDynamicCollision(b, 0.5f);

        Assert.Equal(10f, a.Velocity.X);
        Assert.Equal(0f, b.Velocity.X);
    }

    [Fact]
    [Trait("Obligation", "rule-success.ExplosionImpulseApplied")]
    public void ExplosionImpulse_FallsOffLinearlyWithDistance()
    {
        var physics = new PhysicsWorld(new Terrain(20, 20));
        var body = new CircleBody(new Vector2(3, 0), 1f, 1f);
        physics.AddBody(body);

        physics.ApplyRadialImpulse(Vector2.Zero, 10f, 100f);

        Assert.Equal(70f, body.Velocity.X, 0.01f);
    }

    [Fact]
    [Trait("Obligation", "rule-failure.ExplosionImpulseApplied.1")]
    public void ExplosionImpulse_NoOpWhenRadiusNotPositive()
    {
        var physics = new PhysicsWorld(new Terrain(20, 20));
        var body = new CircleBody(new Vector2(3, 0), 1f, 1f);
        physics.AddBody(body);

        physics.ApplyRadialImpulse(Vector2.Zero, 0f, 100f);

        Assert.Equal(Vector2.Zero, body.Velocity);
    }
}
