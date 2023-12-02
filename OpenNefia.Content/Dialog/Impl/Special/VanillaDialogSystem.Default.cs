using NetVips;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        private void Default_Initialize()
        {
            SubscribeEntity<GetDefaultDialogChoicesEvent>(Default_AddTalkChoice, priority: EventPriorities.Highest);
        }

        private void Default_AddTalkChoice(EntityUid uid, GetDefaultDialogChoicesEvent args)
        {
            args.OutChoices.Add(new() { 
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Villager.Choices.Talk"),
                NextNode = new(Protos.Dialog.Default, "Talk")
            });
        }

        private void ModifyImpressAndInterest(EntityUid player, EntityUid target)
        {
            if (!IsAlive(target) || !TryComp<DialogComponent>(target, out var dialog))
                return;

            // >>>>>>>> elona122/shade2/chat.hsp:2203 		if cInterest(tc)>0 : if cRelation(tc)!cAlly : if ..
            if (dialog.Interest > 0 && !_parties.IsInPlayerParty(target))
            {
                if (_rand.OneIn(3) && dialog.Impression < ImpressionLevels.Friend)
                {
                    var charisma = _skills.Level(player, Protos.Skill.AttrCharisma);
                    if (_rand.Next(charisma + 1) > 10)
                    {
                        _dialog.ModifyImpression(target, _rand.Next(3));
                    }
                    else
                    {
                        _dialog.ModifyImpression(target, -_rand.Next(3));
                    }
                }
            }

            dialog.Interest -= _rand.Next(30);
            dialog.InterestRenewDate = _world.State.GameDate + GameTimeSpan.FromHours(8);
            // <<<<<<<< elona122/shade2/chat.hsp:2208 		} ..
        }

        private List<DialogTextEntry> GetDefaultTalkText(EntityUid target)
        {
            var result = new List<DialogTextEntry>();

            // TODO random talk

            var area = AreaOrNull(target);
            var dialog = CompOrNull<DialogComponent>(target);

            LocaleKey key = "Elona.Dialog.Villager.Talk.Default";

            if (dialog != null && dialog.Interest <= 0)
                key = "Elona.Dialog.Villager.Talk.Bored";
            else if (_parties.IsInPlayerParty(target))
                key = "Elona.Dialog.Villager.Talk.Ally";
            else if (HasComp<RoleProstituteComponent>(target))
                key = "Elona.Dialog.Villager.Talk.Prostitute";
            else if (TryComp<RoleShopkeeperComponent>(target, out var shopkeeper) && shopkeeper.ShopInventoryId == Protos.ShopInventory.Moyer)
                key = "Elona.Dialog.Villager.Talk.Moyer";
            else if (HasComp<RoleSlaverComponent>(target))
                key = "Elona.Dialog.Villager.Talk.Slavekeeper";
            else if (dialog != null && dialog.Impression >= ImpressionLevels.Friend && _rand.OneIn(3))
                key = "Elona.Dialog.Villager.Talk.Rumor";
            // TODO noyel festival
            else if (_rand.OneIn(2) && dialog != null)
                key = $"Elona.Dialog.Villager.Talk.Personality.{dialog.Personality}";
            else if (_rand.OneIn(3) && area != null && TryProtoID(area.AreaEntityUid, out var areaProtoID))
            {
                var areaKey = $"OpenNefia.Prototypes.Entity.{areaProtoID}.VillagerTalk"; // TODO namespace component localizations separately to avoid name clashes
                if (Loc.KeyExists(areaKey))
                    key = areaKey;
            }

            var textEntry = DialogTextEntry.FromLocaleKey(key);
            textEntry.PickRandomly = true;

            result.Add(textEntry);
            return result;
        }

        private List<DialogChoiceEntry> GetDefaultTalkChoices(EntityUid player, EntityUid speaker)
        {
            var result = new List<DialogChoiceEntry>();
            if (!IsAlive(speaker))
                return result;

            var ev = new GetDefaultDialogChoicesEvent(player, speaker);
            RaiseEvent(speaker, ev);
            result.AddRange(ev.OutChoices);

            result.Add(new() { 
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Common.Choices.Bye"),
                NextNode = null,
                IsDefault = true
            });

            return result;
        }

        public QualifiedDialogNode? Default_Talk(IDialogEngine engine, IDialogNode node)
        {
            var target = engine.Speaker;
            if (!IsAlive(target))
                return null;

            ModifyImpressAndInterest(engine.Player, target.Value);

            var texts = GetDefaultTalkText(target.Value);
            var choices = GetDefaultTalkChoices(engine.Player, target.Value);

            var nextNode = new DialogTextNode(texts, choices);
            return new(Protos.Dialog.Default, nextNode);
        }

        public QualifiedDialogNode? Default_Trade(IDialogEngine engine, IDialogNode node)
        {
            // TODO
            return engine.GetNodeByID(Protos.Dialog.Default, "YouKidding");
        }
    }

    public sealed class GetDefaultDialogChoicesEvent : EntityEventArgs
    {
        public EntityUid Player { get; }
        public EntityUid Speaker { get; }

        public List<DialogChoiceEntry> OutChoices { get; } = new();

        public GetDefaultDialogChoicesEvent(EntityUid player, EntityUid speaker)
        {
            Player = player;
            Speaker = speaker;
        }
    }
}