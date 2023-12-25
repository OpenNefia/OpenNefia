using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Talk;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        [Dependency] private readonly IMarriageSystem _marriage = default!;

        private void Ally_Initialize()
        {
            SubscribeEntity<GetDefaultDialogChoicesEvent>(Ally_AddTalkChoices, priority: EventPriorities.VeryHigh);
        }

        private void Ally_AddTalkChoices(EntityUid uid, GetDefaultDialogChoicesEvent args)
        {
            if (!_parties.IsUnderlingOfPlayer(uid))
                return;

            if (TryComp<MarriageComponent>(uid, out var marriage) && !marriage.MarriagePartners.Contains(args.Player))
            {
                args.OutChoices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Ally.Choices.AskForMarriage"),
                    NextNode = new(Protos.Dialog.Ally, "AskForMarriage")
                });
            }

            if (TryComp<ToneComponent>(uid, out var tone))
            {
                if (tone.IsTalkSilenced)
                {
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Ally.Choices.Silence.Stop"),
                        NextNode = new(Protos.Dialog.Ally, "SilenceStop")
                    });
                }
                else
                {
                    args.OutChoices.Add(new()
                    {
                        Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Ally.Choices.Silence.Start"),
                        NextNode = new(Protos.Dialog.Ally, "SilenceStart")
                    });
                }
            }

            args.OutChoices.Add(new() { 
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Ally.Choices.Abandon"),
                NextNode = new(Protos.Dialog.Ally, "AbandonConfirm")
            });
        }

        DialogActionDelegate _Ally_SilenceStart => Ally_SilenceStart;
        public void Ally_SilenceStart(IDialogEngine engine, IDialogNode node)
        {
            Comp<ToneComponent>(engine.Speaker!.Value).IsTalkSilenced = true;
        }

        public void Ally_SilenceStop(IDialogEngine engine, IDialogNode node)
        {
            Comp<ToneComponent>(engine.Speaker!.Value).IsTalkSilenced = false;
        }

        public QualifiedDialogNode? Ally_AskForMarriage(IDialogEngine engine, IDialogNode node)
        {
            if (TryComp<DialogComponent>(engine.Speaker, out var dialog) && dialog.Impression >= ImpressionLevels.Marry)
                return engine.GetNodeByID(Protos.Dialog.Ally, "MarriageAccept");

            return engine.GetNodeByID(Protos.Dialog.Ally, "MarriageRefuse");
        }

        public void Ally_MarriageAccept(IDialogEngine engine, IDialogNode node)
        {
            _marriage.Marry(engine.Player, engine.Speaker!.Value);
        }

        public QualifiedDialogNode? Ally_Abandon(IDialogEngine engine, IDialogNode node)
        {
            _mes.Display(Loc.GetString("Elona.Dialog.Ally.Abandon.YouAbandoned",
                ("ally", engine.Speaker),
                ("player", engine.Player)));
            EntityManager.DeleteEntity(engine.Speaker!.Value);
            return null;
        }
    }
}