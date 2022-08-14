using OpenNefia.Content.Currency;
using OpenNefia.Content.EtherDisease;
using OpenNefia.Content.Fame;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.Skills;
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

namespace OpenNefia.Content.TurnOrder
{
    public sealed class PlayerDeathSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IEtherDiseaseSystem _etherDisease = default!;
        [Dependency] private readonly IFameSystem _fame = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;

        public override void Initialize()
        {
            SubscribeEntity<PlayerRevivingEvent>(HandlePlayerReviving);
        }

        private void HandlePlayerReviving(EntityUid player, PlayerRevivingEvent args)
        {
            // >>>>>>>> shade2/main.hsp:1789  ...
            if (_levels.GetLevel(player) > 5)
            {
                ApplyDeathPenalty(player);
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.PlayerDeath.PenaltyNotApplied"));
            }

            if (TryComp<EtherDiseaseComponent>(player, out var etherDisease)
                && etherDisease.Corruption >= EtherDiseaseSystem.EtherDiseaseDeathThreshold)
            {
                _etherDisease.ModifyCorruption(player, -2000, etherDisease);
            }

            if (TryComp<WalletComponent>(player, out var wallet))
            {
                _mes.Display(Loc.GetString("Elona.PlayerDeath.YouLostSomeMoney"));
                wallet.Gold /= 3;
            }

            _fame.DecrementFame(player, 10);

            _refresh.Refresh(player);
            _saveLoad.QueueAutosave();
            // <<<<<<<< shade2/main.hsp:1816 	swbreak ..
        }

        private void ApplyDeathPenalty(EntityUid player)
        {
            var attr = _skills.PickRandomBaseAttribute();
            if (_skills.Level(player, attr) > 0 && _rand.OneIn(3))
            {
                _skills.GainSkillExp(player, attr, -500);
            }

            if (_karma.GetKarma(player) < KarmaSystem.KarmaThresholdBad)
            {
                _karma.ModifyKarma(player, 10);
            }
        }
    }
}