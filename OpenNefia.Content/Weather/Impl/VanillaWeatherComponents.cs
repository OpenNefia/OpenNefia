using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Weather
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Weather)]
    public sealed class WeatherTypeSunnyComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Weather)]
    public sealed class WeatherTypeEtherwindComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Weather)]
    public sealed class WeatherTypeSnowComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Weather)]
    public sealed class WeatherTypeRainComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Weather)]
    public sealed class WeatherTypeHardRainComponent : Component
    {
    }
}
