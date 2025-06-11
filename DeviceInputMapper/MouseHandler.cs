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

    protected override string GetKeyName(MouseUpdate state)
    {
        throw new NotImplementedException();
    }
}