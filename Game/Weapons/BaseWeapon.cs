using Raylib_cs;
using System.Numerics;
using Wriggle.Characters;
using Wriggle.Physics;


namespace Wriggle.Weapons;


public abstract class Weapon
{
    public abstract void Fire(PhysicsWorld world, Vector2 position, Vector2 direction, float timeCooked, float force);
    public abstract void Update(PhysicsWorld world, IReadOnlyList<Character> characters, float dt);
}
