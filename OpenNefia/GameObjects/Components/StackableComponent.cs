using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class StackableComponent : Component
    {
        public override string Name => "Stackable";

        [ComponentDependency]
        private MetaDataComponent? _metaData;

        // The required dependency on _metaData isn't filled out when deserializing,
        // so set the amount directly.
        [DataField("amount")]
        private int _amount = 1;

        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;

                if (_amount <= 0)
                {
                    _amount = 0;
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
