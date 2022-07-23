using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hud
{
    public class BaseHudWidget : UiElement, IHudWidget
    {
        public virtual float PosX { get; set; }
        public virtual float PosY { get; set; }

        public virtual bool Movable => false;

        public virtual void Initialize()
        {
            EntitySystem.InjectDependencies(this);
        }

        public virtual void RefreshWidget()
        {
        }
    }
}
