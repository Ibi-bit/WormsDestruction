# Content

All files in this folder are copied to the build output automatically
(`CopyToOutputDirectory="PreserveNewest"` in the csproj), so they can be
referenced at runtime by relative path.

Suggested subfolders:

```
Content/
├── textures/   (.png, .bmp, .tga, ...)
├── fonts/      (.ttf)
├── audio/      (.wav, .ogg)
└── data/       (levels, dialogs, ... JSON or other)
```

Load paths are relative to the executable directory:
`Path.Combine(AppContext.BaseDirectory, "Content", "textures", "player.png")`.