using OpenNefia.Content.Charas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [RegisterComponent]
    public class DialogComponent : Component
    {
        public override string Name => "Dialog";

        [DataField]
        public PrototypeId<DialogPrototype>? DialogID;

        [DataField]
        public bool CanTalk { get; set; } = false;

        [DataField]
        public int Interest { get; set; } = 100;

        [DataField]
        public int Impression { get; set; } = Impressions.Normal;

        /// <summary>
        /// Personality type, typically from 0-3.
        /// This affects certain random dialog text for villagers.
        /// </summary>
        /// <remarks>
        /// TODO: This will be replaced by the custom talk system someday.
        /// It is currently an index into some locale list data.
        /// </remarks>
        [DataField]
        public int Personality { get; set; } = 0;

        /// <summary>
        /// Copula type for certain sentence endings in the Japanese translation.
        /// Not used by other languages.
        /// </summary>
        [DataField]
        public CopulaType CopulaType { get; set; } = CopulaType.Desu;
    }

    public static class Impressions
    {
        // >>>>>>>> elona122/shade2/init.hsp:19 	#define global defImpEnemy	0 ..
        public const int Enemy = 0;
        public const int Hate = 25;
        public const int Normal = 50;
        public const int Party = 53;
        public const int Amiable = 75;
        public const int Friend = 100;
        public const int Fellow = 150;
        public const int Marry = 200;
        public const int Soulmate = 300;
        // <<<<<<<< elona122/shade2/init.hsp:27 	#define global defImpSoulMate	300 ..
    }

    public enum CopulaType
    {
        Desu,
        Daze,
        Dayo,
        Da,
        Ja,
        DeGozaru,
        Ssu
    }
}
