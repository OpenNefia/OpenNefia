using Cake.Frosting;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("Default")]
    [IsDependentOn(typeof(PackageFullReleaseTask))]
    public class DefaultTask : FrostingTask
    {
    }
}