using SharpDX.DirectInput;
using Z.Expressions;

namespace DeviceInputMapper;

class JoystickHandler : Handler<JoystickState, RawJoystickState, JoystickUpdate>
{
    private readonly Joystick _joystick;

    public JoystickHandler(string id, DeviceConfig config, Joystick joystick)
        : base(id, config, joystick)
    {
        _joystick = joystick;
    }

    protected override string GetKeyName(JoystickUpdate state)
    {
        return state.Offset.ToString();
    }
}