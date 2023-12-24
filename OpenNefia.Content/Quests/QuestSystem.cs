using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using System.Text;
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
using OpenNefia.Core.Random;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Roles;
using OpenNefia.Content.World;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Game;
using OpenNefia.Content.Fame;
using OpenNefia.Content.UI;
using OpenNefia.Content.Parties;
using OpenNefia.Core;

namespace OpenNefia.Content.Quests
{
    public interface IQuestSystem : IEntitySystem
    {
        IEnumerable<QuestComponent> EnumerateAllQuestsForClient(EntityUid client);
        IEnumerable<(QuestComponent Quest, T QuestType)> EnumerateAllQuestsForClient<T>(EntityUid client)
            where T : class, IComponent;
        IEnumerable<QuestComponent> EnumerateAllQuests();
        IEnumerable<(QuestComponent Quest, T QuestType)> EnumerateAllQuests<T>()
            where T : class, IComponent;
        IEnumerable<QuestComponent> EnumerateAcceptedQuests();
        IEnumerable<(QuestComponent Quest, T QuestType)> EnumerateAcceptedQuests<T>()
            where T : class, IComponent;
        IEnumerable<QuestComponent> EnumerateAcceptedOrCompletedQuests();

        IEnumerable<(IArea Area, MapId MapId, QuestHubData QuestHub)> EnumerateQuestHubs();

        void UpdateInMap(IMap map);

        string FormatQuestObjective(string name);
        string FormatDeadlineText(GameTimeSpan? deadlineSpan);
        LocalizedQuestData LocalizeQuestData(EntityUid quest, EntityUid client, EntityUid player, QuestComponent? questComp = null);
        Dictionary<string, object> GetQuestLocaleParams(EntityUid quest, EntityUid player);
        IEnumerable<EntityUid> EnumerateTargetCharasInMap(IMap map, EntityUid quest, QuestComponent? questComp = null);
        IEnumerable<EntityUid> EnumerateTargetCharasInMap(MapId mapID, EntityUid quest, QuestComponent? questComp = null);

        QualifiedDialogNodeID TurnInQuest(EntityUid quest, EntityUid speaker, IDialogEngine? engine = null);
        void FailQuest(EntityUid quest, QuestComponent? questComp = null);
        void DeleteQuest(QuestComponent quest);

        int RoundDifficultyMargin(int a, int b);
        string LocalizeQuestRewardText(EntityUid quest, QuestComponent? questComp = null);
        string LocalizeQuestDetailText(EntityUid quest, EntityUid player, QuestComponent? questComp = null);
        bool IsReturnForbiddenByActiveQuest();
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
        [Dependency] private readonly IPartySystem _parties = default!;

        [RegisterSaveData("Elona.QuestSystem.Quests")]
        private List<EntityUid> _quests { get; set; } = new();

        #region API Methods

        public IEnumerable<QuestComponent> EnumerateAllQuestsForClient(EntityUid client)
        {
            return EnumerateAllQuests().Where(q => q.ClientEntity == client);
        }

        public IEnumerable<(QuestComponent Quest, T QuestType)> EnumerateAllQuestsForClient<T>(EntityUid client) where T : class, IComponent
        {
            return EnumerateAllQuests<T>().Where(pair => pair.Quest.ClientEntity == client);
        }

        public IEnumerable<QuestComponent> EnumerateAllQuests()
        {
            foreach (var quest in _quests)
            {
                if (TryComp<QuestComponent>(quest, out var questComp))
                    yield return questComp;
                else
                    Logger.ErrorS("quest", $"Entity {quest} does not have a {nameof(QuestComponent)}!");
            }
        }

        public IEnumerable<(QuestComponent Quest, T QuestType)> EnumerateAllQuests<T>() where T : class, IComponent
        {
            foreach (var quest in EnumerateAllQuests())
            {
                // TODO use EntityQuery<T> struct
                if (TryComp<T>(quest.Owner, out var questType))
                    yield return (quest, questType);
            }
        }

        public IEnumerable<QuestComponent> EnumerateAcceptedQuests()
        {
            return EnumerateAllQuests().Where(q => q.State == QuestState.Accepted);
        }

        public IEnumerable<(QuestComponent Quest, T QuestType)> EnumerateAcceptedQuests<T>() where T : class, IComponent
        {
            return EnumerateAllQuests<T>().Where(pair => pair.Quest.State == QuestState.Accepted);
        }

        public IEnumerable<QuestComponent> EnumerateAcceptedOrCompletedQuests()
        {
            return EnumerateAllQuests().Where(q => q.State == QuestState.Accepted || q.State == QuestState.Completed);
        }

