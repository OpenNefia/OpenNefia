using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemMidiDeviceUICell : BaseConfigMenuUICell<ConfigMidiDeviceMenuNode>
    {
        [Dependency] private readonly IMusicManager _music = default!;

        private List<OutputDevice> _outputDevices = new();
        private int _currentIndex = 0;

        public ConfigItemMidiDeviceUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigMidiDeviceMenuNode data) : base(protoId, data)
        {
        }

        public override void Initialize()
        {
            var outputDevices = _music.GetMidiOutputDevices();

            _outputDevices.Clear();
            _outputDevices.AddRange(outputDevices);
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (_currentIndex > 0, _currentIndex < _outputDevices.Count - 1);
        }

        public override void HandleChanged(int delta)
        {
            _currentIndex = Math.Clamp(_currentIndex + delta, 0, _outputDevices.Count - 1);

            ConfigManager.SetCVar(MenuNode.CVar, _currentIndex);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            var deviceIndex = ConfigManager.GetCVar(MenuNode.CVar);
            string text;

            if (_outputDevices.TryGetValue(deviceIndex, out var outputDevice))
            {
                text = $"{deviceIndex}: {outputDevice.Name}";
            }
            else
            {

                text = $"<unknown device {deviceIndex}>";
            }

            ValueText.Text = text;
        }
    }
}
