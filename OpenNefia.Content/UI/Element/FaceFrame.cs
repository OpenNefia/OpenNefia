using OpenNefia.Content.Charas;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element
{
    public class FaceFrame : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private EntityUid _entity;
        private bool RenderFrame;
        private PortraitPrototype? portraitProto;
        private readonly TileAtlasBatch _portraitBatch;

        [Child] private readonly UiTopicWindow WindowFrame;

        public FaceFrame(bool renderFrame = true)
        {
            EntitySystem.InjectDependencies(this);

            RenderFrame = renderFrame;
            WindowFrame = new(UiTopicWindow.FrameStyleKind.One, UiTopicWindow.WindowStyleKind.One);
            _portraitBatch = new TileAtlasBatch(ContentAtlasNames.Portrait);
        }

        public void RefreshFromEntity(EntityUid entity)
        {
            _entity = entity;

            if (_entityManager.TryGetComponent(_entity, out PortraitComponent portraitComp))
                portraitProto = _protos.Index(portraitComp.PortraitID);
            else
                portraitProto = null;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(90, 120);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            WindowFrame.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            WindowFrame.SetPosition(x, y);
        }

        public override void Draw()
        {
            if (RenderFrame)
                WindowFrame.Draw();
            _portraitBatch.Draw(UIScale, WindowFrame.X + 4, WindowFrame.Y + 4);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (portraitProto != null)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(UIScale, portraitProto.Image.AtlasIndex, 0, 0, WindowFrame.Width - 8, WindowFrame.Height - 8);
                _portraitBatch.Flush();
            }
        }
    }
}
