using Love;

namespace OpenNefia.Core.Rendering
{
    public interface IShaderInstance
    {
        public Love.Shader LoveShader { get; }
    }

    public sealed class ShaderInstance : IShaderInstance
    {
        public Shader LoveShader => throw new NotImplementedException();
    }
}
