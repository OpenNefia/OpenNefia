using OpenNefia.Content.Equipment;
using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public class CurseStateSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        public override void Initialize()
        {
            SubscribeLocalEvent<CurseStateComponent, GotEquippedInMenuEvent>(OnEquippedInMenu, nameof(OnEquippedInMenu));
        }

        private void OnEquippedInMenu(EntityUid item, CurseStateComponent component, GotEquippedInMenuEvent args)
        {
            string? key = component.CurseState switch
            {
                CurseState.Cursed => "Elona.CurseState.Equipped.Cursed",
                CurseState.Doomed => "Elona.CurseState.Equipped.Doomed",
                CurseState.Blessed => "Elona.CurseState.Equipped.Blessed",
                _ => null
            };

            if (key != null)
            {
                _mes.Display(Loc.GetString(key, ("actor", args.Equipee), ("target", args.EquipTarget), ("item", item)));
            }
        }
    }
}
