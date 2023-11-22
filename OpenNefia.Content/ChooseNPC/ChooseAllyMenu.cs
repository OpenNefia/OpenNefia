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
using OpenNefia.Content.Skills;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.Parties;
using static OpenNefia.Content.ChooseNPC.ChooseAllyMenu.ChooseAllyData;

namespace OpenNefia.Content.ChooseNPC
{
    public interface IChooseAllyBehavior
    {
        public bool IsMultiSelect => false;
        public int MultiSelectCount => 2;

        public string WindowTitle { get; }
        public string TopicName { get; }
        public string TopicInfo { get; }

        string FormatName(EntityUid entity);
        string FormatInfo(EntityUid entity);
        int GetOrdering(EntityUid entity);
        Color GetTextColor(EntityUid entity) => UiColors.TextBlack;

        /// <summary>
        /// Called when an entity is selected.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns><c>false</c> if the entity should be blocked from selection.</returns>
        bool CanSelect(EntityUid entity) { return true; }
    }

    public class DefaultChooseAllyBehavior : IChooseAllyBehavior
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public virtual string WindowTitle => Loc.GetString("Elona.UI.ChooseAlly.Window.Title");
        public virtual string TopicName => Loc.GetString("Elona.UI.ChooseAlly.Topic.Name");
        public virtual string TopicInfo => Loc.GetString("Elona.UI.ChooseAlly.Topic.Status");

        protected string FormatHP(EntityUid entity)
        {
            if (!_entityManager.TryGetComponent<SkillsComponent>(entity, out var skills))
            {
                return string.Empty;
            }
            var percent = (int)(skills.HP * 100 / skills.MaxHP);
            return $"(Hp: {percent}%)";
        }

        public virtual string FormatInfo(EntityUid entity)
        {
            // >>>>>>>> shade2/command.hsp:551 			s="Lv."+cLevel(i)+" " ...
            // allyCtrl=3
            var chara = _entityManager.GetComponentOrNull<CharaComponent>(entity);
            var statusText = string.Empty;
            switch (chara?.Liveness)
            {
                case CharaLivenessState.PetDead:
                    statusText = Loc.GetString("Elona.UI.ChooseAlly.Status.Dead");
                    break;
                case CharaLivenessState.PetWait:
                    statusText = $"{FormatHP(entity)} {Loc.GetString("Elona.UI.ChooseAlly.Status.Waiting")}";
                    break;
                case CharaLivenessState.Alive:
                    statusText = FormatHP(entity);
                    break;
            }

            var level = _levels.GetLevel(entity);
            return $"Lv.{level} {statusText}".Trim();
            // <<<<<<<< shade2/command.hsp:558 				} ..
        }

        public virtual string FormatName(EntityUid entity)
        {
            // >>>>>>>> shade2/command.hsp:545 		s=""+cnAka(i)+" "+cnName(i)  ...
            var title = _entityManager.GetComponentOrNull<AliasComponent>(entity)?.Alias ?? "";
            var name = _displayNames.GetBaseName(entity);
            var result = $"{name} {title}";
            if (_entityManager.TryGetComponent<StayingComponent>(entity, out var staying)
                && staying.StayingLocation != null)
                result += $"({staying.StayingLocation.AreaName})";
            return result.Trim();
            // <<<<<<<< shade2/command.hsp:546 		if cArea(i)!0:s=s+"("+mapName(cArea(i))+")" ..
        }

