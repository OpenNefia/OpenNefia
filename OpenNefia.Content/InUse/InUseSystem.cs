﻿using OpenNefia.Content.Logic;
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
using OpenNefia.Content.Activity;

namespace OpenNefia.Content.InUse
{
    /// <summary>
    /// Keeps track of items being used by entities.
    /// This is usually used in conjunction with <see cref="IActivitySystem"/> in case there
    /// is an item required by the activity.
    /// </summary>
    public interface IInUseSystem : IEntitySystem
    {
        bool IsUsingAnything(EntityUid user, ItemUserComponent? itemUser = null);
        bool IsUsing(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null);
        void SetItemInUse(EntityUid user, EntityUid item, ItemUserComponent? itemUser = null);
        void RemoveItemInUse(EntityUid item);
        void ClearItemsInUseForUser(EntityUid user, ItemUserComponent? initemUserUse = null);
        bool TryGetUser(EntityUid item, [NotNullWhen(true)] out EntityUid? user, InUseComponent? inUse = null);
        
        /// <summary>
        /// Interrupts the activity this item is associated with.
        /// </summary>
        /// <param name="item"></param>
        void InterruptUserOfItem(EntityUid item);
    }

    public sealed class InUseSystem : EntitySystem, IInUseSystem
    {
        [Dependency] private readonly IActivitySystem _activities = default!;

        public bool IsUsingAnything(EntityUid user, ItemUserComponent? itemUser = null)
        {
            if (!Resolve(user, ref itemUser))
                return false;

            return itemUser.InUse.Any(i => IsAlive(i));
        }

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
                RemoveItemInUse(item);

            EnsureComp<InUseComponent>(item).User = user;

            itemUser.InUse.Add(item);
        }

        public void RemoveItemInUse(EntityUid item)
        {
            // TODO use flags instead of Add/RemoveComponent
            if (TryComp<InUseComponent>(item, out var inUse))
            {
                EntityManager.RemoveComponent(item, inUse);
                if (TryComp<ItemUserComponent>(inUse.User, out var itemUser))
                    itemUser.InUse.Remove(item);
            }
        }

        public void ClearItemsInUseForUser(EntityUid user, ItemUserComponent? itemUser = null)
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

        public void InterruptUserOfItem(EntityUid item)
        {
            if (!TryGetUser(item, out var user))
                return;
            
            _activities.InterruptActivity(user.Value);
            RemoveItemInUse(item);
        }
    }
}