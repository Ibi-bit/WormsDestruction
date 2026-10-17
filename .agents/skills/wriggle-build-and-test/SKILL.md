---
name: wriggle-build-and-test
description: Build the Wriggle game and run its xUnit tests with dotnet. Use for compile errors, running tests, checking which spec tests fail, or verifying that a change works.
---

## Build

From the repository root (`~/WormsDestruction`):

```bash
dotnet build
```

## Test

The test project is `tests/Wriggle.Tests`. Run it from `tests/`:

```bash
cd tests && dotnet test Wriggle.Tests/Wriggle.Tests.csproj
```

Run a single class with a name filter:

```bash
cd tests && dotnet test Wriggle.Tests/Wriggle.Tests.csproj --filter "FullyQualifiedName~WorldSpecTests"
```

The solution file is `tests/WormsDestruction.sln`.

## Baseline

On 2026-10-12 the suite had 43 tests: 35 passed and 8 failed, all in the spec tests (for example `WorldSpecTests.Ammo_HasPlayerWeaponAndEnabled`). Those failures predate any change. Do not weaken or delete a failing assertion to make the suite green.

## When a test fails

1. Read the assertion message and the test's `Obligation` trait.
2. Find that obligation in `specs/*.allium` (see `wriggle-spec-driven`).
3. Decide: the code breaks the spec (fix the code), or the spec changed and the test is stale (update the spec first, then the test). Say which one you chose.
