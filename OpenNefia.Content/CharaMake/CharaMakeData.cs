using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeData
    {
        public Dictionary<Type, Dictionary<string, object>> CharaData { get; set; }
        public CharaMakeStep LastStep { get; set; }

        public CharaMakeData()
        {
            CharaData = new Dictionary<Type, Dictionary<string, object>>();
        }

        public bool TryGetValues(string key, out IEnumerable<object> vals)
        {
            vals = CharaData.Values.Where(x => x.TryGetValue(key, out _)).Select(x => x[key]);
            return vals.Any();
        }

        /// <summary>
        /// Returns the first object with the correct type in the data
        /// </summary>
        public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? val)
        {
            val = default!;
            if (TryGetValues(key, out var vals))
            {
                foreach (var obj in vals)
                {
                    if (obj is T tObj)
                    {
                        val = tObj;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
