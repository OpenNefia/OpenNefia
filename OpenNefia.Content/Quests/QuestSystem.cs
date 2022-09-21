using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using NLua;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.Logic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Random;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Roles;
using OpenNefia.Content.World;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Game;
using OpenNefia.Content.Fame;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Quests
{
    public interface IQuestSystem : IEntitySystem
    {
        IEnumerable<QuestComponent> EnumerateQuestsForClient(EntityUid client);
        IEnumerable<(QuestComponent, T)> EnumerateQuestsForClient<T>(EntityUid client)
            where T : class, IComponent;
        IEnumerable<QuestComponent> EnumerateQuests();
        IEnumerable<QuestComponent> EnumerateAcceptedQuests();

        void UpdateInMap(IMap map);

        string FormatQuestObjective(string name);
        LocalizedQuestData LocalizeQuestData(EntityUid quest, EntityUid client, EntityUid player, QuestComponent? questComp = null);
        Dictionary<string, object> GetQuestLocaleParams(EntityUid quest, EntityUid client, EntityUid player);

        QualifiedDialogNodeID TurnInQuest(EntityUid quest, EntityUid speaker);
        void FailQuest(EntityUid quest);
    }

    public sealed partial class QuestSystem : EntitySystem, IQuestSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFameSystem _fame = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRoleSystem _roles = default!;

        [RegisterSaveData("Elona.QuestSystem.Quests")]
        private List<EntityUid> _quests { get; set; } = new();

        #region API Methods

        public IEnumerable<QuestComponent> EnumerateQuestsForClient(EntityUid client)
        {
            if (!HasComp<QuestClientComponent>(client))
                return Enumerable.Empty<QuestComponent>();

            return EnumerateQuests().Where(q => q.ClientEntity == client);
        }

        public IEnumerable<(QuestComponent, T)> EnumerateQuestsForClient<T>(EntityUid client) where T : class, IComponent
        {
            foreach (var quest in EnumerateQuestsForClient(client))
            {
                // TODO use EntityQuery<T> struct
                if (TryComp<T>(quest.Owner, out var questType))
                    yield return (quest, questType);
            }
        }

        public IEnumerable<QuestComponent> EnumerateQuests()
        {
            foreach (var quest in _quests)
            {
                if (TryComp<QuestComponent>(quest, out var questComp))
                    yield return questComp;
                else
                    Logger.ErrorS("quest", $"Entity {quest} does not have a {nameof(QuestComponent)}!");
            }
        }

        public IEnumerable<QuestComponent> EnumerateAcceptedQuests()
        {
            return EnumerateQuests().Where(q => q.State == QuestState.Accepted);
        }

        private bool IsValidQuestHub(IMap map, IArea area)
        {
            if (!HasComp<MapQuestHubComponent>(map.MapEntityUid))
                return false;

            if (!TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon)
                || mapCommon.IsTemporary
                || !mapCommon.IsRenewable)
                return false;

            return true;
        }

        private void RegisterQuestHub(IMap map, IArea area)
        {
            Logger.InfoS("quest", $"Registering quest hub {map}.");

            var areaQuestHubs = EnsureComp<AreaQuestsComponent>(area.AreaEntityUid);

            if (areaQuestHubs.QuestHubs.ContainsKey(map.Id))
                return;

            areaQuestHubs.QuestHubs.Add(map.Id, new QuestHubData()
            {
                MapName = _displayNames.GetDisplayName(map.MapEntityUid)
            });
        }

        private void UpdateQuestHubRegistration(IMap map, IArea area, AreaQuestsComponent? areaQuests = null)
        {
            if (IsValidQuestHub(map, area))
            {
                RegisterQuestHub(map, area);
            }
            else
            {
                // TODO unregister?
            }
        }

        private bool CanBeQuestClient(CharaComponent chara)
        {
            if (_qualities.GetQuality(chara.Owner) == Quality.Unique)
                return false;

            if (!_roles.HasAnyRoles(chara.Owner)
                || HasComp<RoleSpecialComponent>(chara.Owner)
                || HasComp<RoleAdventurerComponent>(chara.Owner))
                return false;

            return true;
        }

        public void UpdateInMap(IMap map)
        {
            if (!TryArea(map, out var area))
                return;

            Logger.InfoS("quest", $"Updating quests in map {map}.");

            UpdateQuestHubRegistration(map, area);

            if (TryComp<AreaQuestsComponent>(area.AreaEntityUid, out var areaQuests)
                && areaQuests.QuestHubs.TryGetValue(map.Id, out var questHub))
            {
                // Register all characters that can be quest targets.
                foreach (var chara in _charas.EnumerateNonAllies(map).Where(CanBeQuestClient))
                {
                    Logger.DebugS("quest", $"Registering quest client {chara.Owner}");
                    EnsureComp<QuestClientComponent>(chara.Owner);
                    questHub.Clients.Add(chara.Owner);
                }

                // Remove clients that do not exist in this map any longer.
                foreach (var client in questHub.Clients.ToList())
                {
                    if (!IsAlive(client) || !HasComp<QuestClientComponent>(client))
                    {
                        Logger.DebugS("quest", $"Removing missing quest client {client}");
                        questHub.Clients.Remove(client);
                    }
                }

                // Generate quests for characters that need them.
                foreach (var client in questHub.Clients)
                {
                    CleanDeadQuests(client);

                    if (ShouldGenerateQuestsForClient(client) && !_rand.OneIn(3))
                    {
                        if (TryGenerateQuest(client, out var quest))
                        {
                            var questID = MetaData(quest.Owner).EntityPrototype?.GetStrongID();
                            Logger.DebugS("quest", $"Successfully generated quest: {questID} ({quest.Owner})");
                        }
                    }
                }
            }
        }

        private bool ShouldGenerateQuestsForClient(EntityUid client)
        {
            return !EnumerateQuestsForClient(client).Any();
        }

        private bool IsDeadQuest(QuestComponent quest)
        {
            if (quest.State == QuestState.NotAccepted)
                return _world.State.GameDate > quest.TownBoardExpirationDate;

            return false;
        }

        private void DeleteQuest(QuestComponent quest)
        {
            var ev = new QuestTerminatingEvent(quest);
            RaiseEvent(quest.Owner, ev);

            _quests.Remove(quest.Owner);
            EntityManager.DeleteEntity(quest.Owner);
        }

        private void CleanDeadQuests(EntityUid client)
        {
            foreach (var quest in EnumerateQuestsForClient(client).Where(IsDeadQuest).ToList())
            {
                DeleteQuest(quest);
            }
        }

        private void FilterQuestsByPlayerFame(QuestCanGenerateEvent ev)
        {
            if (ev.QuestPrototype.Components.TryGetComponent<QuestMinimumFameComponent>(out var questMinFame))
            {
                var fame = _fame.GetFame(_gameSession.Player);
                if (fame < questMinFame.MinimumFame)
                {
                    ev.OutCanGenerate = false;
                }
            }
        }

        public PrototypeId<EntityPrototype>? PickRandomQuestId()
        {
            int GetWeight(EntityPrototype proto, int minLevel)
            {
                var comps = proto.Components;
                var table = comps.GetComponent<RandomGenComponent>().Tables[RandomGenTables.Quest];

                return table.Rarity / 1000 + 1;
            }

            bool ExtraFilter(EntityPrototype proto)
            {
                if (!proto.Components.HasComponent<QuestComponent>())
                    return false;

                var ev = new QuestCanGenerateEvent(proto);
                RaiseEvent(ev);
                return ev.OutCanGenerate;
            }

            return _randomGen.PickRandomEntityId(RandomGenTables.Quest, GetWeight, ExtraFilter);
        }

        private bool TryGetQuestHub(IMap map, [NotNullWhen(true)] out QuestHubData? questHub)
        {
            if (!TryArea(map.MapEntityUid, out var area) || !TryComp<AreaQuestsComponent>(area.AreaEntityUid, out var areaQuests))
            {
                questHub = null;
                return false;
            }

            return areaQuests.QuestHubs.TryGetValue(map.Id, out questHub);
        }

        public bool TryGenerateQuest(EntityUid client, [NotNullWhen(true)] out QuestComponent? quest)
        {
            if (!HasComp<QuestClientComponent>(client))
            {
                quest = null;
                return false;
            }

            Logger.DebugS("quest", $"Attempting to generate quest for client {client}.");

            var id = PickRandomQuestId();
            if (id == null)
            {
                quest = null;
                Logger.WarningS("quest", $"No valid quest ID selected for client {client}.");
                return false;
            }

            return TryCreateQuest(id.Value, client, out quest);
        }

        public bool TryCreateQuest(PrototypeId<EntityPrototype> questID, EntityUid client, out QuestComponent? quest)
        {
            var questProto = _protos.Index(questID);
            if (!questProto.Components.HasComponent<QuestComponent>())
            {
                Logger.ErrorS("quest", $"Entity prototype {questID} cannot be used as a quest since it is missing a {nameof(QuestComponent)}!");
                quest = null;
                return false;
            }

            if (!TryMap(client, out var map))
            {
                quest = null;
                return false;
            }

            var questEntity = EntityManager.SpawnEntity(questID, MapCoordinates.Global);
            quest = EnsureComp<QuestComponent>(questEntity);

            quest.ClientEntity = client;
            quest.ClientName = _displayNames.GetDisplayName(client);
            quest.ClientOriginatingMap = map.Id;
            quest.ClientOriginatingMapName = _displayNames.GetDisplayName(map.MapEntityUid);
            quest.State = QuestState.NotAccepted;
            quest.RandomSeed = _rand.Next();
            quest.TownBoardExpirationDate = _world.State.GameDate + GameTimeSpan.FromDays(3);

            var evDifficulty = new QuestCalcDifficultyEvent(quest);
            RaiseEvent(quest.Owner, evDifficulty);
            quest.Difficulty = evDifficulty.OutDifficulty;

            var ev = new QuestBeforeGenerateEvent(quest);
            RaiseEvent(quest.Owner, ev);
            if (ev.Cancelled)
            {
                Logger.WarningS("warning", $"Generation for quest of type {questID} ({quest.Owner}) was cancelled.");
                EntityManager.DeleteEntity(quest.Owner);
                quest = null;
                return false;
            }

            _quests.Add(quest.Owner);

            return true;
        }

        public string FormatQuestObjective(string name)
        {
            if (Loc.IsFullwidth())
                return name;

            return $"[{name}]";
        }

        private string LocalizeQuestRewardText(EntityUid quest, QuestComponent? questComp = null)
        {
            if (!Resolve(quest, ref questComp))
                return Loc.GetString("Elona.Quest.Rewards.Nothing");

            var ev = new QuestLocalizeRewardsEvent(questComp);
            RaiseEvent(quest, ev);

            if (ev.OutRewardNames.Count == 0)
                return Loc.GetString("Elona.Quest.Rewards.Nothing");

            var sb = new StringBuilder();

            for (var i = 0; i < ev.OutRewardNames.Count; i++)
            {
                var rewardName = ev.OutRewardNames[i];

                if (i > 0)
                {
                    if (i == ev.OutRewardNames.Count - 1)
                    {
                        sb.Append(Loc.Space);
                        sb.Append(Loc.GetString("Elona.Quest.Rewards.And"));
                        sb.Append(Loc.Space);
                    }
                    else
                    {
                        sb.Append(Loc.GetString("Elona.Quest.Rewards.Comma"));
                        sb.Append(Loc.Space);
                    }
                }

                sb.Append(rewardName);
            }

            return sb.ToString();
        }

        public QualifiedDialogNodeID TurnInQuest(EntityUid quest, EntityUid speaker)
        {
            var nextNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestClient, "Complete");
            if (!TryComp<QuestComponent>(quest, out var questComp))
                return nextNodeID;

            var rewardText = LocalizeQuestRewardText(quest);
            _mes.Display(Loc.GetString("Elona.Quest.Dialog.Complete.TakeReward", ("client", speaker), ("rewardText", rewardText)));

            var ev = new QuestCompletedEvent(questComp, nextNodeID);
            RaiseEvent(quest, ev);

            DeleteQuest(questComp);

            _saveLoad.QueueAutosave();

            return ev.OutNextDialogNodeID;
        }

        public void FailQuest(EntityUid quest)
        {
            if (!TryComp<QuestComponent>(quest, out var questComp))
                return;

            _mes.Display(Loc.GetString("Elona.Quest.FailedTakenFrom", ("clientName", questComp.ClientName)));

            var ev = new QuestFailedEvent(questComp);
            RaiseEvent(quest, ev);

            if (TryComp<FameComponent>(_gameSession.Player, out var fame))
            {
                var fameLost = _fame.DecrementFame(_gameSession.Player, 40);
                _mes.Display(Loc.GetString("Elona.Fame.Lose", ("fameLost", fameLost)), UiColors.MesRed);
            }

            DeleteQuest(questComp);
        }

        public Dictionary<string, object> GetQuestLocaleParams(EntityUid quest, EntityUid client, EntityUid player)
        {
            var localeParams = new Dictionary<string, object>();

            if (!TryComp<QuestComponent>(quest, out var questComp))
                return localeParams;

            localeParams["map"] = questComp.ClientOriginatingMapName;

            var rewardText = LocalizeQuestRewardText(quest);
            localeParams["reward"] = rewardText;

            var ev = new QuestLocalizeDataEvent(questComp, client, player, outParams: localeParams);
            RaiseEvent(quest, ev);

            return ev.OutParams;
        }

        public LocalizedQuestData LocalizeQuestData(EntityUid quest, EntityUid client, EntityUid player, QuestComponent? questComp = null)
        {
            if (!Resolve(quest, ref questComp))
                return new LocalizedQuestData("<unknown>", "<unknown>");

            var luaParams = GetQuestLocaleParams(quest, client, player);

            var variants = Loc.GetTable(questComp.LocaleKeyRoot);
            var keys = variants.Keys.Cast<object>().ToList();
            if (keys.Count == 0)
                return new LocalizedQuestData("<unknown>", "<unknown>");

            int variant = 0;
            _rand.WithSeed(questComp.RandomSeed, () => variant = _rand.Next() % keys.Count);
            var table = (LuaTable)variants[keys[variant]];

            var name = Loc.FormatRaw(table["Name"],
                ("client", client),
                ("player", player),
                ("params", luaParams));
            var desc = Loc.FormatRaw(table["Description"],
                ("client", client),
                ("player", player),
                ("params", luaParams));

            return new LocalizedQuestData(name, desc);
        }

        #endregion
    }

    public sealed record LocalizedQuestData(string Name, string Description);

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestLocalizeDataEvent : EntityEventArgs
    {
        public QuestComponent Quest { get; }
        public EntityUid Client { get; }
        public EntityUid Player { get; }

        public Dictionary<string, object> OutParams { get; }

        public QuestLocalizeDataEvent(QuestComponent quest, EntityUid client, EntityUid player, Dictionary<string, object> outParams)
        {
            Quest = quest;
            Client = client;
            Player = player;
            OutParams = outParams;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestCalcDifficultyEvent : EntityEventArgs
    {
        public QuestComponent Quest { get; }

        public int OutDifficulty { get; set; }

        public QuestCalcDifficultyEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestCanGenerateEvent : EntityEventArgs
    {
        public EntityPrototype QuestPrototype { get; }

        public bool OutCanGenerate { get; set; } = true;

        public QuestCanGenerateEvent(EntityPrototype questProto)
        {
            QuestPrototype = questProto;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestBeforeGenerateEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QuestBeforeGenerateEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestBeforeAcceptEvent : EntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QualifiedDialogNodeID OutNextDialogNodeID { get; set; }

        public QuestBeforeAcceptEvent(QuestComponent quest, QualifiedDialogNodeID outNextNode)
        {
            Quest = quest;
            OutNextDialogNodeID = outNextNode;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestCompletedEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QualifiedDialogNodeID OutNextDialogNodeID { get; set; }

        public QuestCompletedEvent(QuestComponent quest, QualifiedDialogNodeID outNextNode)
        {
            Quest = quest;
            OutNextDialogNodeID = outNextNode;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestFailedEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QuestFailedEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestTerminatingEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QuestTerminatingEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestLocalizeRewardsEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public List<string> OutRewardNames { get; } = new();

        public QuestLocalizeRewardsEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestCalcRewardsEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public int OutGold { get; set; }
        public int OutPlatinum { get; set; }
        public int OutItemCount { get; set; }

        public QuestCalcRewardsEvent(QuestComponent quest, int gold, int platinum, int itemCount)
        {
            Quest = quest;
            OutGold = gold;
            OutPlatinum = platinum;
            OutItemCount = itemCount;
        }
    }

    [EventUsage(EventTarget.Quest)]
    public sealed class QuestGenerateRewardsEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }
        public int Gold { get; }
        public int Platinum { get; }
        public int ItemCount { get; }

        public QuestGenerateRewardsEvent(QuestComponent quest, int gold, int platinum, int itemCount)
        {
            Quest = quest;
            Gold = gold;
            Platinum = platinum;
            ItemCount = itemCount;
        }
    }
}