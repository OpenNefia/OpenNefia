using OpenNefia.Content.Damage;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.InUse;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.World;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IInUseSystem _inUse = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ISleepSystem _sleep = default!;

        public override void Initialize()
        {
            Initialize_Resting();
            Initialize_Eating();
            Initialize_Traveling();
            Initialize_Mining();
            Initialize_PreparingToSleep();
        }
    }
}