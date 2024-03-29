﻿using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Inventory
{
    /// <summary>
    /// The underlying behavior of an inventory screen. Separating it like
    /// this simplifies the creation of item shortcuts, since all that is
    /// needed is creating the context and running its methods without
    /// needing to open any windows.
    /// </summary>
    public sealed class InventoryContext
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public EntityUid User { get; }
        public EntityUid Target { get; }
        public IInventoryBehavior Behavior { get; internal set; }

        public bool ShowInventoryWindow { get; set; } = true;
        public IReadOnlyList<InventoryEntry> AllInventoryEntries { get; internal set; } = new List<InventoryEntry>();

        public InventoryContext(EntityUid user, EntityUid target, IInventoryBehavior behavior)
        {
            EntitySystem.InjectDependencies(this);
            EntitySystem.InjectDependencies(behavior);

            User = user;
            Target = target;
            Behavior = behavior;
        }

        public InventoryContext(EntityUid user, IInventoryBehavior behavior)
            : this(user, user, behavior)
        {
        }

        public IEnumerable<IInventorySource> GetSources()
        {
            return Behavior.GetSources(this)
                .Select(source => EntitySystem.InjectDependencies(source));
        }

        public IEnumerable<(EntityUid, IInventorySource)> GetFilteredEntities()
        {
            var seen = new HashSet<EntityUid>();

            foreach (var source in GetSources())
            {
                foreach (var ent in source.GetEntities())
                {
                    if (IsAccepted(ent) && !seen.Contains(ent))
                    {
                        yield return (ent, source);
                        seen.Add(ent);
                    }
                }
            }
        }

        public bool IsAccepted(EntityUid ent)
        {
            if (!_entityManager.IsAlive(ent))
                return false;

            var ev = new InventoryContextFilterEvent(this);
            _entityManager.EventBus.RaiseEvent(ent, ref ev);
            if (!ev.OutAccepted)
                return false;

            return Behavior.IsAccepted(this, ent);
        }

        public string GetQueryText()
        {
            return Behavior.GetQueryText(this);
        }

        public void OnQuery()
        {
            Behavior.OnQuery(this);
        }

        public InventoryResult OnSelect(EntityUid item)
        {
            var amount = 1;

            if (_entityManager.TryGetComponent<StackComponent>(item, out var stack))
            {
                if (Behavior.QueryAmount && stack.Count > 1)
                {
                    var result = Behavior.OnQueryAmount(this, item);

                    if (!result.HasValue)
                    {
                        return new InventoryResult.Continuing();
                    }

                    amount = result.Value;
                }
                else
                {
                    amount = stack.Count;
                }
            }


            return Behavior.OnSelect(this, item, amount);
        }
    }

    /// <summary>
    /// Raised to determine if an item should be shown in an inventory screen.
    /// Strongly prefer the features exposed by <see cref="IInventoryBehavior"/> instead
    /// of this event.
    /// </summary>
    [ByRefEvent]
    [EventUsage(EventTarget.Normal)]
    public struct InventoryContextFilterEvent
    {
        public InventoryContextFilterEvent(InventoryContext inventoryContext)
        {
            InventoryContext = inventoryContext;
        }

        public bool OutAccepted { get; set; } = true;

        public InventoryContext InventoryContext { get; }
        public IInventoryBehavior Behavior => InventoryContext.Behavior;
    }
}
