---
name: wriggle-spec-driven
description: Use the Allium behaviour specs in Wriggle's specs/ folder as the source of truth for game rules. Use when adding or changing a game rule, writing or checking a spec, or mapping code to spec obligations.
---

## Specs

- `specs/core.allium`: scenes, lifecycle, frame order.
- `specs/physics.allium`: bodies, collisions, impulses.
- `specs/weapons.allium`: grenades, fuses, explosions, ammo use.
- `specs/world.allium`: world, rounds, turns, terrain, player ammo.

Each spec starts with `-- allium: 3` and a `Scope / Includes / Excludes` header. Excludes list what is out of scope. Respect them.

## Workflow

1. Change the rule in the spec first. Note the `Excludes` section so you do not spec infrastructure.
2. Validate: `allium check specs/<file>.allium`.
3. List test obligations: `allium plan specs/<file>.allium`. Run `allium --help` if you need other commands.
4. Implement the code.
5. Write tests tagged with the obligation: `[Trait("Obligation", "<id>")]`. Obligation IDs look like `rule-success.GameStarted`, `entity-fields.Colour`, `entity-optional.SceneManager.current` or `value-equality.Colour`.
6. Run `wriggle-build-and-test`.

## Helpers in `.agents/skills`

`allium`, `distill`, `elicit`, `propagate`, `tend`, `weed` and `witness` are installed for spec authoring and checking. Use them when writing or auditing a spec.

## Before editing

`specs/world.allium` has uncommitted changes and `specs/weapons.allium` is untracked. Run `git status` and do not overwrite someone else's edits.
