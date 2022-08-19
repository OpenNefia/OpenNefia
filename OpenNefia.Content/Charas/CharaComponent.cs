using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System.Security.Cryptography;

namespace OpenNefia.Content.Charas
{
    [RegisterComponent]
    public class CharaComponent : Component
    {
        public override string Name => "Chara";

        /// <summary>
        /// Race of this character.
        /// </summary>
        /// <remarks>
        /// NOTE: Do not set this manually, skills/stats/components won't be updated.
        /// </remarks>
        [DataField(required: true)]
        public PrototypeId<RacePrototype> Race { get; set; } = Protos.Race.Juere;

        /// <summary>
        /// Slot to hold components added by race.
        /// </summary>
        // TODO might be a bad idea since we may want to merge component fields
        // from the RacePrototype instead, but that is incompatible with slots.
        public SlotId RaceSlot { get; set; }

        /// <summary>
        /// Class of this character.
        /// </summary>
        /// <remarks>
        /// NOTE: Do not set this manually, skills/stats/components won't be updated.
        /// </remarks>
        [DataField(required: true)]
        public PrototypeId<ClassPrototype> Class { get; set; } = Protos.Class.Warrior;

        /// <summary>
        /// Slot to hold components added by class.
        /// </summary>
        // TODO might be a bad idea since we may want to merge component fields
        // from the ClassPrototype instead, but that is incompatible with slots.
        [DataField]
        public SlotId ClassSlot { get; set; }

        [DataField]
        public Gender Gender { get; set; } = Gender.Unknown;

        /// <summary>
        /// Date of respawn for the <see cref="VillagerDead"/> state.
        /// </summary>
        [DataField]
        public GameDateTime RevivalDate { get; set; } = new();

        // TODO PreciousComponent?
        [DataField]
        public bool IsPrecious { get; set; }

        [ComponentDependency]
        private MetaDataComponent? _metaData;

        [DataField(required: true)]
        private CharaLivenessState _liveness = CharaLivenessState.Alive;
        
        public CharaLivenessState Liveness
        {
            get => _liveness;
            set
            {
                _liveness = value;
                _metaData!.Liveness = GetGeneralLivenessState(value);
            }
        }

        private static EntityGameLiveness GetGeneralLivenessState(CharaLivenessState charaLiveness)
        {
            switch (charaLiveness)
            {
                case CharaLivenessState.Alive:
                    return EntityGameLiveness.Alive;
                case CharaLivenessState.PetDead:
                case CharaLivenessState.PetWait:
                case CharaLivenessState.VillagerDead:
                    return EntityGameLiveness.Hidden;
                case CharaLivenessState.Dead:
                default:
                    return EntityGameLiveness.DeadAndBuried;
            }
        }
    }

    public enum CharaLivenessState
    {
        Alive,
        PetDead,
        PetWait,
        VillagerDead,
        Dead,
        AdventurerHospital
    }

    public enum Gender : int
    {
        Male = 0,
        Female = 1,
        Unknown = 2
    }
}