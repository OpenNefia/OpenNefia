using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;

namespace OpenNefia.Core.GameObjects
{
    internal sealed partial class ComponentFactory : IComponentFactory
    {
        private readonly IDynamicTypeFactoryInternal _typeFactory;
        private readonly IReflectionManager _reflectionManager;

        private class ComponentRegistration : IComponentRegistration
        {
            public string Name { get; }
            public Type Type { get; }
            internal readonly List<Type> References = new();
            IReadOnlyList<Type> IComponentRegistration.References => References;

            public ComponentRegistration(string name, Type type)
            {
                Name = name;
                Type = type;
                References.Add(type);
            }

            public override string ToString()
            {
                return $"ComponentRegistration({Name}: {Type})";
            }
        }

        // Bunch of dictionaries to allow lookups in all directions.
        /// <summary>
        /// Mapping of component name to type.
        /// </summary>
        private readonly Dictionary<string, ComponentRegistration> names = new();

        /// <summary>
        /// Mapping of lowercase component names to their registration.
        /// </summary>
        private readonly Dictionary<string, string> _lowerCaseNames = new();

        /// <summary>
        /// Mapping of concrete component types to their registration.
        /// </summary>
        private readonly Dictionary<Type, ComponentRegistration> types = new();

        /// <summary>
        /// Set of components that should be ignored. Probably just the list of components unique to the other project.
        /// </summary>
        private readonly HashSet<string> IgnoredComponentNames = new();

        /// <inheritdoc />
        public event Action<IComponentRegistration>? ComponentAdded;

        /// <inheritdoc />
        public event Action<(IComponentRegistration, Type)>? ComponentReferenceAdded;

        /// <inheritdoc />
        public event Action<string>? ComponentIgnoreAdded;

        /// <inheritdoc />
        public IEnumerable<Type> AllRegisteredTypes => types.Keys;

        /// <inheritdoc />
        private bool _wasFinalized = false;

        private IEnumerable<ComponentRegistration> AllRegistrations => types.Values;

        public ComponentFactory(IDynamicTypeFactoryInternal typeFactory, IReflectionManager reflectionManager)
        {
            _typeFactory = typeFactory;
            _reflectionManager = reflectionManager;
        }

        private void Register(Type type, bool overwrite = false)
        {
            if (_wasFinalized)
                throw new ComponentRegistrationLockException();

            if (types.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type is already registered: {type}");
            }

            // Create a dummy to be able to fetch instance properties like name.
            // Not clean but sadly C# doesn't have static virtual members.
            var dummy = (IComponent)Activator.CreateInstance(type)!;

            var name = dummy.Name;
            var lowerCaseName = name.ToLowerInvariant();

            if (IgnoredComponentNames.Contains(name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"{name} is already marked as ignored component");
                }

                IgnoredComponentNames.Remove(name);
            }

            if (names.ContainsKey(name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"{name} is already registered, previous: {names[name]}");
                }

                RemoveComponent(name);
            }

