using OpenNefia.Content.EntityGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public enum CharaMakeStep
    {
        Continue,
        Repeat,
        GoBack,
        Restart,
        Cancel
    }
    public record CharaMakeUIResult(ICharaMakeResult? Added, CharaMakeStep Step = CharaMakeStep.Continue);

    public interface ICharaMakeLayer : IUiLayerWithResult<CharaMakeResultSet, CharaMakeUIResult>
    {
    }

    public interface ICharaMakeLayer<T> : ICharaMakeLayer
        where T : ICharaMakeResult
    {
    }
}
