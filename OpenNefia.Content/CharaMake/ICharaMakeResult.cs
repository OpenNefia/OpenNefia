using OpenNefia.Content.EntityGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public interface ICharaMakeResult
    {
        void ApplyStep(EntityUid entity, EntityGenArgSet args);
    }

    public abstract class CharaMakeResult : ICharaMakeResult
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;

        public abstract void ApplyStep(EntityUid entity, EntityGenArgSet args);
    }

    public sealed class CharaMakeNoResult : ICharaMakeResult
    {
        public void ApplyStep(EntityUid entity, EntityGenArgSet args)
        {
        }
    }
}
