namespace OpenNefia.Core.UI
{
    public static class CoreKeybinds
    {
        public static Keybind Enter = nameof(Enter);
        public static Keybind Cancel = nameof(Cancel);
        public static Keybind Quit = nameof(Quit);
        public static Keybind Escape = nameof(Escape);

        public static Keybind UIUp = nameof(UIUp);
        public static Keybind UIDown = nameof(UIDown);
        public static Keybind UILeft = nameof(UILeft);
        public static Keybind UIRight = nameof(UIRight);

        public static Keybind North = nameof(North);
        public static Keybind South = nameof(South);
        public static Keybind West = nameof(West);
        public static Keybind East = nameof(East);
        public static Keybind Northwest = nameof(Northwest);
        public static Keybind Northeast = nameof(Northeast);
        public static Keybind Southwest = nameof(Southwest);
        public static Keybind Southeast = nameof(Southeast);

        public static Keybind Ascend = nameof(Ascend);
        public static Keybind Descend = nameof(Descend);

        public static Keybind Activate = nameof(Activate);

        public static Keybind Wait = nameof(Wait);
        public static Keybind Identify = nameof(Identify);
        public static Keybind Mode = nameof(Mode);
        public static Keybind Mode2 = nameof(Mode2);
        public static Keybind NextPage = nameof(NextPage);
        public static Keybind PreviousPage = nameof(PreviousPage);

        public static Keybind Repl = nameof(Repl);

        public static Keybind SelectionA = nameof(SelectionA);
        public static Keybind SelectionB = nameof(SelectionB);
        public static Keybind SelectionC = nameof(SelectionC);
        public static Keybind SelectionD = nameof(SelectionD);
        public static Keybind SelectionE = nameof(SelectionE);
        public static Keybind SelectionF = nameof(SelectionF);
        public static Keybind SelectionG = nameof(SelectionG);
        public static Keybind SelectionH = nameof(SelectionH);
        public static Keybind SelectionI = nameof(SelectionI);
        public static Keybind SelectionJ = nameof(SelectionJ);
        public static Keybind SelectionK = nameof(SelectionK);
        public static Keybind SelectionL = nameof(SelectionL);
        public static Keybind SelectionM = nameof(SelectionM);
        public static Keybind SelectionN = nameof(SelectionN);
        public static Keybind SelectionO = nameof(SelectionO);
        public static Keybind SelectionP = nameof(SelectionP);
        public static Keybind SelectionQ = nameof(SelectionQ);
        public static Keybind SelectionR = nameof(SelectionR);

        public static Dictionary<Keys, Keybind> SelectionKeys = new()
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
