using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Items;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Identify;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Sidequests;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.UI;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Chests;
using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Currency;
using Love;
using OpenNefia.Content.Encounters;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.RandomText;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem
    {
        [Dependency] private readonly IEncounterSystem _encounters = default!;
        [Dependency] private readonly IRandomAliasGenerator _randomAliasGenerator = default!;

        /// <summary>
        /// Elona.RogueBoss:Ambush
        /// </summary>
        /// <remarks>
        /// "Halt, halt, traveler. You're a quite fortunate one..."
        /// </remarks>
        [UsedImplicitly]
        public QualifiedDialogNode? RogueBoss_Ambush(IDialogEngine engine, IDialogNode node)
        {
            var rogueGroupName = _randomAliasGenerator.GenerateRandomAlias(AliasType.Party);
            var surrenderCost = _encounters.CalcRogueSurrenderCostGold(engine.Player);

            var text = Loc.GetString("Elona.Dialog.Unique.RogueBoss.Ambush.Text",
                ("speaker", engine.Speaker!.Value),
                ("rogueGroupName", rogueGroupName),
                ("surrenderCost", surrenderCost));

            var texts = new List<DialogTextEntry>();
            texts.Add(DialogTextEntry.FromString(text));

            var choices = new List<DialogChoiceEntry>();
            choices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Unique.RogueBoss.Ambush.Choices.TryMe"),
                NextNode = new(Protos.Dialog.RogueBoss, "TryMe")
            });
            choices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Unique.RogueBoss.Ambush.Choices.Surrender"),
                NextNode = new(Protos.Dialog.RogueBoss, "Surrender")
            });

            var newNode = new DialogTextNode(texts, choices);
            return new(Protos.Dialog.RogueBoss, newNode);
        }

        /// <summary>
        /// Elona.RogueBoss:Surrender - BeforeEnter
        /// </summary>
        /// <remarks>
        /// "A wise choice."
        /// </remarks>
        [UsedImplicitly]
        public void RogueBoss_Surrender_BeforeEnter(IDialogEngine engine, IDialogNode node)
        {
            // >>>>>>>> shade2/chat.hsp:1973 		snd sePayGold ...
            _audio.Play(Protos.Sound.Paygold1);
            if (TryComp<MoneyComponent>(engine.Player, out var money))
            {
                money.Gold -= _encounters.CalcRogueSurrenderCostGold(engine.Player);
            }

            foreach (var item in _inv.EntityQueryInInventory<CargoComponent>(engine.Player).ToList())
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Common.YouHandOver", ("player", engine.Player), ("item", item.Owner)));
                EntityManager.DeleteEntity(item.Owner);
            }
            // <<<<<<<< shade2/chat.hsp:1980 		call calcBurdenPc ..
        }
    }
}