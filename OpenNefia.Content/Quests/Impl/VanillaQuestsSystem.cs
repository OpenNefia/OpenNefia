using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Game;
using OpenNefia.Content.Items;
using OpenNefia.Content.Quests;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Containers;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Parties;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Dialog;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Content.UI;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Charas;
using OpenNefia.Content.VanillaAI;

namespace OpenNefia.Content.Quests
{
    public interface IVanillaQuestsSystem
    {
        int CalcPartyScore(IMap map);
        int CalcPartyScoreBonus(IMap map, bool silent = false);
        bool WasPartyGreatSuccess(QuestTypePartyComponent component);
    }

    public sealed partial class VanillaQuestsSystem : EntitySystem, IVanillaQuestsSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IItemNameSystem _itemName = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IFameSystem _fame = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IMapImmediateQuestSystem _immediateQuests = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            Initialize_Deliver();
            Initialize_Supply();
            Initialize_Escort();
            Initialize_Party();
            Initialize_Collect();
        }

        public sealed class DialogQuestGiveItemData : IDialogExtraData
        {
            public DialogQuestGiveItemData() { }

            public DialogQuestGiveItemData(EntityUid quest, EntityUid item)
            {
                Quest = quest;
                Item = item;
            }

            [DataField]
            public EntityUid Quest { get; }

            [DataField]
            public EntityUid Item { get; }

            /// <summary>
            /// If true, the client will check if the item is poisoned. If so,
            /// karma loss will be applied.
            /// </summary>
            /// <remarks>
            /// Only Delivery quests don't check for poison (in vanilla)
            /// </remarks>
            [DataField]
            public bool CheckPoison { get; set; } = false;
        }

        public QualifiedDialogNode? GiveQuestItemAndTurnIn(IDialogEngine engine, IDialogNode node)
        {
            var data = engine.Data.Get<DialogQuestGiveItemData>();

            _mes.Display(Loc.GetString("Elona.Dialog.Common.YouHandOver", ("player", engine.Player), ("item", data.Item)));

            if (_inv.TryGetInventoryContainer(engine.Speaker!.Value, out var inv)
                && _stacks.TrySplit(data.Item, 1, out var split))
            {
                _inv.EnsureFreeItemSlot(engine.Speaker.Value);
                if (!inv.Insert(split, EntityManager))
                {
                    Logger.ErrorS("quest", $"Failed to give quest item {split} to client {engine.Speaker.Value}");
                    _stacks.Use(split, 1);
                }
                else
                {
                    if (TryComp<VanillaAIComponent>(engine.Speaker.Value, out var ai))
                    {
                        ai.ItemAboutToUse = split;
                    }
                    if (data.CheckPoison)
                    {
                        if (TryComp<QuestClientComponent>(engine.Speaker.Value, out var questClient))
                        {
                            questClient.WasPassedQuestItem = true;
                        }
                    }
                }
            }
            else
            {
                _stacks.Use(data.Item, 1);
            }

            var nextNodeID = _quests.TurnInQuest(data.Quest, engine.Speaker.Value, engine);
            return engine.GetNodeByID(nextNodeID);
        }
    }
}
