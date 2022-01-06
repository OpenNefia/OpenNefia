using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public interface IItemDescriptionSystem
    {
        void GetItemDescription(EntityUid entity, IList<ItemDescriptionEntry> entries);
    }

    public class ItemDescriptionSystem : EntitySystem, IItemDescriptionSystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<ItemComponent, GetItemDescriptionEventArgs>(GetDescItem, nameof(GetDescItem));
            SubscribeLocalEvent<ItemDescriptionComponent, GetItemDescriptionEventArgs>(GetDescItemDesc, nameof(GetDescItemDesc));
        }

        private void GetDescItem(EntityUid uid, ItemComponent item, GetItemDescriptionEventArgs args)
        {
            var identifyState = IdentifyState.Full;

            if (EntityManager.TryGetComponent(uid, out IdentifyComponent identify))
            {
                identifyState = identify.State;
            }

            if (identifyState >= IdentifyState.Quality)
            {
                AddQualityInfo(item, args);
            }
            else
            {
                args.Entries.Add(new ItemDescriptionEntry()
                {
                    Text = Loc.GetString("Elona.ItemDescription.HaveToIdentify")
                });
            }
        }

        private static void AddQualityInfo(ItemComponent item, GetItemDescriptionEventArgs args)
        {
            if (item.Material != null)
            {
                var entry = new ItemDescriptionEntry()
                {
                    Text = Loc.GetString("Elona.ItemDescription.ItIsMadeOf", ("materialName", item.Material.ToString()))
                };
                args.Entries.Add(entry);
            }
        }

        private void GetDescItemDesc(EntityUid uid, ItemDescriptionComponent itemDesc, GetItemDescriptionEventArgs args)
        {
            if (EntityManager.TryGetComponent(uid, out IdentifyComponent identify)
                && identify.State < IdentifyState.Full)
            {
                return;
            }

            if (itemDesc.Primary != null)
            {
                args.Entries.Add(itemDesc.Primary);
            }

            args.Entries.AddRange(itemDesc.Extra);
        }

        public void GetItemDescription(EntityUid entity, IList<ItemDescriptionEntry> entries)
        {
            var ev = new GetItemDescriptionEventArgs(entries);
            RaiseLocalEvent(entity, ev);
         
            if (entries.Count == 0)
            {
                entries.Add(new ItemDescriptionEntry() { Text = Loc.GetString("Elona.ItemDescription.NoInformation") });
            }
        }
    }

    public class GetItemDescriptionEventArgs : EntityEventArgs
    {
        public IList<ItemDescriptionEntry> Entries { get; }

        public GetItemDescriptionEventArgs(IList<ItemDescriptionEntry> entries)
        {
            Entries = entries;
        }
    }
}