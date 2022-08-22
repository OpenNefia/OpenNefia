using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Materials;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Items
{
    public interface IItemDescriptionSystem
    {
        void GetItemDescription(EntityUid entity, IList<ItemDescriptionEntry> entries);
    }

    public class ItemDescriptionSystem : EntitySystem, IItemDescriptionSystem
    {
        public override void Initialize()
        {
            SubscribeComponent<ItemComponent, GetItemDescriptionEventArgs>(GetDescItem, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ItemDescriptionComponent, GetItemDescriptionEventArgs>(GetDescItemDesc, priority: EventPriorities.Lowest);
        }

        private void GetDescItem(EntityUid uid, ItemComponent item, GetItemDescriptionEventArgs args)
        {
            var identifyState = IdentifyState.Full;

            if (EntityManager.TryGetComponent(uid, out IdentifyComponent identify))
            {
                identifyState = identify.IdentifyState;
            }

            if (identifyState < IdentifyState.Quality)
            {
                args.OutEntries.Add(new ItemDescriptionEntry()
                {
                    Text = Loc.GetString("Elona.ItemDescription.HaveToIdentify")
                });
            }
        }

        private void GetDescItemDesc(EntityUid uid, ItemDescriptionComponent itemDesc, GetItemDescriptionEventArgs args)
        {
            if (EntityManager.TryGetComponent(uid, out IdentifyComponent identify)
                && identify.IdentifyState < IdentifyState.Full)
            {
                return;
            }

            if (itemDesc.Primary != null)
            {
                args.OutEntries.Insert(0, itemDesc.Primary);
            }

            args.OutEntries.Add(new ItemDescriptionEntry() { Text = string.Empty });
            args.OutEntries.AddRange(itemDesc.Extra);
        }

        public void GetItemDescription(EntityUid entity, IList<ItemDescriptionEntry> entries)
        {
            var ev = new GetItemDescriptionEventArgs(entries);
            RaiseEvent(entity, ev);

            if (entries.Count == 0)
            {
                entries.Add(new ItemDescriptionEntry() { Text = Loc.GetString("Elona.ItemDescription.NoInformation") });
            }

            foreach (var entry in entries)
            {
                entry.Text = Loc.Capitalize(entry.Text);
            }
        }
    }

    public class GetItemDescriptionEventArgs : EntityEventArgs
    {
        public IList<ItemDescriptionEntry> OutEntries { get; }

        public GetItemDescriptionEventArgs(IList<ItemDescriptionEntry> entries)
        {
            OutEntries = entries;
        }
    }
}