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
using OpenNefia.Content.Fame;
using OpenNefia.Core.Game;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Karma;

namespace OpenNefia.Content.Feats
{
    public interface ICommonFeatsSystem : IEntitySystem
    {
        /// <summary>
        /// Calculates an expense taking into account any reductions stemming from the "Accountant" feat and karma.
        /// </summary>
        /// <param name="goldAmount">Amount in gold</param>
        /// <returns>New gold amount</returns>
        int CalcAdjustedExpenseGold(int goldAmount);
    }

    public sealed class CommonFeatsSystem : EntitySystem, ICommonFeatsSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;

        public int CalcAdjustedExpenseGold(int goldAmount)
        {
            var player = _gameSession.Player;
            var karma = _karma.GetKarma(player);
            var factor = Math.Clamp(100 - karma / 2, 0, 50) * (7 - _feats.Level(player, Protos.Feat.Tax));

            if (karma >= KarmaLevels.Good)
                factor -= 5;

            factor = Math.Clamp(factor, 25, 100);
            double perc = factor / 100.0;
            return (int)(goldAmount * perc);
        }
    }
}