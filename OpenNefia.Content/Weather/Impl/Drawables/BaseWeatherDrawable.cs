using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Weather.Impl.Drawables
{
    public abstract class BaseWeatherDrawable : BaseDrawable, IWeatherDrawable
    {
        public virtual bool CanRenderInIndoorMaps => false;

        public virtual void Initialize()
        {
        }
    }
}
