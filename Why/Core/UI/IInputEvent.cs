namespace OpenNefia.Core.UI
{
    public interface IInputEvent
    {
        public bool Passed { get; }

        public void Pass();
    }
}
