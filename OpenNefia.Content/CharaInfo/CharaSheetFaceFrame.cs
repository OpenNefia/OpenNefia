using OpenNefia.Content.Inventory;
using OpenNefia.Content.PCCs;
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
using OpenNefia.Core.ResourceManagement;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class CharaSheetFaceFrame : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private EntityUid _entity;

        [Child] private readonly UiTopicWindow WindowFrame;
        [Child] private readonly EntitySpriteBatch EntityBatch;
        private readonly TileAtlasBatch _portraitBatch;

        private PortraitPrototype? _portraitProto;
        private PCCDrawable? _pccDrawable;

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
                _portraitProto = _protos.Index(portraitComp.PortraitID);
            else
                _portraitProto = null;

            _pccDrawable?.Dispose();
            if (_entityManager.TryGetComponent<PCCComponent>(_entity, out var pcc) 
                && pcc.UsePCC)
            {
                _pccDrawable = PCCHelpers.CreatePCCDrawable(pcc, _resourceCache);
                _pccDrawable.Direction = PCCDirection.South;
                _pccDrawable.Frame = 0;
            }
            else
            {
                _pccDrawable = null;
            }
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
            EntityBatch.SetPosition(WindowFrame.X + 46, WindowFrame.Y + 61);
        }

        public override void Update(float dt)
        {
            WindowFrame.Update(dt);
            EntityBatch.Update(dt);

            if (_portraitProto != null)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(UIScale, _portraitProto.Image.AtlasIndex, 0, 0, WindowFrame.Width - 8, WindowFrame.Height - 8);
                _portraitBatch.Flush();
            }

            if (_pccDrawable != null)
            {
                _pccDrawable.Update(dt);
            }
            else
            {
                EntityBatch.Clear();
                EntityBatch.Add(_entity, 0, 0);
            }
        }

        public override void Draw()
        {
            WindowFrame.Draw();
            _portraitBatch.Draw(UIScale, WindowFrame.X + 4, WindowFrame.Y + 4);

            if (_pccDrawable != null)
            {
                _pccDrawable.Draw(UIScale, (WindowFrame.X + 44) * UIScale, (WindowFrame.Y + 64) * UIScale);
            }
            else
            {
                EntityBatch.Draw();
            }
        }   
    }
}
