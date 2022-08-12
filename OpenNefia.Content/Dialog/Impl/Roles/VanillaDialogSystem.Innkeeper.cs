using NetVips;
using OpenNefia.Content.Activity;
using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.Weather;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        [Dependency] private readonly IWeatherSystem _weather = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;
        [Dependency] private readonly IFoodSystem _food = default!;

        private void Innkeeper_Initialize()
        {
            SubscribeComponent<RoleInnkeeperComponent, GetDefaultDialogChoicesEvent>(Innkeeper_AddDialogChoices, priority: EventPriorities.High);
        }

        private void Innkeeper_AddDialogChoices(EntityUid uid, RoleInnkeeperComponent component, GetDefaultDialogChoicesEvent args)
        {
            var textBuyMeal = Loc.GetString("Elona.Dialog.Innkeeper.Choices.BuyMeal") + $" ({CalcInnkeeperMealCost()} {Loc.GetString("Elona.Currency.Gold.Pieces")})";
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromString(textBuyMeal),
                NextNode = new(Protos.Dialog.Innkeeper, "BuyMeal")
            });

            if (_weather.IsBadWeather())
            {
                args.OutChoices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Innkeeper.Choices.GoToShelter"),
                    NextNode = new(Protos.Dialog.Innkeeper, "GoToShelter")
                });
            }
        }

        private int CalcInnkeeperMealCost()
        {
            return 140;
        }

        public QualifiedDialogNode? Innkeeper_BuyMeal(IDialogEngine engine, IDialogNode node)
        {
            var cost = CalcInnkeeperMealCost();
            if (!TryComp<MoneyComponent>(engine.Player, out var wallet) || wallet.Gold < cost)
            {
                _mes.Display(Loc.GetString("Elona.Inventory.Behavior.Buy.NotEnoughMoney"));
                return engine.GetNodeByID(Protos.Dialog.Default, "Talk");
            }

            if (!TryComp<HungerComponent>(engine.Player, out var hunger) || hunger.Nutrition >= HungerLevels.InnkeeperMeal)
                return engine.GetNodeByID(Protos.Dialog.Innkeeper, "BuyMealNotHungry");

            _audio.Play(Protos.Sound.Paygold1);
            wallet.Gold -= cost;
            _audio.Play(Protos.Sound.Eat1);

            hunger.Nutrition = HungerLevels.InnkeeperMeal;
            _mes.Display(Loc.GetString("Elona.Dialog.Innkeeper.BuyMeal.Results"));
            _mes.Display(_food.GetNutritionMessage(hunger.Nutrition));
            _hunger.VomitIfAnorexic(engine.Player);

            return engine.GetNodeByID(Protos.Dialog.Innkeeper, "BuyMealFinish");
        }

        public void Innkeeper_GoToShelter(IDialogEngine engine, IDialogNode node)
        {
            _mes.Display("TODO", UiColors.MesYellow);
        }
    }
}