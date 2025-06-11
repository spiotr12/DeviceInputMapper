using SharpDX.DirectInput;
using Z.Expressions;

namespace DeviceInputMapper;

abstract class Handler<T, TRaw, TUpdate>
    where T : class, IDeviceState<TRaw, TUpdate>, new()
    where TRaw : struct
    where TUpdate : struct, IStateUpdate
{
    public bool EnableLogging { get; set; }
    public string Mode { get; set; }

    public IDictionary<string, TUpdate> DeviceState { get; }

    public IDictionary<string, IStateUpdate> GetDeviceState()
    {
        return (IDictionary<string, IStateUpdate>)DeviceState;
    }

    private readonly string _id;
    protected readonly DeviceConfig _config;
    protected readonly CustomDevice<T, TRaw, TUpdate> _device;

    public Handler(string id, DeviceConfig config, CustomDevice<T, TRaw, TUpdate> device)
    {
        _id = id;
        _config = config;
        _device = device;

        DeviceState = new Dictionary<string, TUpdate>();

        if (_config.Modes != null)
            Mode = _config.Modes.ContainsKey("Default") ? "Default" : _config.Modes.Keys.First();

        _device.Properties.BufferSize = 128;
        _device.Acquire();
    }

    protected abstract string GetKeyName(TUpdate state);

    protected IDictionary<string, IEnumerable<InputConfig>> GetCurrentModeConfig()
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

                    DeviceState[GetKeyName(state)] = state;
                    HandleFn(state);
                }
            }
        }, new CancellationToken());
    }

    protected void HandleFn(TUpdate state)
    {
        var config = GetCurrentModeConfig();
        IEnumerable<InputConfig> inputConfigs;
        config.TryGetValue(GetKeyName(state), out inputConfigs);

        if (inputConfigs != null && inputConfigs.Any())
        {
            foreach (var inputConfig in inputConfigs)
            {
                var condition = ParseCondition(inputConfig.Condition, state);
                if (condition)
                {
                    ParseAction(inputConfig.Action, state);
                }
            }
        }
    }

    protected bool ParseCondition(string condition, TUpdate state)
    {
        return Eval.Execute<bool>(condition, new { state, deviceState = DeviceState });
    }

    protected void ParseAction(string action, TUpdate state)
    {
        var keyClick = Keyboard.Click;
        var keyPress = Keyboard.Press;
        var keyRelease = Keyboard.Release;

        var mouseClick = Keyboard.Click;

        Eval.Execute(action, new
        {
            state,
            deviceState = DeviceState,
            keyClick,
            keyPress,
            keyRelease,
        });
    }
}