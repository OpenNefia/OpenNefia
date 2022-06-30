using OpenNefia.Content.Logic;
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
        [Dependency] private readonly IMessagesManager _mes = default!;

        public EntityUid User { get; }
        public IInventoryBehavior Behavior { get; internal set; }

        public bool ShowInventoryWindow { get; set; } = true;

        public InventoryContext(EntityUid user, IInventoryBehavior behavior)
        {
            EntitySystem.InjectDependencies(this);
            EntitySystem.InjectDependencies(behavior);

            User = user;
            Behavior = behavior;
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
                    var min = 1;
                    var max = stack.Count;

                    LocaleKey promptKey;
                    if (Behavior.QueryAmountPrompt != null && Loc.HasString(Behavior.QueryAmountPrompt.Value))
                        promptKey = Behavior.QueryAmountPrompt.Value;
                    else
                        promptKey = "Elona.Inventory.Common.HowMany";

                    var prompt = Loc.GetString(promptKey, ("min", min), ("max", max), ("entity", item));

                    var result = _uiManager.Query<NumberPrompt, NumberPrompt.Args, NumberPrompt.Result>(new(max, min, isCancellable: true, prompt: prompt));

                    if (!result.HasValue)
                    {
                        return new InventoryResult.Continuing();
                    }

                    amount = result.Value.Value;
                }
                else
                {
                    amount = stack.Count;
                }
            }


            return Behavior.OnSelect(this, item, amount);
        }
    }
}
