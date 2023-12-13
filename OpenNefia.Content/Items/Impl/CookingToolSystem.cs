using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Quests;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Food;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Skills;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Items.Impl
{
    public interface ICookingToolSystem : IEntitySystem
    {
        void Cook(EntityUid cooker, EntityUid item, EntityUid tool, FoodComponent? foodComp = null, CookingToolComponent? cookingTool = null);
    }

    public sealed class CookingToolSystem : EntitySystem, ICookingToolSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IFoodSystem _foods = default!;

        public override void Initialize()
        {
            SubscribeComponent<CookingToolComponent, GetVerbsEventArgs>(CookingTool_GetVerbs);
        }

        private void CookingTool_GetVerbs(EntityUid uid, CookingToolComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Use Cooking Tool", () => CookingTool_Use(args.Source, args.Target, component)));
        }

        private TurnResult CookingTool_Use(EntityUid source, EntityUid target, CookingToolComponent component)
        {
            if (!TryMap(target, out var map))
            {
                return TurnResult.Aborted;
            }

            var context = new InventoryContext(source, target, new CookInventoryBehavior());
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

            if (!result.HasValue || !EntityManager.IsAlive(result.Value.SelectedItem))
                return TurnResult.Aborted;

            Cook(source, result.Value.SelectedItem!.Value, target);

            return TurnResult.Succeeded;
        }

        public void Cook(EntityUid cooker, EntityUid food, EntityUid tool, FoodComponent? foodComp = null, CookingToolComponent? cookingTool = null)
        {
            if (!Resolve(food, ref foodComp) || !Resolve(tool, ref cookingTool))
                return;

            _audio.Play(Protos.Sound.Cook1, food);
            var cooking = _skills.Level(cooker, Protos.Skill.Cooking);

            if (!_stacks.TrySplit(food, 1, out var split))
                return;

            var oldFoodName = _displayNames.GetDisplayName(split);

            var toolQuality = cookingTool.Quality;
            var foodQuality = int.Min(_rand.Next(cooking + 6) + _rand.Next(toolQuality / 50 + 1), cooking / 5 + 7);
            foodQuality = _rand.Next(foodQuality + 1);

            if (foodQuality > 3)
                foodQuality = _rand.Next(foodQuality);

            if (cooking >= 5 && foodQuality < 3 && _rand.OneIn(4))
                foodQuality = 3;

            if (cooking >= 10 && foodQuality < 3 && _rand.OneIn(3))
                foodQuality = 3;

            foodQuality = int.Clamp(foodQuality + toolQuality / 100, 1, 9);
            _foods.MakeDish(split, foodQuality);

            _mes.Display(Loc.GetString("Elona.Food.Cook", ("oldFoodName", oldFoodName), ("toolEntity", tool), ("newFoodEntity", split)));

            var newFoodQuality = EnsureComp<FoodComponent>(split).FoodQuality;
            if (newFoodQuality > 2)
                _skills.GainSkillExp(cooker, Protos.Skill.Cooking, 30 + newFoodQuality * 5);
        }
    }
}