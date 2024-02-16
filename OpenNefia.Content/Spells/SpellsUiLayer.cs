using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Skills;

namespace OpenNefia.Content.Spells
{
    /// <summary>
    /// Wraps the <see cref="JournalLayer"/> in a UI layer that's groupable.
    /// </summary>
    public class SpellsUiLayer : SpellGroupUiLayer
    {
        [Dependency] private readonly ISpellSystem _spells = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        [Child] private SpellsLayer _inner = new();
        
        public SpellsUiLayer()
        {
            _inner.EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(SpellGroupSublayerArgs args)
        {
            var spells = _protos.EnumeratePrototypes<SpellPrototype>()
                .Where(s => _skills.HasSkill(args.Caster, s.SkillID));
            var innerArgs = new SpellsLayer.Args(args.Caster, spells)
            {
            };
            UserInterfaceManager.InitializeLayer<SpellsLayer, SpellsLayer.Args, SpellsLayer.Result>(_inner, innerArgs);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _inner.GrabFocus();
        }

        public override void OnQuery()
        {
            _inner.OnQuery();
        }

        public override void OnQueryFinish()
        {
            _inner.OnQueryFinish();
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

        public override UiResult<SpellGroupSublayerResult>? GetResult()
        {
            var innerResult = _inner.GetResult();

            if (innerResult == null)
                return null;

            switch (innerResult)
            {
                case UiResult<SpellsLayer.Result>.Finished finished:
                    var newResult = new SpellGroupSublayerResult.CastSpell(finished.Value.Spell);
                    return new UiResult<SpellGroupSublayerResult>.Finished(newResult);
                case UiResult<SpellsLayer.Result>.Cancelled:
                    return new UiResult<SpellGroupSublayerResult>.Cancelled();
                case UiResult<SpellsLayer.Result>.Error err:
                    return new UiResult<SpellGroupSublayerResult>.Error(err.Exception);
            }

            return null;
        }

        public override void Update(float dt)
        {
            _inner.Update(dt);
        }

        public override void Draw()
        {
            _inner.Draw();
        }
    }
}