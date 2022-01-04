using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeData
    {
        public Dictionary<Type, Dictionary<string, object>> CharData { get; set; }

        public CharaMakeData()
        {
            CharData = new Dictionary<Type, Dictionary<string, object>>();
        }
    }
}
