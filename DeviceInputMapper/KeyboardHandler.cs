using SharpDX.DirectInput;

namespace DeviceInputMapper;

class KeyboardHandler : DirectInputHandler<KeyboardState, RawKeyboardState, KeyboardUpdate>
{
    private readonly SharpDX.DirectInput.Keyboard _keyboard;

    public KeyboardHandler(string id, DeviceConfig config, SharpDX.DirectInput.Keyboard keyboard)
        : base(id, config, keyboard)
    {
        _keyboard = keyboard;
    }

    protected override string GetKeyName(KeyboardUpdate state)
    {
        throw new NotImplementedException();
    }

}