using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemPrototypeIdsUICell : BaseConfigMenuUICell<ConfigPrototypeIdsMenuNode>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private List<IPrototype> _choices = new();
        private int _currentIndex = 0;

        public ConfigItemPrototypeIdsUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigPrototypeIdsMenuNode data) : base(protoId, data)
        {
        }

        public override void Initialize()
        {
            var prototypeType = _protos.GetVariantType(MenuNode.PrototypeType);

            _choices = _protos.EnumeratePrototypes(prototypeType).ToList();

            var currentValue = ConfigManager.GetCVar(MenuNode.CVar);
            _currentIndex = 0;

            var current = _choices.FirstOrDefault(proto => proto.ID == currentValue);
            if (current != null)
                _currentIndex = _choices.IndexOf(current);
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (_currentIndex > 0, _currentIndex < _choices.Count - 1);
        }

        public override void HandleChanged(int delta)
        {
            _currentIndex = Math.Clamp(_currentIndex + delta, 0, _choices.Count);

            if (_choices.Count > 0)
                ConfigManager.SetCVar(MenuNode.CVar, _choices[_currentIndex].ID);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            var prototypeType = _protos.GetVariantType(MenuNode.PrototypeType);
            var currentID = ConfigManager.GetCVar(MenuNode.CVar);
            string text;

            if (_protos.TryIndex(prototypeType, currentID, out var proto))
            {
                text = Loc.GetPrototypeStringRaw(prototypeType, currentID, MenuNode.NameLocaleKey);
            }
            else
            {

                text = $"<{currentID}>";
            }

            ValueText.Text = text;
        }
    }
}
