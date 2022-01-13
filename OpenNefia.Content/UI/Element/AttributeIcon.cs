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
            { Protos.Skill.StatStrength, "0" },
            { Protos.Skill.StatConstitution, "1" },
            { Protos.Skill.StatDexterity, "2" },
            { Protos.Skill.StatPerception, "3" },
            { Protos.Skill.StatLearning, "4" },
            { Protos.Skill.StatWill, "5" },
            { Protos.Skill.StatMagic, "6" },
            { Protos.Skill.StatCharisma, "7" }

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
                AssetAttributeIcons.DrawRegion($"{iconId ?? FallbackIcon}", X, Y, centered: true);
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = new Vector2i(10, 10);
        }
    }
}
