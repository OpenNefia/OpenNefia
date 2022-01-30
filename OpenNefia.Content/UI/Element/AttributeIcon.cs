using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.UI.Element
{
    public class AttributeIcon : UiElement
    {
        public static class Regions
        {
#pragma warning disable format
            public const string IconStrength     = "0";
            public const string IconConstitution = "1";
            public const string IconDexterity    = "2";
            public const string IconPerception   = "3";
            public const string IconLearning     = "4";
            public const string IconWill         = "5";
            public const string IconMagic        = "6";
            public const string IconCharisma     = "7";

            public const string IconResistance   = "11";
#pragma warning restore format
        }

        private static readonly Dictionary<PrototypeId<SkillPrototype>, string> _attributes = new()
        {
#pragma warning disable format
            { Protos.Skill.AttrStrength,      Regions.IconStrength },
            { Protos.Skill.AttrConstitution,  Regions.IconConstitution },
            { Protos.Skill.AttrDexterity,     Regions.IconDexterity },
            { Protos.Skill.AttrPerception,    Regions.IconPerception },
            { Protos.Skill.AttrLearning,      Regions.IconLearning },
            { Protos.Skill.AttrWill,          Regions.IconWill },
            { Protos.Skill.AttrMagic,         Regions.IconMagic },
            { Protos.Skill.AttrCharisma,      Regions.IconCharisma }
#pragma warning restore format
        };

        private IAssetInstance AssetAttributeIcons;
        private string? _regionId;

        public AttributeIcon(PrototypeId<SkillPrototype>? type)
        {
            AssetAttributeIcons = Assets.Get(Protos.Asset.AttributeIcons);
            _regionId = type != null ? _attributes.GetValueOrDefault(type.Value) : null;
        }

        public AttributeIcon(string regionId)
        {
            AssetAttributeIcons = Assets.Get(Protos.Asset.AttributeIcons);
            _regionId = regionId;
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(Color.White);
            if (_regionId != null)
                AssetAttributeIcons.DrawRegion(UIScale, _regionId, X, Y, centered: true);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new Vector2i(10, 10);
        }
    }
}
