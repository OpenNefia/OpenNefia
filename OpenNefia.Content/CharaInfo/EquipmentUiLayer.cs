using OpenNefia.Content.Equipment;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.CharaInfo
{
    public class EquipmentUiLayer : CharaGroupUiLayer
    {
        [Child] private EquipmentLayer _inner = new();

        public EquipmentUiLayer()
        {
            _inner.EventFilter = UIEventFilterMode.Pass;
            _inner.OnEquipped += HandleInnerLayerEquipped;
            _inner.OnUnequipped += HandleInnerLayerUnequipped;
        }

        private void HandleInnerLayerEquipped(WasEquippedInMenuEvent ev)
        {
            SharedSublayerResult.ChangedEquipment = true;
        }

        private void HandleInnerLayerUnequipped()
        {
            SharedSublayerResult.ChangedEquipment = true;
        }

        public override void Initialize(CharaGroupSublayerArgs args)
        {
            var innerArgs = new EquipmentLayer.Args(equipper: args.CharaEntity, equipTarget: args.CharaEntity);
            UserInterfaceManager.InitializeLayer<EquipmentLayer, EquipmentLayer.Args, EquipmentLayer.Result>(_inner, innerArgs);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _inner.GrabFocus();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            _inner.UpdateFromEquipTarget();
            _inner.OnQuery();
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            _inner.GetPreferredBounds(out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _inner.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _inner.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            _inner.Update(dt);
        }

        public override void Draw()
        {
            _inner.Draw();
        }

        public override UiResult<CharaGroupSublayerResult>? GetResult()
        {
            var innerResult = _inner.GetResult();

            if (innerResult == null)
                return null;

            switch (innerResult)
            {
                case UiResult<EquipmentLayer.Result>.Finished:
                    // Override with shared result
                    return new UiResult<CharaGroupSublayerResult>.Finished(SharedSublayerResult);
                case UiResult<EquipmentLayer.Result>.Cancelled:
                    return new UiResult<CharaGroupSublayerResult>.Cancelled();
                case UiResult<EquipmentLayer.Result>.Error err:
                    return new UiResult<CharaGroupSublayerResult>.Error(err.Exception);
            }

            return null;
        }
    }
}