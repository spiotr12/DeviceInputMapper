# Device Input Mapper

Utility to map device inputs to something else

## Usage

1. Create empty json file
2. Run command `DeviceInputMapper.exe "path\to\your\config.json"`
3. First run should create basic fields:

```json
{
  "defaultMode": "Default",
  "modes": {
    "Default": {}
  },
  "devices": {}
}
```

4. Use console output to find devices you want to ma. E.g.:

```text
InstanceName:            Saitek Side Panel Control Deck
InstanceGuid:            dc000000-4000-1000-8000-400000000000
ProductName:             Saitek Side Panel Control Deck
ProductGuid:             22180738-0000-0000-0000-504944564944
ForceFeedbackDriverGuid: 00000000-0000-0000-0000-000000000000
IsHumanInterfaceDevice:  True
Type:                    Gamepad
Subtype:                 258
Usage:                   VrHeadTracker
UsagePage:               Generic
```

5. Configure at will

## Configuration

**Note:** Some device information are autopopulated

### Add new device

Add `"<InstanceGuid>": {}` to "Devices", ad specify device type. E.g.:

```json5
{
  // ... 
  "devices": {
    // The key is InstanceGuid
    "dc000000-4000-1000-8000-400000000000": {
      "inputDeviceType": "joystick",
    }
  }
}
```

Running application again will auto set meta information and it will create empty `"Default"` config.

## API

### Config file schema

`"defaultMode"` {string} [auto] Default mode to load

`"modes"` {string: ModeConfig} [auto] Mode (key) and ModeConfig (value)

`"devices"` {string: DeviceConfig} [manual] InstanceGuid (key) and DeviceConfig (value)

### ModeConfig schema

`"parent"` {string} [optional] Name of different mode to inherit from

### DeviceConfig schema

`"description"`{string} [optional] Your custom description

`"configs"`{string: InputMappingConfig} [auto] Mode name (key) and InputMappingConfig (value) configuration

`"inputDeviceType"`{string} [required] Select one `"joystick"` | `"gamepad"` | `"controller"` | `"mouse"` | `"keyboard"`

`"_instanceName"` {string} [auto] Device meta information

`"_instanceGuid"` {string} [auto] Device meta information

`"_productName"` {string} [auto] Device meta information

`"_productGuid"` {string} [auto] Device meta information

`"_forceFeedbackDriverGuid"` {string} [auto] Device meta information

`"_isHumanInterfaceDevice"` {string} [auto] Device meta information

`"_type"` {string} [auto] Device meta information

`"_subtype"` {string} [auto] Device meta information

`"_usage"` {string} [auto] Device meta information

`"_usagePage"` {string} [auto] Device meta information

### InputMappingConfig schema

Its key value pair of button name and input configuration:

`"label"` {string} [optional] field for notes. Currently not used by anything

`"min"` {number} [optional] field for notes. Currently not used by anything

`"max"` {number} [optional] field for notes. Currently not used by anything

`"rawMin"` {number} [optional] field for notes. Currently not used by anything

`"rawMax"` {number} [optional] field for notes. Currently not used by anything

`"actions"` {ActionConfig[]} [manual] Array of ActionConfig

### ActionConfig schema

`"condition"` {string} [optional] Executable C# string, where you can use helper methods. This specifies condition to run action. If empty
it will be treated as `true`

`"action"` {string} [required] Executable C# string, where you can use helper methods. Action to execute when condition is met.

### Helper variables and functions

#### Variables

`id` {string} Device id. Available in: (***condition*** | ***action***)

`button` {string} Button name. Available in: (***condition*** | ***action***)

`value` {double} Input value. For Buttons 0 = released, 1 = pressed. For Axis its fraction value of rage between -1 and 1. Available in: (
condition | action)

`rawValue` {object} Raw input value. For Buttons 0 = released, 128 = pressed. For Axis its fraction value of rage between 0 and 65535 (
neutral state = 32767). Available in: (***condition*** | ***action***)

`currentMode` {string} Current mode. Available in: (***condition*** | ***action***)

`state`

`globalState`

#### Functions

`changeMode(string nextMode)` Available in: (***action***)

`previousMode()` Available in: (***action***)

`iterateModes(string[] modesInOrder, bool reverse)` e.g.: `"iterateModes(new string[] {"Default", "Layer 1", "Layer 2"}, false)"`. Available
in: (***action***)

