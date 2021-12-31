using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenNefia.Core.Utility;
#if EXCEPTION_TOLERANCE
using OpenNefia.Core.Exceptions;
#endif
using DependencyAttribute = OpenNefia.Core.IoC.DependencyAttribute;

namespace OpenNefia.Core.GameObjects
{
    /// <inheritdoc />
    public partial class EntityManager
    {
        /// <inheritdoc />
        public bool IsAlive([NotNullWhen(true)] EntityUid? uid)
        {
            if (uid == null || !EntityExists(uid.Value) || !TryGetComponent<MetaDataComponent>(uid.Value, out var metadata))
                return false;

            return metadata.IsAlive;
        }

        /// <inheritdoc />
        public bool IsDeadAndBuried(EntityUid? uid)
        {
            if (uid == null || !EntityExists(uid.Value) || !TryGetComponent<MetaDataComponent>(uid.Value, out var metadata))
                return true;

            return metadata.IsDeadAndBuried;
        }
    }
}
