using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Items;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Qualities
{
    public interface IQualitySystem : IEntitySystem
    {
        Quality GetQuality(EntityUid ent, QualityComponent? quality = null);
    }

    public sealed class QualitySystem : EntitySystem, IQualitySystem
    {
        public override void Initialize()
        {
            SubscribeComponent<QualityComponent, EntityBeingGeneratedEvent>(SetQualityFromGenArgs, EventPriorities.Highest);
            SubscribeComponent<ItemComponent, EntityBeingGeneratedEvent>(FixQualityForNonEquipmentItem, EventPriorities.Lowest);
            SubscribeComponent<QualityComponent, GetBaseNameEventArgs>(AddQualityBrackets);
        }

        public Quality GetQuality(EntityUid ent, QualityComponent? quality = null)
        {
            if (!Resolve(ent, ref quality))
                return Quality.Bad;

            return quality.Quality.Buffed;
        }

        private void SetQualityFromGenArgs(EntityUid uid, QualityComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (args.CommonArgs.Quality != null)
                component.Quality.Base = args.CommonArgs.Quality.Value;
        }

        private void FixQualityForNonEquipmentItem(EntityUid item, ItemComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!TryComp<QualityComponent>(item, out var quality))
                return;

            // >>>>>>>> elona122/shade2/item.hsp:541 	if refType<fltPotion{ ...
            if (!HasComp<EquipmentComponent>(item) && quality.Quality.Base != Quality.Unique)
            {
                quality.Quality.Base = Quality.Normal;
            }
            // <<<<<<<< elona122/shade2/item.hsp:545 		} ...
        }

        private void AddQualityBrackets(EntityUid uid, QualityComponent quality, ref GetBaseNameEventArgs args)
        {
            if (quality.Quality.Buffed == Quality.Great)
            {
                args.OutBaseName = Loc.GetString("Elona.Quality.Brackets.Great", ("name", args.OutBaseName));
            }
            else if (quality.Quality.Buffed == Quality.God)
            {
                args.OutBaseName = Loc.GetString("Elona.Quality.Brackets.God", ("name", args.OutBaseName));
            }
        }
    }
}
