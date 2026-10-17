---
name: wriggle-write-tests
description: Write xUnit tests for Wriggle in tests/Wriggle.Tests in the project's spec-obligation style. Use when adding a test, covering new non-trivial logic, or fixing a failing test.
---

## Layout

- Project: `tests/Wriggle.Tests/Wriggle.Tests.csproj` (net8.0, xUnit 2.4.2). It references `My2DGame.csproj`.
- Files: `tests/Wriggle.Tests/Specs/<Area>SpecTests.cs` (`CoreSpecTests`, `PhysicsSpecTests`, `WorldSpecTests`), namespace `Wriggle.Tests.Specs`.
- Shared usings: `tests/Wriggle.Tests/GlobalUsings.cs`.

## Style

- Each test is `[Fact]` plus `[Trait("Obligation", "<id>")]`, where `<id>` is an obligation from `allium plan` (see `wriggle-spec-driven`).
- Name the test `Subject_Behaviour`, for example `Colour_HasRgbaComponents`.
- Header comment on each file names the spec it covers and what cannot be tested.
- Existing helpers in `WorldSpecTests`:
  - `NewWorld()` returns `new GameWorld(new PhysicsWorld(new Terrain(10, 10)))`.
  - `HasMember(type, name)` checks for a member with reflection.
  - `TestWeapon` is a minimal `Weapon` subclass for tests.

## What to test

- Test non-trivial logic only: branches, loops, ammo and fuse maths, collisions. Trivial getters and wiring need no test (see `.github/copilot-instructions.md`).
- Anything that needs a window or a Raylib texture cannot run headless. Skip it and note why in the file header.

## Run

Use `wriggle-build-and-test`.
