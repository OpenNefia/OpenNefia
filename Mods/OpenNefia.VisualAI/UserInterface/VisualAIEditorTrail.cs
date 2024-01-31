using OpenNefia.Core.UI.Element;
using OpenNefia.VisualAI.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIEditorTrail : UiElement
    {
        public VisualAIPlan Plan { get; set; } = new();
    
        public sealed record Data(IReadOnlyList<VisualAITile> Tiles, int SelectedIndex);

        public void Refresh(Data data)
        {

        }
    }
}
