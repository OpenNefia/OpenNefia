using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class CharaInfoUiLayer : CharaGroupUiLayer
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        [Child] private UiKeyHintBar KeyHintBar = new();
        [Child] private CharaInfoPagesControl CharaInfoPages = new();

        private EntityUid _charaEntity;

        public CharaInfoUiLayer()
        {
            CharaInfoPages.Pages.OnPageChanged += HandlePagesPageChanged;
            CharaInfoPages.SkillsList.OnListItemActivated += HandleSkillsListItemActivated;

            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            CharaInfoPages.GrabFocus();
        }

        public override void Initialize(CharaGroupSublayerArgs args)
        {
            _charaEntity = args.CharaEntity;

            CharaInfoPages.Initialize(_charaEntity);
            CharaInfoPages.RefreshFromEntity();
        }

        private void HandlePagesPageChanged(int newPage, int newPageCount)
        {
            UpdateKeyHintBar();
        }

        private void UpdateKeyHintBar()
        {
            KeyHintBar.Text = UserInterfaceManager.FormatKeyHints(MakeKeyHints());
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UICancel)
            {
                Finish(SharedSublayerResult);
            }
        }

        private void HandleSkillsListItemActivated(object? sender, UiListEventArgs<SkillsListControl.SkillsListEntry> e)
        {
            if (e.SelectedCell.Data is not SkillsListControl.SkillsListEntry.Skill skill)
                return;

            if (!_entityManager.TryGetComponent(_charaEntity, out SkillsComponent skills))
                return;

            var unlimited = _config.GetCVar(CCVars.DebugUnlimitedSkillPoints);

            if (skills.BonusPoints <= 0 && !unlimited)
                return;

            if (_skills.HasSkill(_charaEntity, skill.SkillPrototype, skills))
            {
                Sounds.Play(Sound.Spend1);
                _skills.ApplyBonusPoint(_charaEntity, skill.SkillPrototype.GetStrongID(), skills);

                if (!unlimited)
                    skills.BonusPoints--;
                
                CharaInfoPages.RefreshFromEntity();
            }
            else
            {
                Sounds.Play(Sound.Fail1);
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(new LocaleKey("Elona.CharaSheet.KeyHint.BlessingAndHex"), UiKeyNames.Cursor));
            keyHints.AddRange(CharaInfoPages.MakeKeyHints());
            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Sound.Chara);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            CharaInfoPages.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            KeyHintBar.SetSize(_graphics.WindowSize.X - 240, 16);
            CharaInfoPages.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            KeyHintBar.SetPosition(240, 0);
            CharaInfoPages.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            KeyHintBar.Update(dt);
            CharaInfoPages.Update(dt);
        }

        public override void Draw()
        {
            KeyHintBar.Draw();
            CharaInfoPages.Draw();
        }
    }
}