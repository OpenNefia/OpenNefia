using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Markup
{
    public sealed record class ElonaMarkup(List<ElonaMarkupLine> Lines);
    public sealed record class ElonaMarkupLine(string Text, FontSpec Font);
}
