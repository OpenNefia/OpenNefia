using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Rendering
{
    // >>>>>>>> shade2/screen.hsp:588: 	case aniBolt ...
    public sealed class BoltMapDrawable : BaseMapDrawable
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ICoords _coords = default!;

        [Obsolete("TODO remove as it is redundant with the position passed to IMapDrawablesManager.Enqueue()")]
        private MapCoordinates _startPos;
        private MapCoordinates _endPos;
        private IList<Vector2i> _offsets;
        private Color _color;
        private PrototypeId<SoundPrototype>? _impactSound;
        private FrameCounter _counter;
        private IAssetInstance _assetAnimShock;
        private int _endFrames = -1;
        private int _range;
        private Vector2i _tilePos;
        private int _frameIdx = 0;

        private const int MaxBoltFrames = 20;

        private class BoltFrame
        {
            public Vector2 ScreenPos;
            public float Frame;

            public BoltFrame(Vector2 screenPos)
            {
                ScreenPos = screenPos;
                Frame = 0f;
            }
        }
        private BoltFrame?[] _frames = new BoltFrame?[MaxBoltFrames];

        public BoltMapDrawable(MapCoordinates startPos, 
            MapCoordinates endPos,
            IList<Vector2i> offsets,
            int range,
            Color? color = null,
            PrototypeId<SoundPrototype>? impactSound = null)
        {
            IoCManager.InjectDependencies(this);

            _startPos = startPos;
            _endPos = endPos;
            _offsets = offsets;
            _color = color ?? Color.White;
            _impactSound = impactSound;
            _range = range;
            _assetAnimShock = Assets.Get(Prototypes.Protos.Asset.AnimShock);
            _tilePos = startPos.Position;

            _counter = new FrameCounter(_config.GetCVar(CCVars.AnimeAnimationWait) / 2.5f, MaxBoltFrames);
        }

        public override void OnThemeSwitched()
        {
            base.OnThemeSwitched();
        }

        public override bool CanEnqueue()
        {
            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(_gameSession.Player);

            return _startPos.MapId == _endPos.MapId
                && (Map.HasLineOfSight(playerSpatial.WorldPosition, _startPos.Position)
                || Map.HasLineOfSight(playerSpatial.WorldPosition, _endPos.Position));
        }

        public override void OnEnqueue()
        {
            _sounds.Play(Prototypes.Protos.Sound.Bolt1, _startPos);
        }

        public override void Update(float dt)
        {
            _counter.Update(dt);

            for (var i = 0; i < _frames.Length; i++)
            {
                var frame = _frames[i];
                if (frame == null)
                    break;

                frame.Frame += _counter.LastFramesPassed;
            }

            var delta = _counter.FrameInt - _frameIdx;

            while (delta >= 0)
            {
                delta--;
                if (_endFrames == -1)
                {
                    var offset = _offsets[_frameIdx % _offsets.Count];
                    _tilePos += offset;

                    if (Map.IsInBounds(_tilePos) && Map.CanSeeThrough(_tilePos) && Map.IsInWindowFov(_tilePos))
                    {
                        if ((_tilePos - _startPos.Position).Length > _range)
                        {
                            _endFrames = 4;
                        }
                        else if (_frameIdx < _frames.Length)
                        {
                            var screenPos = _coords.TileToScreen(_tilePos);
                            var pos = new Vector2(screenPos.X + _coords.TileSize.X / 2, screenPos.Y + _coords.TileSize.Y / 2);
                            _frames[_frameIdx++] = new BoltFrame(pos);
                        }
                    }
                    else
                    {
                        _endFrames = 4;
                    }
                }
                else
                {
                    _endFrames--;
                    if (_endFrames <= 0 && !IsFinished)
                    {
                        if (_impactSound != null)
                            _sounds.Play(_impactSound.Value, _endPos);
                        Finish();
                        return;
                    }
                }
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(_color);
            var angle = -(float)Angle.BetweenPoints(_endPos.Position, _startPos.Position).Theta;

            for (var i = 0; i < _frames.Length; i++)
            {
                var frame = _frames[i];
                if (frame == null)
                    break;

                if (frame.Frame < 5f)
                {
                    var region = (int)frame.Frame;
                    _assetAnimShock.DrawRegion(_coords.TileScale, region.ToString(), frame.ScreenPos.X + ScreenOffset.X, frame.ScreenPos.Y + ScreenOffset.Y, rotationRads: angle, centered: true);
                }
            }
        }

        public override void Dispose()
        {
        }
    }
    // <<<<<<<< shade2/screen.hsp:621 	swbreak ...
}
