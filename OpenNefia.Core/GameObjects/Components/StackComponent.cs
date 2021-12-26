using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    public class StackComponent : Component
    {
        public override string Name => "Stack";

        /// <summary>
        /// Stack count of this entity. Should not be manually set to a quantity below zero.
        /// </summary>
        [DataField(noCompare: true)]
        public int Count { get; internal set; } = 1;

        /// <summary>
        ///     Set to true to not reduce the count when used.
        /// </summary>
        [DataField]
        public bool Unlimited { get; set; }
    }
}
