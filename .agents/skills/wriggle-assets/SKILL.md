---
name: wriggle-assets
description: Add or load runtime assets for Wriggle, such as textures, maps, sprites and other files the game reads at runtime. Use when adding a PNG, map or any file the game loads.
---

## Steps

1. Put the file under `Game/Content/` in a sensible folder. The sample map is `Game/Content/Maps/Basic.png`.
2. Read `Game/Content/README.md` for the folder conventions.
3. The project copies everything in `Game/Content/**` to `Content/` beside the built executable (`My2DGame.csproj`, `PreserveNewest`). You do not need to edit the csproj for new files.
4. Load from the executable's folder, never the working directory:

   ```csharp
   string path = Path.Combine(AppContext.BaseDirectory, "Content", "Maps", "Basic.png");
   ```

   `Terrain.FromImage` takes that path.

## Notes

- Keep paths case-correct. The folder is `Maps`, not `maps`, and macOS may hide the mismatch.
- Texture loading needs a Raylib window, so it cannot be unit tested. See `wriggle-physics-and-terrain`.
