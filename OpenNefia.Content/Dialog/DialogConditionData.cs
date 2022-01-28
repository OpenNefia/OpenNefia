using OpenNefia.Content.Inventory;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    /// <summary>
    /// Classes which allow for game data to be used to check for conditions
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public abstract class DialogConditionData
    {
        public abstract bool IsConditionFulfilled(DialogContextData context);
    }

    /// <summary>
    /// Returns true if the player has any item with the given ID
    /// </summary>
    public sealed class HasItemCondition : DialogConditionData
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        [DataField(required: true)]
        public PrototypeId<EntityPrototype> Item { get; } = default!;

        public override bool IsConditionFulfilled(DialogContextData context)
        {
            EntitySystem.InjectDependencies(this);
            if (!_entMan.TryGetComponent(GameSession.Player, out InventoryComponent inv))
                return false;

            return inv.Container.ContainedEntities.Any(x =>
            {
                if (!_entMan.TryGetComponent(x, out MetaDataComponent meta))
                    return false;

                return meta.EntityPrototype?.GetStrongID() == Item;
            });
        }
    }


}
