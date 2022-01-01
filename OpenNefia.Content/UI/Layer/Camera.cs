using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Layer
{
    public class Camera
    {
        private Vector2i _mapSize;
        private IDrawable _parent;
        private Vector2i _screenPos;
        public Vector2i ScreenPos { get => _screenPos; set => _screenPos = value; }

        public Camera(IDrawable parent)
        {
            _parent = parent;
        }

        public void SetMapSize(Vector2i mapSize)
        {
            this._mapSize = mapSize;
        }

        public void CenterOnScreenPos(Vector2i screenPos)
        {
            _screenPos = GameSession.Coords.BoundDrawPosition(screenPos, _mapSize, _parent.PixelSize);
        }

        public void CenterOnTilePos(Entity obj)
        {
            CenterOnTilePos(obj.Spatial.MapPosition);
        }

        public void CenterOnTilePos(MapCoordinates coords)
        {
            var screenPos = GameSession.Coords.TileToScreen(coords.Position);
            CenterOnScreenPos(screenPos);
        }

        public void Pan(Vector2 screenDPos)
        {
            _screenPos += (Vector2i)screenDPos;
        }

        public void TileToVisibleScreen(MapCoordinates coords, out Vector2i screenPos)
        {
            screenPos = GameSession.Coords.TileToScreen(coords.Position) + _screenPos;
        }

        public void VisibleScreenToTile(Vector2i screenPos, out Vector2i tilePos)
        {
            tilePos = GameSession.Coords.ScreenToTile(screenPos - _screenPos);
        }
    }
}
