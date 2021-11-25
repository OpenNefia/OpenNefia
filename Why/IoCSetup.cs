using Why.Core.IoC;
using Why.Core.Maps;

namespace Why
{
    internal class IoCSetup
    {
        internal static void Run()
        {
            IoCManager.Register<IMapManager, MapManager>();
        }
    }
}