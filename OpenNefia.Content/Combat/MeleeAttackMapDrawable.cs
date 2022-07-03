using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Combat
{
    public sealed class MeleeAttackMapDrawable : BaseMapDrawable
    {
        private bool _breaksIntoDebris;
        private AttackAnimType _attackAnimType;
        private int _damagePercent;
        private bool _isCritical;

        public MeleeAttackMapDrawable(bool breaksIntoDebris, AttackAnimType attackAnimType, int damagePercent, bool isCritical)
        {
            this._breaksIntoDebris = breaksIntoDebris;
            this._attackAnimType = attackAnimType;
            this._damagePercent = damagePercent;
            this._isCritical = isCritical;
        }

        public override void Update(float dt)
        {
            // TODO
            Finish();
        }

        public override void Draw()
        {
        }
    }
}