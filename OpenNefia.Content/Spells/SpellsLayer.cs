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
using OpenNefia.Content.CharaInfo;
using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Content.Markup;
using OpenNefia.Content.Quests;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Maps;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.Spells;

namespace OpenNefia.Content.Spells
{
    [Localize("Elona.Spells.Layer")]
    public sealed class SpellsLayer : UiLayerWithResult<SpellsLayer.Args, SpellsLayer.Result>
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;

        public class Args
        {
            public Args(EntityUid caster, IEnumerable<SpellPrototype> spells)
            {
                Caster = caster;
                Spells = spells.ToList();
            }

            public EntityUid Caster { get; }
            public IList<SpellPrototype> Spells { get; }
        }

        public class Result
        {
            public SpellPrototype Spell { get; }

            public Result(SpellPrototype spell)
            {
                Spell = spell;
            }
        }

        public sealed record class ListItem(
            string Name,
            SpellPrototype Spell,
            SkillPrototype Skill, 
            EntityPrototype Effect, 
            EntityUid EffectEntity,

            int Cost,
            int Stock,
            int Level,
            float SuccessRate,
            string Description,
            string? ShortcutKey,
            PrototypeId<SkillPrototype>? AttributeID
            );

        public sealed class SpellsListCell : UiListCell<ListItem>
        {
            [Child] private AttributeIcon _icon;
            [Child] private UiText _textCostStock;
            [Child] private UiText _textLvRate;
            [Child] private UiText _textDescription;

            public SpellsListCell(ListItem data, UiListChoiceKey? key = null) : base(data, new UiText(UiFonts.ListText), key)
            {
                Text = Data.Name;
                if (Data.ShortcutKey != null)
                    Text += Data.ShortcutKey;

                _icon = new AttributeIcon(Data.AttributeID);
                _textCostStock = new UiText($"{Data.Cost} ({Data.Stock})");
                _textLvRate = new UiText($"{Data.Level}/{(int)(Data.SuccessRate * 100)}%");
                _textDescription = new UiText(Data.Description);
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                _icon.SetPreferredSize();
                _textCostStock.SetPreferredSize();
                _textLvRate.SetPreferredSize();
                _textDescription.SetPreferredSize();
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                _icon.SetPosition(X - 18, Y + 9);
                _textCostStock.SetPosition(X + 230 - _textCostStock.Width, Y);
                _textLvRate.SetPosition(X + 242, Y );
                _textDescription.SetPosition(X + 322, Y);
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                _icon.Update(dt);
                _textCostStock.Update(dt);
                _textLvRate.Update(dt);
                _textDescription.Update(dt);
            }

            public override void Draw()
            {
                base.Draw();
                _icon.Draw();
                _textCostStock.Draw();
                _textLvRate.Draw();
                _textDescription.Draw();
            }
        }

        private EntityUid _casterEntity;

        [Localize] private LocaleScope _loc = Loc.MakeScope("Elona.Spells.Layer");

        [Child][Localize("Window")] private UiWindow _win = new();
        [Child] private UiPagedList<ListItem> _list = new(16);
        [Child][Localize("Topic.Name")] private UiTextTopic _topicName = new();
        [Child] private UiTextTopic _topicCostStockLvChance;
        [Child][Localize("Topic.Effect")] private UiTextTopic _topicEffect = new();
        [Child] private AssetDrawable _headerIcon;
        private IAssetInstance _assetDecoSpellA;
        private IAssetInstance _assetDecoSpellB;

        public SpellsLayer()
        {
            CanControlFocus = false;
            EventFilter = UIEventFilterMode.Stop;

            OnKeyBindDown += HandleKeyBindDown;
            _list.OnActivated += HandleListActivated;

            var costStock = $"{_loc.GetString("Topic.Cost")}({_loc.GetString("Topic.Stock")})";
            var lvChance = $"{_loc.GetString("Topic.Lv")}/{_loc.GetString("Topic.Chance")}";
            _topicCostStockLvChance = new UiTextTopic(costStock + " " + lvChance);

            _headerIcon = InventoryHelpers.MakeIcon(InventoryIcon.Spell);
            _assetDecoSpellA = Assets.Get(Protos.Asset.DecoSpellA);
            _assetDecoSpellB = Assets.Get(Protos.Asset.DecoSpellB);
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

            var cells = MakeListCells(args.Spells);
            _list.SetCells(cells);
            _list.SelectAcrossAllPages(PreviousListIndex, playSound: false);
        }

        private IEnumerable<SpellsListCell> MakeListCells(IList<SpellPrototype> spells)
        {
            SpellsListCell ToListCell(SpellPrototype proto)
            {
                var skillProto = _protos.Index(proto.SkillID);
                var entityProto = _protos.Index(proto.EffectID);

                var effect = _entityManager.SpawnEntity(proto.EffectID, MapCoordinates.Global);
                _entityManager.GetComponent<MetaDataComponent>(effect).IsMapSavable = false;

                var name = Loc.GetPrototypeString(proto.SkillID, "Name");

                var cost = _spells.CalcBaseSpellMPCost(proto, _casterEntity, effect);
                var stock = _spells.SpellStock(_casterEntity, proto);
                var level = _skills.Level(_casterEntity, proto.SkillID);
                var successRate = _spells.CalcSpellSuccessRate(proto, _casterEntity, effect);
                var description = _spells.LocalizeSpellDescription(proto, _casterEntity, effect);
                string? shortcutKey = null;
                var attribute = skillProto.RelatedSkill;

                var data = new ListItem(name, proto, skillProto, entityProto, effect, 
                    cost, stock, level, successRate, description, shortcutKey, attribute);

                return new SpellsListCell(data);
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

        private void HandleListActivated(object? sender, UiListEventArgs<ListItem> e)
        {
            PreviousListIndex = _list.SelectedIndexAcrossAllPages;
            Finish(new(e.SelectedCell.Data.Spell));
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
            _audio.Play(Protos.Sound.Spell);
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
            UiUtils.GetCenteredParams(730, 438, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _win.SetSize(Width, Height);
            _topicName.SetPreferredSize();
            _topicCostStockLvChance.SetPreferredSize();
            _topicEffect.SetPreferredSize();
            _list.SetSize(Width, Height);
            _headerIcon.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _win.SetPosition(X, Y);
            _topicName.SetPosition(X + 28, Y + 36);
            _topicCostStockLvChance.SetPosition(X + 220, Y + 36);
            _topicEffect.SetPosition(X + 400, Y + 36);
            _list.SetPosition(X + 58, Y + 66);
            _headerIcon.SetPosition(X + 46, Y - 16);
        }

        public override void Update(float dt)
        {
            _win.Update(dt);
            _topicName.Update(dt);
            _topicCostStockLvChance.Update(dt);
            _topicEffect.Update(dt);
            _list.Update(dt);
            _headerIcon.Update(dt);
        }

        public override void Draw()
        {
            _win.Draw();
            _headerIcon.Draw();
            Love.Graphics.SetColor(Color.White);
            _assetDecoSpellA.Draw(UIScale, X + Width - 78, Y);
            _assetDecoSpellB.Draw(UIScale, X + Width - 180, Y);
            _topicName.Draw();
            _topicCostStockLvChance.Draw();
            _topicEffect.Draw();
            _list.Draw();
        }

        public override void Dispose()
        {
        }
    }
}