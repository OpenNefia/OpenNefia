using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Activity.Impl
{
    public sealed partial class VanillaActivitiesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            Initialize_Resting();
        }
    }
}