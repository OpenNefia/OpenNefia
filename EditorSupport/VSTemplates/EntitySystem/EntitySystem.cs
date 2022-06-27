using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace $rootnamespace$
{
    public interface I$safeitemrootname$ : IEntitySystem 
    {
    }

    public sealed class $safeitemrootname$ : EntitySystem, I$safeitemrootname$
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
        }
    }
}