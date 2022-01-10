using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Locale
{
    /// <summary>
    ///     Contains based localized entity prototype data.
    /// </summary>
    /// <param name="Attributes">Any extra attributes that can be used for localization, such as gender, proper, ...</param>
    public record EntityLocData(
        ImmutableDictionary<string, string> Attributes);
}