        public virtual int GetOrdering(EntityUid entity)
        {
            return _levels.GetLevel(entity);
        }
    }

    public sealed class ChooseAllyMenu : UiLayerWithResult<ChooseAllyMenu.Args, ChooseAllyMenu.Result>
    {
        public abstract record ChooseAllyData
        {
            public sealed record Ally(EntityUid Entity) : ChooseAllyData;
            public sealed record Proceed() : ChooseAllyData;
        }

        public sealed class ChooseAllyListItem
        {
            public ChooseAllyListItem(ChooseAllyData data)
            {
                Data = data;
            }

            public ChooseAllyData Data { get; set; }

            public Color TextColor { get; set; } = UiColors.MesBlack;
            public string Text { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
            public long Ordering { get; set; }

            public bool Selected { get; set; } = false;
        }

        public class ChooseAllyListCell : UiListCell<ChooseAllyListItem>
        {
            [Child] private UiText InfoText;
            private EntityUid? _entity;
            private EntitySpriteBatch _batch;

            public ChooseAllyListCell(ChooseAllyListItem item, EntitySpriteBatch batch)
                : base(item, new UiText())
            {
                Text = item.Text;
                UiText.Color = item.TextColor;
                InfoText = new UiText(Data.Info);

                if (item.Data is ChooseAllyData.Ally ally)
                    _entity = ally.Entity;
                else
                    _entity = null;
                _batch = batch;
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                InfoText.SetPosition(x + 272, y + 2);
            }

            public override void SetSize(float w, float h)
            {
                base.SetSize(w, h);
                InfoText.SetPreferredSize();
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                UiText.Update(dt);
                InfoText.Update(dt);
            }

            public override void Draw()
            {
                base.Draw();
                if (_entity != null)
                    _batch.Add(_entity.Value, X - 44, Y + 8, centering: BatchCentering.Centered);
                UiText.Draw();
                InfoText.Draw();
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public override int? DefaultZOrder => HudLayer.HudZOrder + 10000;

        [Child] private EntitySpriteBatch _batch = new();
        [Child] private UiPagedList<ChooseAllyListItem> _list = new();
        [Child] private UiWindow _win = new();
        [Child] private UiText _textTopicName = new UiTextTopic();
        [Child] private UiText _textTopicInfo = new UiTextTopic();

        private string? _prompt;
        private IChooseAllyBehavior _behavior = new DefaultChooseAllyBehavior();

        public ChooseAllyMenu()
        {
            _list.PageTextElement = _win;

            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;

            _list.OnActivated += HandleListOnActivate;
        }

        public class Args
        {
            public Args()
            {
            }

            public Args(IEnumerable<EntityUid> candidates)
            {
                Candidates = candidates.ToList();
            }

            public IList<EntityUid>? Candidates { get; }

            public string? WindowTitle { get; set; }
            public string? TopicName { get; set; }
            public string? TopicInfo { get; set; }
            public int XOffset { get; set; } = 0;
            public IChooseAllyBehavior Behavior { get; set; } = new DefaultChooseAllyBehavior();
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

        private void HandleListOnActivate(object? sender, UiListEventArgs<ChooseAllyListItem> e)
        {
            var data = e.SelectedCell.Data.Data;

            if (data is ChooseAllyData.Ally allyData)
            {
                if (_behavior.IsMultiSelect)
                {
                    _audio.Play(Protos.Sound.Fail1);
                    _mes.Display("TODO", color: UiColors.MesRed);
                }
                else
                {
                    if (!_behavior.CanSelect(allyData.Entity))
                        return;

                    Finish(new(allyData.Entity));
                }
            }
            else if (data is ChooseAllyData.Proceed proceed && _behavior.IsMultiSelect)
            {
                _audio.Play(Protos.Sound.Fail1);
                _mes.Display("TODO", color: UiColors.MesRed);
            }
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

        private IEnumerable<ChooseAllyListItem> GenerateList(IEnumerable<EntityUid> entities, Args args)
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

            ChooseAllyListItem Transform(EntityUid entity)
            {
                var name = args.Behavior.FormatName(entity);
                var info = args.Behavior.FormatInfo(entity);
                var color = args.Behavior.GetTextColor(entity);
                var ordering = args.Behavior.GetOrdering(entity);

                return new ChooseAllyListItem(new ChooseAllyData.Ally(entity))
                {
                    Text = name,
                    Info = info,
                    TextColor = color,
                    Ordering = ordering
                };
            }

            var result = entities.Where(CanShow).Select(Transform).OrderBy(c => c.Ordering);

            return result;
        }

        public override void Initialize(Args args)
        {
            EntitySystem.InjectDependencies(args.Behavior);

            _win.KeyHints = MakeKeyHints();

            _win.Title = args.Behavior.WindowTitle;
            _textTopicName.Text = args.Behavior.TopicName;
            _textTopicInfo.Text = args.Behavior.TopicInfo;

            var entities = args.Candidates ?? _parties.EnumerateUnderlings(_gameSession.Player);
            var data = GenerateList(entities, args);
            _prompt = args.Prompt;
            _behavior = args.Behavior;

            if (_behavior.IsMultiSelect)
            {
                var selected = 0;
                // by default, select the first N allies until max is reached
                foreach (var (entry, i) in data.WithIndex())
                {
                    if (selected >= _behavior.MultiSelectCount)
                        break;
                    if (entry.Data is ChooseAllyData.Ally allyData)
                    {
                        if (_entityManager.IsAlive(allyData.Entity))
                        {
                            entry.Selected = true;
                            selected++;
                        }
                    }
                }
            }

            _list.SetCells(data.Select(c => new ChooseAllyListCell(c, _batch)));
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _win.SetSize(width, height);
            _list.SetSize(width, height);
            _textTopicName.SetPreferredSize();
            _textTopicInfo.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _win.SetPosition(x, y);
            _list.SetPosition(X + 58, Y + 66);
            _textTopicName.SetPosition(X + 28, Y + 36);
            _textTopicInfo.SetPosition(X + 350, Y + 36);
        }

        public override void OnQuery()
        {
            if (_prompt != null)
                _mes.Display(_prompt);
            _audio.Play(Protos.Sound.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(620, 400, out bounds);
        }

        public override void Update(float dt)
        {
            _win.Update(dt);

            _textTopicName.Update(dt);
            _textTopicInfo.Update(dt);

            _list.Update(dt);
            _batch.Update(dt);
        }

        public override void Draw()
        {
            _win.Draw();

            _textTopicName.Draw();
            _textTopicInfo.Draw();

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