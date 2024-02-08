using OpenNefia.Content.Effects.New;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Effects;

namespace OpenNefia.Content.Items
{
    /// <summary>
    /// Component for handling the effects of the "use" command (<c>t</c>) on items.
    /// Best used for cases in which triggering an effect is all that's needed, and
    /// the item needs no extra state.
    /// For more complex cases involving state on one or more of the item's components,
    /// usually it's better to create a new component and handle <see cref="GetVerbsEventArgs"/>
    /// on it instead.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ItemUseComponent : Component
    {
        /// <summary>
        /// Message displayed when the item is used.
        /// </summary>
        [DataField]
        public LocaleKey? MessageKey { get; set; }

        /// <summary>
        /// Color of the displayed message, if any.
        /// </summary>
        [DataField]
        public Color MessageColor { get; set; } = UiColors.MesWhite;

        /// <summary>
        /// Number of this item to consume per use.
        /// To ensure the item will be available in the effect handlers,
        /// the item will be consumed *after* the effects have been run.
        /// Defaults to <c>0</c>, meaning it won't be consumed.
        /// </summary>
        [DataField]
        public int AmountConsumed { get; set; } = 0;

        /// <summary>
        /// <para>
        /// Effects this item triggers on use.
        /// </para>
        /// <para>
        /// Each effect will receieve the item in question as the
        /// <see cref="EffectCommonArgs.SourceItem"/> property on the
        /// effect arguments. After the effects are run, <see cref="AmountConsumed"/>
        /// is subtracted from the item's amount.
        /// </para>
        /// </summary>
        [DataField]
        public IEffectSpecs Effects { get; set; } = new NullEffectSpec();
    }
}