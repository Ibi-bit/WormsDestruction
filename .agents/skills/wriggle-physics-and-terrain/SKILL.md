---
name: wriggle-physics-and-terrain
description: Change Wriggle's physics bodies, collisions, gravity, radial impulses and destructible terrain carving, including loading the terrain map. Use when working on movement, bouncing, explosions that carve holes, or terrain collision.
---

## Physics (`Game/Physics/`)

- `PhysicsWorld`: owns the terrain and bodies.
  - `AddBody` / `RemoveBody`
  - `Step(dt)`: integrates bodies and resolves collisions
  - `ApplyRadialImpulse(center, radius, force)`: explosion push
  - `CarveTerrain(center, radius)`: destroys terrain in a circle
  - `DebugMenu()`: ImGui tuning controls
- `PhysicsBody` and `Body`: `Integrate(dt, gravity)` applies velocity and acceleration.
- `Ball`: circle body with `position`, `velocity`, `radius`, `mass` and `color`.
- `AreaEffect`: explosion effects applied to bodies and characters.

## Terrain (`Game/World/Terrain.cs`)

- A grid of cells with width `W` and height `H`.
- `Terrain.FromImage(path)` builds solid cells from a PNG's alpha channel. The sample map is `Game/Content/Maps/Basic.png`.
- `IsSolid(x, y)` checks a cell. `Carve(cx, cy, r)` removes a circle. `Paint` recolours cells.
- Collision normals: `CircleCollisionNormal` and `CapsuleCollisionNormal`.
- `Restitution` controls bounce.

## Testing limits

`FromImage`, `Carve`, `Paint` and drawing need a Raylib texture and cannot run in unit tests. `WorldSpecTests.cs` says so in its header. For logic tests, build `new Terrain(w, h)` in memory and test everything else.

## Specs

Physics rules are in `specs/physics.allium`. Terrain and world rules are in `specs/world.allium`. Change the spec first (`wriggle-spec-driven`).
