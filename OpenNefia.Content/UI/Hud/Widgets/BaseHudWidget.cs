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
    public class BaseHudWidget : BaseDrawable, IHudWidget
    {
        public virtual float PosX { get; set; }
        public virtual float PosY { get; set; }

        public virtual bool Movable => false;

        public virtual void Initialize()
        {
            EntitySystem.InjectDependencies(this);
        }

        public override void Draw()
        {
        }

        public override void Update(float dt)
        {
        }

        public virtual void UpdateWidget()
        {
        }
    }
}
