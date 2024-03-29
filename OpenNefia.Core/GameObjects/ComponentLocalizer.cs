﻿using NLua;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    public interface IComponentLocalizer
    {
        void LocalizeComponents(EntityUid entityUid);
        void LocalizeComponent(EntityUid entity, IComponentLocalizable compLocalizable);
    }

    internal interface IComponentLocalizerInternal : IComponentLocalizer
    {
        void Initialize();
    }

    public sealed class ComponentLocalizer : IComponentLocalizerInternal, IEntityEventSubscriber
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        
        public void Initialize()
        {
            _entityManager.EventBus.SubscribeBroadcastEvent<EntityInitializedEvent>(this, HandleEntityInitialized);
            _entityManager.ComponentAdded += HandleComponentAdded;
        }

        private void HandleEntityInitialized(EntityInitializedEvent ev)
        {
            LocalizeComponents(ev.EntityUid);
        }

        private void HandleComponentAdded(object? sender, ComponentEventArgs e)
        {
            if (e.Component is IComponentLocalizable localizable)
                LocalizeComponent(e.Owner, localizable);
        }

        public void LocalizeComponents(EntityUid entity)
        {
            if (!_localizationManager.TryGetLocalizationData(entity, out var locData))
                return;

            foreach (var comp in _entityManager.GetComponents(entity))
            {
                if (comp is IComponentLocalizable compLocalizable)
                    LocalizeComponent(entity, locData, compLocalizable);
            }
        }

        public void LocalizeComponent(EntityUid entity, IComponentLocalizable compLocalizable)
        {
            if (!_localizationManager.TryGetLocalizationData(entity, out var locData))
                return;

            LocalizeComponent(entity, locData, compLocalizable);
        }

        private void LocalizeComponent(EntityUid entity, LuaTable locData, IComponentLocalizable compLocalizable)
        {
            var compName = _componentFactory.GetComponentName(compLocalizable.GetType());
            var obj = locData[compName];
            if (obj is not LuaTable compLocaleData)
                return;

            try
            {
                compLocalizable.LocalizeFromLua(compLocaleData);
            }
            catch (Exception ex)
            {
                EntityPrototype? protoId = null;
                if (_entityManager.TryGetComponent(entity, out MetaDataComponent metaData))
                {
                    protoId = metaData.EntityPrototype;
                }
                Logger.ErrorS("entity.localize", ex, $"Failed to localize component {compName} ({protoId}): {ex}");
            }
        }
    }
}
