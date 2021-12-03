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
    public sealed partial class EntityManager
    {
        /// <inheritdoc />
        public bool IsAlive(EntityUid uid)
        {
            if (!EntityExists(uid) || !TryGetComponent<MetaDataComponent>(uid, out var metadata))
                return false;

            return metadata.IsAlive;
        }

        /// <inheritdoc />
        public bool IsDeadAndBuried(EntityUid uid)
        {
            if (!EntityExists(uid) || !TryGetComponent<MetaDataComponent>(uid, out var metadata))
                return true;

            return metadata.IsDeadAndBuried;
        }
    }
}
