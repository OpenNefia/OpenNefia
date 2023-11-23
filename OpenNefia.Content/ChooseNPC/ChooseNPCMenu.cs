using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI;
using Love;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.Content.DisplayName;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Weight;
using OpenNefia.Content.Levels;
using System.Collections.Generic;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Logic;
using CSharpRepl.Services.Roslyn.Formatting;

namespace OpenNefia.Content.ChooseNPC
{
    public interface IChooseNPCBehavior
    {
        public string WindowTitle { get; }
        public string TopicName { get; }
        public string TopicInfo { get; }
        public string? TopicCustom { get; }

        string FormatName(EntityUid entity);
        string FormatDetail(EntityUid entity);
        int GetOrdering(EntityUid entity);
    }

    public class DefaultChooseNPCBehavior : IChooseNPCBehavior
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public virtual string WindowTitle => Loc.GetString("Elona.UI.ChooseNPC.Window.Title");
        public virtual string TopicName => Loc.GetString("Elona.UI.ChooseNPC.Topic.Name");
        public virtual string TopicInfo => Loc.GetString("Elona.UI.ChooseNPC.Topic.Info");
        public virtual string? TopicCustom => null;

        public virtual string FormatName(EntityUid entity)
        {
            var chara = _entityManager.GetComponent<CharaComponent>(entity);
            var age = _entityManager.GetComponentOrNull<WeightComponent>(entity)?.Age ?? 0;
            var genderText = Loc.Capitalize(Loc.GetString($"Elona.Gender.Names.{chara.Gender}.Polite"));
            var ageText = Loc.GetString("Elona.UI.ChooseNPC.AgeCounter", ("age", age));
            var level = _levels.GetLevel(entity);
            return $"Lv.{level} {genderText}{ageText}";
        }

        public virtual string FormatDetail(EntityUid entity)
        {
            return string.Empty;
        }

