---
name: wriggle-dependencies
description: Manage Wriggle's NuGet packages and version pins, including Raylib-cs 7.0.1 and rlImgui-cs 3.2.0, and the xUnit test packages. Use when adding a package, upgrading raylib or ImGui, or diagnosing a native crash after a dependency change.
---

## Pinned versions

- `Raylib-cs` 7.0.1 and `rlImgui-cs` 3.2.0 in `My2DGame.csproj`. Upgrade them together and only to a pair the ImGui backend supports. The csproj comment explains the raylib native ABI mismatch that floating versions cause.
- Test packages in `tests/Wriggle.Tests/Wriggle.Tests.csproj`: `Microsoft.NET.Test.Sdk` 17.6.0, `xunit` 2.4.2, `xunit.runner.visualstudio` 2.4.5, `coverlet.collector` 6.0.0.

## Rules

- Do not add a package for a few lines of code (`.github/copilot-instructions.md`). Check the standard library and existing code first.
- After any dependency change, run `wriggle-build-and-test`. If the game crashes at startup, suspect the raylib and rlImgui pair first.
- The `skills-lock.json` file in the repository root tracks installed agent skills. Do not hand-edit it.
