using OpenNefia.Content.Roles;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Home;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class VanillaDialogSystem : EntitySystem
    {
        [Dependency] private readonly IHomeSystem _homes = default!;

        private void Maid_Initialize()
        {
            SubscribeComponent<RoleMaidComponent, GetDefaultDialogChoicesEvent>(Maid_AddDialogChoices);
        }

        private void Maid_AddDialogChoices(EntityUid uid, RoleMaidComponent component, GetDefaultDialogChoicesEvent args)
        {
            //if (_homes.GuestsWaiting > 0)
            //{
            //    args.OutChoices.Add(new()
            //    {
            //        Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Maid.Choices.MeetGuest"),
            //        NextNode = new(Protos.Dialog.Maid, "MeetGuest"),
            //    });
            //    args.OutChoices.Add(new()
            //    {
            //        Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Maid.Choices.RefuseGuest"),
            //        NextNode = new(Protos.Dialog.Maid, "RefuseGuest"),
            //    });
            //}
        }
    }
}