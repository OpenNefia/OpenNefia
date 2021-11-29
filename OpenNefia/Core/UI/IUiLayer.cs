using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public interface IUiLayer : IUiInputElement, ILoveEventReceiever, ILocalizable
    {
        int ZOrder { get; set; }

        void GetPreferredBounds(out int x, out int y, out int width, out int height);
        void OnQuery();
        void OnQueryFinish();
        bool IsQuerying();
    }
}
