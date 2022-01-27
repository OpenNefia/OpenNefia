using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public interface IDefferedActionSystem : IEntitySystem
    {
        /// <summary>
        /// Queue an action, which is executed the next time the player gains control.
        /// </summary>
        void EnqeueDefferedAction(DefferedActionSystem.DefferedAction action);
    }

    public class DefferedActionSystem : EntitySystem, IDefferedActionSystem
    {
        public struct DefferedAction
        {
            public Guid ID { get; set; }
            public Action Action { get; set; }

            public DefferedAction(Action action)
            {
                ID = Guid.NewGuid();
                Action = action;
            }
        }
        private Stack<DefferedAction> DefferedActions = new();

        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerTurnStartedEvent>(PlayerTurnStarted, nameof(PlayerTurnStarted));
        }

        private void PlayerTurnStarted(ref PlayerTurnStartedEvent ev)
        {
            for (int i = 0; i < DefferedActions.Count; i++)
            {
                DefferedActions.Pop().Action?.Invoke();
            }
        }

        /// <inheritdoc/>
        public void EnqeueDefferedAction(DefferedAction action)
        {
            DefferedActions.Push(action);
        }
    }
}
