using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
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
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Items;

namespace OpenNefia.Content.Fishing
{
    public interface IFishingSystem : IEntitySystem
    {
    }

    public sealed class FishingSystem : EntitySystem, IFishingSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<BaitComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Bait);
            SubscribeComponent<BaitComponent, EntityBeingGeneratedEvent>(EntityBeingGenerated_Bait);

            SubscribeComponent<FishingPoleComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_FishingPole);
        }

        private void LocalizeExtra_Bait(EntityUid uid, BaitComponent component, ref LocalizeItemNameExtraEvent args)
        {
            var baitName = Loc.GetPrototypeString(component.BaitID, "Name");
            var s = Loc.GetString("Elona.Fishing.ItemName.Bait", ("name", args.OutFullName.ToString()), ("baitName", baitName));
            args.OutFullName = new StringBuilder(s);
        }

        private int GetDefaultBaitValue(int rank)
        {
            return rank * rank * 500 + 200;
        }

        private void EntityBeingGenerated_Bait(EntityUid uid, BaitComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!component.BaitID.IsValid())
            {
                component.BaitID = _rand.Pick(_protos.EnumeratePrototypes<BaitPrototype>().ToList()).GetStrongID();
            }

            var proto = _protos.Index(component.BaitID);
            
            if (TryComp<ChipComponent>(uid, out var chip))
                chip.ChipID = proto.ChipID;

            if (TryComp<ValueComponent>(uid, out var value))
                value.Value = proto.Value ?? GetDefaultBaitValue(proto.Rank);
        }

        private void LocalizeExtra_FishingPole(EntityUid uid, FishingPoleComponent component, ref LocalizeItemNameExtraEvent args)
        {
            if (component.BaitID != null && component.BaitAmount > 0)
            {
                var baitName = Loc.GetPrototypeString(component.BaitID.Value, "Name");
                var s = Loc.GetString("Elona.Fishing.ItemName.FishingPole",
                    ("name", args.OutFullName.ToString()),
                    ("baitName", baitName),
                    ("baitAmount", component.BaitAmount));
                args.OutFullName = new StringBuilder(s);
            }
        }
    }
}