        public virtual int GetOrdering(EntityUid entity)
        {
            return _levels.GetLevel(entity);
        }
    }

    public sealed class ChooseNPCMenu : UiLayerWithResult<ChooseNPCMenu.Args, ChooseNPCMenu.Result>
    {
        public sealed class ChooseNPCListItem
        {
            public EntityUid Entity { get; set; }

            public string Text { get; set; } = string.Empty;
            public string Info1 { get; set; } = string.Empty;
            public string Info2 { get; set; } = string.Empty;
            public long Ordering { get; set; }
        }

        public class ChooseNPCListCell : UiListCell<ChooseNPCListItem>
        {
            [Child] private UiText Info1Text;
            [Child] private UiText Info2Text;
            private EntityUid _entity;
            private EntitySpriteBatch _batch;

            public ChooseNPCListCell(ChooseNPCListItem data, EntitySpriteBatch batch)
                : base(data, new UiText())
            {
                Text = data.Text;
                Info1Text = new UiText(Data.Info1);
                Info2Text = new UiText(Data.Info2);

                _entity = data.Entity;
                _batch = batch;
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                Info1Text.SetPosition(x + 288, y + 2);
                Info2Text.SetPosition(x + 428, y + 2);
            }

            public override void SetSize(float w, float h)
            {
                base.SetSize(w, h);
                Info1Text.SetPreferredSize();
                Info2Text.SetPreferredSize();
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                UiText.Update(dt);
                Info1Text.Update(dt);
                Info2Text.Update(dt);
            }

            public override void Draw()
            {
                base.Draw();
                _batch.Add(_entity, X - 44, Y - 7, centering: BatchCentering.AlignBottom);
                UiText.Draw();
                Info1Text.Draw();
                Info2Text.Draw();
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override int? DefaultZOrder => HudLayer.HudZOrder + 10000;

        [Child] private EntitySpriteBatch _batch = new();
        [Child] private UiPagedList<ChooseNPCListItem> _list = new();
        [Child] private UiWindow _win = new();
        [Child] private UiText _textTopicName = new UiTextTopic();
        [Child] private UiText _textTopicInfo = new UiTextTopic();
        [Child] private UiText _textTopicCustom = new UiTextTopic();

        private string? _prompt;

        public ChooseNPCMenu()
        {
            _list.PageTextElement = _win;

            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;

            _list.OnActivated += HandleListOnActivate;
        }

        public class Args
        {
            public Args(IEnumerable<EntityUid> candidates)
            {
                Candidates = candidates.ToList();
            }

            public IList<EntityUid> Candidates { get; } = new List<EntityUid>();

            public IChooseNPCBehavior Behavior { get; set; } = new DefaultChooseNPCBehavior();
            public string? Prompt { get; set; }
        }

        public class Result
        {
            public Result(EntityUid selected)
            {
                Selected = selected;
            }

            public EntityUid Selected { get; }
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
                args.Handle();
            }
        }

        private void HandleListOnActivate(object? sender, UiListEventArgs<ChooseNPCListItem> e)
        {
            Finish(new(e.SelectedCell.Data.Entity));
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _list.GrabFocus();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        private IEnumerable<ChooseNPCListItem> GenerateList(Args args)
        {
            bool CanShow(EntityUid entity)
            {
                if (!_entityManager.HasComponent<CharaComponent>(entity))
                    return false;

                if (_gameSession.IsPlayer(entity))
                    return false;

                if (_entityManager.HasComponent<TemporaryAllyComponent>(entity))
                    return false;

                return true;
            }

            ChooseNPCListItem Transform(EntityUid entity)
            {
                var info1 = args.Behavior.FormatDetail(entity);
                var info2 = args.Behavior.FormatDetail(entity);
                var ordering = args.Behavior.GetOrdering(entity);

                return new ChooseNPCListItem()
                {
                    Text = _displayNames.GetDisplayName(entity).WideSubstring(0, 36),
                    Info1 = info1,
                    Info2 = info2,
                    Entity = entity,
                    Ordering = ordering
                };
            }

            return args.Candidates.Where(CanShow).Select(Transform).OrderBy(c => c.Ordering);
        }

        public override void Initialize(Args args)
        {
            EntitySystem.InjectDependencies(args.Behavior);

            _win.KeyHints = MakeKeyHints();

            _win.Title = args.Behavior.WindowTitle;
            _textTopicName.Text = args.Behavior.TopicName;
            _textTopicInfo.Text = args.Behavior.TopicInfo;

            if (args.Behavior.TopicCustom != null)
            {
                _textTopicCustom.Visible = true;
                _textTopicCustom.Text = args.Behavior.TopicCustom;
            }
            else
            {
                _textTopicCustom.Visible = false;
            }

            var data = GenerateList(args);
            _list.SetCells(data.Select(c => new ChooseNPCListCell(c, _batch)));
            _prompt = args.Prompt;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _win.SetSize(width, height);
            _list.SetSize(width, height);
            _textTopicName.SetPreferredSize();
            _textTopicInfo.SetPreferredSize();
            _textTopicCustom.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _win.SetPosition(x, y);
            _list.SetPosition(X + 58, Y + 66);
            _textTopicName.SetPosition(X + 28, Y + 36);
            _textTopicInfo.SetPosition(X + 350, Y + 36);
            _textTopicCustom.SetPosition(X + 490, Y + 36);
        }

        public override void OnQuery()
        {
            if (_prompt != null)
                _mes.Display(_prompt);
            _audio.Play(Protos.Sound.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(700, 448, out bounds);
        }

        public override void Update(float dt)
        {
            _win.Update(dt);

            _textTopicName.Update(dt);
            _textTopicInfo.Update(dt);
            _textTopicCustom.Update(dt);

            _list.Update(dt);
            _batch.Update(dt);
        }

        public override void Draw()
        {
            _win.Draw();

            _textTopicName.Draw();
            _textTopicInfo.Draw();
            _textTopicCustom.Draw();

            _batch.Clear();
            _list.Draw();
            _batch.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _batch.Dispose();
        }
    }
}