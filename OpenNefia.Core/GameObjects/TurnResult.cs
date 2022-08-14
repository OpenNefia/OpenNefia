namespace OpenNefia.Core.GameObjects
{
    public enum TurnResult
    {
        // TODO remove this and replace with TurnResult?
        // There are some cases where a turn result is required,
        // so it would not make sense to be able to pass NoResult
        // in those cases.
        NoResult = 0,

        Failed = 1,
        Aborted = 2,
        Succeeded = 3
    }
}