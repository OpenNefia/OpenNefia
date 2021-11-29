using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using KeybindPrototypeID = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.UI.KeybindPrototype>;

namespace OpenNefia.Core.UI
{
    [Prototype("Keybind")]
    public class KeybindPrototype : IPrototype, IKeybind
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public bool IsShiftDelayed { get; }
    }
    
    public static class Keybind
    {
        public static KeybindPrototypeID Enter = new(nameof(Enter));
        public static KeybindPrototypeID Cancel = new(nameof(Cancel));
        public static KeybindPrototypeID Quit = new(nameof(Quit));
        public static KeybindPrototypeID Escape = new(nameof(Escape));

        public static KeybindPrototypeID UIUp = new(nameof(UIUp));
        public static KeybindPrototypeID UIDown = new(nameof(UIDown));
        public static KeybindPrototypeID UILeft = new(nameof(UILeft));
        public static KeybindPrototypeID UIRight = new(nameof(UIRight));

        public static KeybindPrototypeID North = new(nameof(North));
        public static KeybindPrototypeID South = new(nameof(South));
        public static KeybindPrototypeID West = new(nameof(West));
        public static KeybindPrototypeID East = new(nameof(East));
        public static KeybindPrototypeID Northwest = new(nameof(Northwest));
        public static KeybindPrototypeID Northeast = new(nameof(Northeast));
        public static KeybindPrototypeID Southwest = new(nameof(Southwest));
        public static KeybindPrototypeID Southeast = new(nameof(Southeast));

        public static KeybindPrototypeID Wait = new(nameof(Wait));
        public static KeybindPrototypeID Identify = new(nameof(Identify));
        public static KeybindPrototypeID Mode = new(nameof(Mode));
        public static KeybindPrototypeID Mode2 = new(nameof(Mode2));
        public static KeybindPrototypeID NextPage = new(nameof(NextPage));
        public static KeybindPrototypeID PreviousPage = new(nameof(PreviousPage));

        public static KeybindPrototypeID Repl = new(nameof(Repl));

        public static KeybindPrototypeID SelectionA = new(nameof(SelectionA));
        public static KeybindPrototypeID SelectionB = new(nameof(SelectionB));
        public static KeybindPrototypeID SelectionC = new(nameof(SelectionC));
        public static KeybindPrototypeID SelectionD = new(nameof(SelectionD));
        public static KeybindPrototypeID SelectionE = new(nameof(SelectionE));
        public static KeybindPrototypeID SelectionF = new(nameof(SelectionF));
        public static KeybindPrototypeID SelectionG = new(nameof(SelectionG));
        public static KeybindPrototypeID SelectionH = new(nameof(SelectionH));
        public static KeybindPrototypeID SelectionI = new(nameof(SelectionI));
        public static KeybindPrototypeID SelectionJ = new(nameof(SelectionJ));
        public static KeybindPrototypeID SelectionK = new(nameof(SelectionK));
        public static KeybindPrototypeID SelectionL = new(nameof(SelectionL));
        public static KeybindPrototypeID SelectionM = new(nameof(SelectionM));
        public static KeybindPrototypeID SelectionN = new(nameof(SelectionN));
        public static KeybindPrototypeID SelectionO = new(nameof(SelectionO));
        public static KeybindPrototypeID SelectionP = new(nameof(SelectionP));
        public static KeybindPrototypeID SelectionQ = new(nameof(SelectionQ));
        public static KeybindPrototypeID SelectionR = new(nameof(SelectionR));

        public static Dictionary<Keys, KeybindPrototypeID> SelectionKeys = new()
            {
                { Keys.A, SelectionA },
                { Keys.B, SelectionB },
                { Keys.C, SelectionC },
                { Keys.D, SelectionD },
                { Keys.E, SelectionE },
                { Keys.F, SelectionF },
                { Keys.G, SelectionG },
                { Keys.H, SelectionH },
                { Keys.I, SelectionI },
                { Keys.J, SelectionJ },
                { Keys.K, SelectionK },
                { Keys.L, SelectionL },
                { Keys.M, SelectionM },
                { Keys.N, SelectionN },
                { Keys.O, SelectionO },
                { Keys.P, SelectionP },
                { Keys.Q, SelectionQ },
                { Keys.R, SelectionR },
            };
    }
}
