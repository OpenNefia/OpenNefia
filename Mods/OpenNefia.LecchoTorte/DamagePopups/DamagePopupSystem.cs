using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.LecchoTorte.DamagePopups
{
    public interface IDamagePopupSystem : IEntitySystem
    {
        void AddDamagePopup(DamagePopup popup, MapCoordinates coords);
        void AddDamagePopup(DamagePopup popup, EntityUid uid);
        void ClearDamagePopups();
    }

    public sealed class DamagePopupSystem : EntitySystem, IDamagePopupSystem
    {
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<ActiveMapChangedEvent>(DamagePopup_ActiveMapChanged);
            SubscribeEntity<AfterDamageHPEvent>(DamagePopup_AfterDamageHP);
            SubscribeEntity<AfterHealEvent>(DamagePopup_AfterHeal);
            SubscribeEntity<AfterPhysicalAttackMissEventArgs>(DamagePopup_AfterPhysicalAttackMiss);
        }

        private void DamagePopup_ActiveMapChanged(ActiveMapChangedEvent ev)
        {
            ClearDamagePopups();
        }

        private void DamagePopup_AfterDamageHP(EntityUid uid, ref AfterDamageHPEvent args)
        {
            if (!_vis.IsInWindowFov(uid))
                return;

            AddDamagePopup(new DamagePopup()
            {
                Text = args.FinalDamage.ToString()
            }, uid);
        }

        private void DamagePopup_AfterHeal(EntityUid uid, ref AfterHealEvent args)
        {
            if (!args.ShowMessage || !_vis.IsInWindowFov(uid))
                return;

            var color = args.Type switch
            {
                HealType.HP => new Color(175, 255, 175),
                HealType.MP => new Color(175, 175, 255),
                HealType.Stamina => new Color(255, 255, 175),
                _ => Color.White
            };
            
            AddDamagePopup(new DamagePopup()
            {
                Text = args.Amount.ToString(),
                Color = color
            }, uid);
        }

        private void DamagePopup_AfterPhysicalAttackMiss(EntityUid uid, AfterPhysicalAttackMissEventArgs args)
        {
            if (!_vis.IsInWindowFov(uid))
                return;

            if (args.HitResult == HitResult.Evade)
            {
                AddDamagePopup(new DamagePopup()
                {
                    Text = "evade!!"
                }, uid);
            }
            else
            {
                
                AddDamagePopup(new DamagePopup()
                {
                    Text = "miss"
                }, uid);
            }
        }

        public void AddDamagePopup(DamagePopup popup, MapCoordinates coords)
        {
            var tileLayer = _mapRenderer.GetTileLayer<DamagePopupsTileLayer>();
            tileLayer.AddDamagePopup(popup, coords);
        }

        public void AddDamagePopup(DamagePopup popup, EntityUid uid)
        {
            var tileLayer = _mapRenderer.GetTileLayer<DamagePopupsTileLayer>();
            tileLayer.AddDamagePopup(popup, uid);
        }

        public void ClearDamagePopups()
        {
            var tileLayer = _mapRenderer.GetTileLayer<DamagePopupsTileLayer>();
            tileLayer.ClearDamagePopups();
        }
    }
}