
using System.Numerics;
using Raylib_cs;
using Wriggle.World;

namespace Wriggle.Physics;

public abstract class Body
{
    public Vector2 Position;
    public Vector2 Velocity;
    public bool isGrounded = false;
    public bool IsKnockedBack;
    public float Mass;
    public float InverseMass => Mass > 0f ? 1f / Mass : 0f;

    public virtual float Radius { get; protected set; }
    public virtual float MaxWalkableSlopeRatio => float.PositiveInfinity;
    private float restitution;
    public float Restitution
    { 
        get => restitution;
        set => restitution = Math.Clamp(value, 0f, 1f);
    }
    public virtual Rectangle BroadPhaseBounds => new(
        Position.X - Radius,
        Position.Y - Radius,
        Radius * 2f,
        Radius * 2f);

    public Body(Vector2 position, float mass, float radius, float restitution = 0f)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Mass = mass;
        Radius = radius;
        Restitution = restitution;
    }

    public virtual void Integrate(float dt, Vector2 gravity)
    {
        Velocity += gravity * dt;
        Position += Velocity * dt;
    }

    public virtual void CompleteStep() { }

    public virtual bool ShouldCollideWith(Body other) => true;

    public virtual void GetCollisionSegment(out Vector2 start, out Vector2 end)
    {
        start = Position;
        end = Position;
    }

    public virtual void OnCollision(Vector2 normal, float restitution)
    {
        float velocityAlongNormal = Vector2.Dot(Velocity, normal);
        if (velocityAlongNormal < 0f)
        {
            Velocity -= (1f + Math.Clamp(restitution, 0f, 1f)) * velocityAlongNormal * normal;
        }
    }

    public bool IsWalkableSurface(Vector2 normal)
    {
        return normal.Y < 0f
            && MathF.Abs(normal.X) <= MaxWalkableSlopeRatio * -normal.Y;
    }

    public void ResolveDynamicCollision(Body other, float restitution)
    {
        if (!TryGetCollision(other, out Vector2 normal, out float penetration))
            return;

        float inverseMass = Mass > 0f ? 1f / Mass : 0f;
        float otherInverseMass = other.Mass > 0f ? 1f / other.Mass : 0f;
        float inverseMassSum = inverseMass + otherInverseMass;
        if (inverseMassSum <= 0f)
            return;

        float relativeNormalSpeed = Vector2.Dot(other.Velocity - Velocity, normal);
        if (relativeNormalSpeed < 0f)
        {
            float impulseMagnitude = -(1f + restitution) * relativeNormalSpeed / inverseMassSum;
            Velocity -= impulseMagnitude * inverseMass * normal;
            other.Velocity += impulseMagnitude * otherInverseMass * normal;
        }

        float correctionMagnitude = MathF.Max(penetration - 0.01f, 0f) * 0.85f / inverseMassSum;
        Vector2 correction = correctionMagnitude * normal;
        Position -= correction * inverseMass;
        other.Position += correction * otherInverseMass;
    }

    private bool TryGetCollision(Body other, out Vector2 normal, out float penetration)
    {
        GetCollisionSegment(out Vector2 a0, out Vector2 a1);
        other.GetCollisionSegment(out Vector2 b0, out Vector2 b1);
        ClosestPointsOnSegments(a0, a1, b0, b1, out Vector2 pointA, out Vector2 pointB);

        Vector2 delta = pointB - pointA;
        float distanceSquared = delta.LengthSquared();
        float minimumDistance = Radius + other.Radius;
        if (distanceSquared >= minimumDistance * minimumDistance)
        {
            normal = Vector2.Zero;
            penetration = 0f;
            return false;
        }

        float distance = MathF.Sqrt(distanceSquared);
        if (distance > 0.0001f)
        {
            normal = delta / distance;
        }
        else
        {
            Vector2 centerDelta = other.Position - Position;
            normal = centerDelta.LengthSquared() > 0.0001f
                ? Vector2.Normalize(centerDelta)
                : Vector2.UnitX;
        }

        penetration = minimumDistance - distance;
        return true;
    }

    private static void ClosestPointsOnSegments(
        Vector2 a0,
        Vector2 a1,
        Vector2 b0,
        Vector2 b1,
        out Vector2 pointA,
        out Vector2 pointB)
    {
        const float epsilon = 0.000001f;
        Vector2 directionA = a1 - a0;
        Vector2 directionB = b1 - b0;
        Vector2 offset = a0 - b0;
        float lengthA = Vector2.Dot(directionA, directionA);
        float lengthB = Vector2.Dot(directionB, directionB);
        float projectionA = Vector2.Dot(directionA, offset);
        float projectionB = Vector2.Dot(directionB, offset);
        float alongA;
        float alongB;

        if (lengthA <= epsilon && lengthB <= epsilon)
        {
            pointA = a0;
            pointB = b0;
            return;
        }

        if (lengthA <= epsilon)
        {
            alongA = 0f;
            alongB = Math.Clamp(projectionB / lengthB, 0f, 1f);
        }
        else if (lengthB <= epsilon)
        {
            alongB = 0f;
            alongA = Math.Clamp(-projectionA / lengthA, 0f, 1f);
        }
        else
        {
            float crossProjection = Vector2.Dot(directionA, directionB);
            float denominator = lengthA * lengthB - crossProjection * crossProjection;
            alongA = denominator > epsilon
                ? Math.Clamp((crossProjection * projectionB - projectionA * lengthB) / denominator, 0f, 1f)
                : 0f;

            alongB = (crossProjection * alongA + projectionB) / lengthB;
            if (alongB < 0f)
            {
                alongB = 0f;
                alongA = Math.Clamp(-projectionA / lengthA, 0f, 1f);
            }
            else if (alongB > 1f)
            {
                alongB = 1f;
                alongA = Math.Clamp((crossProjection - projectionA) / lengthA, 0f, 1f);
            }
        }

        pointA = a0 + directionA * alongA;
        pointB = b0 + directionB * alongB;
    }

    public abstract Vector2 TerrainCollisionNormal(Terrain terrain);
    public abstract void Draw();
}

