using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Profiles
{
    public interface IProfileManager
    {
        public ResourcePath CurrentProfileRoot { get; }
        public IWritableDirProvider CurrentProfile { get; }

        void Initialize();
    }
}