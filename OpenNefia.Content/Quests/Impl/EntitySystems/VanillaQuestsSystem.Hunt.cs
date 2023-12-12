using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly IMusicManager _music = default!;

        private void Initialize_Hunt()
        {
            SubscribeComponent<QuestTypeHuntComponent, QuestCalcDifficultyEvent>(QuestHunt_CalcDifficulty);
            SubscribeComponent<QuestTypeHuntComponent, QuestBeforeGenerateEvent>(QuestHunt_BeforeGenerate);
            SubscribeComponent<QuestTypeHuntComponent, QuestBeforeAcceptEvent>(QuestHunt_BeforeAccept);
            SubscribeComponent<MapImmediateQuestComponent, MapQuestTargetsEliminatedEvent>(QuestHunt_TargetsEliminated);
        }

        private void QuestHunt_CalcDifficulty(EntityUid uid, QuestTypeHuntComponent collectQuest, QuestCalcDifficultyEvent args)
        {
            var playerLevel = _levels.GetLevel(_gameSession.Player);
            var playerFame = _fame.GetFame(_gameSession.Player);
            var difficulty = int.Clamp(_rand.Next(playerLevel + 10) + _rand.Next(playerFame / 500) + 1, 1, 80);
            args.OutDifficulty = _quests.RoundDifficultyMargin(difficulty, playerLevel);
        }

        private void QuestHunt_BeforeGenerate(EntityUid uid, QuestTypeHuntComponent collectQuest, QuestBeforeGenerateEvent args)
        {
            if (!TryMap(args.Quest.ClientEntity, out var map))
            {
                args.Cancel();
                return;
            }
        }

        private void QuestHunt_BeforeAccept(EntityUid uid, QuestTypeHuntComponent huntQuest, QuestBeforeAcceptEvent args)
        {
            args.OutNextDialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestHunt, "Accept");
        }

        public void QuestHunt_TravelToMap(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();

            PrototypeId<MapTilesetPrototype>? tileset = null;
            if (TryArea(engine.Speaker!.Value, out var area) && AreaIsNoyel(area))
                tileset = Protos.MapTileset.DungeonForestNoyel;

            // TODO: It would be nice to override the map generation routine based on the current area, etc.
            var partyMap = QuestMap_GenerateHunt(quest.Quest.Difficulty, tileset);

            var spatial = Spatial(_gameSession.Player);
            var prevLocation = MapEntrance.FromMapCoordinates(spatial.MapPosition);

            _immediateQuests.SetImmediateQuest(partyMap, quest.Quest, prevLocation);
            _mapEntrances.SetPreviousMap(partyMap, spatial.MapPosition);

            _mapTransfer.DoMapTransfer(spatial, partyMap, new CenterMapLocation());
        }

        public const string QuestHuntTargetTagId = "Elona.QuestHunt";

        private void QuestHunt_TargetsEliminated(EntityUid uid, MapImmediateQuestComponent component, MapQuestTargetsEliminatedEvent args)
        {
            CheckHuntQuest<QuestTypeHuntComponent>(uid, args.Tag, QuestHuntTargetTagId);
        }

        private void CheckHuntQuest<T>(EntityUid uid, string tag, string requiredTag) where T: class, IComponent
        {
            if (tag != requiredTag
                || !TryMap(uid, out var map)
                || !_immediateQuests.TryGetImmediateQuest<T>(map, out var quest, out _, out _))
                return;

            HuntQuestAreaIsSecure(quest);
        }

        private void HuntQuestAreaIsSecure(QuestComponent quest)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:420 	call *music_play,(music=mcFanfare,musicLoop=1) ...
            _music.Play(Protos.Music.Fanfare, loop: false);
            quest.State = QuestState.Completed; // Now the player may leave the map without penalty.
            _mes.Display(Loc.GetString("Elona.Quest.Eliminate.Complete"), color: UiColors.MesGreen);
            // <<<<<<<< elona122/shade2/quest.hsp:421 	gQuestStatus=qSuccess ...
        }
    }
}