using SharpDX.DirectInput;
using Z.Expressions;

namespace DeviceInputMapper;

class JoystickHandler : DirectInputHandler<JoystickState, RawJoystickState, JoystickUpdate>
{
    private readonly Joystick _joystick;

    public JoystickHandler(string id, DeviceConfig config, Joystick joystick)
        : base(id, config, joystick)
    {
        _joystick = joystick;
    }

    protected override string GetButtonName(JoystickUpdate state)
    {
        return state.Offset.ToString();
    }

    protected override double ParseValue(JoystickUpdate state)
    {
        // Axis
        if (!GetButtonName(state).Contains("Buttons"))
        {
            return Math.Round(((double)(state.Value - 32767) / 32767), 4);
        }

        // Button
        return state.Value == 0 ? 0 : 1;
    }
}