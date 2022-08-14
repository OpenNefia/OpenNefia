﻿using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Material;
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
            SubscribeComponent<ItemDescriptionComponent, GetItemDescriptionEventArgs>(GetDescItemDesc, priority: EventPriorities.VeryHigh);
        }

        private void GetDescItem(EntityUid uid, ItemComponent item, GetItemDescriptionEventArgs args)
        {
            var identifyState = IdentifyState.Full;

            if (EntityManager.TryGetComponent(uid, out IdentifyComponent identify))
            {
                identifyState = identify.IdentifyState;
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

        private void AddQualityInfo(ItemComponent item, GetItemDescriptionEventArgs args)
        {
            if (TryComp<MaterialComponent>(item.Owner, out var material) && material.MaterialID != null)
            {
                var materialName = Loc.GetPrototypeString(material.MaterialID.Value, "Name");
                var entry = new ItemDescriptionEntry()
                {
                    Text = Loc.GetString("Elona.ItemDescription.ItIsMadeOf", ("materialName", materialName))
                };
                args.Entries.Add(entry);
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
                args.Entries.Add(itemDesc.Primary);
            }

            args.Entries.AddRange(itemDesc.Extra);
        }

        public void GetItemDescription(EntityUid entity, IList<ItemDescriptionEntry> entries)
        {
            var ev = new GetItemDescriptionEventArgs(entries);
            RaiseEvent(entity, ev);

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