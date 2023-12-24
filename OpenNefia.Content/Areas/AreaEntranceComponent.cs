using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using static OpenNefia.Core.Prototypes.EntityPrototype;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Areas
{
    [DataDefinition]
    public sealed class GlobalAreaSpec
    {
        [DataField(required: true)]
        public GlobalAreaId ID { get; }

        [DataField]
        public GlobalAreaId? Parent { get; } = null;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class AreaEntranceComponent : Component, IComponentLocalizable
    {
        /// <summary>
        /// If non-null, create a global area with this ID when 
        /// starting a new save.
        /// </summary>
        [DataField("globalArea")]
        public GlobalAreaSpec? GlobalAreaSpec { get; }

        /// <summary>
        /// Starting floor of this area.
        /// </summary>
        [DataField]
        public AreaFloorId StartingFloor { get; set; } = AreaFloorId.Default;

        /// <summary>
        /// Position to place the player on when entering the starting floor. 
        /// This is copied to the generated <see cref="WorldMapEntranceComponent"/>.
        /// </summary>
        [DataField]
        public IMapStartLocation? StartLocation { get; set; }

        /// <summary>
        /// Entity to spawn as the entrance.
        /// Usually inherits from <see cref="Protos.MObj.MapEntrance"/>.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> EntranceEntity { get; set; } = Protos.MObj.MapEntrance;

        /// <summary>
        /// Chip ID to set the entrance to.
        /// Useful when you don't want to go through the effort of defining
        /// an entirely new entity for the area entrance.
        /// </summary>
        [DataField]
        public PrototypeId<ChipPrototype>? ChipID { get; set; }

        /// <summary>
        /// Message to display when the entrance to this area is stepped on.
        /// </summary>
        [Localize]
        public string? EntranceMessage { get; set; }

        void IComponentLocalizable.LocalizeFromLua(NLua.LuaTable table)
        {
            EntranceMessage = table.GetStringOrNull(nameof(EntranceMessage));
        }
    }
}
