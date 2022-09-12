using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Food;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Items;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Chests;
using System.Text.RegularExpressions;
using OpenNefia.Content.Fishing;
using OpenNefia.Content.Materials;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Items
{
    public sealed partial class ItemNameSystem
    {
        public string ItemNameEN(EntityUid uid,
            int? amount = null, 
            bool noArticle = false,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return $"<item {uid}>";

            var basename = meta.DisplayName ?? "(null)";
            var quality = CompOrNull<QualityComponent>(uid)?.Quality.Buffed ?? Quality.Bad;
            var identify = CompOrNull<IdentifyComponent>(uid)?.IdentifyState ?? IdentifyState.None;
            var curse = CompOrNull<CurseStateComponent>(uid)?.CurseState ?? CurseState.Normal;
            var materialID = CompOrNull<MaterialComponent>(uid)?.MaterialID;
            var enchantments = CompOrNull<EnchantmentsComponent>(uid);
            amount ??= _stacks.GetCount(uid);

            var food = CompOrNull<FoodComponent>(uid);
            var isCookedDish = _food.IsCooked(uid, food);

            var fullName = new StringBuilder();

            // >>>>>>>> shade2/item_func.hsp:495 		}else{	 ..
            if (identify >= IdentifyState.Full)
            {
                switch (curse)
                {
                    case CurseState.Blessed:
                        fullName.Append(Loc.GetString("Elona.CurseState.ItemName.Blessed") + Loc.Space);
                        break;
                    case CurseState.Cursed:
                        fullName.Append(Loc.GetString("Elona.CurseState.ItemName.Cursed") + Loc.Space);
                        break;
                    case CurseState.Doomed:
                        fullName.Append(Loc.GetString("Elona.CurseState.ItemName.Doomed") + Loc.Space);
                        break;
                }
            }

            // a <cargo/pair/dish> of [...]
            var typeName = "";

            // a dish <with/of> [...]
            var preposition = "";

            if (!(HasComp<RandomItemComponent>(uid) && identify < IdentifyState.Name))
            {
                typeName = item.ItemTypeName;
                preposition = item.ItemPreposition ?? "of";

                if (identify > IdentifyState.None && string.IsNullOrEmpty(typeName))
                {
                    if (HasComp<CargoComponent>(uid))
                    {
                        typeName = "cargo";
                    }
                    if (_tags.HasTag(uid, Protos.Tag.ItemCatEquipWrist) || _tags.HasTag(uid, Protos.Tag.ItemCatEquipLeg))
                    {
                        typeName = "pair";
                    }
                }

                if (isCookedDish)
                {
                    typeName = "dish";
                }
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                if (amount > 1)
                {
                    // [3] [blessed ][pair]s [of]
                    fullName.Insert(0, amount.ToString() + " ");
                    fullName.Append($"{typeName}s {preposition} ");
                }
                else
                {
                    // [blessed ][pair] [of]
                    fullName.Append($"{typeName} {preposition} ");
                }
            }
            else
            {
                if (amount > 1)
                {
                    // [3] [blessed ]
                    fullName.Insert(0, amount.ToString() + " ");
                }
            }

            if (food != null && food.IsRotten)
            {
                fullName.Append(Loc.GetString("Elona.Food.ItemName.Rotten") + Loc.Space);
            }
            // <<<<<<<< shade2/item_func.hsp:521 		} ..

            // >>>>>>>> shade2/item_func.hsp:527 	if en@{ ..
            var skipDetail = false;
            if (isCookedDish)
                skipDetail = true;

            if (TryComp<FurnitureComponent>(uid, out var furniture) && furniture.FurnitureQuality > 0)
            {
                fullName.Append(Loc.GetString($"Elona.Furniture.ItemName.Qualities.{furniture.FurnitureQuality}") + " ");
            }
            // TODO recipe
            // <<<<<<<< shade2/item_func.hsp:534 	} ..

            // >>>>>>>> shade2/item_func.hsp:545 	if iId(id)=idMaterialKit:s+=""+mtName@(0,iMateria ..
            if (HasComp<MaterialKitComponent>(uid))
            {
                if (materialID != null)
                {
                    var materialName = Loc.GetPrototypeString(materialID.Value, "Name");
                    fullName.Append(Loc.GetString("Elona.MaterialKit.ItemName.Name", ("materialName", materialName)) + Loc.Space);
                }
                else
                {
                    fullName.Append("???" + Loc.Space);
                }
            }
            // <<<<<<<< shade2/item_func.hsp:546 			 ...

            // >>>>>>>> shade2/item_func.hsp:549 	if a=fltFurniture{ ..
            if (HasComp<FurnitureComponent>(uid) && materialID != null)
            {
                var matName = Loc.GetPrototypeString(materialID.Value, "Name");
                fullName.Append(Loc.GetString("Elona.Furniture.ItemName.Work", ("matName", matName)) + Loc.Space);
            }

            if (TryComp<GiftComponent>(uid, out var gift))
            {
                fullName.Append(Loc.GetString($"Elona.Gift.ItemName.Ranks.{gift.GiftRank}") + Loc.Space);
            }

            if (!skipDetail)
            {
                if (identify >= IdentifyState.Full && HasComp<EquipmentComponent>(uid))
                {
                    if (CompOrNull<EternalForceComponent>(uid)?.IsEternalForce.Buffed == true)
                    {
                        fullName.Append(Loc.GetString("Elona.Item.ItemName.EternalForce") + Loc.Space);
                    }
                    else
                    {
                        if (enchantments != null && enchantments.EgoMinorEnchantment != null)
                        {
                            var s = Loc.GetPrototypeString(enchantments.EgoMinorEnchantment.Value, "NameModifier", ("name", fullName.ToString()));
                            fullName = new StringBuilder(s);
                        }

                        if (materialID != null)
                        {
                            if (quality != Quality.Unique && quality >= Quality.Great)
                            {
                                fullName.Append(Loc.GetPrototypeString(materialID.Value, "Alias") + Loc.Space);
                            }
                            else
                            {
                                fullName.Append(Loc.GetPrototypeString(materialID.Value, "Name") + Loc.Space);
                            }
                        }
                    }
                }

                var unidentifiedName = GetUnidentifiedName(uid);

                switch (identify)
                {
                    case IdentifyState.None:
                        fullName.Append(unidentifiedName);
                        break;
                    case IdentifyState.Name:
                    case IdentifyState.Quality:
                        if (quality < Quality.Great && !HasComp<EquipmentComponent>(uid))
                        {
                            fullName.Append(basename);
                        }
                        else
                        {
                            fullName.Append(unidentifiedName);
                        }
                        break;
                    case IdentifyState.Full:
                    default:
                        if (quality == Quality.Unique || item.IsPrecious)
                        {
                            fullName.Append(basename);
                        }
                        else
                        {
                            if (HasComp<EquipmentComponent>(uid) && enchantments != null && enchantments.EgoMajorEnchantment != null)
                            {
                                var s = Loc.GetPrototypeString(enchantments.EgoMajorEnchantment.Value, "NameModifier", ("name", fullName.ToString()));
                                fullName = new StringBuilder(s);
                            }
                            else
                            {
                                fullName.Append(basename);
                            }

                            if (TryComp<AliasComponent>(uid, out var alias) && !string.IsNullOrWhiteSpace(alias.Alias))
                            {
                                if (quality == Quality.Great)
                                {
                                    fullName.Append(Loc.GetString("Elona.Quality.Brackets.Great", ("name", alias.Alias)));
                                }
                                else
                                {
                                    fullName.Append(Loc.GetString("Elona.Quality.Brackets.God", ("name", alias.Alias)));
                                }
                            }
                        }
                        break;
                }
            }
            // <<<<<<<< shade2/item_func.hsp:605 *skipName ..

            // >>>>>>>> shade2/item_func.hsp:606 	if en@{ ..
            if (string.IsNullOrEmpty(typeName) && !HasComp<FishComponent>(uid) && amount > 1)
            {
                fullName.Append("s");
            }

            ItemNameSub(ref fullName, uid, isJapanese: false);

            if (identify >= IdentifyState.Full)
            {
                fullName.Append(GetItemKnownInfo(uid));
            }
            // <<<<<<<< shade2/item_func.hsp:615 		} ..

            var ev = new LocalizeItemNameExtraEvent(fullName);
            RaiseEvent(uid, ref ev);

            // >>>>>>>> shade2/item_func.hsp:640 	if iId(id)=idFishingPole{ ..
            if (identify == IdentifyState.Quality && HasComp<EquipmentComponent>(uid))
            {
                if (materialID != null)
                {
                    var materialName = Loc.GetPrototypeString(materialID.Value, "Name");
                    fullName.Append(Loc.GetString("Elona.SenseQuality.ItemName",
                        ("quality", Loc.GetString($"Elona.Quality.Names.{quality}")),
                        ("materialName", materialName)));
                }
                if (curse == CurseState.Cursed || curse == CurseState.Doomed)
                {
                    fullName.Append(Loc.GetString($"Elona.SenseQuality.CurseStates.{curse}"));
                }
            }

            if (!noArticle)
            {
                if (identify >= IdentifyState.Full && quality >= Quality.Great && HasComp<EquipmentComponent>(uid))
                {
                    fullName.Insert(0, "the ");
                }
                else if (amount == 1)
                {
                    if (fullName.Length > 0 && IsVowel(fullName[0]))
                    {
                        fullName.Insert(0, "an ");
                    }
                    else
                    {
                        fullName.Insert(0, "a ");
                    }
                }
            }

            return fullName.ToString();
        }

        private static bool IsVowel(char c)
        {
            return "aeiou".Contains(c);
        }
    }

    [ByRefEvent]
    public struct LocalizeItemNameExtraEvent
    {
        public StringBuilder OutFullName { get; }

        public LocalizeItemNameExtraEvent(StringBuilder fullName)
        {
            OutFullName = fullName;
        }
    }
}
