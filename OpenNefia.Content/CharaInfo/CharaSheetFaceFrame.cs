using OpenNefia.Content.Inventory;
using OpenNefia.Content.Portraits;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class CharaSheetFaceFrame : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private EntityUid _entity;

        [Child] private readonly UiTopicWindow WindowFrame;
        [Child] private readonly EntitySpriteBatch EntityBatch;
        private readonly TileAtlasBatch _portraitBatch;

        private PortraitPrototype? portraitProto;

        public CharaSheetFaceFrame()
        {
            EntitySystem.InjectDependencies(this);

            WindowFrame = new(UiTopicWindow.FrameStyleKind.One, UiTopicWindow.WindowStyleKind.One);
            EntityBatch = new EntitySpriteBatch();
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
            WindowFrame.SetSize(Width, Height);
            EntityBatch.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            WindowFrame.SetPosition(X, Y);
            EntityBatch.SetPosition(X + 46, Y + 61);
        }

        public override void Update(float dt)
        {
            WindowFrame.Update(dt);
            EntityBatch.Update(dt);

            if (portraitProto != null)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(UIScale, portraitProto.Image.AtlasIndex, 0, 0, WindowFrame.Width - 8, WindowFrame.Height - 8);
                _portraitBatch.Flush();
            }

            EntityBatch.Clear();
            EntityBatch.Add(_entity, 0, 0);
        }

        public override void Draw()
        {
            WindowFrame.Draw();
            _portraitBatch.Draw(UIScale, WindowFrame.X + 4, WindowFrame.Y + 4);
            EntityBatch.Draw();
        }
    }
}
