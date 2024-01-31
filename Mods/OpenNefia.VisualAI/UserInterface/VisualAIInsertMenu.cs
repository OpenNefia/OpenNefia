using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Layer;
using OpenNefia.VisualAI.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIInsertMenu : UiLayerWithResult<VisualAIInsertMenu.Args, VisualAIInsertMenu.Result>
    {
        public class Args
        {
            public VisualAIBlockType Category { get; set; }
            public PrototypeId<VisualAIBlockPrototype>? BlockID { get; set; }
        }

        public class Result
        {
            public Result(VisualAIBlock block)
            {
                Block = block;
            }

            public VisualAIBlock Block { get; set; }
        }
    }
}
