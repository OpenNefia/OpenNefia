using NLua;
using OpenNefia.Content.Charas;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Items;
using OpenNefia.Content.Materials;
using OpenNefia.Content.Qualities;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Items
{
    public sealed partial class ItemNameSystem
    {
        public string ItemNameJP(EntityUid uid,
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
            var amount = _stacks.GetCount(uid);

            var food = CompOrNull<FoodComponent>(uid);
            var isCookedDish = _food.IsCooked(uid, food);

            var fullName = new StringBuilder();

            var counter = "";

            if (amount > 1)
            {
                counter = Loc.GetString("Elona.Item.Japanese.Counters.Default");
                if (TryComp<JapaneseCounterComponent>(uid, out var counterComp))
                    counter = counterComp.CounterText;
                fullName.Append($"{amount}{counter}の");
            }
            
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
            
            if (food != null && food.IsRotten)
            {
                fullName.Append(Loc.GetString("Elona.Food.ItemName.Rotten") + Loc.Space);
            }

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

            var skip = ItemNameSub(ref fullName, uid, isJapanese: true);

            if (HasComp<FurnitureComponent>(uid) && materialID != null)
            {
                var matName = Loc.GetPrototypeString(materialID.Value, "Name");
                fullName.Append(Loc.GetString("Elona.Furniture.ItemName.Work", ("matName", matName)) + Loc.Space);
            }

            if (TryComp<GiftComponent>(uid, out var gift))
            {
                fullName.Append(Loc.GetString($"Elona.Gift.ItemName.Ranks.{gift.GiftRank}") + Loc.Space);
            }

            var katakana = false;

            if (!skip)
            {
                if (identify >= IdentifyState.Full && HasComp<EquipmentComponent>(uid))
                {
                    if (CompOrNull<EternalForceComponent>(uid)?.IsEternalForce.Buffed == true)
                    {
                        fullName.Append(Loc.GetString("Elona.Item.ItemName.EternalForce") + Loc.Space);
                    }
                    else
                    {
                        if (enchantments != null)
                        {
                            if (enchantments.EgoMajorEnchantment != null)
                            {
                                var s = Loc.GetPrototypeString(enchantments.EgoMajorEnchantment.Value, "NameModifier", ("name", fullName.ToString()));
                                fullName.Clear().Append(s);
                            }
                            if (enchantments.EgoMinorEnchantment != null)
                            {
                                var s = Loc.GetPrototypeString(enchantments.EgoMinorEnchantment.Value, "NameModifier", ("name", fullName.ToString()));
                                fullName.Clear().Append(s);
                            }
                        }

                        if (materialID != null)
                        {
                            if (quality != Quality.Unique && quality >= Quality.Great)
                            {
                                fullName.Append(Loc.GetPrototypeString(materialID.Value, "Alias") + Loc.Space);
                            }
                            else
                            {
                                var materialName = Loc.GetPrototypeString(materialID.Value, "Name");
                                fullName.Append(materialName + Loc.Space);
                                if (StartsWithKatakana(materialName))
                                {
                                    katakana = true;
                                }
                                else
                                {
                                    fullName.Append("の");
                                }
                            }
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
                        fullName.Insert(0, "﻿★");
                        fullName.Append(basename);
                    }
                    else
                    {
                        if (quality >= Quality.Great)
                        {
                            fullName.Insert(0, "﻿☆");
                        }
                        
                        if (katakana)
                        {
                            fullName.Append(item.KatakanaName);
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

            if (identify >= IdentifyState.Full)
            {
                fullName.Append(GetItemKnownInfo(uid));
            }

            var ev = new LocalizeItemNameExtraEvent(fullName);
            RaiseEvent(uid, ref ev);

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

            return fullName.ToString();
        }

        private bool StartsWithKatakana(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            
            var firstRune = str.EnumerateRunes().First();
            var cp = firstRune.Value;
            return cp >= 0x30A0 && cp <= 0x30FF;
        }

        private void AddJapaneseCounterTranslation(EntityUid uid, ref EntityGeneratedEvent args)
        {
            // Automatically add component if it's not in the prototype.
            // Might worth refactoring out later.
            if (Loc.TryGetLocalizationData(uid, out var table)
                && table.TryGetValue("JapaneseCounter", out var counter)
                && counter is LuaTable counterTable
                && counterTable.TryGetValue("CounterText", out var text))
            {
                EnsureComp<JapaneseCounterComponent>(uid).CounterText = text.ToString();
                return;
            }

            if (TryComp<TagComponent>(uid, out var tags))
            {
                foreach (var tag in tags.Tags)
                {
                    if (_protos.TryGetExtendedData<TagPrototype, ExtTagJapaneseCounter>(tag, out var ext))
                    {
                        EnsureComp<JapaneseCounterComponent>(uid).CounterText = Loc.GetString(ext.CounterKey);
                        return;
                    }
                }
            }
        }
    }

    public sealed class ExtTagJapaneseCounter : IPrototypeExtendedData<TagPrototype>
    {
        [DataField(required: true)]
        public LocaleKey CounterKey { get; set; }
    }
}
