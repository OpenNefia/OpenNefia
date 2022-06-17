using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Nefia
{
    public sealed class BasicNefiaTemplate : IVanillaNefiaTemplate
    {
        [DataField(required: true)]
        public IVanillaNefiaLayout Layout { get; set; } = new NefiaLayoutStandard();

        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data) => Layout;
    }
}
