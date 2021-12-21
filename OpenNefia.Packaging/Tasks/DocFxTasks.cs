using Cake.Frosting;
using Cake.DocFx;
using Cake.Core.IO;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("DocFxMetadata")]
    public sealed class DocFxMetadataTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DocFxMetadata(new FilePath("docs/docfx.json"));
        }
    }

    [TaskName("DocFxBuild")]
    [IsDependentOn(typeof(DocFxMetadataTask))]
    public sealed class DocFxBuildTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DocFxBuild(new FilePath("docs/docfx.json"));
        }
    }

    [TaskName("DocFxServe")]
    public sealed class DocFxServeTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DocFxServe(new DirectoryPath("docs/_site"));
        }
    }
}