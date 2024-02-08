using OpenNefia.Content.Prototypes;
using OpenNefia.VisualAI.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protos_VisualAI = OpenNefia.VisualAI.Prototypes.Protos;

namespace OpenNefia.VisualAI.Engine
{
    public static class VisualAIPlanTemplates
    {
        public static VisualAIPlan Default()
        {
            var plan = new VisualAIPlan();

            VisualAIPlan? trueBranch;
            VisualAIPlan? falseBranch;

            plan.AddBlock(Protos_VisualAI.VisualAIBlock.TargetEnemies);
            plan.AddBlock(Protos_VisualAI.VisualAIBlock.TargetOrderingNearest);
            plan.AddBlock(Protos_VisualAI.VisualAIBlock.ConditionTargetInSight);
            {
                trueBranch = plan.SubplanTrueBranch!;
                trueBranch.AddBlock(Protos_VisualAI.VisualAIBlock.ConditionCanDoMeleeAttack);
                {
                    var plan2 = trueBranch;

                    trueBranch = plan2.SubplanTrueBranch!;
                    trueBranch.AddBlock(Protos_VisualAI.VisualAIBlock.ActionMeleeAttack);

                    falseBranch = plan2.SubplanFalseBranch!;
                    falseBranch.AddBlock(Protos_VisualAI.VisualAIBlock.ActionMoveCloseAsPossible);
                }

                falseBranch = plan.SubplanFalseBranch!;
                falseBranch.AddBlock(Protos_VisualAI.VisualAIBlock.SpecialClearTarget);
                falseBranch.AddBlock(Protos_VisualAI.VisualAIBlock.TargetPlayer);
                falseBranch.AddBlock(Protos_VisualAI.VisualAIBlock.ConditionTargetInSight);
                {
                    var plan2 = falseBranch;

                    trueBranch = plan2.SubplanTrueBranch!;
                    trueBranch.AddBlock(Protos_VisualAI.VisualAIBlock.ActionMoveCloseAsPossible);

                    falseBranch = plan2.SubplanFalseBranch!;
                    falseBranch.AddBlock(Protos_VisualAI.VisualAIBlock.ActionWander);
                }
            }

            return plan;
        }
    }
}
