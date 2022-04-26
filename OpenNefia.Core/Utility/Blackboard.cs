using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Utility
{
    public class Blackboard<TType>
    {
        protected Dictionary<Type, object> _instances = new();

        public bool TryGet<T>([NotNullWhen(true)] out T? instance)
            where T : class, TType
        {
            if (_instances.TryGetValue(typeof(T), out var obj))
            {
                instance = (T)obj;
                return true;
            }
            instance = null;
            return false;
        }

        public T Get<T>() where T : class, TType
        {
            return (T)_instances[typeof(T)];
        }

        public T Ensure<T>() where T: class, TType, new()
        {
            if (TryGet<T>(out var instance))
                return instance;

            instance = new T();
            _instances[typeof(T)] = instance;
            return instance;
        }

        public void Add<T>(T instance) where T: class, TType
        {
            if (_instances.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Key {typeof(T)} already exists.");
            }

            _instances[typeof(T)] = instance;
        }

        public bool Remove<T>() where T: class, TType
        {
            return _instances.Remove(typeof(T));
        }

        public void Clear() 
        { 
            _instances.Clear(); 
        }
    }
}
