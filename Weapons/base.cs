using Raylib_cs;
using System.Numerics;
using Wriggle.Physics;


namespace Wriggle.Weapons;


public abstract class Weapon
{
    public abstract void Fire(Vector2 position, Vector2 direction,float timeCooked, float force);
    public abstract void Update(float dt);
}