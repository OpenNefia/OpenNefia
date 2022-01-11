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
        Cancel
    }
    public record CharaMakeResult(Dictionary<string, object> Added, CharaMakeStep Step = CharaMakeStep.Continue);

    public interface ICharaMakeLayer : IUiLayerWithResult<CharaMakeData, CharaMakeResult>
    {
        void ApplyStep(EntityUid entity);
    }
}
