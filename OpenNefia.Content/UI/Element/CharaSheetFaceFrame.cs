using OpenNefia.Content.Charas;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public sealed class CharaSheetFaceFrame : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private EntityUid _entity;

        private readonly UiTopicWindow WindowFrame;
        private readonly EntitySpriteBatch _entityBatch;
        private readonly TileAtlasBatch _portraitBatch;

        private PortraitPrototype? portraitProto;

        public CharaSheetFaceFrame()
        {
            EntitySystem.InjectDependencies(this);

            WindowFrame = new(UiTopicWindow.FrameStyleKind.One, UiTopicWindow.WindowStyleKind.One);
            _entityBatch = new EntitySpriteBatch();
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
            _entityBatch.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            WindowFrame.SetPosition(X, Y);
            _entityBatch.SetPosition(X + 46, Y + 61);
        }

        public override void Update(float dt)
        {
            WindowFrame.Update(dt);
            _entityBatch.Update(dt);

            if (portraitProto != null)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(portraitProto.Image.AtlasIndex, 0, 0, WindowFrame.PixelWidth - 8, WindowFrame.PixelHeight - 8);
                _portraitBatch.Flush();
            }

            _entityBatch.Clear();
            _entityBatch.Add(_entity, 0, 0);
        }

        public override void Draw()
        {
            WindowFrame.Draw();
            _portraitBatch.Draw(WindowFrame.PixelX + 4, WindowFrame.PixelY + 4);
            _entityBatch.Draw();
        }
    }
}
