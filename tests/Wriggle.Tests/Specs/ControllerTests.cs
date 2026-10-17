using Raylib_cs;
using Wriggle.Core;
using Xunit;

namespace Wriggle.Tests.Specs;

// Covers the controller's input normalization and frame-state transitions without opening a Raylib window.
public class ControllerTests
{
    private sealed class TestInput : IInputProvider
    {
        public HashSet<KeyboardKey> Keys { get; } = [];
        public HashSet<KeyboardKey> PressedKeys { get; } = [];
        public HashSet<MouseButton> MouseButtons { get; } = [];
        public HashSet<MouseButton> PressedMouseButtons { get; } = [];
        public float Wheel { get; set; }

        public bool IsKeyDown(KeyboardKey key) => Keys.Contains(key);
        public bool IsKeyPressed(KeyboardKey key) => PressedKeys.Contains(key);
        public bool IsMouseButtonDown(MouseButton button) => MouseButtons.Contains(button);
        public bool IsMouseButtonPressed(MouseButton button) => PressedMouseButtons.Contains(button);
        public float GetMouseWheelMove() => Wheel;
    }

    [Fact]
    [Trait("Obligation", "controller.binary-state")]
    public void BinaryInput_TracksPressAndHeldTime()
    {
        var input = new TestInput();
        var controller = new Controller(input);
        controller.Bind("Fire", InputBinding.Keyboard(KeyboardKey.F));

        controller.Update(0.1f);
        Assert.False(controller.IsDown("Fire"));

        input.Keys.Add(KeyboardKey.F);
        controller.Update(0.1f);
        Assert.True(controller.WasPressed("Fire"));
        Assert.Equal(0f, controller.HeldTime("Fire"));

        controller.Update(0.25f);
        Assert.True(controller.IsDown("Fire"));
        Assert.Equal(0.25f, controller.HeldTime("Fire"));
        Assert.Equal(0.25f, controller.TimeSincePressed("Fire"));

        input.Keys.Remove(KeyboardKey.F);
        controller.Update(0.1f);
        Assert.True(controller.WasReleased("Fire"));
        Assert.Equal(0f, controller.HeldTime("Fire"));
        Assert.Equal(0.25f, controller.LastHeldTime("Fire"));
    }

    [Fact]
    [Trait("Obligation", "controller.analog-normalization")]
    public void KeyboardAxis_ProvidesAnalogValueAndBinaryState()
    {
        var input = new TestInput();
        var controller = new Controller(input);
        controller.Bind("Move", InputBinding.KeyboardAxis(KeyboardKey.A, KeyboardKey.D));

        input.Keys.Add(KeyboardKey.D);
        controller.Update(0.1f);
        Assert.Equal(1f, controller.Value("Move"));
        Assert.True(controller.IsDown("Move"));

        input.Keys.Add(KeyboardKey.A);
        controller.Update(0.1f);
        Assert.Equal(0f, controller.Value("Move"));
        Assert.False(controller.IsDown("Move"));
    }

    [Fact]
    [Trait("Obligation", "controller.rebind")]
    public void Rebind_ReplacesNamedActionInput()
    {
        var input = new TestInput();
        var controller = new Controller(input);
        controller.Bind("Jump", InputBinding.Keyboard(KeyboardKey.Space));
        controller.Rebind("Jump", InputBinding.Mouse(MouseButton.Left));

        input.Keys.Add(KeyboardKey.Space);
        controller.Update(0.1f);
        Assert.False(controller.IsDown("Jump"));

        input.MouseButtons.Add(MouseButton.Left);
        controller.Update(0.1f);
        Assert.True(controller.WasPressed("Jump"));
    }

    [Fact]
    [Trait("Obligation", "controller.tap")]
    public void PressAndReleaseBetweenFrames_StillProducesPressAndRelease()
    {
        var input = new TestInput();
        var controller = new Controller(input);
        controller.Bind("Fire", InputBinding.Keyboard(KeyboardKey.F));

        input.PressedKeys.Add(KeyboardKey.F);
        controller.Update(0.1f);

        Assert.True(controller.WasPressed("Fire"));
        Assert.True(controller.WasReleased("Fire"));
        Assert.Equal(0f, controller.LastHeldTime("Fire"));
    }
}