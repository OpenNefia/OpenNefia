using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Talk
{
    public interface ITalkSystem : IEntitySystem
    {
        bool Say(EntityUid ent, string talkId);
    }

    public sealed class TalkSystem : EntitySystem, ITalkSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
        }

        public bool Say(EntityUid ent, string talkId)
        {
            // TODO
            return false;
        }
    }
}