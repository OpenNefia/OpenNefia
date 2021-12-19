using Cake.Frosting;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("Default")]
    [IsDependentOn(typeof(PackageTask))]
    public class DefaultTask : FrostingTask
    {
    }
}