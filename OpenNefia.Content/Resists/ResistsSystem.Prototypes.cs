namespace OpenNefia.Content.Resists
{
    public sealed partial class ResistsSystem
    {
        /// <inheritdoc/>
        public IEnumerable<ElementPrototype> EnumerateResistableElements()
        {
            return _protos.EnumeratePrototypes<ElementPrototype>()
                .Where(elemProto => elemProto.CanResist);
        }
    }
}
