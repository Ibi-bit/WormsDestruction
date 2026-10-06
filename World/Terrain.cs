using System;
using System.Numerics;
using Raylib_cs;


namespace My2DGame.World;



public unsafe sealed class Terrain
{
    public readonly int W, H;
    readonly bool[] solid;
    readonly Color[] pixels;
    Color[] scratch;
    Texture2D tex;

    private int Index(int x, int y) => x + y * W;

    public Terrain(int w, int h)
    {
        W = w;
        H = h;
        solid = new bool[w * h];
        pixels = new Color[w * h];
        scratch = new Color[w * h];
    }
    public static Terrain FromImage(string path)
    {
        Image img = Raylib.LoadImage(path);
        Terrain t = new Terrain(img.Width, img.Height);
        for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                Color c = Raylib.GetImageColor(img, x, y);
                if (c.A > 128)
                {
                    t.solid[y * t.W + x] = true;
                    t.pixels[y * t.W + x] = c;
                }
            }
        t.tex = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img);
        return t;
    }
    public bool IsSolid(int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return false;
        return solid[y * W + x];
    }
    public void Carve(int cx, int cy, int r)
    {
        int r2 = r * r;
        int x0 = Math.Max(0, cx - r), x1 = Math.Min(W - 1, cx + r);
        int y0 = Math.Max(0, cy - r), y1 = Math.Min(H - 1, cy + r);
        if (x0 > x1 || y0 > y1) return;

        for (int y = y0; y <= y1; y++)
        {
            int dy = y - cy;
            for (int x = x0; x <= x1; x++)
            {
                int dx = x - cx;
                int d2 = dx * dx + dy * dy;
                int i = y * W + x;
                if (d2 <= r2)
                {
                    solid[i] = false;
                    pixels[i] = new Color(0, 0, 0, 0);
                }
                else if (solid[i] && d2 <= (r + 4) * (r + 4))
                {
                    Color c = pixels[i];
                    pixels[i] = new Color((byte)(c.R * 0.55f), (byte)(c.G * 0.55f), (byte)(c.B * 0.55f), c.A);
                }
            }
        }

        UploadRect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
    }

    void UploadRect(int rx, int ry, int rw, int rh)
    {
        for (int row = 0; row < rh; row++)
            Array.Copy(pixels, (ry + row) * W + rx, scratch, row * rw, rw);

        fixed (Color* p = scratch)
        {
            Raylib.UpdateTextureRec(tex, new Rectangle(rx, ry, rw, rh), p);
        }
    }
    public Vector2 Normal(int px, int py, int radius)
    {
        Vector2 n = new Vector2(0, 0);
        int r2 = radius * radius;
        int x0 = Math.Max(0, px - radius), x1 = Math.Min(W - 1, px + radius);
        int y0 = Math.Max(0, py - radius), y1 = Math.Min(H - 1, py + radius);
        if (x0 > x1 || y0 > y1) return n;

        for (int y = y0; y <= y1; y++)
        {
            int dy = y - py;
            for (int x = x0; x <= x1; x++)
            {
                int dx = x - px;
                int d2 = dx * dx + dy * dy;
                if (d2 <= r2 && solid[y * W + x])
                {
                    float d = MathF.Sqrt(d2);
                    float f = 1f - d / radius;
                    n.X += dx / d * f;
                    n.Y += dy / d * f;
                }
            }
        }

        if (n.X != 0 || n.Y != 0)
            n = Vector2.Normalize(n);

        return n;
    }
    public void Draw()
    {
        Raylib.DrawTexture(tex, 0, 0, Color.White);
    }
    public void Unload()
    {
        Raylib.UnloadTexture(tex);
    }


}