`getDeviceState(string deviceGuid)` Available in: (***condition***)

`getDeviceButtonState(string deviceGuid, string button)` Available in: (***condition***)

`getDeviceButtonValue(string deviceGuid, string button)` Available in: (***condition***)

`getDeviceButtonRawValue(string deviceGuid, string button)` Available in: (***condition***)

`getButtonState(string button)` For current device. Available in: (***condition***)

`getButtonValue(string button)` For current device. Available in: (***condition***)

`getButtonRawValue(string button)` For current device. Available in: (***condition***)

`stateToString()` Return printable string of state. Use it with log e.g.: `"log(stateToString())"`. Available in: (***condition*** |
***action***)

`deviceStateToString()` Return printable string of state. Use it with log e.g.: `"log(deviceStateToString())"`. Available in: (
***condition*** | ***action***)

`globalStateToString()` Return printable string of state. Use it with log e.g.: `"log(globalStateToString())"`. Available in: (
***condition*** | ***action***)

`getDynamicStateValue(string key)` Allows to get your custom value across whole application. Available in: (***condition*** | ***action***)

`setDynamicStateValue(string key, object value)` Allows to store your custom value across whole application. Available in: (
***condition*** | ***action***)

`keyClick(string key)` Simulate keyboard press and release. Available in: (***action***)

`keyHold(string key, int delay)` Simulate keyboard press and release for given delay time. Available in: (***action***)

`keyPress(string key)` Simulate keyboard press. Available in: (***action***)

`keyRelease(string key)` Simulate keyboard release. Available in: (***action***)

`keyAutoRepeat(string key, int delay)` Simulate autorepeat keyboard press and release with delay time between clicks. Kills previous
autorepeat for given key. Available in: (***action***)

`keyDynamicAutoRepeat(string key, int delay)` Simulate autorepeat keyboard press and release with delay time between clicks. Dynamically
change delay time. Updates delay for given key, reusing previous autorepeat. E.g.:
`"var delay = Math.Round(1000 * (1 - Math.Abs(value))); keyDynamicAutoRepeat(\"A\", delay);"`. Available in: (***action***)

`keyDynamicAutoRepeatMinMaxTime(string key, double value, int minTime, int maxTime)` Wrapper for:
`"var delay = Math.Round(maxTime * (1 - Math.Abs(value))) + minTime; keyDynamicAutoRepeat(key, delay);"`. Available in: (***action***)

`keyStopAutoRepeat(string key)` Kills autorepeat for given key. Available in: (***action***)

`keyStopAllAutoRepeat()` Kills all autorepeat threads. Available in: (***action***)

`log(object obj)` Log anything. Available in: (***condition*** | ***action***)

`reloadJsonConfig()` Reloads json allowing you to change file without restarting application. Available in: (***action***)

`exit()` Stop application. Available in: (***action***)

### Examples

#### Modifier key

Change to "Layer 1" during key hold and return to previous when released

```json5
{
  "Buttons0": {
    "actions": [
      {
        "condition": "value == 1",
        "action": "changeMode(\"Layer 1\")"
      },
      {
        "condition": "value == 0",
        "action": "previousMode()"
      }
    ]
  },
}
```

#### Dynamic axis autorepeat

Example of how to map W | A | S | D keys to X | Y axis

```json5
{
  "X": {
    "actions": [
      {
        "condition": "value < 0",
        "action": "keyDynamicAutoRepeatMinMaxTime(\"A\", value, 0, 1000);"
      },
      {
        "condition": "value > 0",
        "action": "keyDynamicAutoRepeatMinMaxTime(\"D\", value, 0, 1000);"
      },
      {
        "condition": "value == 0",
        "action": "keyStopAutoRepeat(\"A\"); keyStopAutoRepeat(\"D\");"
      }
    ]
  },
  "Y": {
    "actions": [
      {
        "condition": "value < 0",
        "action": "keyDynamicAutoRepeatMinMaxTime(\"W\", value, 0, 1000);"
      },
      {
        "condition": "value > 0",
        "action": "keyDynamicAutoRepeatMinMaxTime(\"S\", value, 0, 1000);"
      },
      {
        "condition": "value == 0",
        "action": "keyStopAutoRepeat(\"W\"); keyStopAutoRepeat(\"S\");"
      }
    ]
  },
}
```