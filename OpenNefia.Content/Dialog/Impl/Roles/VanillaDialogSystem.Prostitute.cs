﻿using NetVips;
using OpenNefia.Content.Activity;
using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Skills;
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
        private void Prostitute_Initialize()
        {
            SubscribeComponent<RoleProstituteComponent, GetDefaultDialogChoicesEvent>(Prostitute_AddTalkChoices);
        }

        private void Prostitute_AddTalkChoices(EntityUid uid, RoleProstituteComponent component, GetDefaultDialogChoicesEvent args)
        {
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Prostitute.Choices.Buy"),
                NextNode = Protos.Dialog.Prostitute.WithDialogNode("BuyInit")
            });
        }

        private int CalcProstituteGoldCost(EntityUid speaker, EntityUid player)
        {
            return _skills.Level(speaker, Protos.Skill.AttrCharisma) * 25 + 100 + (CompOrNull<FameComponent>(player)?.Fame.Buffed ?? 0 / 10);
        }

        public void Prostitute_BuyInit(IDialogEngine engine, IDialogNode node)
        {
            engine.Data.Add(new DialogProstituteData()
            {
                GoldCost = CalcProstituteGoldCost(engine.Speaker!.Value, engine.Player)
            });
        }

        public QualifiedDialogNode? Prostitute_BuyConfirm(IDialogEngine engine, IDialogNode node)
        {
            if (!engine.Data.TryGet<DialogProstituteData>(out var data))
                return null;

            var text = Loc.GetString("Elona.Dialog.Prostitute.Buy.Text",
                ("speaker", engine.Speaker!.Value),
                ("cost", data.GoldCost));

            var texts = new List<DialogTextEntry>();
            texts.Add(DialogTextEntry.FromString(text));

            var choices = new List<DialogChoiceEntry>();
            if (TryComp<WalletComponent>(engine.Player, out var wallet) && wallet.Gold >= data.GoldCost)
            {
                choices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Common.Sex.Choices.Confirm"),
                    NextNode = Protos.Dialog.Prostitute.WithDialogNode("BuyExecute")
                });
            }
            choices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Common.Sex.Choices.GoBack"),
                NextNode = Protos.Dialog.Villager.WithDialogNode("YouKidding"),
                IsDefault = true
            });

            var newNode = new DialogTextNode(texts, choices);
            return new(Protos.Dialog.Prostitute, newNode);
        }

        public void Prostitute_BuyExecute_Before(IDialogEngine engine, IDialogNode node)
        {
            _audio.Play(Protos.Sound.Paygold1);
            var data = engine.Data.Get<DialogProstituteData>();
            Comp<WalletComponent>(engine.Player).Gold -= data.GoldCost;
            Comp<WalletComponent>(engine.Speaker!.Value).Gold += data.GoldCost;
        }

        public void Prostitute_BuyExecute_After(IDialogEngine engine, IDialogNode node)
        {
            var sexActivity = EntityManager.SpawnEntity(Protos.Activity.Sex, MapCoordinates.Global);
            var sexActivityComp = Comp<ActivitySexComponent>(sexActivity);
            sexActivityComp.Partner = engine.Player;
            sexActivityComp.IsTopping = true;
            _activities.StartActivity(engine.Speaker!.Value, sexActivity);
        }
    }

    public sealed class DialogProstituteData : IDialogExtraData
    {
        public int GoldCost { get; set; }
    }
}