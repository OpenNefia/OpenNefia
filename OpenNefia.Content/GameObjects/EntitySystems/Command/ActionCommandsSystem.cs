using OpenNefia.Content.Pickable;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UserInterface;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Mining;
using OpenNefia.Core.Maths;
using OpenNefia.Core;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Utility;
using OpenNefia.Content.UI;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Charas;

namespace OpenNefia.Content.GameObjects
{
    public class ActionCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ITargetingSystem _targeting = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IActionBashSystem _actionBash = default!;
        [Dependency] private readonly IActionDigSystem _actionDig = default!;
        [Dependency] private readonly IActionInteractSystem _actionInteract = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IVerbSystem _verbs = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IVanillaAISystem _vai = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Dig, InputCmdHandler.FromDelegate(CommandDig))
                .Bind(ContentKeyFunctions.Bash, InputCmdHandler.FromDelegate(CommandBash))
                .Bind(ContentKeyFunctions.Fire, InputCmdHandler.FromDelegate(CommandFire))
                .Bind(ContentKeyFunctions.Ammo, InputCmdHandler.FromDelegate(CommandAmmo))
                .Bind(ContentKeyFunctions.Rest, InputCmdHandler.FromDelegate(CommandRest))
                .Bind(ContentKeyFunctions.Interact, InputCmdHandler.FromDelegate(CommandInteract))
                .Bind(ContentKeyFunctions.Target, InputCmdHandler.FromDelegate(CommandTarget))
                .Bind(ContentKeyFunctions.Look, InputCmdHandler.FromDelegate(CommandLook))
                .Register<ActionCommandsSystem>();
        }

        private bool BlockIfWorldMap(EntityUid player)
        {
            if (!TryMap(player, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                _mes.Display(Loc.GetString("Elona.Common.CannotDoInGlobal"));
                return true;
            }
            return false;
        }

        private TurnResult? CommandDig(IGameSessionManager? session)
        {
            if (BlockIfWorldMap(session!.Player))
                return TurnResult.Aborted;

            var dir = _uiManager.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(session!.Player, Loc.GetString("Elona.Dig.Prompt")));
            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            if (dir.Value.Direction == Direction.Center)
            {
                var playerSpatial = Spatial(session.Player);
                var activity = EntityManager.SpawnEntity(Protos.Activity.DiggingSpot, MapCoordinates.Global);
                Comp<ActivityDiggingSpotComponent>(activity).TargetTile = playerSpatial.MapPosition;
                _activities.StartActivity(session.Player, activity);
                return TurnResult.Succeeded;
            }

            return _actionDig.StartMining(session!.Player, dir.Value.Coords);
        }

        private TurnResult? CommandBash(IGameSessionManager? session)
        {
            if (BlockIfWorldMap(session!.Player))
                return TurnResult.Aborted;

            var dir = _uiManager.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(session!.Player, Loc.GetString("Elona.Bash.Prompt")));
            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            return _actionBash.DoBash(session!.Player, dir.Value.Coords);
        }

        private TurnResult? CommandFire(IGameSessionManager? session)
        {
            if (BlockIfWorldMap(session!.Player))
                return TurnResult.Aborted;

            if (!_targeting.TrySearchForTarget(session.Player, out var target))
                return TurnResult.Aborted;

            if (_factions.GetRelationTowards(session.Player, target.Value) >= Relation.Neutral
                && !_targeting.PromptReallyAttack(session.Player, target.Value))
                return TurnResult.Aborted;

            if (!_combat.TryGetRangedWeaponAndAmmo(session.Player, out var rangedWeapon, out _, out var errorReason))
            {
                _mes.Display(Loc.GetString($"Elona.Combat.RangedAttack.Errors.{errorReason}"), combineDuplicates: true);
                return TurnResult.Aborted;
            }

            return _combat.RangedAttack(session.Player, target.Value, rangedWeapon.Value);
        }

        private TurnResult? CommandAmmo(IGameSessionManager? session)
        {
            // >>>>>>>> elona122/shade2/command.hsp:4694 *com_ammo ..
            var player = session!.Player;

            var ammo = _equipSlots.EnumerateEquippedEntities(player)
                .Select(e => CompOrNull<AmmoComponent>(e))
                .WhereNotNull()
                .FirstOrDefault();

            if (ammo == null)
            {
                _mes.Display(Loc.GetString("Elona.Ammo.NeedToEquip"));
                return TurnResult.Aborted;
            }

            var ammoEncs = _enchantments.QueryEnchantmentsOnItem<EncAmmoComponent>(ammo.Owner).Select(pair => pair.Item2).ToList();
            if (ammoEncs.Count == 0)
            {
                ammo.ActiveAmmoEnchantment = null;
                _mes.Display(Loc.GetString("Elona.Ammo.IsNotCapableOfSwitching", ("item", ammo)));
                return TurnResult.Aborted;
            }

            _audio.Play(Protos.Sound.Ammo);

            var currentIndex = -1;
            if (IsAlive(ammo.ActiveAmmoEnchantment))
                currentIndex = ammoEncs.FindIndex(c => c.Owner == ammo.ActiveAmmoEnchantment.Value);

            if (currentIndex == -1)
                currentIndex = 0;
            else
            {
                currentIndex++;
                if (currentIndex >= ammoEncs.Count)
                    currentIndex = -1;
            }

            if (currentIndex == -1)
                ammo.ActiveAmmoEnchantment = null;
            else
                ammo.ActiveAmmoEnchantment = ammoEncs[currentIndex].Owner;

            _mes.Display(Loc.GetString("Elona.Ammo.Current"));

            for (var i = -1; i < ammoEncs.Count; i++)
            {
                string name, capacity;

                if (i == -1)
                {
                    name = Loc.GetString("Elona.Ammo.Name.Normal");
                    capacity = Loc.GetString("Elona.Ammo.Capacity.Unlimited");
                }
                else
                {
                    var ammoEnc = ammoEncs[i];
                    name = Loc.GetPrototypeString(ammoEnc.AmmoEnchantmentID, "Name");
                    capacity = $"{ammoEnc.CurrentAmmoAmount}/{ammoEnc.MaxAmmoAmount}";
                }

                string mes;
                Color? color = null;

                if (currentIndex == i)
                {
                    mes = $"[{name}:{capacity}]";
                    color = UiColors.MesBlue;
                }
                else
                {
                    mes = $" {name}:{capacity} ";
                }

                // BUG: fix leading space not printed in message window
                _mes.Display(" " + mes, color: color);
            }
            
            return TurnResult.Aborted;
            // <<<<<<<< elona122/shade2/command.hsp:4739 	goto *pc_turn ..
        }

        private TurnResult? CommandRest(IGameSessionManager? session)
        {
            _activities.StartActivity(session!.Player, Protos.Activity.Resting);
            return TurnResult.Succeeded;
        }

        private TurnResult? CommandInteract(IGameSessionManager? session)
        {
            return _actionInteract.PromptInteract(session!.Player);
        }

        private TurnResult? CommandTarget(IGameSessionManager? session)
        {
            if (!TryMap(session!.Player, out var map) || !TryComp<VanillaAIComponent>(session.Player, out var vai))
                return TurnResult.Aborted;

            var coords = Spatial(session!.Player).MapPosition;
            MapCoordinates? targetCoords = null;
            if (vai.CurrentTarget != null)
                targetCoords = Spatial(vai.CurrentTarget.Value).MapPosition;

            var args = new PositionPrompt.Args(coords, session.Player, targetCoords);
            var posResult = _uiManager.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(args);

            if (!posResult.HasValue)
                return TurnResult.Aborted;

            var result = posResult.Value;
            if (!result.CanSee || !map.IsFloor(result.Coords))
            {
                _mes.Display(Loc.GetString("Elona.Targeting.Prompt.CannotSeeLocation"));
            }    
            else
            {
                _audio.Play(Protos.Sound.Ok1);
                var chara = _entityLookup.QueryLiveEntitiesAtCoords<CharaComponent>(result.Coords).FirstOrDefault();
                if (chara != null && !session.IsPlayer(chara.Owner))
                {
                    _vai.SetTarget(session.Player, chara.Owner, 0, vai);
                    _mes.Display(Loc.GetString("Elona.Targeting.Action.YouTarget", ("onlooker", session.Player), ("target", chara.Owner)));
                }
                else if (result.Coords.TryToEntity(_mapManager, out var entityCoords))
                {
                    vai.CurrentTargetLocation = entityCoords;
                    _mes.Display(Loc.GetString("Elona.Targeting.Action.YouTargetGround", ("onlooker", session.Player)));
                }
            }

            return TurnResult.Aborted;
        }

        private TurnResult? CommandLook(IGameSessionManager? session)
        {
            if (!TryMap(session!.Player, out var map))
                return TurnResult.Aborted;
            
            if (HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                var args = new PositionPrompt.Args(Spatial(session.Player).MapPosition);
                _uiManager.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(args);
            }
            else
            {
                var args = new TargetPrompt.Args(session.Player);
                var result = _uiManager.Query<TargetPrompt, TargetPrompt.Args, TargetPrompt.Result>(args);
                if (result.HasValue && IsAlive(result.Value.Target))
                {
                    var target = result.Value.Target.Value;
                    _vai.SetTarget(session.Player, target, 0);
                    _audio.Play(Protos.Sound.Ok1);
                    _mes.Display(Loc.GetString("Elona.Targeting.Action.YouTarget", ("onlooker", session.Player), ("target", target)));
                }
            }
        
            return TurnResult.Aborted;
        }
    }
}