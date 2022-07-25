using OpenNefia.Content.Damage;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.InUse;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.World;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
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
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISanitySystem _sanity = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly CommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;

        public override void Initialize()
        {
            Initialize_Resting();
            Initialize_Eating();
            Initialize_Traveling();
            Initialize_Mining();
            Initialize_PreparingToSleep();
            Initialize_Sex();
            Initialize_Performing();
        }
    }
}