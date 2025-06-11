using SharpDX.DirectInput;

namespace DeviceInputMapper;

abstract class Handler<T, TRaw, TUpdate>
    where T : class, IDeviceState<TRaw, TUpdate>, new()
    where TRaw : struct
    where TUpdate : struct, IStateUpdate
{
    public bool EnableLogging { get; set; }
    public string Mode { get; set; }

    protected readonly DeviceConfig _config;
    protected readonly CustomDevice<T, TRaw, TUpdate> _device;

    public Handler(DeviceConfig config, CustomDevice<T, TRaw, TUpdate> device)
    {
        _config = config;
        _device = device;

        if (_config.Modes != null)
            Mode = _config.Modes.ContainsKey("Default") ? "Default" : _config.Modes.Keys.First();

        _device.Properties.BufferSize = 128;
        _device.Acquire();
    }

    protected abstract void HandleFn(TUpdate state);

    protected object GetCurrentModeConfig()
    {
        if (_config.Modes == null || _config.Modes[Mode] == null)
        {
            throw new Exception("No device configuration found for this mode");
        }

        return _config.Modes[Mode];
    }


    public Task Run()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("{0} [{1}]\n> Task listening to events\n", _config.InstanceName, _config.InstanceGuid);

            // Poll events from joystick
            while (true)
            {
                _device.Poll();
                var datas = _device.GetBufferedData();
                foreach (var state in datas)
                {
                    if (EnableLogging)
                    {
                        Console.WriteLine("{0} [{1}] ", _config.InstanceName, _config.InstanceGuid);
                        Console.WriteLine("> {0}", state);
                        Console.WriteLine();
                    }

                    HandleFn(state);
                }
            }
        }, new CancellationToken());
    }
}