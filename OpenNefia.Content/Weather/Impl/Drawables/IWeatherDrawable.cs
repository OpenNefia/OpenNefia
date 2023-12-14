using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Weather
{
    public interface IWeatherDrawable : IDrawable
    {
        bool CanRenderInIndoorMaps { get; }

        public void Initialize();
    }
}
