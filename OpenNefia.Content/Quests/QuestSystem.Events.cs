using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.World;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Maps;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Fame;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Quests
{
    public sealed partial class QuestSystem
    {
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public override void Initialize()
        {
            SubscribeEntity<AfterAreaFloorGeneratedEvent>(HandleAddQuestHub);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestClient_GetDefaultDialogChoices);
            SubscribeBroadcast<QuestCanGenerateEvent>(FilterQuestsByPlayerFame);

            SubscribeComponent<QuestDeadlinesComponent, QuestBeforeGenerateEvent>(QuestDeadlines_BeforeGenerate, priority: EventPriorities.VeryHigh);

            SubscribeComponent<QuestComponent, QuestCompletedEvent>(Quest_Completed, priority: EventPriorities.VeryHigh);
            SubscribeComponent<QuestComponent, QuestGenerateRewardsEvent>(Quest_GenerateRewards, priority: EventPriorities.VeryHigh);
            SubscribeComponent<QuestRewardGoldComponent, QuestCalcRewardsEvent>(QuestRewardGold_CalcRewards, priority: EventPriorities.Highest);
            SubscribeComponent<QuestRewardRandomCategoryComponent, QuestGenerateRewardsEvent>(QuestRewardRandomCategory_GenerateRewards);
            SubscribeComponent<QuestRewardSingleCategoryComponent, QuestGenerateRewardsEvent>(QuestRewardSingleCategory_GenerateRewards);

            SubscribeComponent<QuestRewardGoldComponent, QuestLocalizeRewardsEvent>(QuestRewardGold_LocalizeRewards, priority: EventPriorities.Highest);
            SubscribeComponent<QuestRewardRandomCategoryComponent, QuestLocalizeRewardsEvent>(QuestRewardRandomCategory_LocalizeRewards);
            SubscribeComponent<QuestRewardSingleCategoryComponent, QuestLocalizeRewardsEvent>(QuestRewardSingleCategory_LocalizeRewards);
        }

        private void HandleAddQuestHub(EntityUid areaEntityUid, AfterAreaFloorGeneratedEvent ev)
        {
            UpdateQuestHubRegistration(ev.Map, ev.Area);
        }

        private void QuestClient_GetDefaultDialogChoices(EntityUid uid, QuestClientComponent component, GetDefaultDialogChoicesEvent args)
        {
            // TODO multiple quests per client
            foreach (var quest in EnumerateQuestsForClient(uid).Where(q => q.State == QuestState.NotAccepted))
            {
                args.OutChoices.Add(new DialogChoiceEntry()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Quest.Dialog.Choices.About"),
                    NextNode = new(Protos.Dialog.QuestClient, "About"),
                    ExtraData = new List<IDialogExtraData>()
                    {
                        new DialogQuestData(quest.Owner)
                    }
                });
            }
        }

        private void QuestDeadlines_BeforeGenerate(EntityUid uid, QuestDeadlinesComponent questDeadlines, QuestBeforeGenerateEvent args)
        {
            args.Quest.TownBoardExpirationDate = _world.State.GameDate + GameTimeSpan.FromDays(_rand.NextIntInRange(questDeadlines.TownBoardExpirationDays));

            if (questDeadlines.DeadlineDays != null)
                args.Quest.Deadline = _world.State.GameDate + GameTimeSpan.FromDays(_rand.NextIntInRange(questDeadlines.DeadlineDays.Value));
        }

        private void Quest_Completed(EntityUid uid, QuestComponent component, QuestCompletedEvent args)
        {
            CreateQuestRewards(component);

            var player = _gameSession.Player;
            _karma.ModifyKarma(player, 1);

            _mes.Display(Loc.GetString("Elona.Quest.CompletedTakenFrom", ("clientName", component.ClientName)), UiColors.MesGreen);

            if (TryComp<FameComponent>(_gameSession.Player, out var fame))
            {
                var baseFameGained = component.Difficulty * 3 + 10;
                var fameGained = _fame.CalcFameGained(player, baseFameGained);
                fame.Fame.Base += fameGained;
                _mes.Display(Loc.GetString("Elona.Fame.Gain", ("fameGained", fameGained)), UiColors.MesGreen);
            }

            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
            _audio.Play(Protos.Sound.Complete1);
        }

        private int CalcDefaultRewardItemCount()
        {
            return _rand.Next(_rand.Next(4) + 1) + 1;
        }

        private void CreateQuestRewards(QuestComponent quest)
        {
            var ev = new QuestCalcRewardsEvent(quest, 0, 1, CalcDefaultRewardItemCount());
            RaiseEvent(quest.Owner, ev);

            var ev2 = new QuestGenerateRewardsEvent(quest, ev.OutGold, ev.OutPlatinum, ev.OutItemCount);
            RaiseEvent(quest.Owner, ev2);
        }

        private void Quest_GenerateRewards(EntityUid uid, QuestComponent component, QuestGenerateRewardsEvent ev)
        {
            var player = _gameSession.Player;

            if (ev.Gold > 0)
                _itemGen.GenerateItem(player, Protos.Item.GoldPiece, amount: ev.Gold);
            if (ev.Platinum > 0)
                _itemGen.GenerateItem(player, Protos.Item.PlatinumCoin, amount: ev.Platinum);
        }

        private int CalcQuestRewardGoldAmount(EntityUid uid, QuestRewardGoldComponent component, QuestComponent quest)
        {
            var amount = ((quest.Difficulty + 3) * 100 + _rand.Next(quest.Difficulty * 30 + 200) + 400) * component.GoldModifier / 100;
            amount = amount * 100 / (100 + quest.Difficulty * 2 / 3);

            if (!component.ModifyGoldBasedOnPlayerLevel)
                return amount;

            return CalcQuestRewardGoldAmountFromPlayerLevel(uid, component, quest, amount);
        }

        private int CalcQuestRewardGoldAmountFromPlayerLevel(EntityUid uid, QuestRewardGoldComponent component, QuestComponent quest, int baseAmount)
        {
            var level = _levels.GetLevel(_gameSession.Player);
            if (level >= quest.Difficulty)
            {
                return baseAmount * 10 / (100 + (level - quest.Difficulty) * 10);
            }
            else
            {
                return baseAmount * (100 + Math.Clamp((quest.Difficulty - level) / 5 * 25, 0, 200)) / 100;
            }
        }

        private void QuestRewardGold_CalcRewards(EntityUid uid, QuestRewardGoldComponent component, QuestCalcRewardsEvent args)
        {
            args.OutGold = CalcQuestRewardGoldAmount(uid, component, args.Quest);
        }

        private void QuestRewardRandomCategory_GenerateRewards(EntityUid uid, QuestRewardRandomCategoryComponent component, QuestGenerateRewardsEvent args)
        {
            for (var i = 0; i < args.ItemCount; i++)
            {
                var tag = _randomGen.PickTag(component.ItemCategories);
                ItemFilter filter = GetQuestRewardItemFilter(args.Quest, tag);
                _itemGen.GenerateItem(_gameSession.Player, filter);
            }
        }

        private void QuestRewardSingleCategory_GenerateRewards(EntityUid uid, QuestRewardSingleCategoryComponent component, QuestGenerateRewardsEvent args)
        {
            for (var i = 0; i < args.ItemCount; i++)
            {
                ItemFilter filter = GetQuestRewardItemFilter(args.Quest, component.ItemCategory);
                _itemGen.GenerateItem(_gameSession.Player, filter);
            }
        }

        private void QuestRewardGold_LocalizeRewards(EntityUid uid, QuestRewardGoldComponent component, QuestLocalizeRewardsEvent args)
        {
            var goldAmount = CalcQuestRewardGoldAmount(uid, component, args.Quest);
            args.OutRewardNames.Add(Loc.GetString("Elona.Quest.Rewards.GoldPieces", ("amount", goldAmount)));
        }

        private void QuestRewardRandomCategory_LocalizeRewards(EntityUid uid, QuestRewardRandomCategoryComponent component, QuestLocalizeRewardsEvent args)
        {
            args.OutRewardNames.Add(Loc.GetPrototypeString(component.ItemCategories, "Name"));
        }

        private void QuestRewardSingleCategory_LocalizeRewards(EntityUid uid, QuestRewardSingleCategoryComponent component, QuestLocalizeRewardsEvent args)
        {
            args.OutRewardNames.Add(Loc.GetPrototypeString(component.ItemCategory, "Name"));
        }

        private ItemFilter GetQuestRewardItemFilter(QuestComponent quest, PrototypeId<TagPrototype> itemCategory)
        {
            var quality = Quality.Normal;

            if (_rand.OneIn(2))
            {
                quality = Quality.Good;

                if (_rand.OneIn(12))
                {
                    quality = Quality.Great;
                }
            }

            var filter = new ItemFilter()
            {
                MinLevel = quest.Difficulty + _levels.GetLevel(_gameSession.Player) / 2 + 1,
                Quality = _randomGen.CalcObjectQuality(quality),
                Tags = new[] { itemCategory }
            };

            return filter;
        }
    }
}
