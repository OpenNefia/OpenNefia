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
using OpenNefia.Content.Maps;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Parties;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaItemEffectLogicSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectGainAllyComponent, ApplyEffectDamageEvent>(ApplyDamage_GainAlly);
        }

        private void ApplyDamage_GainAlly(EntityUid uid, EffectGainAllyComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2892 	if (cc!pc)&(cc<maxFollower):txtNothingHappen:swbr ...
            if (args.Handled)
                return;

            if (!_gameSession.IsPlayer(args.Source))
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            var level = args.OutDamage;
            var quality = Quality.Good;
            var filter = component.CharaFilter;

            // TODO CharaFilter.Clone()
            if (_rand.OneIn(3))
                filter.Tags = new[] { Protos.Tag.CharaMan };
            else
                filter.Tags = null;

            filter.CommonArgs.NoLevelScaling = true;
            filter.CommonArgs.NoRandomModify = true;

            var chara = _charaGen.GenerateChara(args.Source, filter);
            if (!IsAlive(chara))
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.CommonArgs.OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            if (component.MessageKey != null)
                _mes.Display(Loc.GetString(component.MessageKey.Value, ("source", args.Source), ("ally", chara.Value)));

            _parties.TryRecruitAsAlly(args.Source, chara.Value);
            
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2910 	rc=nc:gosub *add_ally ...
        }
    }
}