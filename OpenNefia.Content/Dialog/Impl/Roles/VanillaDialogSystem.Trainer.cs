using NetVips;
using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Factions;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        [Dependency] private readonly ITrainerSystem _trainers = default!;

        private void Trainer_Initialize()
        {
            SubscribeComponent<RoleTrainerComponent, GetDefaultDialogChoicesEvent>(Trainer_AddDialogChoices, priority: EventPriorities.High);
        }

        private void Trainer_AddDialogChoices(EntityUid uid, RoleTrainerComponent component, GetDefaultDialogChoicesEvent args)
        {
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Trainer.Choices.Train"),
                NextNode = new(Protos.Dialog.Trainer, "Train")
            });
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Trainer.Choices.Learn"),
                NextNode = new(Protos.Dialog.Trainer, "Learn"),
            });
        }

        #region Training

        public QualifiedDialogNode? Trainer_Train(IDialogEngine engine, IDialogNode node)
        {
            var args = new CharaInfoTrainerUiLayer.Args(new TrainerMode.Train(), engine.Speaker!.Value, engine.Player);
            var result = _uiManager.Query<CharaInfoTrainerUiLayer, CharaInfoTrainerUiLayer.Args, CharaInfoTrainerUiLayer.Result>(args);

            if (!result.HasValue)
            {
                return engine.GetNodeByID(Protos.Dialog.Trainer, "ComeAgain");
            }

            engine.Data.Add(new DialogTrainerData()
            {
                SkillID = result.Value.SelectedSkillID,
                PlatinumCost = _trainers.CalcTrainSkillCost(engine.Player, result.Value.SelectedSkillID)
            });

            return engine.GetNodeByID(Protos.Dialog.Trainer, "TrainConfirm");
        }

        public QualifiedDialogNode? Trainer_TrainConfirm(IDialogEngine engine, IDialogNode node)
        {
            var speaker = engine.Speaker;

            if (!engine.Data.TryGet<DialogTrainerData>(out var data))
            {
                Logger.WarningS("dialog.role", $"Missing {nameof(DialogTrainerData)} in {nameof(engine.Data)}");
                return null;
            }

            var text = Loc.GetString("Elona.Dialog.Trainer.Train.Cost",
                ("speaker", speaker),
                ("skillName", Loc.GetPrototypeString(data.SkillID, "Name")),
                ("cost", data.PlatinumCost));

            var texts = new List<DialogTextEntry>();
            texts.Add(DialogTextEntry.FromString(text));

            var choices = new List<DialogChoiceEntry>();
            if (TryComp<WalletComponent>(engine.Player, out var wallet) && wallet.Platinum >= data.PlatinumCost)
            {
                choices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Trainer.Train.Choices.Confirm"),
                    NextNode = new(Protos.Dialog.Trainer, "TrainExecute")
                });
            }
            choices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Trainer.Choices.GoBack"),
                NextNode = new(Protos.Dialog.Trainer, "ComeAgain"),
                IsDefault = true
            });

            var nextNode = new DialogTextNode(texts, choices);
            return new(Protos.Dialog.Trainer, nextNode);
        }

        public void Trainer_TrainExecute(IDialogEngine engine, IDialogNode node)
        {
            if (!engine.Data.TryGet<DialogTrainerData>(out var data))
            {
                Logger.WarningS("dialog.role", $"Missing {nameof(DialogTrainerData)} in {nameof(engine.Data)}");
                return;
            }
            if (!TryComp<WalletComponent>(engine.Player, out var wallet))
            {
                Logger.WarningS("dialog.role", $"Missing {nameof(WalletComponent)} in {nameof(engine.Player)}");
                return;
            }

            _audio.Play(Protos.Sound.Paygold1);
            wallet.Platinum -= data.PlatinumCost;
            var amount = _trainers.CalcTrainPotentialAmount(engine.Player, data.SkillID);
            _skills.ModifyPotential(engine.Player, data.SkillID, amount);
        }

        #endregion

        #region Learning

        public QualifiedDialogNode? Trainer_Learn(IDialogEngine engine, IDialogNode node)
        {
            var trainableSkills = _trainers.CalcLearnableSkills(engine.Speaker!.Value).ToHashSet();
            var args = new CharaInfoTrainerUiLayer.Args(new TrainerMode.Learn(trainableSkills), engine.Speaker!.Value, engine.Player);
            var result = _uiManager.Query<CharaInfoTrainerUiLayer, CharaInfoTrainerUiLayer.Args, CharaInfoTrainerUiLayer.Result>(args);

            if (!result.HasValue)
            {
                return engine.GetNodeByID(Protos.Dialog.Trainer, "ComeAgain");
            }

            engine.Data.Add(new DialogTrainerData()
            {
                SkillID = result.Value.SelectedSkillID,
                PlatinumCost = _trainers.CalcLearnSkillCost(engine.Player, result.Value.SelectedSkillID)
            });

            return engine.GetNodeByID(Protos.Dialog.Trainer, "LearnConfirm");
        }

        public QualifiedDialogNode? Trainer_LearnConfirm(IDialogEngine engine, IDialogNode node)
        {
            var speaker = engine.Speaker;

            if (!engine.Data.TryGet<DialogTrainerData>(out var data))
            {
                Logger.WarningS("dialog.role", $"Missing {nameof(DialogTrainerData)} in {nameof(engine.Data)}");
                return null;
            }

            var text = Loc.GetString("Elona.Dialog.Trainer.Learn.Cost",
                ("speaker", speaker),
                ("skillName", Loc.GetPrototypeString(data.SkillID, "Name")),
                ("cost", data.PlatinumCost));

            var texts = new List<DialogTextEntry>();
            texts.Add(DialogTextEntry.FromString(text));

            var choices = new List<DialogChoiceEntry>();
            if (TryComp<WalletComponent>(engine.Player, out var wallet) && wallet.Platinum >= data.PlatinumCost)
            {
                choices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Trainer.Learn.Choices.Confirm"),
                    NextNode = new(Protos.Dialog.Trainer, "LearnExecute")
                });
            }
            choices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Trainer.Choices.GoBack"),
                NextNode = new(Protos.Dialog.Trainer, "ComeAgain"),
                IsDefault = true
            });

            var nextNode = new DialogTextNode(texts, choices);
            return new(Protos.Dialog.Trainer, nextNode);
        }

        public void Trainer_LearnExecute(IDialogEngine engine, IDialogNode node)
        {
            if (!engine.Data.TryGet<DialogTrainerData>(out var data))
            {
                Logger.WarningS("dialog.role", $"Missing {nameof(DialogTrainerData)} in {nameof(engine.Data)}");
                return;
            }
            if (!TryComp<WalletComponent>(engine.Player, out var wallet))
            {
                Logger.WarningS("dialog.role", $"Missing {nameof(WalletComponent)} in {nameof(engine.Player)}");
                return;
            }

            _audio.Play(Protos.Sound.Paygold1);
            wallet.Platinum -= data.PlatinumCost;
            _skills.GainSkill(engine.Player, data.SkillID);
            _trainers.TotalSkillsLearned++;
        }

        #endregion
    }

    public sealed class DialogTrainerData : IDialogExtraData
    {
        public PrototypeId<SkillPrototype> SkillID { get; set; }
        public int PlatinumCost { get; set; }
    }
}