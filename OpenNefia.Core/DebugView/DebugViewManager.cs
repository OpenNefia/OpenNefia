using ImGuiNET;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Timing;
using System.Buffers;
using System.Runtime.InteropServices;

namespace OpenNefia.Core.DebugView
{
    public interface IDebugViewManager : IDisposable
    {
        void Initialize();
        void NewFrame();
        void Draw();
        void Update(FrameEventArgs frame);
    }

    /// <summary>
    /// Based on https://github.com/mellinoe/ImGui.NET/blob/master/src/ImGui.NET.SampleProgram/ImGuiController.cs
    /// </summary>
    public sealed class DebugViewManager : IDebugViewManager
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        private Love.MeshFormatDescribe _meshFormat;
        private Love.Image _fontTexture = default!;
        private Dictionary<IntPtr, Love.Texture> _imGuiToLoveTextures = new();

        private IntPtr _imCtx = IntPtr.Zero;
        private bool _frameBegun;

        public DebugViewManager()
        {
            var meshFormatEntries = new List<Love.MeshFormatDescribe.Entry>()
            {
                new("VertexPosition", Love.VertexDataType.FLOAT, 2),
                new("VertexTexCoord", Love.VertexDataType.FLOAT, 2),
                new("VertexColor", Love.VertexDataType.UNORM8, 4),
            };
            _meshFormat = Love.MeshFormatDescribe.New(meshFormatEntries);
        }

        public void Initialize()
        {
            if (_imCtx != IntPtr.Zero)
            {
                throw new InvalidOperationException("ImGui already initialized");
            }

            _imCtx = ImGui.CreateContext();
            ImGui.SetCurrentContext(_imCtx);

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.Fonts.GetTexDataAsRGBA32(out IntPtr fontTexPixels, out var fontTexWidth, out var fontTexHeight);
            var fontTexBuffer = new byte[fontTexWidth * fontTexHeight * 4];
            Marshal.Copy(fontTexPixels, fontTexBuffer, 0, fontTexBuffer.Length);
            var fontImageData = Love.Image.NewImageData(fontTexWidth, fontTexHeight, Love.ImageDataPixelFormat.RGBA8, fontTexBuffer);
            _fontTexture = Love.Graphics.NewImage(fontImageData);

            var fontTextureId = (IntPtr)1;
            io.Fonts.SetTexID(fontTextureId);
            _imGuiToLoveTextures.Add(fontTextureId, _fontTexture);

            SetPerFrameImGuiData(new FrameEventArgs(1f / 60f));

            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void NewFrame()
        {
            _frameBegun = true;
            ImGui.NewFrame();
        }

        public void Update(FrameEventArgs frame)
        {
            SetPerFrameImGuiData(frame);
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(FrameEventArgs frame)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = _graphics.WindowSize;
            io.DisplayFramebufferScale = new System.Numerics.Vector2(_graphics.WindowScale, _graphics.WindowScale);
            io.DeltaTime = frame.DeltaSeconds; // DeltaTime is in seconds.
        }

        public void Draw()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                DrawImGuiData(ImGui.GetDrawData());
            }
        }

        private void DrawImGuiData(ImDrawDataPtr imDrawDataPtr)
        {
            imDrawDataPtr.ScaleClipRects(new(_graphics.WindowScale, _graphics.WindowScale));

            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);

            for (int i = 0; i < imDrawDataPtr.CmdListsCount; i++)
            {
                var cmdList = imDrawDataPtr.CmdListsRange[i];

                var vertexSize = 20; // sizeof(ImDrawVert)
                var buffer = ArrayPool<byte>.Shared.Rent(cmdList.VtxBuffer.Size * vertexSize);
                Marshal.Copy(cmdList.VtxBuffer.Data, buffer, 0, cmdList.VtxBuffer.Size * vertexSize);

                var renderMesh = Love.Graphics.NewMesh(_meshFormat, buffer, cmdList.VtxBuffer.Size * vertexSize, Love.MeshDrawMode.Trangles);

                var vertexMap = ArrayPool<uint>.Shared.Rent(cmdList.IdxBuffer.Size);
                for (int j = 0; j < cmdList.IdxBuffer.Size; j++)
                {
                    vertexMap[j] = cmdList.IdxBuffer[j];
                }

                renderMesh.SetVertexMap(vertexMap, cmdList.IdxBuffer.Size);

                uint position = 0;
                for (int k = 0; k < cmdList.CmdBuffer.Size; k++)
                {
                    var cmd = cmdList.CmdBuffer[k];

                    if (cmd.TextureId == IntPtr.Zero)
                    {
                        renderMesh.SetTexture();
                    }
                    else
                    {
                        if (_imGuiToLoveTextures.TryGetValue(cmd.TextureId, out var loveTexture))
                        {
                            renderMesh.SetTexture(loveTexture);
                        }
                        else
                        {
                            Logger.ErrorS("debugview", $"Missing binding for imGui texture {cmd.TextureId}!");
                        }
                    }

                    Love.Graphics.SetScissor((int)cmd.ClipRect.X, (int)cmd.ClipRect.Y, (int)cmd.ClipRect.Z, (int)cmd.ClipRect.W);
                    renderMesh.SetDrawRange((int)position, (int)cmd.ElemCount);
                    Love.Graphics.Draw(renderMesh);

                    position += cmd.ElemCount;
                }

                Love.Graphics.SetScissor();

                renderMesh.Dispose();
                ArrayPool<uint>.Shared.Return(vertexMap);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public void Dispose()
        {
            _fontTexture?.Dispose();
            foreach (var loveTex in _imGuiToLoveTextures.Values)
            {
                loveTex.Dispose();
            }
            _imGuiToLoveTextures.Clear();
            _imCtx = IntPtr.Zero;
        }
    }
}
