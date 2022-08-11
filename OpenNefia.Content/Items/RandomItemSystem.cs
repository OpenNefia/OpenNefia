using Love;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Material;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.Utility;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Items
{
    public interface IRandomItemSystem : IEntitySystem
    {
        Color GetDefaultItemColor(EntityUid uid, int? seed = null, RandomItemComponent? randomItem = null);

        /// <summary>
        /// Gets an integer randomized per-save for this entity based on its prototype ID.
        /// All entities with the same prototype ID will have the same random index.
        /// Used for things like random item names/colors.
        /// </summary>
        long GetRandomEntityIndex(EntityUid uid, int? seed = null);
        long GetRandomEntityIndex(PrototypeId<EntityPrototype> id, int? seed = null);
    }

    public sealed class RandomItemSystem : EntitySystem, IRandomItemSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        
        public override void Initialize()
        {
            SubscribeComponent<RandomItemComponent, EntityBeingGeneratedEvent>(SetRandomItemColor, priority: EventPriorities.VeryHigh);
        }

        private void SetRandomItemColor(EntityUid uid, RandomItemComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (TryComp<ChipComponent>(uid, out var chip))
            {
                chip.Color = GetDefaultItemColor(uid);
            }
        }

        private static readonly Color[] RandomItemColors =
        {
            UiColors.MesWhite,
            UiColors.MesGreen,
            UiColors.MesBlue,
            UiColors.MesYellow,
            UiColors.MesBrown,
            UiColors.MesRed
        };
        
        private static readonly Color[] RandomFurnitureColors =
        {
            UiColors.MesWhite,
            UiColors.MesGreen,
            UiColors.MesBlue,
            UiColors.MesYellow,
            UiColors.MesBrown,
        };

        public long GetRandomEntityIndex(EntityUid uid, int? seed = null)
        {
            seed ??= _world.State.RandomSeed;
            return StringHelpers.HashStringToUInt32(ProtoIDOrNull(uid)?.ToString() ?? "") % seed.Value;
        }

        public long GetRandomEntityIndex(PrototypeId<EntityPrototype> id, int? seed = null)
        {
            seed ??= _world.State.RandomSeed;
            return StringHelpers.HashStringToUInt32(id.ToString()) % seed.Value;
        }

        private Color RandomItemColor(EntityUid uid, int? seed = null)
        {
            var index = GetRandomEntityIndex(uid, seed);
            return RandomItemColors[index % RandomItemColors.Length];
        }

        public Color GetDefaultItemColor(EntityUid uid, int? seed = null, RandomItemComponent? randomItem = null)
        {
            // >>>>>>>> shade2/item.hsp:615 	iCol(ci)=iColOrg(ci) ...
            if (Resolve(uid, ref randomItem, logMissing: false))
            {
                if (randomItem.RandomColor == RandomColorType.RandomItem)
                {
                    return RandomItemColor(uid, seed);
                }
                else if (randomItem.RandomColor == RandomColorType.Furniture)
                {
                    return _rand.Pick(RandomFurnitureColors);
                }
            }
            
            if (TryComp<MaterialComponent>(uid, out var material) && material.MaterialID != null && _protos.TryIndex(material.MaterialID.Value, out var materialProto))
            {
                return materialProto.Color;
            }
            
            if (TryProto(uid, out var proto) && proto.Components.TryGetComponent<ChipComponent>(out var chip))
            {
                return chip.Color;
            }

            return Color.White;
            // <<<<<<<< shade2/item.hsp:616 	if iCol(ci)=coRand	:iCol(ci)=randColor(rnd(length ..
        }
    }
}