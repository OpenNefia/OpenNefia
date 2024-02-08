using OpenNefia.Content.Logic;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.VisualAI.UserInterface;

namespace OpenNefia.VisualAI
{
    /// <summary>
    /// Visual AI is a mod that lets you create custom procedural AI routines
    /// for your allies with a grid-based system.
    /// </summary>
    public class EntryPoint : ModEntryPoint
    {
        public override void PreInit()
        {
            var inputMan = IoCManager.Resolve<IInputManager>();
            VisualAIKeyFunctions.SetupContexts(inputMan.Contexts);
        }

        public override void Init()
        {
        }

        public override void PostInit()
        {
        }
    }
}