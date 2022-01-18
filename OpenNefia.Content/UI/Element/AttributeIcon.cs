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

namespace OpenNefia.Content.UI.Element
{
    public class AttributeIcon : UiElement
    {
        private const string FallbackIcon = "2";
        private readonly Dictionary<PrototypeId<SkillPrototype>, string> _attributes = new()
        {
            { Protos.Skill.AttrStrength, "0" },
            { Protos.Skill.AttrConstitution, "1" },
            { Protos.Skill.AttrDexterity, "2" },
            { Protos.Skill.AttrPerception, "3" },
            { Protos.Skill.AttrLearning, "4" },
            { Protos.Skill.AttrWill, "5" },
            { Protos.Skill.AttrMagic, "6" },
            { Protos.Skill.AttrCharisma, "7" }

        };
        private IAssetInstance AssetAttributeIcons;
        private PrototypeId<SkillPrototype>? Type;

        public AttributeIcon(PrototypeId<SkillPrototype>? type)
        {
            AssetAttributeIcons = Assets.Get(Protos.Asset.AttributeIcons);
            Type = type;
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(Color.White);
            if (Type != null && _attributes.TryGetValue(Type.Value, out var iconId))
                AssetAttributeIcons.DrawRegion($"{iconId ?? FallbackIcon}", PixelX, PixelY, centered: true);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new Vector2i(10, 10);
        }
    }
}
