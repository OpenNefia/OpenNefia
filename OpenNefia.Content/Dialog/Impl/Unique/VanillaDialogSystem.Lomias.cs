using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem
    {
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly ISidequestSystem _sidequests = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;

        /// <summary>
        /// Elona.Lomias:Tutorial0_Start - AfterEnter
        /// </summary>
        /// <remarks>
        /// "A wise choice. I will start from the beginning."
        /// </remarks>
        public void Lomias_Tutorial0_Start_AfterEnter(IDialogEngine engine, IDialogNode node)
        {
            var corpse = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.Corpse);
            if (IsAlive(corpse))
            {
                var protoSource = EnsureComp<EntityProtoSourceComponent>(corpse.Value);
                protoSource.EntityID = Protos.Chara.Beggar;
                EnsureComp<IdentifyComponent>(corpse.Value).IdentifyState = IdentifyState.Full;
                _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
            }
            _sidequests.SetState(Protos.Sidequest.Tutorial, 1);
        }

        /// <summary>
        /// Elona.Lomias:Tutorial3 - BeforeEnter
        /// </summary>
        /// <remarks>
        /// "Looks like you found something."
        /// </remarks>
        public void Lomias_Tutorial3_BeforeEnter(IDialogEngine engine, IDialogNode node)
        {
            var scroll = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.ScrollOfIdentify);
            if (IsAlive(scroll))
            {
                _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
                EnsureComp<IdentifyComponent>(scroll.Value).IdentifyState = IdentifyState.Full;
            }
        }

        /// <summary>
        /// Elona.Lomias:Tutorial4 - AfterEnter
        /// </summary>
        /// <remarks>
        /// "Okay, I will now tell you how to fight. Before the combat starts, you need to equip
        /// weapons. Take my old bow and arrows and equip them."
        /// </remarks>
        public void Lomias_Tutorial4_AfterEnter(IDialogEngine engine, IDialogNode node)
        {
            var item = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.LongBow);
            if (IsAlive(item))
            {
                EnsureComp<CurseStateComponent>(item.Value).CurseState = CurseState.Cursed;
            }

            item = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.Arrow);
            if (IsAlive(item))
            {
                EnsureComp<CurseStateComponent>(item.Value).CurseState = CurseState.Normal;
            }

            item = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.ScrollOfVanishCurse);
            if (IsAlive(item))
            {
                EnsureComp<IdentifyComponent>(item.Value).IdentifyState = IdentifyState.Full;
                EnsureComp<CurseStateComponent>(item.Value).CurseState = CurseState.Blessed;
            }

            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
        }

        /// <summary>
        /// Elona.Lomias:Tutorial5_EquipDone - AfterEnter
        /// </summary>
        /// <remarks>
        /// "Get ready. I will summon some weak monsters. Kill them with your bow if possible. Try
        /// to stay away from the enemies as bows aren't effective in close range. I've dropped a
        /// few potions in case you get hurt. You know how to use them right? Yes, use [x] key."
        /// </remarks>
        public void Lomias_Tutorial5_EquipDone_AfterEnter(IDialogEngine engine, IDialogNode node)
        {
            _mes.Display(Loc.GetString("Elona.Dialog.Unique.Lomias.Tutorial5_EquipDone.LomiasReleasesPutits"), UiColors.MesSkyBlue);

            for (var i = 0; i < 3; i++)
            {
                var putit = _charaGen.GenerateChara(_gameSession.Player, Protos.Chara.Putit);
                if (IsAlive(putit))
                {
                    EnsureComp<AINoTargetComponent>(putit.Value);
                    EnsureComp<TagComponent>(putit.Value).AddTag(Protos.Tag.TutorialPutit);
                }
            }

            var item = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.PotionOfCureMinorWound);
            if (IsAlive(item))
            {
                EnsureComp<IdentifyComponent>(item.Value).IdentifyState = IdentifyState.Full;
            }
        }

        /// <summary>
        /// Elona.Lomias:Tutorial7_Chest - BeforeEnter
        /// </summary>
        /// <remarks>
        /// "You might find chests containing loot in ruins. There's one nearby, open it."
        /// </remarks>
        public void Lomias_Tutorial7_Chest_BeforeEnter(IDialogEngine engine, IDialogNode node)
        {
            var chest = _itemGen.GenerateItem(_gameSession.Player, Protos.Item.Chest);
            if (IsAlive(chest) && TryComp<ChestComponent>(chest.Value, out var chestComp))
            {
                chestComp.ItemLevel = 35;
                chestComp.LockpickDifficulty = 25;
            }
            _itemGen.GenerateItem(_gameSession.Player, Protos.Item.Lockpick, amount: 2);
            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
        }
    }
}
