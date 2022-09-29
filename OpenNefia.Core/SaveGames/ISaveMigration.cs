using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.SaveGames
{
    public interface IGlobalSaveMigration
    {
        void Apply(MappingDataNode saveData, IDependencyCollection dependencies);
    }

    public interface IMapSaveMigration
    {
        void Apply(MappingDataNode saveData, MapId mapID, IDependencyCollection dependencies);
    }
}
