using SharpDX.DirectInput;

namespace DeviceInputMapper;

class MouseHandler : DirectInputHandler<MouseState, RawMouseState, MouseUpdate>
{
    private readonly SharpDX.DirectInput.Mouse _mouse;

    public MouseHandler(string id, DeviceConfig config, SharpDX.DirectInput.Mouse mouse)
        : base(id, config, mouse)
    {
        _mouse = mouse;
    }

    protected override string GetButtonName(MouseUpdate state)
    {
        return state.Offset.ToString();
    }

    protected override double ParseValue(MouseUpdate state)
    {
        // Button
        if (GetButtonName(state).Contains("Buttons"))
        {
            return state.Value == 0 ? 0 : 1;
        }

        if (GetButtonName(state).Contains("Z"))
        {
            return state.Value > 0 ? 1 : -1;
        }

        return state.Value;
    }
}