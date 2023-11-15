using OpenNefia.Content.Dialog;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Quests;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using static OpenNefia.Content.Quests.VanillaQuestsSystem;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Roles
{
    public interface IRoleSystem : IEntitySystem
    {
        IEnumerable<IRoleComponent> EnumerateRoles(EntityUid uid);
        bool HasAnyRoles(EntityUid uid);
        void RemoveAllRoles(EntityUid uid);
    }

    public sealed class RoleSystem : EntitySystem, IRoleSystem
    {
        [Dependency] private readonly IQuestSystem _quests = default!;

        public IEnumerable<IRoleComponent> EnumerateRoles(EntityUid uid)
        {
            return EntityManager.GetComponents(uid).WhereAssignable<IComponent, IRoleComponent>();
        }

        public bool HasAnyRoles(EntityUid uid)
        {
            foreach (var comp in EntityManager.GetComponents(uid))
            {
                if (comp is IRoleComponent)
                    return true;
            }
            return false;
        }

        public void RemoveAllRoles(EntityUid uid)
        {
            foreach (var comp in EnumerateRoles(uid).ToList())
            {
                EntityManager.RemoveComponent(uid, comp);
            }
        }
    }
}