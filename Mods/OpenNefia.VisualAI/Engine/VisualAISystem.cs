using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.VisualAI.Block;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Parties;
using OpenNefia.Core.UserInterface;
using OpenNefia.VisualAI.UserInterface;
using Protos_VisualAI = OpenNefia.VisualAI.Prototypes.Protos;

namespace OpenNefia.VisualAI.Engine
{
    public interface IVisualAISystem : IEntitySystem
    {
        void OpenEditor(EntityUid? target = null);
        TurnResult Run(EntityUid entity, VisualAIComponent? vai = null);
    }

    public sealed class VisualAISystem : EntitySystem, IVisualAISystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override void Initialize()
        {
            _protos.PrototypesReloaded += ResolveBlockDependencies;

            SubscribeEntity<NPCTurnStartedEvent>(NPCTurnStarted_RunVisualAI);
            SubscribeEntity<GetInteractActionsEvent>(GetInteractActions_VisualAI);
        }

        private void ResolveBlockDependencies(PrototypesReloadedEventArgs args)
        {
            if (!args.TryGetModified<VisualAIBlockPrototype>(_protos, out var modified))
                return;

            foreach (var proto in modified)
            {
                proto.InjectDependencies();
            }
        }

        private void NPCTurnStarted_RunVisualAI(EntityUid uid, ref NPCTurnStartedEvent args)
        {
            if (args.Handled)
                return;

            if (TryComp<VisualAIComponent>(uid, out var vai) && vai.Enabled)
            {
                args.Handle(Run(uid));
            }
        }

        private void GetInteractActions_VisualAI(EntityUid uid, GetInteractActionsEvent args)
        {
            if (_parties.IsUnderlingOfPlayer(uid))
                args.OutInteractActions.Add(new(Loc.GetString("VisualAI.Interact.Actions.EditVisualAI"), OpenEditor));
        }

        private TurnResult OpenEditor(EntityUid source, EntityUid target)
        {
            OpenEditor(target);
            return TurnResult.Aborted;
        }

        public void OpenEditor(EntityUid? target = null)
        {
            VisualAIComponent? comp = null;
            VisualAIPlan plan;
            if (IsAlive(target))
            {
                comp = EnsureComp<VisualAIComponent>(target.Value);
                plan = comp.Plan;
            }
            else
            {
                plan = new VisualAIPlan();
            }
            var args = new VisualAIEditor.Args(plan, comp);
            _uiManager.Query<VisualAIEditor, VisualAIEditor.Args>(args);
        }

        /// <summary>
        /// Target source to use if none was specified by the time a condition/action block is encountered.
        /// </summary>
        private IVisualAITargetSource _defaultTargetSource = new AllEntitiesTargetSource();

        private bool TryChooseTarget(VisualAIState state, VisualAIBlock block, [NotNullWhen(true)] out IVisualAITargetValue? target)
        {
            if (state.TriedToChooseTarget)
            {
                target = state.ChosenTarget;
                return target != null;
            }

            bool Filter(IVisualAITargetValue target)
            {
                foreach (var pred in state.TargetFilters)
                {
                    if (!pred.IsAccepted(state, target))
                        return false;
                }
                return true;
            }

            var targetSource = state.TargetSource ?? _defaultTargetSource;

            var candidates = targetSource.GetTargets(state)
                .Where(Filter)
                .ToList();

            if (state.TargetOrder != null)
            {
                Comparison<IVisualAITargetValue> comparison = (a, b) =>
                {
                    return state.TargetOrder.Compare(state, a, b);
                };
                candidates.Sort(comparison);
            }

            target = candidates.FirstOrDefault();

            state.ChosenTarget = target;
            state.TriedToChooseTarget = true;

            return target != null;
        }

        private VisualAIPlan? RunOnePlan(VisualAIState state, VisualAIPlan currentPlan)
        {
            foreach (var block in currentPlan.Blocks)
            {
                Logger.DebugS("visualAI", $"Running block : {block}");

                switch (block.Proto.Type)
                {
                    case VisualAIBlockType.Condition:
                        var success = RunBlockCondition(state, block);
                        Logger.DebugS("visualAI", $"Condition branch : {success} ({block.ProtoID})");
                        if (success)
                            return currentPlan.SubplanTrueBranch;
                        else
                            return currentPlan.SubplanFalseBranch;
                        break;
                    case VisualAIBlockType.Target:
                        RunBlockTarget(state, block);
                        break;
                    case VisualAIBlockType.Action:
                        RunBlockAction(state, block);
                        break;
                    case VisualAIBlockType.Special:
                        RunBlockSpecial(state, block);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown block type : {block.Proto.Type} ({block.ProtoID})");
                }
            }

            return null;
        }

