using OpenNefia.Content.GameObjects.EntitySystems;
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
using OpenNefia.Core.UI.Layer;
using Serilog.Parsing;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    public abstract record class TrainerMode
    {
        public sealed record class Train : TrainerMode;
        public sealed record class Learn(ISet<PrototypeId<SkillPrototype>> TrainableSkills) : TrainerMode;
    }

    public sealed class CharaInfoTrainerUiLayer : UiLayerWithResult<CharaInfoTrainerUiLayer.Args, CharaInfoTrainerUiLayer.Result>
    {
        public class Args
        {
            public TrainerMode Mode { get; }
            public EntityUid TrainerEntity { get; }
            public EntityUid CharaEntity { get; }

            public Args(TrainerMode mode, EntityUid trainer, EntityUid chara)
            {
                Mode = mode;
                TrainerEntity = trainer;
                CharaEntity = chara;
            }
        }

        public new class Result
        {
            public Result(PrototypeId<SkillPrototype> id)
            {
                SelectedSkillID = id;
            }

            public PrototypeId<SkillPrototype> SelectedSkillID { get; }
        }

        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly ITrainerSystem _trainers = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        [Child] private UiKeyHintBar KeyHintBar = new();
        [Child] private CharaInfoPagesControl CharaInfoPages = new();

        private TrainerMode _mode = default!;
        private EntityUid _charaEntity;

        public CharaInfoTrainerUiLayer()
        {
            CharaInfoPages.Pages.OnPageChanged += HandlePagesPageChanged;
            CharaInfoPages.SkillsList.OnListItemActivated += HandleSkillsListItemActivated;
            CharaInfoPages.SkillsList.ShouldDisplaySkill = SkillsList_ShouldDisplaySkill;
            CharaInfoPages.SkillsList.ShouldDisplayResist = (_, _) => false;
            CharaInfoPages.SkillsList.FormatSkillDetail = SkillsList_FormatSkillDetail;
            CharaInfoPages.SkillsList.DetailAlignment = SkillsListControl.Alignment.Right;

            OnKeyBindDown += HandleKeyBindDown;
        }

        private bool SkillsList_ShouldDisplaySkill(SkillPrototype proto, EntityUid charaEntity)
        {
            if (_mode is TrainerMode.Learn modeLearn)
            {
                var protoID = proto.GetStrongID();
                return modeLearn.TrainableSkills.Contains(protoID)
                    && !_skills.HasSkill(charaEntity, protoID)
                    && proto.RelatedSkill != null;
            }
            return CharaInfoPages.SkillsList.DefaultShouldDisplaySkill(proto, charaEntity);
        }

        private string SkillsList_FormatSkillDetail(SkillPrototype proto, EntityUid charaEntity)
        {
            if (_mode is TrainerMode.Train)
            {
                var cost = _trainers.CalcTrainSkillCost(charaEntity, proto.GetStrongID());
                return $"{cost}p";
            }
            else if (_mode is TrainerMode.Learn)
            {
                var cost = _trainers.CalcLearnSkillCost(charaEntity, proto.GetStrongID());
                return $"{cost}p";
            }
            return CharaInfoPages.SkillsList.DefaultFormatSkillDetail(proto, charaEntity);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            CharaInfoPages.GrabFocus();
        }

        public override void Initialize(Args args)
        {
            _mode = args.Mode;
            _charaEntity = args.CharaEntity;

            CharaInfoPages.Initialize(_charaEntity);
            CharaInfoPages.RefreshFromEntity();

            // Switch to the skills page.
            CharaInfoPages.Pages.SetPage(1, playSound: false);
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
                Cancel();
            }
        }

        private void HandleSkillsListItemActivated(object? sender, UiListEventArgs<SkillsListControl.SkillsListEntry> e)
        {
            if (e.SelectedCell.Data is SkillsListControl.SkillsListEntry.Skill skill)
            {
                Finish(new(skill.SkillPrototype.GetStrongID()));
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            if (CharaInfoPages.Pages.CurrentElement is SkillsListControl)
            {
                if (_mode is TrainerMode.Train)
                {
                    keyHints.Add(new(new LocaleKey("Elona.CharaSheet.KeyHint.TrainSkill"), UiKeyNames.EnterKey));
                }
                else if (_mode is TrainerMode.Learn)
                {
                    keyHints.Add(new(new LocaleKey("Elona.CharaSheet.KeyHint.LearnSkill"), UiKeyNames.EnterKey));
                }
            }
            if (CharaInfoPages.Pages.PageCount > 1)
            {
                keyHints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));
            }
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