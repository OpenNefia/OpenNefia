using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.InUse
{
    public interface IInUseSystem : IEntitySystem
    {
        bool IsUsing(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null);
        void SetItemInUse(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null);
        void RemoveItemInUse(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null);
        void ClearItemsInUse(EntityUid user, ItemUserComponent? initemUserUse = null);
        bool TryGetUser(EntityUid item, [NotNullWhen(true)] out EntityUid? user, InUseComponent? inUse = null);
    }

    public sealed class InUseSystem : EntitySystem, IInUseSystem
    {
        public bool IsUsing(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null)
        {
            if (!Resolve(user, ref itemUser) || !IsAlive(item))
                return false;

            return itemUser.InUse.Contains(item);
        }

        public void SetItemInUse(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null)
        {
            if (!Resolve(user, ref itemUser))
                return;

            if (TryGetUser(item, out var otherUser))
                RemoveItemInUse(otherUser.Value, item);

            EnsureComp<InUseComponent>(item).User = user;

            itemUser.InUse.Add(item);
        }

        public void RemoveItemInUse(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null)
        {
            if (!Resolve(user, ref itemUser))
                return;

            if (TryComp<InUseComponent>(item, out var inUse) && inUse.User == user)
                EntityManager.RemoveComponent(item, inUse);

            itemUser.InUse.Remove(item);
        }

        public void ClearItemsInUse(EntityUid user, ItemUserComponent? itemUser = null)
        {
            if (!Resolve(user, ref itemUser))
                return;

            foreach (var item in itemUser.InUse)
            {
                if (TryComp<InUseComponent>(item, out var inUse) && inUse.User == user)
                    EntityManager.RemoveComponent(item, inUse);
            }

            itemUser.InUse.Clear();
        }

        public bool TryGetUser(EntityUid item, [NotNullWhen(true)] out EntityUid? user, InUseComponent? inUse = null)
        {
            if (!Resolve(item, ref inUse))
            {
                user = null;
                return false;
            }

            user = inUse.User;
            return IsAlive(inUse.User);
        }
    }
}