            if (_lowerCaseNames.ContainsKey(lowerCaseName))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"{lowerCaseName} is already registered, previous: {_lowerCaseNames[lowerCaseName]}");
                }
            }

            var registration = new ComponentRegistration(name, type);
            names[name] = registration;
            _lowerCaseNames[lowerCaseName] = name;
            types[type] = registration;

            ComponentAdded?.Invoke(registration);
        }

        private void RegisterReference(Type target, Type @interface)
        {
            if (_wasFinalized)
                throw new ComponentRegistrationLockException();

            if (!types.ContainsKey(target))
            {
                throw new InvalidOperationException($"Unregistered type: {target}");
            }

            var registration = types[target];
            if (registration.References.Contains(@interface))
            {
                throw new InvalidOperationException($"Attempted to register a reference twice: {@interface}");
            }
            registration.References.Add(@interface);
            ComponentReferenceAdded?.Invoke((registration, @interface));
        }

        public void RegisterIgnore(string name, bool overwrite = false)
        {
            if (IgnoredComponentNames.Contains(name))
            {
                throw new InvalidOperationException($"{name} is already registered as ignored");
            }

            if (names.ContainsKey(name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"{name} is already registered as a component");
                }

                RemoveComponent(name);
            }

            IgnoredComponentNames.Add(name);
            ComponentIgnoreAdded?.Invoke(name);
        }

        private void RemoveComponent(string name)
        {
            if (_wasFinalized)
                throw new ComponentRegistrationLockException();

            var registration = names[name];

            names.Remove(registration.Name);
            _lowerCaseNames.Remove(registration.Name.ToLowerInvariant());
            types.Remove(registration.Type);
        }

        public bool IsRegistered(string componentName)
        {
            return names.ContainsKey(componentName);
        }

        public IComponent GetComponent(Type componentType)
        {
            if (!types.ContainsKey(componentType))
            {
                throw new InvalidOperationException($"{componentType} is not a registered component.");
            }
            return _typeFactory.CreateInstanceUnchecked<IComponent>(types[componentType].Type);
        }

        public T GetComponent<T>() where T : IComponent, new()
        {
            if (!types.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"{typeof(T)} is not a registered component.");
            }
            return _typeFactory.CreateInstanceUnchecked<T>(types[typeof(T)].Type);
        }

        public IComponent GetComponent(string componentName, bool ignoreCase = false)
        {
            if (ignoreCase && _lowerCaseNames.TryGetValue(componentName, out var lowerCaseName))
            {
                componentName = lowerCaseName;
            }

            return _typeFactory.CreateInstanceUnchecked<IComponent>(GetRegistration(componentName).Type);
        }

        public IComponentRegistration GetRegistration(string componentName, bool ignoreCase = false)
        {
            if (ignoreCase && _lowerCaseNames.TryGetValue(componentName, out var lowerCaseName))
            {
                componentName = lowerCaseName;
            }

            try
            {
                return names[componentName];
            }
            catch (KeyNotFoundException)
            {
                throw new UnknownComponentException($"Unknown name: {componentName}");
            }
        }

        public IComponentRegistration GetRegistration(Type reference)
        {
            try
            {
                return types[reference];
            }
            catch (KeyNotFoundException)
            {
                throw new UnknownComponentException($"Unknown type: {reference}");
            }
        }

        public IComponentRegistration GetRegistration<T>() where T : IComponent, new()
        {
            return GetRegistration(typeof(T));
        }

        public IComponentRegistration GetRegistration(IComponent component)
        {
            return GetRegistration(component.GetType());
        }

        public bool TryGetRegistration(string componentName, [NotNullWhen(true)] out IComponentRegistration? registration, bool ignoreCase = false)
        {
            if (ignoreCase && _lowerCaseNames.TryGetValue(componentName, out var lowerCaseName))
            {
                componentName = lowerCaseName;
            }

            if (names.TryGetValue(componentName, out var tempRegistration))
            {
                registration = tempRegistration;
                return true;
            }

            registration = null;
            return false;
        }

        public bool TryGetRegistration(Type reference, [NotNullWhen(true)] out IComponentRegistration? registration)
        {
            if (types.TryGetValue(reference, out var tempRegistration))
            {
                registration = tempRegistration;
                return true;
            }

            registration = null;
            return false;
        }

        public bool TryGetRegistration<T>([NotNullWhen(true)] out IComponentRegistration? registration) where T : IComponent, new()
        {
            return TryGetRegistration(typeof(T), out registration);
        }

        public bool TryGetRegistration(IComponent component, [NotNullWhen(true)] out IComponentRegistration? registration)
        {
            return TryGetRegistration(component.GetType(), out registration);
        }

        public void DoAutoRegistrations()
        {
            foreach (var type in _reflectionManager.FindTypesWithAttribute<RegisterComponentAttribute>())
            {
                RegisterClass(type);
            }
        }

        /// <inheritdoc />
        public void RegisterClass<T>(bool overwrite = false)
            where T : IComponent, new()
        {
            RegisterClass(typeof(T));
        }

        private void RegisterClass(Type type)
        {
            if (!typeof(IComponent).IsAssignableFrom(type))
            {
                Logger.Error("Type {0} has RegisterComponentAttribute but does not implement IComponent.", type);
                return;
            }

            Register(type);

            foreach (var attribute in Attribute.GetCustomAttributes(type, typeof(ComponentReferenceAttribute)))
            {
                var cast = (ComponentReferenceAttribute)attribute;

                var refType = cast.ReferenceType;

                if (!refType.IsAssignableFrom(type))
                {
                    Logger.Error("Type {0} has reference for type it does not implement: {1}.", type, refType);
                    continue;
                }

                RegisterReference(type, refType);
            }
        }

        public IEnumerable<Type> GetAllRefTypes()
        {
            return AllRegistrations.SelectMany(r => r.References).Distinct();
        }

        public void FinishRegistration()
        {
            if (_wasFinalized)
            {
                throw new Exception("ComponentFactory was finalized twice.");
            }
            _wasFinalized = true;
            Logger.Info($"Registered {types.Count} components.");
        }
    }

    [Serializable]
    public class UnknownComponentException : Exception
    {
        public UnknownComponentException()
        {
        }
        public UnknownComponentException(string message) : base(message)
        {
        }
        public UnknownComponentException(string message, Exception inner) : base(message, inner)
        {
        }
        protected UnknownComponentException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }

    public class ComponentRegistrationLockException : Exception { }
}