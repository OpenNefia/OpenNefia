using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class StackComponent : Component
    {
        public override string Name => "Stack";

        [ComponentDependency]
        private MetaDataComponent? _metaData;

        [DataField("count", noCompare: true)]
        private int _count = 1;

        /// <summary>
        /// Amount of this entity. Cannot be set to a quantity below zero.
        /// </summary>
        /// <remarks>
        /// Normally this would be ECS instead of having the logic in the component,
        /// but since it's so integral to significant portions of the game I opted
        /// for having a getter/setter instead. Everything else related to stacking
        /// should go in <see cref="StackSystem"/> instead, though.
        /// </remarks>
        public int Count
        {
            get => _count;
            set
            {
                _count = value;

                if (_count <= 0)
                {
                    _count = 0;
                    _metaData!.Liveness = EntityGameLiveness.DeadAndBuried;
                }
                else
                {
                    _metaData!.Liveness = EntityGameLiveness.Alive;
                }
            }
        }
    }
}
