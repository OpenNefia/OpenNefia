using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.GameObjects
{
    public class CharaSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypes = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<CharaComponent, ComponentStartup>(HandleStartup, nameof(HandleStartup));
        }

        private void HandleStartup(EntityUid uid, CharaComponent chara, ComponentStartup args)
        {
            SetRaceDefaultChip(uid, chara);
        }

        private void SetRaceDefaultChip(EntityUid uid, CharaComponent chara, ChipComponent? chip = null)
        {
            if (!Resolve(uid, ref chip))
                return;

            var race = _prototypes.Index(chara.Race);

            if (chip.ChipID == Protos.Chip.Default)
            {
                switch (chara.Gender)
                {
                    case Gender.Male:
                        chip.ChipID = race.ChipMale;
                        break;
                    case Gender.Female:
                    default:
                        chip.ChipID = race.ChipFemale;
                        break;
                }
            }
        }
    }
}
