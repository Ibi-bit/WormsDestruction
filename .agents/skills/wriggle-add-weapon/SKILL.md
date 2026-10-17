---
name: wriggle-add-weapon
description: Add a new weapon, projectile, ammo type or explosion to Wriggle, following the grenade pattern in Weapons/Grenades.cs with ammo in the player inventory. Use when creating or changing weapons, ammo, throws, fuses or explosions.
---

## Pattern (copy `Grenades.cs`)

- A weapon subclasses `Weapon` (`Game/Weapons/BaseWeapon.cs`) and overrides:
  - `Fire(PhysicsWorld world, Vector2 position, Vector2 direction, float timeCooked, float force)`: spawn the projectile.
  - `Update(PhysicsWorld world, IReadOnlyList<Character> characters, float dt)`: fuses, detonation and cleanup.
- `Grenade` keeps one in-flight instance and returns early from `Fire` if one already exists. Keep that rule for limited weapons.
- A projectile body subclasses `Ball` (`Game/Physics/Ball.cs`). Add it with `world.AddBody(body)` and remove it with `world.RemoveBody(body)` on detonation.
- Explosions use `ExplosionAreaEffect(position, radius, force, damage)` (`Game/Physics/AreaEffect.cs`). Grenade exposes `explosionRadius`, `explosionForce` and `explosionDamage` fields for tuning.

## Ammo and inventory

- Weapons are added to the world with `GameWorld.AddWeapon(weapon, ammoByRound)` (or without the dictionary).
- Each player holds `Dictionary<Weapon, Ammo>` in `Player.WeaponInventory`.
- `Ammo.AddRoundAmmo(round, count)` grants ammo, `Ammo.TrySpendAmmo(amount)` spends it and returns false if there is not enough, and `IsEnabled` controls availability.
- `GameWorld.GiveRoundAmmo(player, round)` grants the per-round allowance.

## Process

1. Update `specs/weapons.allium` (or `world.allium` for ammo grants) first. See `wriggle-spec-driven`.
2. Implement by copying the grenade structure. Do not add a base class or factory unless two weapons need it.
3. Add tests for non-trivial logic (fuse timing, ammo spend, detonation). See `wriggle-write-tests`.
4. Run `wriggle-build-and-test`.