        public IEnumerable<(IArea Area, MapId MapId, QuestHubData QuestHub)> EnumerateQuestHubs()
        {
            return _areaManager.LoadedAreas.Values.SelectMany(area =>
                {
                    if (!TryComp<AreaQuestsComponent>(area.AreaEntityUid, out var areaQuests))
                        return Enumerable.Empty<(IArea, MapId, QuestHubData)>();

                    return areaQuests.QuestHubs.Select(pair => (area, pair.Key, pair.Value));
                });
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

        // XXX: I don't like this but inserting QuestClientComponent into every prototype sounds tedious
        private bool CanBeQuestClient(EntityUid client)
        {
            if (_parties.IsInPlayerParty(client))
                return false;

            if (!HasComp<CharaComponent>(client))
                return false;

            if (_qualities.GetQuality(client) == Quality.Unique)
                return false;

            if (!_roles.HasAnyRoles(client)
                || HasComp<RoleSpecialComponent>(client)
                || HasComp<RoleAdventurerComponent>(client))
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
                foreach (var chara in _charas.EnumerateNonAllies(map))
                {
                    if (CanBeQuestClient(chara.Owner))
                    {
                        if (!questHub.Clients.ContainsKey(chara.Owner))
                        {
                            Logger.DebugS("quest", $"Registering quest client {chara.Owner}");
                            EnsureComp<QuestClientComponent>(chara.Owner);
                            questHub.Clients.Add(chara.Owner, new QuestClient(chara.Owner, _displayNames.GetDisplayName(chara.Owner)));
                        }
                    }
                    else
                    {
                        if (HasComp<QuestClientComponent>(chara.Owner))
                            EntityManager.RemoveComponent<QuestClientComponent>(chara.Owner);
                        questHub.Clients.Remove(chara.Owner);
                        foreach (var quest in EnumerateAllQuestsForClient(chara.Owner).ToList())
                        {
                            DeleteQuest(quest);
                        }
                    }
                }

                // Remove clients that do not exist in this map any longer.
                foreach (var client in questHub.Clients.Keys.ToList())
                {
                    if (!IsAlive(client))
                    {
                        Logger.DebugS("quest", $"Removing missing quest client {client}");
                        questHub.Clients.Remove(client);
                        foreach (var quest in EnumerateAllQuestsForClient(client).ToList())
                        {
                            DeleteQuest(quest);
                        }
                    }
                }

                // Generate quests for characters that need them.
                foreach (var client in questHub.Clients.Keys)
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
            return CanBeQuestClient(client) && !EnumerateAllQuestsForClient(client).Any();
        }

        private bool IsDeadQuest(QuestComponent quest)
        {
            if (quest.State == QuestState.NotAccepted)
                return _world.State.GameDate > quest.TownBoardExpirationDate;

            return false;
        }

        public void DeleteQuest(QuestComponent quest)
        {
            Logger.DebugS("quest", $"Terminating quest {quest.Owner} for client {quest.ClientEntity} ({quest.ClientName} in map {quest.ClientOriginatingMapID} ({quest.ClientOriginatingMapName})");

            var ev = new QuestTerminatingEvent(quest);
            RaiseEvent(quest.Owner, ev);

            _quests.Remove(quest.Owner);
            EntityManager.DeleteEntity(quest.Owner);
        }

        private void CleanDeadQuests(EntityUid client)
        {
            foreach (var quest in EnumerateAllQuestsForClient(client).Where(IsDeadQuest).ToList())
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

        public PrototypeId<EntityPrototype>? PickRandomQuestID(IMap map)
        {
            int GetWeight(EntityPrototype proto, int minLevel)
            {
                // Higher weight = greater chance
                return _randomGen.GetBaseRarity(proto, RandomGenTables.Quest, map).Rarity;
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
            if (!TryMap(client, out var map) || !HasComp<QuestClientComponent>(client))
            {
                quest = null;
                return false;
            }

            Logger.DebugS("quest", $"Attempting to generate quest for client {client}.");

            var id = PickRandomQuestID(map);
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

            if (!TryMap(client, out var map) || !TryGetQuestHub(map, out var questHub))
            {
                quest = null;
                return false;
            }

            var questEntity = EntityManager.SpawnEntity(questID, MapCoordinates.Global);
            quest = EnsureComp<QuestComponent>(questEntity);

            quest.ClientEntity = client;
            quest.ClientName = _displayNames.GetDisplayName(client);
            quest.ClientOriginatingMapID = map.Id;
            quest.ClientOriginatingMapName = _displayNames.GetDisplayName(map.MapEntityUid);
            quest.State = QuestState.NotAccepted;
            quest.RandomSeed = _rand.Next();
            quest.TownBoardExpirationDate = _world.State.GameDate + GameTimeSpan.FromDays(3);

            var evDifficulty = new QuestCalcDifficultyEvent(quest);
            RaiseEvent(quest.Owner, evDifficulty);
            quest.Difficulty = evDifficulty.OutDifficulty;

            var ev = new QuestBeforeGenerateEvent(quest, questHub, map);
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

        public string FormatDeadlineText(GameTimeSpan? deadlineSpan)
        {
            if (deadlineSpan == null)
                return Loc.GetString("Elona.Quest.Deadline.NoDeadline");
            else
                return Loc.GetString("Elona.Quest.Deadline.Days", ("deadlineDays", deadlineSpan.Value.TotalDays));
        }

        public string LocalizeQuestRewardText(EntityUid quest, QuestComponent? questComp = null)
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

        public const string QuestDetailTextParamKey = "detailText";

        public string LocalizeQuestDetailText(EntityUid quest, EntityUid player, QuestComponent? questComp = null)
        {
            if (!Resolve(quest, ref questComp))
                return string.Empty;

            var localeParams = GetQuestLocaleParams(quest, player);
            if (!localeParams.TryGetValue(QuestDetailTextParamKey, out var text))
                return string.Empty;

            return $"{text}";
        }

        public QualifiedDialogNodeID TurnInQuest(EntityUid quest, EntityUid speaker, IDialogEngine? engine = null)
        {
            var nextNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestClient, "Complete");
            if (!TryComp<QuestComponent>(quest, out var questComp))
                return nextNodeID;

            var rewardText = LocalizeQuestRewardText(quest);
            _mes.Display(Loc.GetString("Elona.Quest.Dialog.Complete.TakeReward", ("client", speaker), ("rewardText", rewardText)));

            var ev = new QuestCompletedEvent(questComp, nextNodeID, engine);
            RaiseEvent(quest, ev);

            DeleteQuest(questComp);

            _saveLoad.QueueAutosave();

            return ev.OutNextDialogNodeID;
        }

        public void FailQuest(EntityUid quest, QuestComponent? questComp = null)
        {
            if (!Resolve(quest, ref questComp))
                return;

            _mes.Display(Loc.GetString("Elona.Quest.FailedTakenFrom", ("clientName", questComp.ClientName)));

            var ev = new QuestFailedEvent(questComp);
            RaiseEvent(quest, ev);

            if (TryComp<FameComponent>(_gameSession.Player, out var fame))
            {
                var fameLost = _fame.DecrementFame(_gameSession.Player, 40, fame);
                _mes.Display(Loc.GetString("Elona.Fame.Lose", ("fameLost", fameLost)), UiColors.MesRed);
            }

            DeleteQuest(questComp);
        }

        public Dictionary<string, object> GetQuestLocaleParams(EntityUid quest, EntityUid player)
        {
            var localeParams = new Dictionary<string, object>();

            if (!TryComp<QuestComponent>(quest, out var questComp))
                return localeParams;

            localeParams["map"] = questComp.ClientOriginatingMapName;
            localeParams[QuestDetailTextParamKey] = string.Empty;

            var rewardText = LocalizeQuestRewardText(quest);
            localeParams["reward"] = rewardText;

            var ev = new QuestLocalizeDataEvent(questComp, player, outParams: localeParams);
            RaiseEvent(quest, ev);

            if (ev.OutDetailLocaleKey != null)
                localeParams[QuestDetailTextParamKey] = Loc.GetString(ev.OutDetailLocaleKey.Value, ("params", ev.OutParams));

            return ev.OutParams;
        }

        public LocalizedQuestData LocalizeQuestData(EntityUid quest, EntityUid client, EntityUid player, QuestComponent? questComp = null)
        {
            if (!Resolve(quest, ref questComp))
                return new LocalizedQuestData("<unknown>", "<unknown>");

            var luaParams = GetQuestLocaleParams(quest, player);

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

        public IEnumerable<EntityUid> EnumerateTargetCharasInMap(IMap map, EntityUid quest, QuestComponent? questComp = null)
         => EnumerateTargetCharasInMap(map.Id, quest, questComp);

        public IEnumerable<EntityUid> EnumerateTargetCharasInMap(MapId mapID, EntityUid quest, QuestComponent? questComp = null)
        {
            if (!Resolve(quest, ref questComp))
                yield break;

            var ev = new QuestGetTargetCharasEvent(questComp);
            RaiseEvent(quest, ev);

            foreach (var entity in ev.OutTargetCharas)
            {
                if (IsAlive(entity) && Spatial(entity).MapID == mapID)
                    yield return entity;
            }
        }

        public int RoundDifficultyMargin(int a, int b)
        {
            // >>>>>>>> elona122/shade2/init.hsp:4226 	#defcfunc roundMargin int a,int b ...
            if (a > b)
            {
                return a - _rand.Next(a - b);
            }
            else if (a < b)
            {
                return a + _rand.Next(b - a);
            }
            return a;
            // <<<<<<<< elona122/shade2/init.hsp:4229 	return a ...
        }

        #endregion
    }

    /// <summary>
    /// Localized name and description of a quest.
    /// </summary>
    public sealed record LocalizedQuestData(string Name, string Description);

    /// <summary>
    /// Retrieves localized data for this quest. These are passed into
    /// the quest's localization functions in Lua as a table of parameters.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestLocalizeDataEvent : EntityEventArgs
    {
        public QuestComponent Quest { get; }
        public EntityUid Player { get; }

        public Dictionary<string, object> OutParams { get; }
        public LocaleKey? OutDetailLocaleKey { get; set; }

        public QuestLocalizeDataEvent(QuestComponent quest, EntityUid player, Dictionary<string, object> outParams)
        {
            Quest = quest;
            Player = player;
            OutParams = outParams;
        }
    }

    /// <summary>
    /// Calculates the quest's difficulty. The parameters of some quests
    /// depend on their calculated difficulty; others set the difficulty
    /// based on the random parameters. Hence, not all quests have to use
    /// this event as the difficulty is only symbolic in the latter cases.
    /// </summary>
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

    /// <summary>
    /// You can use this event to prevent quests from being generated,
    /// for example based on fame calculation.
    /// </summary>
    // TODO merge with QuestBeforeGenerateEvent?
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

    /// <summary>
    /// Raised when this quest type is being considered for generation.
    /// In these event handlers, you should attempt to generate random
    /// parameters for the quest. If generation fails for some reason,
    /// you can use <see cref="CancellableEntityEventArgs.Cancel"/>.
    /// Otherwise, the quest will be made available in the quest board.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestBeforeGenerateEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }
        public QuestHubData QuestHub { get; }
        public IMap Map { get; }

        public QuestBeforeGenerateEvent(QuestComponent quest, QuestHubData questHub, IMap map)
        {
            Quest = quest;
            QuestHub = questHub;
            Map = map;
        }
    }

    /// <summary>
    /// When the player first accepts the quest from the quest giver, this event
    /// determines which dialog node to jump to next.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestBeforeAcceptEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QualifiedDialogNodeID OutNextDialogNodeID { get; set; }

        public QuestBeforeAcceptEvent(QuestComponent quest, QualifiedDialogNodeID outNextNode)
        {
            Quest = quest;
            OutNextDialogNodeID = outNextNode;
        }
    }

    /// <summary>
    /// Raised when the player completes a quest via
    /// <see cref="IQuestSystem.TurnInQuest(EntityUid, EntityUid, IDialogEngine?)"/>.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestCompletedEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QualifiedDialogNodeID OutNextDialogNodeID { get; set; }

        public IDialogEngine? DialogEngine { get; }

        public QuestCompletedEvent(QuestComponent quest, QualifiedDialogNodeID outNextNode, IDialogEngine? dialogEngine)
        {
            Quest = quest;
            OutNextDialogNodeID = outNextNode;
            DialogEngine = dialogEngine;
        }
    }

