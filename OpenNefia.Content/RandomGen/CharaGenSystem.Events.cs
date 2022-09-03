using OpenNefia.Content.Charas;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomGen
{
    public sealed partial class CharaGenSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, EntityBeingGeneratedEvent>(FixLevelAndQuality, priority: EventPriorities.VeryHigh);

            _protos.PrototypesReloaded += ev =>
            {
                if (ev.ByType == null || ev.ByType.ContainsKey(typeof(ChipPrototype)))
                    RebuildHumanChipPrototypeCache();
            };
        }

        private readonly List<PrototypeId<ChipPrototype>> MaleHumanChips = new();
        private readonly List<PrototypeId<ChipPrototype>> FemaleHumanChips = new();

        private void RebuildHumanChipPrototypeCache()
        {
            MaleHumanChips.Clear();
            FemaleHumanChips.Clear();

            foreach (var proto in _protos.EnumeratePrototypes<ChipPrototype>())
            {
                var id = proto.GetStrongID();
                if (_protos.TryGetExtendedData(id, out ExtRandomHumanChip? humanChip))
                {
                    if (humanChip.Gender == Gender.Male)
                        MaleHumanChips.Add(id);
                    else
                        FemaleHumanChips.Add(id);
                }
            }
        }

        private void FixLevelAndQuality(EntityUid uid, CharaComponent component, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> shade2/chara.hsp:492 *chara_fix ..
            if (!TryComp<QualityComponent>(uid, out var quality) || !TryComp<LevelComponent>(uid, out var level))
                return;

            if (quality.Quality == Quality.Great)
            {
                level.Level = level.Level * 10 / 8;
            }
            else if (quality.Quality == Quality.God)
            {
                level.Level = level.Level * 10 / 6;
            }
            // <<<<<<<< shade2/chara.hsp:503 	return ..
        }
    }

    [DataDefinition]
    public sealed class ExtRandomHumanChip : IPrototypeExtendedData<ChipPrototype>
    {
        [DataField]
        public Gender Gender { get; set; }
    }
}
