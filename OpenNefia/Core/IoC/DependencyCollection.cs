﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Utility;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace OpenNefia.Core.IoC
{
    public delegate T DependencyFactoryDelegate<out T>()
        where T : class;

    /// <inheritdoc />
    internal class DependencyCollection : IDependencyCollection
    {
        private delegate void InjectorDelegate(object target, object[] services);
        private static readonly Type[] InjectorParameters = { typeof(object), typeof(object[]) };

        /// <summary>
        /// Dictionary that maps the types passed to <see cref="Resolve{T}"/> to their implementation.
        /// </summary>
        private readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// The types interface types mapping to their registered implementations.
        /// This is pulled from to make a service if it doesn't exist yet.
        /// </summary>
        private readonly Dictionary<Type, Type> _resolveTypes = new();

        private readonly Dictionary<Type, DependencyFactoryDelegate<object>> _resolveFactories = new();

        private readonly Queue<Type> _pendingResolves = new();

        // To do injection of common types like components, we make DynamicMethods to do the actual injecting.
        // This is way faster than reflection and should be allocation free outside setup.
        private readonly Dictionary<Type, (InjectorDelegate? @delegate, object[]? services)> _injectorCache =
            new();

        private readonly IDependencyCollection? _parentCollection;

        public DependencyCollection() { }

        public DependencyCollection(IDependencyCollection parentCollection)
        {
            _parentCollection = parentCollection;
        }

        /// <inheritdoc />
        public bool TryResolveType<T>([NotNullWhen(true)] out T? instance)
        {
            if (TryResolveType(typeof(T), out object? rawInstance))
            {
                if (rawInstance is T typedInstance)
                {
                    instance = typedInstance;
                    return true;
                }
            }

            instance = default;
            return false;
        }

        /// <inheritdoc />
        public bool TryResolveType(Type objectType, [MaybeNullWhen(false)] out object instance)
        {
            if (!_services.TryGetValue(objectType, out instance))
                return _parentCollection is not null && _parentCollection.TryResolveType(objectType, out instance);

            return true;
        }

        /// <inheritdoc />
        public void Register<TInterface, TImplementation>(bool overwrite = false)
            where TImplementation : class, TInterface
        {
            Register<TInterface, TImplementation>(() =>
            {
                var objectType = typeof(TImplementation);
                var constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (constructors.Length != 1)
                    throw new InvalidOperationException($"Dependency '{typeof(TImplementation).FullName}' requires exactly one constructor.");

                var chosenConstructor = constructors[0];
                var constructorParams = constructors[0].GetParameters();
                var parameters = new object[constructorParams.Length];

                for (var index = 0; index < constructorParams.Length; index++)
                {
                    var param = constructorParams[index];

                    if (TryResolveType(param.ParameterType, out var instance))
                    {
                        parameters[index] = instance;
                    }
                    else
                    {
                        if (_resolveTypes.ContainsKey(param.ParameterType))
                        {
                            throw new InvalidOperationException($"Dependency '{typeof(TImplementation).FullName}' ctor requires {param.ParameterType.FullName} registered before it.");
                        }

                        throw new InvalidOperationException($"Dependency '{typeof(TImplementation).FullName}' ctor has unknown dependency {param.ParameterType.FullName}");
                    }
                }

                return (TImplementation)chosenConstructor.Invoke(parameters);
            }, overwrite);
        }



        /// <inheritdoc />
        public void Register<TInterface, TImplementation>(DependencyFactoryDelegate<TImplementation> factory, bool overwrite = false)
            where TImplementation : class, TInterface
        {
            var interfaceType = typeof(TInterface);
            CheckRegisterInterface(interfaceType, typeof(TImplementation), overwrite);

            _resolveTypes[interfaceType] = typeof(TImplementation);
            _resolveFactories[typeof(TImplementation)] = factory;
            _pendingResolves.Enqueue(interfaceType);
        }

        /// <inheritdoc />
        public void Register(Type implementation, DependencyFactoryDelegate<object>? factory = null,
            bool overwrite = false) => Register(implementation, implementation, factory, overwrite);

        public void Register(Type interfaceType, Type implementation, DependencyFactoryDelegate<object>? factory = null, bool overwrite = false)
        {
            CheckRegisterInterface(interfaceType, implementation, overwrite);

            object DefaultFactory()
            {
                var constructors = implementation.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (constructors.Length != 1)
                    throw new InvalidOperationException($"Dependency '{implementation.FullName}' requires exactly one constructor.");

                var chosenConstructor = constructors[0];
                var constructorParams = chosenConstructor.GetParameters();
                var parameters = new object[constructorParams.Length];

                for (var index = 0; index < constructorParams.Length; index++)
                {
                    var param = constructorParams[index];

                    if (TryResolveType(param.ParameterType, out var instance))
                    {
                        parameters[index] = instance;
                    }
                    else
                    {
                        if (_resolveTypes.ContainsKey(param.ParameterType))
                        {
                            throw new InvalidOperationException($"Dependency '{implementation.FullName}' ctor requires {param.ParameterType.FullName} registered before it.");
                        }

                        throw new InvalidOperationException($"Dependency '{implementation.FullName}' ctor has unknown dependency {param.ParameterType.FullName}");
                    }
                }

                return chosenConstructor.Invoke(parameters);
            }

            _resolveTypes[interfaceType] = implementation;
            _resolveFactories[implementation] = factory ?? DefaultFactory;
            _pendingResolves.Enqueue(interfaceType);
        }

        [AssertionMethod]
        private void CheckRegisterInterface(Type interfaceType, Type implementationType, bool overwrite)
        {
            if (!_resolveTypes.ContainsKey(interfaceType))
                return;

            if (!overwrite)
            {
                throw new InvalidOperationException
                (
                    string.Format(
                        "Attempted to register already registered interface {0}. New implementation: {1}, Old implementation: {2}",
                        interfaceType, implementationType, _resolveTypes[interfaceType]
                    ));
            }

            if (_services.ContainsKey(interfaceType))
            {
                throw new InvalidOperationException(
                    $"Attempted to overwrite already instantiated interface {interfaceType}.");
            }
        }

        /// <inheritdoc />
        public void RegisterInstance<TInterface>(object implementation, bool overwrite = false)
        {
            RegisterInstance(typeof(TInterface), implementation, overwrite);
        }

        /// <inheritdoc />
        public void RegisterInstance(Type type, object implementation, bool overwrite = false)
        {
            if (implementation == null)
                throw new ArgumentNullException(nameof(implementation));

            if (!implementation.GetType().IsAssignableTo(type))
                throw new InvalidOperationException(
                    $"Implementation type {implementation.GetType()} is not assignable to type {type}");

            CheckRegisterInterface(type, implementation.GetType(), overwrite);

            // do the equivalent of BuildGraph with a single type.
            _resolveTypes[type] = implementation.GetType();
            _services[type] = implementation;

            InjectDependencies(implementation, true);

            if (implementation is IPostInjectInit init)
                init.PostInject();
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var service in _services.Values.OfType<IDisposable>().Distinct())
            {
                service.Dispose();
            }

            _services.Clear();
            _resolveTypes.Clear();
            _resolveFactories.Clear();
            _injectorCache.Clear();
        }

        /// <inheritdoc />
        [System.Diagnostics.Contracts.Pure]
        public T Resolve<T>()
        {
            return (T)ResolveType(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T>([NotNull] ref T? instance)
        {
            // Resolve<T>() will either throw or return a concrete instance, therefore we suppress the nullable warning.
            instance ??= Resolve<T>()!;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T1, T2>([NotNull] ref T1? instance1, [NotNull] ref T2? instance2)
        {
            Resolve(ref instance1);
            Resolve(ref instance2);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T1, T2, T3>([NotNull] ref T1? instance1, [NotNull] ref T2? instance2, [NotNull] ref T3? instance3)
        {
            Resolve(ref instance1, ref instance2);
            Resolve(ref instance3);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T1, T2, T3, T4>([NotNull] ref T1? instance1, [NotNull] ref T2? instance2, [NotNull] ref T3? instance3, [NotNull] ref T4? instance4)
        {
            Resolve(ref instance1, ref instance2);
            Resolve(ref instance3, ref instance4);
        }

        /// <inheritdoc />
        [System.Diagnostics.Contracts.Pure]
        public object ResolveType(Type type)
        {
            if (TryResolveType(type, out var value))
            {
                return value;
            }

            if (_resolveTypes.ContainsKey(type))
            {
                // If we have the type registered but not created that means we haven't been told to initialize the graph yet.
                throw new InvalidOperationException(
                    $"Attempted to resolve type {type} before the object graph for it has been populated.");
            }

            if (type == typeof(IDependencyCollection))
            {
                return this;
            }

            throw new UnregisteredTypeException(type);
        }

        /// <inheritdoc />
        public void BuildGraph()
        {
            // List of all objects we need to inject dependencies into.
            var injectList = new List<object>();

            // First we build every type we have registered but isn't yet built.
            // This allows us to run this after the content assembly has been loaded.
            while (_pendingResolves.Count > 0)
            {
                Type key = _pendingResolves.Dequeue();
                var value = _resolveTypes[key];

                // Find a potential dupe by checking other registered types that have already been instantiated that have the same instance type.
                // Can't catch ourselves because we're not instantiated.
                // Ones that aren't yet instantiated are about to be and will find us instead.
                var (type, _) = _resolveTypes.FirstOrDefault(p => _services.ContainsKey(p.Key) && p.Value == value)!;

                // Interface key can't be null so since KeyValuePair<> is a struct,
                // this effectively checks whether we found something.
                if (type != null)
                {
                    // We have something with the same instance type, use that.
                    _services[key] = _services[type];
                    continue;
                }

                try
                {
                    // Yay for delegate covariance
                    object instance = _resolveFactories[value].Invoke();
                    _services[key] = instance;
                    injectList.Add(instance);
                }
                catch (TargetInvocationException e)
                {
                    throw new ImplementationConstructorException(value, e.InnerException);
                }
            }

            // Because we only ever construct an instance once per registration, there is no need to keep the factory
            // delegates. Also we need to free the delegates because lambdas capture variables.
            _resolveFactories.Clear();

            // Graph built, go over ones that need injection.
            foreach (var implementation in injectList)
            {
                InjectDependencies(implementation, true);
            }

            foreach (var injectedItem in injectList.OfType<IPostInjectInit>())
            {
                injectedItem.PostInject();
            }
        }

        /// <inheritdoc />
        public void InjectDependencies(object obj, bool oneOff = false)
        {
            var type = obj.GetType();

            if (!_injectorCache.TryGetValue(type, out var injector))
            {
                if (oneOff)
                {
                    // If this is a one-off injection then use the old reflection method.
                    // Won't cache a bunch of later-unused stuff.
                    InjectDependenciesReflection(obj);
                    return;
                }

                CacheInjector(type);
                injector = _injectorCache[type];
            }

            var (@delegate, services) = injector;

            // If @delegate is null then the type has no dependencies.
            // So running an initializer would be quite wasteful.
            @delegate?.Invoke(obj, services!);
        }

        private void InjectDependenciesReflection(object obj)
        {
            var type = obj.GetType();
            foreach (var field in type.GetAllFields())
            {
                if (!Attribute.IsDefined(field, typeof(DependencyAttribute)))
                {
                    continue;
                }

                // Not using Resolve<T>() because we're literally building it right now.
                if (TryResolveType(field.FieldType, out var dep))
                {
                    // Quick note: this DOES work with read only fields, though it may be a CLR implementation detail.
                    field.SetValue(obj, dep);
                    continue;
                }

                // A hard-coded special case so the DependencyCollection can inject itself.
                // This is not put into the services so it can be overridden if needed.
                if (field.FieldType == typeof(IDependencyCollection))
                {
                    field.SetValue(obj, this);
                    continue;
                }

                throw new UnregisteredDependencyException(type, field.FieldType, field.Name);
            }
        }

        private void CacheInjector(Type type)
        {
            var fields = new List<FieldInfo>();

            foreach (var field in type.GetAllFields())
            {
                if (!Attribute.IsDefined(field, typeof(DependencyAttribute)))
                {
                    continue;
                }

                fields.Add(field);
            }

            if (fields.Count == 0)
            {
                _injectorCache.Add(type, (null, null));
                return;
            }

            var dynamicMethod = new DynamicMethod($"_injector<>{type}", null, InjectorParameters, type, true);

            dynamicMethod.DefineParameter(1, ParameterAttributes.In, "target");
            dynamicMethod.DefineParameter(2, ParameterAttributes.In, "services");

            var i = 0;
            var services = new List<object>();

            var generator = dynamicMethod.GetILGenerator();

            foreach (var field in fields)
            {
                // Load object to inject into.
                generator.Emit(OpCodes.Ldarg_0);

                // Not using Resolve<T>() because we're literally building it right now.
                if (!TryResolveType(field.FieldType, out var service))
                {
                    // A hard-coded special case so the DependencyCollection can inject itself.
                    // This is not put into the services so it can be overridden if needed.
                    if (field.FieldType == typeof(IDependencyCollection))
                    {
                        service = this;
                    }
                    else
                    {
                        throw new UnregisteredDependencyException(type, field.FieldType, field.Name);
                    }
                }

                services.Add(service);

                // Load services array.
                generator.Emit(OpCodes.Ldarg_1);
                // Load service from array.
                generator.Emit(OpCodes.Ldc_I4, i++);
                generator.Emit(OpCodes.Ldelem_Ref);

                // Set service into field.
                generator.Emit(OpCodes.Stfld, field);
            }

            generator.Emit(OpCodes.Ret);

            var @delegate = (InjectorDelegate)dynamicMethod.CreateDelegate(typeof(InjectorDelegate));
            _injectorCache.Add(type, (@delegate, services.ToArray()));
        }
    }
}
