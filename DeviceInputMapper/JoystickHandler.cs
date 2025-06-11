using SharpDX.DirectInput;

namespace DeviceInputMapper;

class JoystickHandler : Handler<JoystickState, RawJoystickState, JoystickUpdate>
{
    private readonly Joystick _joystick;

    public JoystickHandler(DeviceConfig config, Joystick joystick)
        : base(config, joystick)
    {
        _joystick = joystick;
    }

    protected override void HandleFn(JoystickUpdate state)
    {
        var config = GetCurrentModeConfig();
    }
}