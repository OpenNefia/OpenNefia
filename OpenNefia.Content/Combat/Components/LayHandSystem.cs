using OpenNefia.Content.Damage;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
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

namespace OpenNefia.Content.Combat
{
    public sealed class LayHandSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IAudioManager _audio = default!;


        public override void Initialize()
        {
            SubscribeComponent<LayHandComponent, EntityBeingGeneratedEvent>(InitLayHand);
            SubscribeComponent<SkillsComponent, AfterDamageAppliedEvent>(ProcLayHand);
        }

        private void InitLayHand(EntityUid uid, LayHandComponent component, ref EntityBeingGeneratedEvent args)
        {
            component.HasLayHand = true;
        }

        private void ProcLayHand(EntityUid inTrouble, SkillsComponent skills, ref AfterDamageAppliedEvent ev)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1486 	if cHP(tc)<0 : if tc<maxFollower{ ...
            if (!_parties.IsLeaderOfSomeParty(inTrouble))
                return;

            if (skills.HP < 0)
            {
                foreach (var ally in _parties.EnumerateUnderlings(inTrouble))
                {
                    if (EntityManager.IsAlive(ally)
                        && TryComp<LayHandComponent>(ally, out var layHand)
                        && layHand.HasLayHand)
                    {
                        layHand.HasLayHand = false;

                        _mes.Display(Loc.GetString("Elona.LayHand.Dialog", ("healer", ally)));
                        _mes.Display(Loc.GetString("Elona.LayHand.IsHealed", ("entity", inTrouble)));

                        skills.HP = skills.MaxHP / 2;
                        _audio.Play(Protos.Sound.Pray2, inTrouble);

                        return;
                    }
                }
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:1500 		} ...
        }
    }
}