        private bool RunBlockCondition(VisualAIState state, VisualAIBlock block)
        {
            DebugTools.AssertNotNull(block.Proto.Condition);

            if (!TryChooseTarget(state, block, out var target))
            {
                Logger.DebugS("visualAI", $"Run block condition : No target found");
                return false;
            }

            Logger.DebugS("visualAI", $"Run block condition : {block.Proto.Condition}");
            return block.Proto.Condition.IsAccepted(state, target);
        }

        private void RunBlockTarget(VisualAIState state, VisualAIBlock block)
        {
            DebugTools.AssertNotNull(block.Proto.Target);

            if (state.ChosenTarget != null)
            {
                Logger.DebugS("visualAI", $"Run block target : Target already chosen");
                return;
            }

            state.TargetFilters.Add(block.Proto.Target.Filter);

            if (block.Proto.Target.Ordering != null)
            {
                Logger.DebugS("visualAI", $"Set target ordering : {block.Proto.Target.Ordering}");
                state.TargetOrder = block.Proto.Target.Ordering;
            }

            if (block.Proto.Target.Source != null)
            {
                if (state.TargetSource != null)
                {
                    Logger.DebugS("visualAI", $"Target source exists already : {block.Proto.Target.Source} & {state.TargetSource}");

                    // Normally, the first target block that sets a target source determines
                    // the target type for that plan, until a "clear target" block is encountered.
                    // If nothing sets the target then default to all entities.
                    // *However*, if a target source is set (non-null) then other target blocks should not have
                    // incompatible target source types.
                    if (block.Proto.Target.Source.GetType() != state.TargetSource.GetType())
                    {
                        // Differing target source found. This is an error, just ensure nothing gets filtered.
                        state.TargetFilters.Add(new AcceptNoneCondition());
                    }
                }
                else
                {
                    Logger.DebugS("visualAI", $"Set target source : {block.Proto.Target.Source}");
                    state.TargetSource = block.Proto.Target.Source;
                }
            }
        }

        private void RunBlockAction(VisualAIState state, VisualAIBlock block)
        {
            DebugTools.AssertNotNull(block.Proto.Action);

            if (!TryChooseTarget(state, block, out var target))
            {
                Logger.DebugS("visualAI", $"Run block action : No target found");
                return;
            }


            Logger.DebugS("visualAI", $"Run block action : {block.Proto.Action}");
            block.Proto.Action.Apply(state, block, target);
        }

        private void RunBlockSpecial(VisualAIState state, VisualAIBlock block)
        {
            Logger.DebugS("visualAI", $"Run block special : {block.ProtoID}");

            if (block.ProtoID == Protos_VisualAI.VisualAIBlock.SpecialClearTarget)
            {
                state.ClearTarget();
            }
            else
            {
                throw new InvalidOperationException($"Invalid special block {block.ProtoID}");
            }
        }

        public TurnResult Run(EntityUid entity, VisualAIComponent? vai = null)
        {
            if (!Resolve(entity, ref vai) || !TryMap(entity, out var map))
                return TurnResult.Failed;

            Logger.DebugS("visualAI", $"+++++  Running Visual AI for {entity}  +++++");

            VisualAIPlan? plan = vai.Plan;

            if (!plan.Validate(out var errors))
            {
                Logger.ErrorS("visualAI", $"Plan validation failed ({errors.Count} errors):");
                foreach (var error in errors)
                    Logger.ErrorS("visualAI", $"  * {error}");

                return TurnResult.Failed;
            }

            foreach (var (key, value) in vai.StoredTargets.ToList())
            {
                if (!value.IsValid(entity))
                {
                    vai.StoredTargets.Remove(key);
                    continue;
                }
            }

            var state = new VisualAIState(entity, map);

            while (plan != null)
            {
                plan = RunOnePlan(state, plan);
            }

            return TurnResult.Succeeded;
        }
    }

    public sealed class VisualAIState
    {
        public VisualAIState(EntityUid aiEntity, IMap map)
        {
            AIEntity = aiEntity;
            Map = map;

            foreach (var filter in TargetFilters)
                EntitySystem.InjectDependencies(filter);
        }

        public EntityUid AIEntity { get; }
        public IMap Map { get; }
        public IVisualAITargetSource? TargetSource { get; set; }
        public List<IVisualAICondition> TargetFilters { get; } = new() { new IsInFovCondition() };
        public IVisualAITargetOrdering? TargetOrder { get; set; }
        public IVisualAITargetValue? ChosenTarget { get; set; }
        public bool TriedToChooseTarget { get; set; }

        public void ClearTarget()
        {
            TargetFilters.Clear();
            TargetFilters.Add(new IsInFovCondition());
            foreach (var filter in TargetFilters)
                EntitySystem.InjectDependencies(filter);

            TargetOrder = null;
            ChosenTarget = null;
            TriedToChooseTarget = false;
        }
    }

    public enum VisualAIBlockType
    {
        Target,
        Condition,
        Action,
        Special
    }
}