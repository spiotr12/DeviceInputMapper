using SharpDX.DirectInput;
using Z.Expressions;

namespace DeviceInputMapper;

class ControllerHandler : JoystickHandler
{
    private readonly Joystick _joystick;

    public ControllerHandler(string id, DeviceConfig config, Joystick joystick)
        : base(id, config, joystick)
    {
        _joystick = joystick;
    }

    protected override string GetButtonName(JoystickUpdate state)
    {
        return state.Offset.ToString();
    }
}