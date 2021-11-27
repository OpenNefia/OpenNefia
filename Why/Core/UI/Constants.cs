using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    internal class Constants
    {
        public const int INF_BARH = 16;
        public const int INF_MSGH = 72;
        public const int INF_VERH = INF_BARH + INF_MSGH;

        public const float FRAME_MS = (16.66f / 1000f);
        public const float SCREEN_REFRESH = 20f * FRAME_MS;
    }
}
