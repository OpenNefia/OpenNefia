using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Fishing;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Home;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Items;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.DisplayName
{
    public sealed partial class ItemNameSystem
    {
        [Dependency] private readonly IRandomItemSystem _randomItems = default!;

        // TODO split up across entity systems
        private bool ItemNameSub(ref StringBuilder fullName, EntityUid uid, bool isJapanese)
        {
            var skip = false;

            // TODO recipe

            if (TryComp<FoodComponent>(uid, out var food))
            {
                if (_food.IsCooked(uid, food))
                {
                    skip = true;

                    if (TryComp<FishComponent>(uid, out var fish))
                    {
                        var fishName = Loc.GetPrototypeString(fish.FishID, "Name");
                        fullName.Append(Loc.GetPrototypeString(food.FoodType!.Value, $"Names.{food.FoodQuality}", ("origin", fishName)));
                    }
                    else
                    {
                        fullName.Append(_food.GetFoodName(uid, food));
                    }
                    return skip;
                }

                if (TryComp<HarvestedFoodComponent>(uid, out var harvested))
                {
                    var weight = Loc.GetString($"Elona.Food.Harvesting.Weight.{harvested.WeightClass}");
                    fullName.Append(Loc.Space() + Loc.GetString("Elona.Food.Harvesting.ItemName.Grown", ("weight", weight)));
                }
            }
            else if (TryComp<FishComponent>(uid, out var fish))
            {
                var fishName = Loc.GetPrototypeString(fish.FishID, "Name");
                fullName.Append(fishName);
            }

            if (TryComp<ItemFromEntityComponent>(uid, out var fromEntity) && CompOrNull<PickableComponent>(uid)?.OwnState != OwnState.Quest)
            {
                var name = Loc.GetPrototypeString(fromEntity.EntityID, "MetaData.Name");
                if (!isJapanese)
                {
                    fullName.Append(" of ");
                }
                fullName.Append(name);
                if (isJapanese)
                {
                    fullName.Append("の");
                }
            }

            if (isJapanese && TryComp<FurnitureComponent>(uid, out var furniture) && furniture.FurnitureQuality > 0)
            {
                fullName.Append(Loc.GetString($"Elona.Furniture.Qualities.{furniture.FurnitureQuality}"));
            }

            if (TryComp<DeedComponent>(uid, out var deed))
            {
                var homeName = Loc.GetPrototypeString(deed.HomeID, "Name");
                fullName.Append(Loc.Space() + Loc.GetString($"Elona.Home.ItemName.Deed", ("homeName", homeName)));
            }

            if (TryComp<BillComponent>(uid, out var bill))
{
                fullName.Append(Loc.Space() + Loc.GetString($"Elona.Salary.ItemName.Bill", ("amount", bill.GoldOwed)));
            }

            if (TryComp<SecretTreasureComponent>(uid, out var secretTreasure))
            {
                var s = Loc.GetPrototypeString(secretTreasure.FeatID, "SecretTreasureName", ("itemName", fullName.ToString()));
                fullName = new StringBuilder(s);
            }

            return skip;
        }

        private (int, int) GetDice(EntityUid uid)
        {
            if (TryComp<WeaponComponent>(uid, out var weapon))
            {
                return (weapon.DiceX, weapon.DiceY);
            }
            else if (TryComp<AmmoComponent>(uid, out var ammo))
            {
                return (ammo.DiceX, ammo.DiceY);
            }

            return (0, 0);
        }

        private string GetItemKnownInfo(EntityUid uid)
        {
            var knownInfo = new StringBuilder();

            if (TryComp<BonusComponent>(uid, out var bonus) && bonus.Bonus != 0)
            {
                var sign = bonus.Bonus >= 0 ? "+" : "";
                knownInfo.Append($"{sign}{bonus.Bonus}");
            }

            if (TryComp<ChargedComponent>(uid, out var charged) && charged.DisplayChargeCount)
            {
                knownInfo.Append(Loc.Space() + Loc.GetString("Elona.Charged.ItemName.Charges", ("charges", charged.Charges)));
            }

            var (diceX, diceY) = GetDice(uid);
            var equipStats = CompOrNull<EquipStatsComponent>(uid);
            var hitBonus = equipStats?.HitBonus.Buffed ?? 0;
            var damageBonus = equipStats?.DamageBonus.Buffed ?? 0;

            if (diceX != 0 || diceY != 0 || hitBonus != 0 || damageBonus != 0)
            {
                knownInfo.Append(" (");
                if (diceX != 0 || diceY != 0)
                {
                    knownInfo.Append($"{diceX}d{diceY}");

                    if (damageBonus != 0)
                    {
                        var sign = damageBonus >= 0 ? "+" : "";
                        knownInfo.Append($"{sign}{damageBonus}");
                    }

                    knownInfo.Append(")");

                    if (hitBonus != 0)
                    {
                        knownInfo.Append($"({hitBonus})");
                    }
                }
                else
                {
                    knownInfo.Append($"{hitBonus},{damageBonus})");
                }
            }

            var dv = equipStats?.DV.Buffed ?? 0;
            var pv = equipStats?.PV.Buffed ?? 0;
            if (pv != 0 || dv != 0)
            {
                knownInfo.Append($" [{dv},{pv}]");
            }

            return knownInfo.ToString();
        }

        public string GetUnidentifiedName(EntityUid uid, int? seed = null)
        {
            var unidentifiedName = CompOrNull<IdentifyComponent>(uid)?.UnidentifiedName;
            if (unidentifiedName != null)
                return unidentifiedName;

            if (TryComp<RandomItemComponent>(uid, out var randomItem))
            {
                var name = Loc.GetString($"Elona.RandomItem.Kinds.{randomItem.KnownNameRef}.Name");
                var index = _randomItems.GetRandomEntityIndex(uid, seed);
                var adjective = "???";

                if (Loc.TryGetTable($"Elona.RandomItem.Kinds.{randomItem.KnownNameRef}.Adjectives", out var adjectives))
                {
                    adjective = adjectives[(index % adjectives.Keys.Count).ToString()].ToString();
                }

                return $"{adjective}{Loc.Space()}{name}";
            }

            return MetaData(uid).DisplayName ?? "<???>";
        }
    }
}
