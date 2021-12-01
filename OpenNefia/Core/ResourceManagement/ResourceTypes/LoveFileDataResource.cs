using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ResourceManagement
{
    public sealed class LoveFileDataResource : BaseResource
    {
        private Love.FileData _fileData = default!;
        public override ResourcePath? Fallback => null;

        public Love.FileData FileData => _fileData;

        private static Love.FileData LoadFileData(IResourceCache cache, ResourcePath path)
        {
            var filename = path.Filename;
            var contents = cache.ContentFileRead(path).CopyToArray();
            return Love.FileSystem.NewFileData(contents, filename);
        }

        public override void Load(IResourceCache cache, ResourcePath path)
        {
            _fileData = LoadFileData(cache, path);
        }

        public override void Reload(IResourceCache cache, ResourcePath path, CancellationToken ct = default)
        {
            _fileData.Dispose();
            _fileData = LoadFileData(cache, path);
        }

        // TODO: Due to a bug in Roslyn, NotNullIfNotNullAttribute doesn't work.
        // So this can't work with both nullables and non-nullables at the same time.
        // I decided to only have it work with non-nullables as such.
        public static implicit operator Love.FileData(LoveFileDataResource res)
        {
            return res.FileData;
        }
    }
}