public class CircleBody : Body
{
    public CircleBody(Vector2 position, float mass, float radius, float restitution = 0.5f)
        : base(position, mass, radius, restitution)
    {
    }

    public override Vector2 TerrainCollisionNormal(Terrain terrain)
    {

        return terrain.CircleCollisionNormal(Position, Radius);
        
    }

    public override void Draw()
    {
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, Radius, Color.Red);
    }
    
}
public class CapsuleBody : Body
{
    public float Height { get; set; } = 20f;
    public float Width { get; set; } = 10f;
    public override float MaxWalkableSlopeRatio => 4f;
    public override float Radius
    {
        get => CapsuleRadius;
        protected set => Width = value * 2f;
    }
    public float CapsuleRadius =>  Width / 2f;
    public float SegmentHalfLength =>  Math.Max(0f, (Height - Width) / 2f);
    public override Rectangle BroadPhaseBounds => new(
        Position.X - Width / 2f,
        Position.Y - Height / 2f,
        Width,
        Height);

    public override void GetCollisionSegment(out Vector2 start, out Vector2 end)
    {
        start = Position - new Vector2(0f, SegmentHalfLength);
        end = Position + new Vector2(0f, SegmentHalfLength);
    }

    public CapsuleBody(Vector2 position, float mass, float height, float width) : base(position, mass, 0f, 0.5f)
    {
        Height = height;
        Width = width;
    }

    public override void OnCollision(Vector2 normal, float restitution)
    {
        float velocityAlongNormal = Vector2.Dot(Velocity, normal);
        if (velocityAlongNormal < 0f)
        {
            Velocity -= (1f + Math.Clamp(restitution, 0f, 1f)) * velocityAlongNormal * normal;
        }
    }

    public override Vector2 TerrainCollisionNormal(Terrain terrain)
    {
        return terrain.CapsuleCollisionNormal(Position, Height, Width);
       
    }

    public override void Draw()
    {
        Vector2 start = Position - new Vector2(0f, SegmentHalfLength);
        Vector2 end = Position + new Vector2(0f, SegmentHalfLength);
        Raylib.DrawLineEx(start, end, Width, Color.Red);
        Raylib.DrawCircle((int)start.X, (int)start.Y, CapsuleRadius, Color.Red);
        Raylib.DrawCircle((int)end.X, (int)end.Y, CapsuleRadius, Color.Red);
    }
}
