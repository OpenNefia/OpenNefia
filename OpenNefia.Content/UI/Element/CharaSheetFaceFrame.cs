using OpenNefia.Content.Charas;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public sealed class CharaSheetFaceFrame : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private EntityUid _entity;

        [Child] private readonly EntitySpriteBatch EntityBatch;
        [Child] private FaceFrame FaceFrame;

        public CharaSheetFaceFrame()
        {
            EntitySystem.InjectDependencies(this);

            FaceFrame = new FaceFrame();
            EntityBatch = new EntitySpriteBatch();
        }

        public void RefreshFromEntity(EntityUid entity)
        {
            _entity = entity;
            FaceFrame.RefreshFromEntity(entity);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            FaceFrame.GetPreferredSize(out size);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            FaceFrame.SetSize(Width, Height);
            EntityBatch.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            FaceFrame.SetPosition(X, Y);
            EntityBatch.SetPosition(X + 46, Y + 61);
        }

        public override void Update(float dt)
        {
            FaceFrame.Update(dt);
            EntityBatch.Update(dt);

            EntityBatch.Clear();
            EntityBatch.Add(_entity, 0, 0);
        }

        public override void Draw()
        {
            FaceFrame.Draw();
            EntityBatch.Draw();
        }
    }
}
