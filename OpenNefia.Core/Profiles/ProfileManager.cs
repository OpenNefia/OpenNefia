using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Profiles
{
    public class ProfileManager : IProfileManager
    {
        [Dependency] private readonly IResourceManager _resourceManager = default!;

        /// <inheritdoc />
        // NOTE: For now, this is located directly under /UserData.
        public ResourcePath CurrentProfileRoot => new("/Profiles/Default");

        /// <inheritdoc />
        public IWritableDirProvider CurrentProfile { get; private set; } = default!;

        public void Initialize()
        {
            CurrentProfile = _resourceManager.UserData.GetChild(CurrentProfileRoot);
        }
    }
}
