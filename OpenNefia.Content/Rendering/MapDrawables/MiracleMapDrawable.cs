using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Audio;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Rendering
{
    // >>>>>>>> shade2/screen.hsp:775: 	case aniHoly ...
    public sealed class MiracleMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        private readonly PrototypeId<SoundPrototype>? _soundID;
        private readonly PrototypeId<SoundPrototype>? _endSoundID;
        private readonly List<MiracleBeam> _beams;
        protected IAssetInstance AssetAnimMiracle;
        protected FrameCounter Counter;

        private class MiracleBeam
        {
            public MapCoordinates MapCoords { get; set; }
            public Vector2 ScreenPos { get; set; }
            public float Duration { get; set; }
        }

        public MiracleMapDrawable(IEnumerable<MapCoordinates> positions, PrototypeId<SoundPrototype>? soundID = null, PrototypeId<SoundPrototype>? endSoundID = null)
        {
            IoCManager.InjectDependencies(this);

            var waitSecs = IoCManager.Resolve<IConfigurationManager>().GetCVar(CCVars.AnimeAnimationWait) / 2;

            _soundID = soundID;
            _endSoundID = endSoundID;
            _beams = positions.Select(CreateMiracleBeam).WhereNotNull().ToList();
            AssetAnimMiracle = Assets.Get(Protos.Asset.AnimMiracle);
            Counter = new FrameCounter(waitSecs);
        }

        private MiracleBeam? CreateMiracleBeam(MapCoordinates coords, int index)
        {
            var screenPos = _coords.TileToScreen(coords.Position);
            var sx = screenPos.X - _coords.TileSize.X / 2;
            if (index != 0)
            {
                sx += 4 - _rand.Next(8);
            }
            var sy = screenPos.Y + 32;

            var newPos = new Vector2(sx, sy);
            if (!_hud.GamePixelBounds.Contains((newPos + ScreenOffset) * _coords.TileScale))
                return null;

            var duration = 20;
            if (index != 0)
                duration += _rand.Next(5);

            return new MiracleBeam()
            {
                MapCoords = coords,
                ScreenPos = newPos,
                Duration = duration
            };
        }

        public override void OnEnqueue()
        {
            if (_soundID != null)
                _audio.Play(_soundID.Value, ScreenLocalPos); // TODO is this position correct?
        }

        public override void Draw()
        {
            if (_beams.Count == 0)
                return;

            Love.Graphics.SetColor(Color.White);

            foreach (var beam in _beams)
            {
                if (beam.Duration <= 0)
                    continue;

                var animY = (beam.ScreenPos.Y * float.Clamp(20f - beam.Duration, 0, 6) / 6) - 96;

                // Beam below
                var region = int.Clamp((int)(8 - beam.Duration), 0, 8);
                if (beam.Duration < 15)
                    region++;

                AssetAnimMiracle.DrawRegion(_coords.TileScale, region.ToString(), ScreenOffset.X + beam.ScreenPos.X, ScreenOffset.Y + animY);

                // Bottom ripples
                if (beam.Duration <= 14 && beam.Duration >= 6)
                {
                    var region2 = 10 + (int)double.Floor(14 - beam.Duration) / 2;
                    AssetAnimMiracle.DrawRegion(_coords.TileScale, region2.ToString(), ScreenOffset.X + beam.ScreenPos.X, ScreenOffset.Y + animY + 16);
                }

                // Beam above
                var beamVertCount = double.Floor(double.Clamp(animY / 55 + 1, 0, 7 - double.Clamp((11 - beam.Duration) * 2, 0, 7)));

                for (var i = 0; i < beamVertCount; i++)
                {
                    var region2 = "beamBottom";
                    if (beam.Duration < 15)
                        region2 = "beamMiddle"; 

                    AssetAnimMiracle.DrawRegion(_coords.TileScale, region2, ScreenOffset.X + beam.ScreenPos.X, ScreenOffset.Y +  animY - (i+1) * 55);

                    if (i == beamVertCount - 1)
                    {
                        region2 = "beamTop";
                        AssetAnimMiracle.DrawRegion(_coords.TileScale, region2, ScreenOffset.X + beam.ScreenPos.X, ScreenOffset.Y + animY - (i+1) * 55 - 40);
                    }
                }
            }
        }

        private int _soundsPlayed = 0;

        public override void Update(float dt)
        {
            var running = false;

            var i = 0;
            foreach (var beam in _beams)
            {
                i += 1;
                if (beam.Duration <= 0f)
                    continue;

                running = true;

                if (beam.Duration >= 20f)
                {
                    beam.Duration -= dt * 100 * _rand.Next(2);
                }
                else
                {
                    beam.Duration -= dt * 100 * Counter.FrameDelaySecs;
                }

                if (_soundID != null && i > 1 && beam.Duration < 16 && _soundsPlayed < _beams.Count / 3)
                {
                    _soundsPlayed++;
                    _audio.Play(_soundID.Value, (Vector2i)(beam.ScreenPos * _coords.TileScale));
                }
            }

            if (!running)
            {
                if (_endSoundID != null && !IsFinished)
                    _audio.Play(_endSoundID.Value, ScreenLocalPos);
                Finish();
            }
        }
    }
    // <<<<<<<< shade2/screen.hsp:825 	swbreak ...
}
