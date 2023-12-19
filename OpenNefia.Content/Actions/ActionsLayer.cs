using OpenNefia.Content.Equipment;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using Love;
using OpenNefia.Core.UserInterface;
using System.Collections.Generic;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Maps;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.Actions;

namespace OpenNefia.Content.Spells
{
    [Localize("Elona.Actions.Layer")]
    public sealed class ActionsLayer : UiLayerWithResult<ActionsLayer.Args, ActionsLayer.Result>
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;
        [Dependency] private readonly IActionSystem _actions = default!;

        public class Args
        {
            public Args(EntityUid caster, IEnumerable<ActionPrototype> actions)
            {
                Caster = caster;
                Actions = actions.ToList();
            }

            public EntityUid Caster { get; }
            public IList<ActionPrototype> Actions { get; }
        }

        public class Result
        {
            public ActionPrototype Action { get; }

            public Result(ActionPrototype action)
            {
                Action = action;
            }
        }

        public sealed record class ListItem(
            string Name,
            ActionPrototype Action,
            SkillPrototype Skill, 
            EntityPrototype Effect, 
            EntityUid EffectEntity,

            int Cost,
            string Description,
            string? ShortcutKey,
            PrototypeId<SkillPrototype>? AttributeID
            );

        public sealed class ActionsListCell : UiListCell<ListItem>
        {
            [Child] private AttributeIcon _icon;
            [Child] private UiText _textCost;
            [Child] private UiText _textDescription;

            public ActionsListCell(ListItem data, UiListChoiceKey? key = null) : base(data, new UiText(UiFonts.ListText), key)
            {
                Text = Data.Name;
                if (Data.ShortcutKey != null)
                    Text += Data.ShortcutKey;

                _icon = new AttributeIcon(Data.AttributeID);
                _textCost = new UiText($"{Data.Cost} Sp");
                _textDescription = new UiText(Data.Description);
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                _icon.SetPreferredSize();
                _textCost.SetPreferredSize();
                _textDescription.SetPreferredSize();
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                _icon.SetPosition(X - 18, Y + 9);
                _textCost.SetPosition(X + 200 - _textCost.Width, Y);
                _textDescription.SetPosition(X + 237, Y);
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                _icon.Update(dt);
                _textCost.Update(dt);
                _textDescription.Update(dt);
            }

            public override void Draw()
            {
                base.Draw();
                _icon.Draw();
                _textCost.Draw();
                _textDescription.Draw();
            }
        }

        private EntityUid _casterEntity;

        [Localize] private LocaleScope _loc = Loc.MakeScope("Elona.Actions.Layer");

        [Child][Localize("Window")] private UiWindow _win = new(yOffset: 60);
        [Child] private UiPagedList<ListItem> _list = new(16);
        [Child][Localize("Topic.Name")] private UiTextTopic _topicName = new();
        [Child][Localize("Topic.Cost")] private UiTextTopic _topicCost = new();
        [Child][Localize("Topic.Detail")] private UiTextTopic _topicDetail = new();
        [Child] private AssetDrawable _headerIcon;
        private IAssetInstance _assetDecoSkillA;
        private IAssetInstance _assetDecoSkillB;

        public ActionsLayer()
        {
            CanControlFocus = false;
            EventFilter = UIEventFilterMode.Stop;

            OnKeyBindDown += HandleKeyBindDown;
            _list.OnSelected += HandleListSelected;
            _list.OnActivated += HandleListActivated;

            _headerIcon = InventoryHelpers.MakeIcon(InventoryIcon.Skill);
            _assetDecoSkillA = Assets.Get(Protos.Asset.DecoSkillA);
            _assetDecoSkillB = Assets.Get(Protos.Asset.DecoSkillB);
        }

        private static int PreviousListIndex = 0;

        public override void Initialize(Args args)
        {
            _casterEntity = args.Caster;

            foreach (var cell in _list)
            {
                if (_entityManager.IsAlive(cell.Data.EffectEntity))
                    _entityManager.DeleteEntity(cell.Data.EffectEntity);
            }

            var cells = MakeListCells(args.Actions);
            _list.SetCells(cells);
            _list.SelectAcrossAllPages(PreviousListIndex, playSound: false);
        }

        private IEnumerable<ActionsListCell> MakeListCells(IList<ActionPrototype> spells)
        {
            ActionsListCell ToListCell(ActionPrototype proto)
            {
                var skillProto = _protos.Index(proto.SkillID);
                var entityProto = _protos.Index(proto.EffectID);

                var effect = _entityManager.SpawnEntity(proto.EffectID, MapCoordinates.Global);
                _entityManager.GetComponent<MetaDataComponent>(effect).IsMapSavable = false;

                var name = Loc.GetPrototypeString(proto.SkillID, "Name");

                var cost = proto.StaminaCost;
                var description = _actions.LocalizeActionDescription(proto, _casterEntity, effect);
                string? shortcutKey = null;
                var attribute = skillProto.RelatedSkill;

                var data = new ListItem(name, proto, skillProto, entityProto, effect, 
                    cost, description, shortcutKey, attribute);

                return new ActionsListCell(data);
            }

            return spells.Select(ToListCell);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _list.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                PreviousListIndex = _list.SelectedIndexAcrossAllPages;
                Cancel();
                args.Handle();
            }
        }

        private void HandleListSelected(object? sender, UiListEventArgs<ListItem> e)
        {
        }

        private void HandleListActivated(object? sender, UiListEventArgs<ListItem> e)
        {
            PreviousListIndex = _list.SelectedIndexAcrossAllPages;
            Finish(new(e.SelectedCell.Data.Action));
            e.Handle();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints =  base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));
            keyHints.AddRange(_list.MakeKeyHints());
            // TODO shortcuts

            return keyHints;
        }

        private void AssignShortcut(int index)
        {
            // TODO shortcuts
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Skill);
        }

        public override void OnQueryFinish()
        {
            base.OnQueryFinish();

            foreach (var cell in _list)
            {
                if (_entityManager.IsAlive(cell.Data.EffectEntity))
                    _entityManager.DeleteEntity(cell.Data.EffectEntity);
            }
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(600, 438, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _win.SetSize(Width, Height);
            _topicName.SetPreferredSize();
            _topicCost.SetPreferredSize();
            _topicDetail.SetPreferredSize();
            _list.SetSize(Width, Height);
            _headerIcon.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _win.SetPosition(X, Y);
            _topicName.SetPosition(X + 28, Y + 36);
            _topicCost.SetPosition(X + 220, Y + 36);
            _topicDetail.SetPosition(X + 320, Y + 36);
            _list.SetPosition(X + 58, Y + 66);
            _headerIcon.SetPosition(X + 46, Y - 16);
        }

        public override void Update(float dt)
        {
            _win.Update(dt);
            _topicName.Update(dt);
            _topicCost.Update(dt);
            _topicDetail.Update(dt);
            _list.Update(dt);
            _headerIcon.Update(dt);
        }

        public override void Draw()
        {
            _win.Draw();
            _headerIcon.Draw();
            Love.Graphics.SetColor(Color.White);
            _assetDecoSkillA.Draw(UIScale, X + Width - 78, Y + Height - 165);
            _assetDecoSkillB.Draw(UIScale, X + Width - 168, Y);
            _topicName.Draw();
            _topicCost.Draw();
            _topicDetail.Draw();
            _list.Draw();
        }

        public override void Dispose()
        {
        }
    }
}