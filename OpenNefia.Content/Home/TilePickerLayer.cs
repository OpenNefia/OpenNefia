using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Maps;
using Love;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;
using NetVips;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using Serilog.Debugging;
using Vector2 = OpenNefia.Core.Maths.Vector2;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Content.Home
{
    public sealed class TilePickerLayer : UiLayerWithResult<TilePickerLayer.Args, TilePickerLayer.Result>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        public class Args
        {
            public IEnumerable<PrototypeId<TilePrototype>>? Tiles { get; }
        } 

        public class Result
        {
            public PrototypeId<TilePrototype> TileID { get; set; }
        }

        public TilePickerLayer()
        {
            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        private List<TilePrototype> _tiles = new();
        private Vector2i _tileSize;
        private int _countX;
        private TileAtlasBatch _tileBatch = default!;

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel || args.Function == EngineKeyFunctions.UIRightClick)
            {
                Cancel();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIClick)
            {
                CheckMouseClick();
                args.Handle();
            }
        }

        private void CheckMouseClick()
        {
            var pos = UserInterfaceManager.MousePositionScaled.Position / UIScale;
            var index = PosToIndex(pos);
            if (index >= 0 && index < _tiles.Count)
            {
                _audio.Play(Prototypes.Protos.Sound.Ok1);
                Finish(new() { TileID = _tiles[index].GetStrongID() });
            }
            else
            {
                _audio.Play(Prototypes.Protos.Sound.Fail1);
            }
        }

        private IEnumerable<TilePrototype> GetSelectableTiles()
        {
            return _protos.EnumeratePrototypes<TilePrototype>()
                .Where(t => MapDesignerLayer.IsTileSelectable(t, _protos));
        }

        public override void Initialize(Args args)
        {
            // TODO remove
            LayerUIScale = _config.GetCVar(CCVars.DisplayUIScale);

            _tiles = (args.Tiles != null ? args.Tiles.Select(id => _protos.Index(id)) : GetSelectableTiles()).ToList();
            _tileSize = _coords.TileSize / 2;
            _countX = (int)(_graphics.WindowSize.X / _tileSize.X);
            _tileBatch = MakeTileBatch();
        }

        private Vector2 IndexToPos(int i)
        {
            return new Vector2(X + (i % _countX) * _tileSize.X, (Y + (i / _countX) * _tileSize.Y));
        }

        private int PosToIndex(Vector2 pos)
        {
            return (int)((pos.X - X) / _tileSize.X) + (int)((pos.Y - X) / _tileSize.Y) * _countX;
        }

        private TileAtlasBatch MakeTileBatch()
        {
            var batch = new TileAtlasBatch(AtlasNames.Tile);

            foreach (var (tile, i) in _tiles.WithIndex())
            {
                var pos = IndexToPos(i);
                batch.Add(UIScale, tile.Image.AtlasIndex, pos.X, pos.Y, _tileSize.X, _tileSize.Y);
            }

            return batch;
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            _tileBatch.Draw(UIScale, X, Y);

            Graphics.SetLineWidth(1);
            foreach (var (tile, i) in _tiles.WithIndex())
            {
                var pos = IndexToPos(i);
                if (tile.IsSolid)
                {
                    Graphics.SetColor(240, 230, 220);
                }
                else
                {
                    Graphics.SetColor(200, 200, 200);
                }
                GraphicsS.RectangleS(UIScale, DrawMode.Line, pos, _tileSize);
            }
        }
    }
}
