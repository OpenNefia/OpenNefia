using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.GameObjects
{
    public class StairsSystem : EntitySystem
    {
        public const string VerbTypeAscend = "Elona.Ascend";
        public const string VerbTypeDescend = "Elona.Descend";
        public const string VerbTypeActivate = "Elona.Activate";

        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;

        public override void Initialize()
        {
            SubscribeComponent<StairsComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, StairsComponent component, GetVerbsEventArgs args)
        {
            switch (component.Direction)
            {
                case StairsDirection.Up:
                    args.OutVerbs.Add(new Verb(VerbTypeAscend, "Ascend Stairs", () => UseStairs(args.Source, args.Target)));
                    break;
                case StairsDirection.Down:
                    args.OutVerbs.Add(new Verb(VerbTypeDescend, "Descend Stairs", () => UseStairs(args.Source, args.Target)));
                    break;
            }

            args.OutVerbs.Add(new Verb(VerbTypeActivate, "Use Stairs", () => UseStairs(args.Source, args.Target)));
        }

        private TurnResult UseStairs(EntityUid user, EntityUid entrance,
            StairsComponent? stairs = null)
        {
            if (!Resolve(entrance, ref stairs))
                return TurnResult.Aborted;

            return _mapEntrances.UseMapEntrance(user, stairs.Entrance) 
                ? TurnResult.Succeeded : TurnResult.Aborted;
        }
    }
}
