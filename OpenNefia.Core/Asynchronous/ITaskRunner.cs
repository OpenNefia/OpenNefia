namespace OpenNefia.Core.Asynchronous
{
    public interface ITaskRunner
    {
        void Run(Task task);
        T Run<T>(Task<T> task);
    }
}