    /// <summary>
    /// Raised when the player fails a quest via
    /// <see cref="IQuestSystem.FailQuest(EntityUid, QuestComponent?)"/>.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestFailedEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QuestFailedEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    /// <summary>
    /// Raised when a quest is being deleted, either because it was completed,
    /// failed, or the town board deadline expired.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestTerminatingEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public QuestTerminatingEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }

    /// <summary>
    /// Determines how the quest client will talk about quest rewards.
    /// Example: The quest hands out some gold and a random supply item.
    /// This event will output something like ["1234 gold pieces", "ores"].
    /// </summary>
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

    /// <summary>
    /// Calculates gold, platinum and number of items to generate for quest rewards.
    /// </summary>
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

    /// <summary>
    /// Raised after quest rewards have been calculated and they're being handed out.
    /// Use this event to generate extra things like music tickets.
    /// </summary>
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

    /// <summary>
    /// Event for collecting the relevant target characters of this quest.
    /// This is used to add the "Where is [...]?" dialog options when speaking to guards.
    /// </summary>
    [EventUsage(EventTarget.Quest)]
    public sealed class QuestGetTargetCharasEvent : CancellableEntityEventArgs
    {
        public QuestComponent Quest { get; }

        public List<EntityUid> OutTargetCharas { get; } = new();

        public QuestGetTargetCharasEvent(QuestComponent quest)
        {
            Quest = quest;
        }
    }
}