using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
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
using OpenNefia.Core.Game;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Skills;

namespace OpenNefia.Content.Items.Impl
{
    public interface ITightRopeSystem : IEntitySystem
    {
        TurnResult UseTightRope(EntityUid source, EntityUid target, TightRopeComponent? tightRope = null);
    }

    public sealed class TightRopeSystem : EntitySystem, ITightRopeSystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;

        public override void Initialize()
        {
            SubscribeComponent<TightRopeComponent, GetVerbsEventArgs>(GetVerbs_TightRope);
        }

        private void GetVerbs_TightRope(EntityUid uid, TightRopeComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Use Tight Rope", () => UseTightRope(args.Source, args.Target)));
        }

        public TurnResult UseTightRope(EntityUid source, EntityUid target, TightRopeComponent? tightRope = null)
        {
            // >>>>>>>> elona122/shade2/action.hsp:2074 	case effRope ...
            if (!Resolve(target, ref tightRope))
                return TurnResult.Aborted;

            if (TryComp<SkillsComponent>(source, out var skills)
                && _gameSession.IsPlayer(source) 
                && _playerQuery.YesOrNo(Loc.GetString("Elona.Item.TightRope.Prompt")))
            {
                _damage.DamageHP(source, Math.Max(skills.HP + 1, 99999), damageType: new GenericDamageType("Elona.DamageType.Hanging"));
                return TurnResult.Succeeded;
            }

            return TurnResult.Aborted;
            // <<<<<<<< elona122/shade2/action.hsp:2079 	swbreak ...
        }
    }
}