using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Metadata component used to keep consistent UIDs inside map files cross saving.
    /// </summary>
    /// <remarks>
    ///     This component stores the previous map UID of entities from map load.
    ///     This can then be used to re-serialize the entity with the same UID for the merge driver to recognize.
    /// </remarks>
    public sealed class MapSaveIdComponent : Component
    {
        [DataField(noCompare: true)]
        public int Uid { get; set; }
    }
}
