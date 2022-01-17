using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests
{
    public class TempSaveGameHandle : ISaveGameHandle, IDisposable
    {
        private readonly TempWritableDirProvider _temp;
        private readonly TempWritableDirProvider _committed;

        public int SaveFormatVersion => SaveGameManager.SaveFormatVersion;
        public ResourcePath SaveDirectory { get; }
        public DateTime LastWriteTime { get; }
        public SaveGameHeader Header { get; }
        public ISaveGameDirProvider Files { get; }

        public TempSaveGameHandle(string prefix = "TempSave")
        {
            _temp = new TempWritableDirProvider(prefix);
            _committed = new TempWritableDirProvider(prefix);

            SaveDirectory = new(Path.GetFileName(_committed.RootDir));
            LastWriteTime = DateTime.MinValue;
            Header = new SaveGameHeader("TempSave");
            Files = new SaveGameDirProvider(_temp, _committed);
        }

        public void Dispose()
        {
            _temp.Dispose();
            _committed.Dispose();
        }
    }
}
