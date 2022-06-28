using OpenNefia.Content.DisplayName;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Qualities
{
    public sealed class QualitySystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<QualityComponent, GetBaseNameEventArgs>(AddQualityBrackets);
        }

        private void AddQualityBrackets(EntityUid uid, QualityComponent quality, ref GetBaseNameEventArgs args)
        {
            if (quality.Quality.Buffed == Quality.Great)
            {
                args.BaseName = Loc.GetString("Elona.Quality.Brackets.Great", ("name", args.BaseName));
            }
            else if (quality.Quality.Buffed == Quality.God)
            {
                args.BaseName = Loc.GetString("Elona.Quality.Brackets.God", ("name", args.BaseName));
            }
        }
    }
}
