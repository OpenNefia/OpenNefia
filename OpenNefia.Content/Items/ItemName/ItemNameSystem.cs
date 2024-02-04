using OpenNefia.Content.Locale.Funcs;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Activity;
using OpenNefia.Core.Game;
using OpenNefia.Content.Charas;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Identify;
using OpenNefia.Content.CurseStates;
using OpenNefia.Core.Maps;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.World;
using OpenNefia.Content.Food;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Chargeable;

namespace OpenNefia.Content.Items
{
    public interface IItemNameSystem : IEntitySystem
    {
        /// <summary>
        /// Formats an item's name like "scroll of ally" instead of just "ally".
        /// </summary>
        /// <param name="itemID">The prototype ID of the item.</param>
        /// <returns>The formatted name.</returns>
        string QualifyNameWithItemType(PrototypeId<EntityPrototype> itemID);
    }

    public sealed partial class ItemNameSystem : EntitySystem, IItemNameSystem
    {
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IChargeableSystem _chargeds = default!;
        [Dependency] private readonly IIdentifySystem _identifies = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IFoodSystem _food = default!;
        [Dependency] private readonly IHungerSystem _hungers = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeEntity<EntityGeneratedEvent>(AddJapaneseCounterTranslation);
            SubscribeComponent<ItemComponent, GetItemNameEvent>(BasicName, priority: EventPriorities.VeryHigh);

            Initialize_ItemEvents();
        }

        // TODO: refactoring out component-specific name modifiers.
        // will take forever, and this set of functions have already aged me enough as-is
        public void BasicName(EntityUid uid, ItemComponent component, ref GetItemNameEvent args)
        {
            switch(Loc.Language)
            {
                case var jp when jp == LanguagePrototypeOf.Japanese:
                    args.OutItemName += ItemNameJP(uid, args.AmountOverride, component);
                    break;
                case var cn when cn == LanguagePrototypeOf.Chinese:
                    args.OutItemName += ItemNameCN(uid, args.AmountOverride, component);
                    break;
                case var de when de == LanguagePrototypeOf.German:
                    args.OutItemName += ItemNameDE(uid, args.AmountOverride, component);
                    break;
                case var en when en == LanguagePrototypeOf.English:
                default:
                    args.OutItemName += ItemNameEN(uid, args.AmountOverride, args.NoArticle, component);
                    break;
            }
        }

        public string ItemNameDE(EntityUid uid,
            int? amount = null,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return $"<item {uid}>";

            return GermanBuiltins.GetDisplayData(uid, meta.DisplayName!).GetIndirectName(stack.Count);
        }

        /// <inheritdoc />
        public string QualifyNameWithItemType(PrototypeId<EntityPrototype> itemID)
        {
            var name = Loc.GetPrototypeString(itemID, "MetaData.Name");
            var hasTypeName = Loc.TryGetPrototypeString(itemID, "Item.ItemTypeName", out var itemTypeName);
            if (!hasTypeName)
                return name;

            return Loc.GetString("Elona.Common.QualifiedName", ("basename", name), ("itemTypeName", itemTypeName));
        }
    }

    [ByRefEvent]
    public struct GetItemNameEvent
    {
        public int? AmountOverride { get; }
        public bool NoArticle { get; }

        public string OutItemName { get; set; } = string.Empty;

        public GetItemNameEvent(int? amount, bool noArticle)
        {
            AmountOverride = amount;
            NoArticle = noArticle;
        }
    }
}
