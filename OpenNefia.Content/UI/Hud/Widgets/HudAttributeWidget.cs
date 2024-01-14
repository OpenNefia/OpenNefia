using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using System.Drawing;
using OpenNefia.Core.Stats;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Content.Combat;

namespace OpenNefia.Content.Hud
{
    public class HudAttributeWidget : BaseHudWidget
    {
        public enum HudSkillIconType
        {
            Str = 0,
            Con = 1,
            Dex = 2,
            Per = 3,
            Lea = 4,
            Wil = 5,
            Mag = 6,
            Cha = 7,
            Spd = 8,
            DvPv = 9
        }

        [Dependency] private readonly IEntityManager _entMan = default!;

        public HudSkillIconType Type { get; }
        private IAssetInstance SkillIcons;
        [Child] private UiText UiText;

        public HudAttributeWidget(HudSkillIconType type)
        {
            Type = type;
            UiText = new UiText(UiFonts.HUDSkillText);
            SkillIcons = Assets.Get(Protos.Asset.HudSkillIcons);
        }

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            if (_entMan.TryGetComponent<EquipStatsComponent>(GameSession.Player, out var equipStats)
                && _entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                PrototypeId<SkillPrototype>? protoId = Type switch
                {
                    HudSkillIconType.Str => Protos.Skill.AttrStrength,
                    HudSkillIconType.Con => Protos.Skill.AttrConstitution,
                    HudSkillIconType.Dex => Protos.Skill.AttrDexterity,
                    HudSkillIconType.Per => Protos.Skill.AttrPerception,
                    HudSkillIconType.Lea => Protos.Skill.AttrLearning,
                    HudSkillIconType.Wil => Protos.Skill.AttrWill,
                    HudSkillIconType.Mag => Protos.Skill.AttrMagic,
                    HudSkillIconType.Cha => Protos.Skill.AttrCharisma,
                    HudSkillIconType.Spd => Protos.Skill.AttrSpeed,
                    _ => null
                };
                if (protoId != null)
                {
                    var skillLevel = skills.Skills[protoId.Value].Level;
                    UiText.Text = skillLevel.Buffed.ToString();
                    UiText.Color = GetColorForStat(skillLevel);
                }
                else
                {
                    UiText.Color = Color.Black;
                    UiText.Text = $"{equipStats.DV.Buffed}/{equipStats.PV.Buffed}";
                }
            }
        }

        private Color GetColorForStat<T>(Stat<T> stat)
            where T : IComparable<T>
        {
            return stat.Buffed.CompareTo(stat.Base) switch
            {
                < 0 => Color.Red,
                > 0 => Color.Green,
                _ => Color.Black
            };
        }

        private string GetIconRegion()
        {
            return $"{(int)Type}";
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X + 18, Y);
        }

        public override void Draw()
        {
            base.Draw();
            SkillIcons.DrawRegion(UIScale, GetIconRegion(), X, Y);
            UiText.Draw();
        }
    }
}
