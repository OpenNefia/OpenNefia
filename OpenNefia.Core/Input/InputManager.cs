using OpenNefia.Core.Input.Binding;

namespace OpenNefia.Core.Input
{
    public interface IInputManager
    {
        /// <summary>
        ///     Holds the keyFunction -> handler bindings for the simulation.
        /// </summary>
        public ICommandBindRegistry BindRegistry { get; }
    }

    public sealed class InputManager : IInputManager
    {
        private readonly CommandBindRegistry _bindRegistry = new();

        /// <inheritdoc/>
        public ICommandBindRegistry BindRegistry => _bindRegistry;
    }
}