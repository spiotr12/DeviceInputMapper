using SharpDX.DirectInput;

namespace DeviceInputMapper;

class KeyboardHandler : Handler<KeyboardState, RawKeyboardState, KeyboardUpdate>
{
    private readonly Keyboard _keyboard;

    public KeyboardHandler(DeviceConfig config, Keyboard keyboard)
        : base(config, keyboard)
    {
        _keyboard = keyboard;
    }

    protected override void HandleFn(KeyboardUpdate state)
    {
    }
}