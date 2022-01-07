using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeData
    {
        public Dictionary<Type, Dictionary<string, object>> CharaData { get; set; }

        public CharaMakeData()
        {
            CharaData = new Dictionary<Type, Dictionary<string, object>>();
        }
    }
}
