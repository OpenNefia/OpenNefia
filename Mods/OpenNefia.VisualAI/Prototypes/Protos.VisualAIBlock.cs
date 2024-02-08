using VisualAIBlockPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.VisualAI.Engine.VisualAIBlockPrototype>;

namespace OpenNefia.VisualAI.Prototypes
{
    public static partial class Protos
    {
        public static class VisualAIBlock
        {
            #pragma warning disable format

            #region Target

            public static readonly VisualAIBlockPrototypeId TargetPlayer                   = new($"VisualAI.{nameof(TargetPlayer)}");
            public static readonly VisualAIBlockPrototypeId TargetSelf                     = new($"VisualAI.{nameof(TargetSelf)}");
            public static readonly VisualAIBlockPrototypeId TargetAllies                   = new($"VisualAI.{nameof(TargetAllies)}");
            public static readonly VisualAIBlockPrototypeId TargetEnemies                  = new($"VisualAI.{nameof(TargetEnemies)}");
            public static readonly VisualAIBlockPrototypeId TargetInventory                = new($"VisualAI.{nameof(TargetInventory)}");
            public static readonly VisualAIBlockPrototypeId TargetGroundItems              = new($"VisualAI.{nameof(TargetGroundItems)}");
            public static readonly VisualAIBlockPrototypeId TargetCharacters               = new($"VisualAI.{nameof(TargetCharacters)}");
            public static readonly VisualAIBlockPrototypeId TargetPlayerTargetingCharacter = new($"VisualAI.{nameof(TargetPlayerTargetingCharacter)}");
            public static readonly VisualAIBlockPrototypeId TargetSpecificLocation         = new($"VisualAI.{nameof(TargetSpecificLocation)}");
            public static readonly VisualAIBlockPrototypeId TargetHpMpSpThreshold          = new($"VisualAI.{nameof(TargetHpMpSpThreshold)}");
            public static readonly VisualAIBlockPrototypeId TargetTag                      = new($"VisualAI.{nameof(TargetTag)}");
            public static readonly VisualAIBlockPrototypeId TargetOrderingNearest          = new($"VisualAI.{nameof(TargetOrderingNearest)}");
            public static readonly VisualAIBlockPrototypeId TargetOrderingFurthest         = new($"VisualAI.{nameof(TargetOrderingFurthest)}");
            public static readonly VisualAIBlockPrototypeId TargetOrderingHpMpSp           = new($"VisualAI.{nameof(TargetOrderingHpMpSp)}");

            #endregion

            #region Condition

            public static readonly VisualAIBlockPrototypeId ConditionTargetInSight     = new($"VisualAI.{nameof(ConditionTargetInSight)}");
            public static readonly VisualAIBlockPrototypeId ConditionTargetTileDist    = new($"VisualAI.{nameof(ConditionTargetTileDist)}");
            public static readonly VisualAIBlockPrototypeId ConditionSpellInRange      = new($"VisualAI.{nameof(ConditionSpellInRange)}");
            public static readonly VisualAIBlockPrototypeId ConditionActionInRange     = new($"VisualAI.{nameof(ConditionActionInRange)}");
            public static readonly VisualAIBlockPrototypeId ConditionRandomChance      = new($"VisualAI.{nameof(ConditionRandomChance)}");
            public static readonly VisualAIBlockPrototypeId ConditionCanDoMeleeAttack  = new($"VisualAI.{nameof(ConditionCanDoMeleeAttack)}");
            public static readonly VisualAIBlockPrototypeId ConditionCanDoRangedAttack = new($"VisualAI.{nameof(ConditionCanDoRangedAttack)}");
            public static readonly VisualAIBlockPrototypeId ConditionHpMpSpThreshold   = new($"VisualAI.{nameof(ConditionHpMpSpThreshold)}");

            #endregion

            #region Action

            public static readonly VisualAIBlockPrototypeId ActionMoveCloseAsPossible  = new($"VisualAI.{nameof(ActionMoveCloseAsPossible)}");
            public static readonly VisualAIBlockPrototypeId ActionMoveWithinDistance   = new($"VisualAI.{nameof(ActionMoveWithinDistance)}");
            public static readonly VisualAIBlockPrototypeId ActionRetreatFromTarget    = new($"VisualAI.{nameof(ActionRetreatFromTarget)}");
            public static readonly VisualAIBlockPrototypeId ActionRetreatUntilDistance = new($"VisualAI.{nameof(ActionRetreatUntilDistance)}");
            public static readonly VisualAIBlockPrototypeId ActionMeleeAttack          = new($"VisualAI.{nameof(ActionMeleeAttack)}");
            public static readonly VisualAIBlockPrototypeId ActionRangedAttack         = new($"VisualAI.{nameof(ActionRangedAttack)}");
            public static readonly VisualAIBlockPrototypeId ActionCastSpell            = new($"VisualAI.{nameof(ActionCastSpell)}");
            public static readonly VisualAIBlockPrototypeId ActionInvokeAction         = new($"VisualAI.{nameof(ActionInvokeAction)}");
            public static readonly VisualAIBlockPrototypeId ActionPickUp               = new($"VisualAI.{nameof(ActionPickUp)}");
            public static readonly VisualAIBlockPrototypeId ActionDrop                 = new($"VisualAI.{nameof(ActionDrop)}");
            public static readonly VisualAIBlockPrototypeId ActionWander               = new($"VisualAI.{nameof(ActionWander)}");
            public static readonly VisualAIBlockPrototypeId ActionDoNothing            = new($"VisualAI.{nameof(ActionDoNothing)}");

            #endregion

            #region Special

            public static readonly VisualAIBlockPrototypeId SpecialClearTarget = new($"VisualAI.{nameof(SpecialClearTarget)}");

            #endregion
            
            #pragma warning restore format
        }
    }
}
