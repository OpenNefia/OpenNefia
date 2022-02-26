using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.CharaInfo
{
    public class CharaGroupSublayerArgs
    {
        public enum CharaTab
        {
            CharaInfo,
            Equipment,
            FeatInfo,
            MaterialInfo
        }

        public CharaTab Type { get; }
        public EntityUid CharaEntity { get; }

        public CharaGroupSublayerArgs(CharaTab type, EntityUid chara)
        {
            Type = type;
            CharaEntity = chara;
        }
    }

    public class CharaGroupSublayerResult
    {
        /// <summary>
        /// If true, the character changed equipment. This means the character's
        /// turn should be passed when the menu is exited.
        /// </summary>
        public bool ChangedEquipment { get; set; }
    }

    public class CharaGroupUiLayer : GroupableUiLayer<CharaGroupSublayerArgs, CharaGroupSublayerResult>
    {
        public CharaGroupSublayerResult SharedSublayerResult { get; set; } = default!;

        public CharaGroupUiLayer()
        {
            EventFilter = UIEventFilterMode.Pass;
            CanControlFocus = true;
        }
    }

    public class CharaUiGroupArgs : UiGroupArgs<CharaGroupUiLayer, CharaGroupSublayerArgs>
    {
        public CharaUiGroupArgs(CharaGroupSublayerArgs.CharaTab selectedTab, EntityUid chara)
        {
            foreach (CharaGroupSublayerArgs.CharaTab logType in Enum.GetValues(typeof(CharaGroupSublayerArgs.CharaTab)))
            {
                var args = new CharaGroupSublayerArgs(logType, chara);

                if (logType == selectedTab)
                    SelectedArgs = args;

                Layers[args] = logType switch
                {
                    CharaGroupSublayerArgs.CharaTab.CharaInfo => new CharaInfoUiLayer(),
                    CharaGroupSublayerArgs.CharaTab.Equipment => new EquipmentUiLayer(),
                    CharaGroupSublayerArgs.CharaTab.FeatInfo => new FeatInfoUiLayer(),
                    // TODO: add other group layers
                    _ => new CharaGroupUiLayer()
                };
            }
        }
    }

    public class CharaUiGroup : UiGroup<CharaGroupUiLayer, CharaUiGroupArgs, CharaGroupSublayerArgs, CharaGroupSublayerResult>
    {
        public override void Initialize(CharaUiGroupArgs args)
        {
            base.Initialize(args);
            
            // for tracking equipment status
            CharaGroupSublayerResult sharedResult = new();

            foreach (var layer in Layers.Values)
            {
                layer.SharedSublayerResult = sharedResult;
            }
        }

        protected override AssetDrawable? GetIcon(CharaGroupSublayerArgs args)
        {
            var iconType = args.Type switch
            {
                CharaGroupSublayerArgs.CharaTab.CharaInfo => InventoryIcon.Chara,
                CharaGroupSublayerArgs.CharaTab.Equipment => InventoryIcon.Equip,
                CharaGroupSublayerArgs.CharaTab.FeatInfo => InventoryIcon.Feat,
                _ => InventoryIcon.Drink
            };

            // FIXME
            var icon = InventoryHelpers.MakeIcon(iconType);
            if (icon is not AssetDrawable iconAsset)
                return null;

            return iconAsset;
        }

        protected override string GetTabName(CharaGroupSublayerArgs args)
        {
            return Loc.GetString($"Elona.UI.MenuGroup.Chara.{args.Type}");
        }
    }
}
