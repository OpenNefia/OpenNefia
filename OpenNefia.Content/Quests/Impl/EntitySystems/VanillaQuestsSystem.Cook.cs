using OpenNefia.Content.Dialog;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Food;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        private void Initialize_Cook()
        {
            SubscribeComponent<QuestTypeCookComponent, QuestLocalizeDataEvent>(QuestCook_Localize);
            SubscribeComponent<QuestTypeCookComponent, QuestCalcDifficultyEvent>(QuestCook_CalcDifficulty);
            SubscribeComponent<QuestTypeCookComponent, QuestBeforeGenerateEvent>(QuestCook_BeforeGenerate);

            SubscribeComponent<QuestClientComponent, GetDefaultDialogChoicesEvent>(QuestCook_AddGiveDialogChoices);
        }

        private void QuestCook_Localize(EntityUid uid, QuestTypeCookComponent component, QuestLocalizeDataEvent args)
        {
            var ingredient = Loc.GetPrototypeString(component.TargetFoodType, "DefaultOrigin");
            var foodName = Loc.GetPrototypeString(component.TargetFoodType, $"Names.{component.TargetFoodQuality}", ("ingredient", ingredient));
            args.OutParams["foodName"] = _quests.FormatQuestObjective(foodName);
            args.OutDetailLocaleKey = "Elona.Quest.Types.Cook.Detail";
        }

        private void QuestCook_CalcDifficulty(EntityUid uid, QuestTypeCookComponent component, QuestCalcDifficultyEvent args)
        {
            // Calculated in generate
            args.OutDifficulty = 0;
        }

        private void QuestCook_BeforeGenerate(EntityUid uid, QuestTypeCookComponent component, QuestBeforeGenerateEvent args)
        {
            var foodType = _rand.Pick(_protos.EnumeratePrototypes<FoodTypePrototype>().ToList());
            var rewardCategory = foodType.QuestRewardCategory;

            if (rewardCategory != null)
            {
                EnsureComp<QuestRewardSingleCategoryComponent>(uid).ItemCategory = rewardCategory.Value;
            }
            else
            {
                Logger.DebugS("quest.cook", $"No quest reward category for food type {foodType}, using default");
                EnsureComp<QuestRewardRandomCategoryComponent>(uid).ItemCategories = Protos.TagSet.ItemRewardSupply;
            }

            var foodQuality = _rand.Next(7) + 3;
            args.Quest.Difficulty = foodQuality * 3;
            EnsureComp<QuestRewardGoldComponent>(uid).GoldModifier = 60 + args.Quest.Difficulty;

            args.Quest.LocaleKeyRoot = new LocaleKey("Elona.Quest.Types.Cook.Variants.FoodType").With(foodType.ID);

            component.TargetFoodType = foodType.GetStrongID();
            component.TargetFoodQuality = foodQuality;
        }

        private bool TryGetCookRequestItem(EntityUid player, QuestTypeCookComponent questCook, [NotNullWhen(true)] out FoodComponent? item)
        {
            item = _inv.EnumerateInventory(player)
                .Where(ent =>
                {
                    if (!TryComp<FoodComponent>(ent, out var food))
                        return false;

                    return food.FoodType == questCook.TargetFoodType && food.FoodQuality == questCook.TargetFoodQuality;
                })
                .Select(ent => EntityManager.GetComponent<FoodComponent>(ent))
                .FirstOrDefault();
            return item != null;
        }

        /// <summary>
        /// Add the "give item" choice if the target item is in the player's inventory.
        /// </summary>
        private void QuestCook_AddGiveDialogChoices(EntityUid uid, QuestClientComponent component, GetDefaultDialogChoicesEvent args)
        {
            foreach (var (quest, questCook) in _quests.EnumerateAllQuestsForClient<QuestTypeCookComponent>(args.Speaker))
            {
                if (TryGetCookRequestItem(args.Player, questCook, out var item))
                {
                    var choiceExtraData = new DialogQuestGiveItemData(quest.Owner, item.Owner);
                    choiceExtraData.CheckPoison = true;
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromString(Loc.GetString("Elona.Quest.Dialog.Choices.Give", ("item", item.Owner))),
                        NextNode = new(Protos.Dialog.QuestCommon, "Give"),
                        ExtraData = new List<IDialogExtraData>() { choiceExtraData }
                    });
                }
            }
        }
    }
}