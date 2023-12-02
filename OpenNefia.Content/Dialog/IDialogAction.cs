using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetVips;
using OpenNefia.Content.Sidequests;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Log;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Logic;
using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Dialog
{
    [ImplicitDataDefinitionForInheritors]
    public interface IDialogAction
    {
        void Invoke(IDialogEngine engine, IDialogNode node);
    }
    
    public sealed class DialogCallbackAction : IDialogAction
    {
        [DataField]
        public DialogActionDelegate Callback { get; } = default!;

        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            Callback.Invoke(engine, node);
        }
    }

    public sealed class SetSidequestStateDialogAction : IDialogAction
    {
        [Dependency] private readonly ISidequestSystem _sidequests = default!;

        [DataField]
        public PrototypeId<SidequestPrototype> SidequestID { get; }

        [DataField]
        public int State { get; }

        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            _sidequests.SetState(SidequestID, State);
        }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IEntityLookupCriteria
    {
        IEnumerable<EntityUid> FindEntities(IDialogEngine engine);
    }

    public sealed class CurrentSpeakerCriteria : IEntityLookupCriteria
    {
        public IEnumerable<EntityUid> FindEntities(IDialogEngine engine)
        {
            if (engine.Speaker == null)
                yield break;

            yield return engine.Speaker.Value;
        }
    }

    public sealed class CurrentPlayerCriteria : IEntityLookupCriteria
    {
        public IEnumerable<EntityUid> FindEntities(IDialogEngine engine)
        {
            yield return engine.Player;
        }
    }

    public sealed class ByPrototypeCriteria : IEntityLookupCriteria
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;

        [DataField]
        public PrototypeId<EntityPrototype> PrototypeID { get; set; }

        public IEnumerable<EntityUid> FindEntities(IDialogEngine engine)
        {
            var spatial = _entityManager.GetComponent<SpatialComponent>(engine.Player);

            return _entityLookup.EntityQueryInMap<MetaDataComponent>(spatial.MapID)
                .Where(metaData => metaData.EntityPrototype?.GetStrongID() == PrototypeID)
                .Select(metaData => metaData.Owner);
        }
    }

    public sealed class SetSpeakerAction : IDialogAction
    {
        [DataField]
        public IEntityLookupCriteria? Target { get; } = default!;

        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            if (Target == null)
            {
                engine.Speaker = null;
                return;
            }

            EntitySystem.InjectDependencies(Target); // TODO autoinject?
            var newSpeaker = Target.FindEntities(engine).FirstOrNull();
            if (newSpeaker == null)
            {
                Logger.ErrorS("dialog.action", $"Could not find next speaker based on lookup criteria {Target}!");
                return;
            }

            engine.Speaker = newSpeaker;
        }
    }

    public sealed class TurnHostileAction : IDialogAction
    {
        [Dependency] private readonly IFactionSystem _factions = default!;

        [DataField(required: true)]
        public IEntityLookupCriteria Target { get; } = default!;

        [DataField]
        public IEntityLookupCriteria Aggressor { get; } = new CurrentPlayerCriteria();

        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            foreach (var aggressor in Aggressor.FindEntities(engine))
            {
                foreach (var target in Target.FindEntities(engine))
                {
                    _factions.ActHostileTowards(aggressor, target);
                }
            }
        }
    }

    public sealed class DisplayMessageAction : IDialogAction
    {
        [Dependency] private readonly IMessagesManager _mes = default!;

        [DataField(required: true)]
        public LocaleKey Key { get; }

        [DataField]
        public Color? Color { get; }

        [DataField]
        public IEntityLookupCriteria? Entity { get; }
        
        public void Invoke(IDialogEngine engine, IDialogNode node)
        {
            EntityUid? entity = null;
            if (Entity != null)
                entity = Entity.FindEntities(engine).FirstOrNull();
            
            _mes.Display(Loc.GetString(Key, ("speaker", engine.Speaker), ("player", engine.Player)), Color, entity: entity);
        }
    }
}
