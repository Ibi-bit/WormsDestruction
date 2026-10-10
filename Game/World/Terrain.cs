using System;
using System.Numerics;
using Raylib_cs;


namespace Wriggle.World;



public unsafe sealed class Terrain
{
    public readonly int W, H;
    private float restitution = 0.25f;
    public float Restitution
    {
        get => restitution;
        set => restitution = Math.Clamp(value, 0f, 1f);
    }
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
    public void Paint(int x, int y,int r, Color color)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return;
        int r2 = r * r;
        int x0 = Math.Max(0, x - r), x1 = Math.Min(W - 1, x + r);
        int y0 = Math.Max(0, y - r), y1 = Math.Min(H - 1, y + r);
        if (x0 > x1 || y0 > y1) return;

        for (int py = y0; py <= y1; py++)
        {
            int dy = py - y;
            for (int px = x0; px <= x1; px++)
            {
                int dx = px - x;
                int d2 = dx * dx + dy * dy;
                int i = py * W + px;
                if (d2 <= r2)
                {
                    solid[i] = true;
                    pixels[i] = color;
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
    public Vector2 CircleCollisionNormal(Vector2 center, float radius)
    {
        if (radius <= 0f) return Vector2.Zero;

        int x0 = Math.Max(0, (int)MathF.Floor(center.X - radius) - 1);
        int x1 = Math.Min(W - 1, (int)MathF.Floor(center.X + radius));
        int y0 = Math.Max(0, (int)MathF.Floor(center.Y - radius) - 1);
        int y1 = Math.Min(H - 1, (int)MathF.Floor(center.Y + radius));
        if (x0 > x1 || y0 > y1) return Vector2.Zero;

        bool overlaps = false;
        float radiusSquared = radius * radius;
        Vector2 normal = Vector2.Zero;
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                if (!solid[y * W + x]) continue;

                float closestX = Math.Clamp(center.X, x, x + 1f);
                float closestY = Math.Clamp(center.Y, y, y + 1f);
                Vector2 awayFromPixel = center - new Vector2(closestX, closestY);
                float distanceSquared = awayFromPixel.LengthSquared();
                if (distanceSquared >= radiusSquared) continue;

                overlaps = true;
                if (distanceSquared <= 0.000001f) continue;

                float distance = MathF.Sqrt(distanceSquared);
                normal += awayFromPixel / distance * (radius - distance);
            }
        }

        if (!overlaps) return Vector2.Zero;
        if (normal.LengthSquared() > 0.000001f)
            return Vector2.Normalize(normal);

        return FindEscapeNormal(center, radius + 2f);
    }
    public Vector2 CapsuleCollisionNormal(Vector2 center, float height, float width)
    {
        if (height <= 0f || width <= 0f) return Vector2.Zero;

        int x0 = Math.Max(0, (int)MathF.Floor(center.X - width / 2f) - 1);
        int x1 = Math.Min(W - 1, (int)MathF.Floor(center.X + width / 2f));
        int y0 = Math.Max(0, (int)MathF.Floor(center.Y - height / 2f) - 1);
        int y1 = Math.Min(H - 1, (int)MathF.Floor(center.Y + height / 2f));
        if (x0 > x1 || y0 > y1) return Vector2.Zero;

        bool overlaps = false;
        Vector2 normal = Vector2.Zero;
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                if (!solid[y * W + x]) continue;

                float closestX = Math.Clamp(center.X, x, x + 1f);
                float closestY = Math.Clamp(center.Y, y, y + 1f);
                Vector2 awayFromPixel = center - new Vector2(closestX, closestY);
                if (Math.Abs(awayFromPixel.X) > width / 2f || Math.Abs(awayFromPixel.Y) > height / 2f) continue;

                overlaps = true;
                if (awayFromPixel.LengthSquared() <= 0.000001f) continue;

                normal += Vector2.Normalize(awayFromPixel);
            }
        }

        if (!overlaps) return Vector2.Zero;
        if (normal.LengthSquared() > 0.000001f)
            return Vector2.Normalize(normal);

        return FindEscapeNormal(center, Math.Max(width, height) / 2f + 2f);
    }

    private Vector2 FindEscapeNormal(Vector2 center, float searchDistance)
    {
        const int directions = 16;
        Vector2 normal = Vector2.Zero;
        for (int i = 0; i < directions; i++)
        {
            float angle = i * MathF.Tau / directions;
            Vector2 direction = new(MathF.Cos(angle), MathF.Sin(angle));
            for (float distance = 1f; distance <= searchDistance; distance += 1f)
            {
                int x = (int)MathF.Floor(center.X + direction.X * distance);
                int y = (int)MathF.Floor(center.Y + direction.Y * distance);
                if (!IsSolid(x, y))
                {
                    normal += direction;
                    break;
                }
            }
        }

        return normal.LengthSquared() > 0.000001f
            ? Vector2.Normalize(normal)
            : new Vector2(0f, -1f);
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
