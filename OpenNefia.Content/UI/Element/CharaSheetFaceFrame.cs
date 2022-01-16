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

        public CharaSheetFaceFrame(EntityUid entity)
        {
            EntitySystem.InjectDependencies(this);

            _entity = entity;

            WindowFrame = new(UiTopicWindow.FrameStyleKind.One, UiTopicWindow.WindowStyleKind.One);
            _entityBatch = new EntitySpriteBatch();
            _portraitBatch = new TileAtlasBatch(ContentAtlasNames.Portrait);

            if (_entityManager.TryGetComponent(_entity, out PortraitComponent portraitComp))
                portraitProto = _protos.Index(portraitComp.PortraitID);
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = new(90, 120);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            WindowFrame.SetSize(Width, Height);
            _entityBatch.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
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
                _portraitBatch.Add(portraitProto.Image.AtlasIndex, 0, 0, WindowFrame.Width - 8, WindowFrame.Height - 8);
                _portraitBatch.Flush();
            }

            _entityBatch.Clear();
            _entityBatch.Add(_entity, 0, 0);
        }

        public override void Draw()
        {
            WindowFrame.Draw();
            _portraitBatch.Draw(WindowFrame.X + 4, WindowFrame.Y + 4);
            _entityBatch.Draw();
        }
    }
}
