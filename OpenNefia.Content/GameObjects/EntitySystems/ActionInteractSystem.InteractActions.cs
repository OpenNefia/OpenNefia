using OpenNefia.Content.CharaAppearance;
using OpenNefia.Content.Combat;
using OpenNefia.Core.Locale;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.CharaInfo;
using static OpenNefia.Content.CharaInfo.CharaGroupSublayerArgs;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Inventory;

namespace OpenNefia.Content.GameObjects
{
    public sealed partial class ActionInteractSystem
    {
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ICustomNameSystem _customNames = default!;

        private TurnResult InteractAction_Talk(EntityUid source, EntityUid target)
        {
            return _dialog.TryToChatWith(source, target);
        }
        
        private TurnResult InteractAction_Attack(EntityUid source, EntityUid target)
        {
            // TODO
            return _combat.MeleeAttack(source, target) ?? TurnResult.Succeeded;
        }

        private TurnResult InteractAction_Inventory(EntityUid source, EntityUid target)
        {
            _mes.Display("TODO", UiColors.MesYellow);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_Give(EntityUid source, EntityUid target)
        {
            _mes.Display("TODO", UiColors.MesYellow);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_Appearance(EntityUid source, EntityUid target)
        {
            var args = new CharaAppearanceLayer.Args(target);
            _uiManager.Query<CharaAppearanceLayer, CharaAppearanceLayer.Args>(args);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_TeachWords(EntityUid source, EntityUid target)
        {
            if (HasComp<TaughtWordsComponent>(target))
                EntityManager.RemoveComponent<TaughtWordsComponent>(target);

            var args = new TextPrompt.Args(20, isCancellable: true, queryText: Loc.GetString("Elona.Tone.ChangeTone.Prompt", ("entity", target)));
            var result = _uiManager.Query<TextPrompt, TextPrompt.Args, TextPrompt.Result>(args);
            
            if (result.HasValue && !string.IsNullOrWhiteSpace(result.Value.Text))
            {
                EnsureComp<TaughtWordsComponent>(target).TaughtWords.Add(result.Value.Text);
                _mes.Display(result.Value.Text, UiColors.MesSkyBlue);
            }

            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_ChangeTone(EntityUid source, EntityUid target)
        {
            _mes.Display("TODO", UiColors.MesYellow);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_Release(EntityUid source, EntityUid target)
        {
            _audio.Play(Protos.Sound.Build1, target);
            EntityManager.RemoveComponent<SandBaggedComponent>(target);
            _mes.Display(Loc.GetString("Elona.Interact.Release", ("source", source), ("entity", target)));
            _refresh.Refresh(target);
            
            return TurnResult.Succeeded;
        }

        private TurnResult InteractAction_Name(EntityUid source, EntityUid target)
        {
            _customNames.PromptForNewName(target);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_Info(EntityUid source, EntityUid target)
        {
            var context = new CharaUiGroupArgs(CharaTab.CharaInfo, target);
            var result = _uiManager.Query<CharaUiGroup, CharaUiGroupArgs, CharaGroupSublayerResult>(context);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_Items(EntityUid source, EntityUid target)
        {
            var context = new InventoryContext(source, target, new ExamineInventoryBehavior());
            _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);
            return TurnResult.Aborted;
        }

        private TurnResult InteractAction_Equipment(EntityUid source, EntityUid target)
        {
            var context = new EquipmentLayer.Args(source, target);
            var result = _uiManager.Query<EquipmentLayer, EquipmentLayer.Args, EquipmentLayer.Result>(context);
            return result.HasValue && result.Value.ChangedEquipment ? TurnResult.Succeeded : TurnResult.Aborted;
        }
    }
}
