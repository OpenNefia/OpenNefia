using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.UserInterface.XAML
{
    /// <summary>
    /// Implements a name scope.
    /// </summary>
    public sealed class NameScope
    {
        public bool IsCompleted { get; private set; }

        private readonly Dictionary<string, UiElement> _inner = new Dictionary<string, UiElement>();

        public void Register(string name, UiElement element)
        {
            if (IsCompleted)
                throw new InvalidOperationException("NameScope is completed, no further registrations are allowed");
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (_inner.TryGetValue(name, out UiElement? existing))
            {
                if (existing != element)
                {
                    throw new ArgumentException($"Control with the name '{name}' already registered.");
                }
            }
            else
            {
                _inner.Add(name, element);
            }
        }

        public void Absorb(NameScope? nameScope)
        {
            if (nameScope == null) return;

            foreach (var (name, control) in nameScope._inner)
            {
                try
                {
                    Register(name, control);
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Exception occured when trying to absorb NameScope (at name {name})", e);
                }
            }
        }

        public UiElement? Find(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _inner.TryGetValue(name, out var result);
            return result;
        }

        public void Complete()
        {
            IsCompleted = true;
        }
    }
}
