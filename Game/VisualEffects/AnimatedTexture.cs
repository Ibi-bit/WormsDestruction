using System.Numerics;
using Raylib_cs;

namespace Wriggle.VisualEffects;

public sealed class AnimatedTexture : IDisposable
{
    private float elapsedTime;
    private int currentFrame;

    public Texture2D Texture { get; }
    public int FrameWidth { get; }
    public int FrameHeight => Texture.Height;
    public int Fps { get; }
    public int FrameCount { get; }
    public bool AutoPlay { get; }
    public int CurrentFrame => currentFrame;
    public float Duration => FrameCount / (float)Fps;
    public bool IsFinished => elapsedTime >= Duration;

    public AnimatedTexture(string path, int frameWidth, int fps, bool autoPlay = false)
    {
        if (frameWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameWidth));
        if (fps <= 0)
            throw new ArgumentOutOfRangeException(nameof(fps));

        Texture = Raylib.LoadTexture(path);
        FrameWidth = frameWidth;
        Fps = fps;
        AutoPlay = autoPlay;
        FrameCount = Texture.Width / frameWidth;

        if (FrameCount == 0)
        {
            Raylib.UnloadTexture(Texture);
            throw new ArgumentException("The frame width is larger than the texture width.", nameof(frameWidth));
        }
    }

    public void Update(float dt)
    {
        if (!AutoPlay || dt <= 0f || IsFinished)
            return;

        elapsedTime = MathF.Min(elapsedTime + dt, Duration);
        currentFrame = Math.Min((int)(elapsedTime * Fps), FrameCount - 1);
    }

    public void Step()
    {
        if (currentFrame >= FrameCount - 1)
            return;

        currentFrame++;
        elapsedTime = currentFrame == FrameCount - 1
            ? Duration
            : currentFrame / (float)Fps;
    }
    public void Reset()
    {
        elapsedTime = 0f;
        currentFrame = 0;
    }

    public void Draw(Vector2 position, Color tint, float scale = 1f, bool centered = false)
    {
        Rectangle source = new(
            CurrentFrame * FrameWidth,
            0f,
            FrameWidth,
            FrameHeight);
        Rectangle destination = new(
            position.X,
            position.Y,
            FrameWidth * scale,
            FrameHeight * scale);
        Vector2 origin = centered
            ? new Vector2(destination.Width, destination.Height) / 2f
            : Vector2.Zero;
        Raylib.DrawTexturePro(Texture, source, destination, origin, 0f, tint);
    }

    public void Dispose()
    {
        Raylib.UnloadTexture(Texture);
    }
}
