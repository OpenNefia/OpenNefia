using OpenNefia.Content.Buffs;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Prototypes;

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
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<ActiveMapChangedEvent>(DamagePopup_ActiveMapChanged);
            SubscribeEntity<AfterDamageHPEvent>(DamagePopup_AfterDamageHP);
            SubscribeEntity<AfterDamageMPEvent>(DamagePopup_AfterDamageMP);
            SubscribeEntity<AfterHealEvent>(DamagePopup_AfterHeal);
            SubscribeEntity<AfterPhysicalAttackMissEventArgs>(DamagePopup_AfterPhysicalAttackMiss);
            SubscribeEntity<AfterEntityReceivedBuffEvent>(DamagePopup_AfterReceivedBuff);
            SubscribeEntity<BeforeEntityLosesBuffEvent>(DamagePopup_BeforeLoseBuff);
        }

        private void DamagePopup_BeforeLoseBuff(EntityUid uid, BeforeEntityLosesBuffEvent args)
        {
            Color color;
            if (args.Buff.Alignment == BuffAlignment.Negative)
                color = new Color(0, 200, 0);
            else
                color = new Color(200, 100, 100);

            AddDamagePopup(new DamagePopup()
            {
                Text = "-" + _displayNames.GetDisplayName(args.Buff.Owner),
                Color = color,
                Icon = Assets.Get(args.Buff.Icon)
            }, uid);
        }

        private void DamagePopup_ActiveMapChanged(ActiveMapChangedEvent ev)
        {
            ClearDamagePopups();
        }

        private void DamagePopup_AfterDamageHP(EntityUid uid, ref AfterDamageHPEvent args)
        {
            if (!_vis.IsInWindowFov(uid))
                return;

            var color = Color.White;
            if (args.DamageType is ElementalDamageType elemDamage && _protos.TryIndex(elemDamage.ElementID, out var elemProto))
            {
                color = elemProto.Color;
            }

            AddDamagePopup(new DamagePopup()
            {
                Text = args.FinalDamage.ToString(),
                Color = color
            }, uid);
        }

        private void DamagePopup_AfterDamageMP(EntityUid uid, ref AfterDamageMPEvent args)
        {
            if (!args.ShowMessage || !_vis.IsInWindowFov(uid))
                return;

            AddDamagePopup(new DamagePopup()
            {
                Text = args.Amount.ToString(),
                Color = Color.Purple
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
                Text = args.OriginalAmount.ToString(),
                Color = color
            }, uid);
        }

        private void DamagePopup_AfterPhysicalAttackMiss(EntityUid uid, AfterPhysicalAttackMissEventArgs args)
        {
            if (!_vis.IsInWindowFov(args.Target))
                return;

            if (args.HitResult == HitResult.Evade)
            {
                AddDamagePopup(new DamagePopup()
                {
                    Text = "evade!!"
                }, args.Target);
            }
            else
            {

                AddDamagePopup(new DamagePopup()
                {
                    Text = "miss"
                }, args.Target);
            }
        }

        private void DamagePopup_AfterReceivedBuff(EntityUid uid, AfterEntityReceivedBuffEvent args)
        {
            Color color;
            if (args.Buff.Alignment == BuffAlignment.Negative)
                color = new Color(200, 0, 0);
            else
                color = new Color(0, 200, 200);

            AddDamagePopup(new DamagePopup()
            {
                Text = _displayNames.GetDisplayName(args.Buff.Owner),
                Color = color,
                Icon = Assets.Get(args.Buff.Icon)
            }, uid);
        }

        public void AddDamagePopup(DamagePopup popup, MapCoordinates coords)
        {
            if (_mapRenderer.TryGetTileLayer<DamagePopupsTileLayer>(out var tileLayer))
                tileLayer.AddDamagePopup(popup, coords);
        }

        public void AddDamagePopup(DamagePopup popup, EntityUid uid)
        {
            if (_mapRenderer.TryGetTileLayer<DamagePopupsTileLayer>(out var tileLayer))
                tileLayer.AddDamagePopup(popup, uid);
        }

        public void ClearDamagePopups()
        {
            if (_mapRenderer.TryGetTileLayer<DamagePopupsTileLayer>(out var tileLayer))
                tileLayer.ClearDamagePopups();
        }
    }
}