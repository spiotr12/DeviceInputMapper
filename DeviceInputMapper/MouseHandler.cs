using SharpDX.DirectInput;

namespace DeviceInputMapper;

class MouseHandler : Handler<MouseState, RawMouseState, MouseUpdate>
{
    private readonly Mouse _mouse;

    public MouseHandler(DeviceConfig config, Mouse mouse)
        : base(config, mouse)
    {
        _mouse = mouse;
    }

    protected override void HandleFn(MouseUpdate state)
    {
    }
}