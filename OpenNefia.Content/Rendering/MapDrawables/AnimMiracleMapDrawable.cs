using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Rendering
{
    public sealed class AnimMiracleMapDrawable : BaseMapDrawable
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly ICoords _coords = default!;

        private IList<Vector2i> Positions;

        public AnimMiracleMapDrawable(IList<Vector2i> positions)
        {
            // TODO
            IoCManager.InjectDependencies(this);

            Positions = positions;
        }

        public override void Draw()
        {
        }

        public override void Update(float dt)
        {
            Finish();
        }
    }
}
