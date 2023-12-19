using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Love;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Utility;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Content.UI.Layer
{
    // TODO: Needs to support tilemap-level scaling (#134)
    public sealed class TargetPromptList : UiPagedList<TargetPromptList.Item>
    {
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly ICoords _coords = default!;

        public MapCoordinates TileOrigin { get; set; }

        public sealed class Item
        {
            public Item(EntityUid ent, MapCoordinates tilePosition)
            {
                Entity = ent;
                TilePosition = tilePosition;
            }

            public EntityUid Entity { get; }
            public MapCoordinates TilePosition { get; }
        }

        public TargetPromptList() : base()
        {
            EntitySystem.InjectDependencies(this);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            var iy = Y;

            for (int index = 0; index < DisplayedCells.Count; index++)
            {
                var cell = DisplayedCells[index];

                // One of the few cases where tile scaling instead of UI scaling should be used for laying out the list cells.
                // TileToVisibleScreen uses TileScale internally.
                // But UiListCell.SetPosition uses UIScale.
                // So, remove UIScale part first.
                var pos = _field.Camera.TileToVisibleScreen(cell.Data.TilePosition) / UIScale;
                cell.SetPosition(pos.X, pos.Y);
            }
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = IoCManager.Resolve<IGraphics>().WindowSize;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            for (int index = 0; index < DisplayedCells.Count; index++)
            {
                var cell = DisplayedCells[index];

                cell.SetSize(_coords.TileSize.X * _coords.TileScale, _coords.TileSize.Y * _coords.TileScale);
            }
        }

        private void DrawTileLineTo(Vector2i targetTilePos)
        {
            var targetScreenPos = _field.Camera.TileToVisibleScreen(targetTilePos);
            Love.Graphics.SetBlendMode(BlendMode.Add);
            Love.Graphics.SetColor(UiColors.PromptTargetedTile);
            Love.Graphics.Rectangle(DrawMode.Fill, targetScreenPos.X, targetScreenPos.Y, _coords.TileSize.X * _coords.TileScale, _coords.TileSize.Y * _coords.TileScale);

            Love.Graphics.SetColor(UiColors.PromptHighlightedTile);
            foreach (var pos in PosHelpers.EnumerateLine(TileOrigin.Position, targetTilePos))
            {
                var screenPos = _field.Camera.TileToVisibleScreen(pos);
                Love.Graphics.Rectangle(DrawMode.Fill, screenPos.X, screenPos.Y, _coords.TileSize.X * _coords.TileScale, _coords.TileSize.Y * _coords.TileScale);
            }

            Love.Graphics.SetBlendMode(BlendMode.Alpha);
        }

        public override void Draw()
        {
            if (SelectedCell != null)
            {
                DrawTileLineTo(SelectedCell.Data.TilePosition.Position);
            }

            for (var i = 0; i < DisplayedCells.Count; i++)
            {
                if (CanSelect(i))
                {
                    DisplayedCells[i].Draw();
                }
            }
        }
    }

    public sealed class TargetPrompt : UiLayerWithResult<TargetPrompt.Args, TargetPrompt.Result>
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly TargetTextSystem _targetText = default!;
        [Dependency] private readonly TargetingSystem _targeting = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public class Args
        {
            public EntityUid Onlooker { get; set; }
            public IReadOnlyList<EntityUid>? Targets { get; set; }
            public EntityUid? CurrentTarget { get; set; }

            public Args(EntityUid onlooker, IReadOnlyList<EntityUid>? targets = null, EntityUid? currentTarget = null)
            {
                Onlooker = onlooker;
                Targets = targets;
                CurrentTarget = currentTarget;
            }
        }

        public new class Result
        {
            public EntityUid? Target { get; }

            public Result(EntityUid? target)
            {
                Target = target;
            }
        }

        [Child] private TargetPromptList List = new();
        [Child] private UiText TextTarget = new UiTextOutlined(UiFonts.TargetText);

        private EntityUid _onlooker;

        public TargetPrompt()
        {
            OnKeyBindDown += HandleKeyBindDown;
            EventFilter = UIEventFilterMode.Pass;

            List.OnActivated += List_OnActivated;
            List.OnSelected += List_OnSelected;
        }

        private void List_OnSelected(object? sender, UiListEventArgs<TargetPromptList.Item> e)
        {
            _field.Camera.CenterOnTilePos(e.SelectedCell.Data.TilePosition);

            // This will update the positions of the target cells based on the new camera position.
            List.SetPosition(0, 0);

            UpdateTargetText();
        }

        private void List_OnActivated(object? sender, UiListEventArgs<TargetPromptList.Item> e)
        {
            Finish(new Result(e.SelectedCell.Data.Entity));
        }

        public override void Initialize(Args args)
        {
            _onlooker = args.Onlooker;
            List.TileOrigin = _entityManager.GetComponent<SpatialComponent>(_onlooker)
                .MapPosition;

            var targets = args.Targets?.Select(u => _entityManager.GetComponent<SpatialComponent>(u))
                          ?? _targeting.FindTargets(_onlooker);

            List.SetCells(targets.Select(spatial =>
            {
                var item = new TargetPromptList.Item(spatial.Owner, spatial.MapPosition);
                return new UiListCell<TargetPromptList.Item>(item)
                {
                    Text = string.Empty
                };
            }));

            if (args.CurrentTarget != null)
            {
                var index = targets.FindIndex(t => t.Owner == args.CurrentTarget.Value);
                if (index != -1)
                    List.Select(index);
            }
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }

        public override void OnQuery()
        {
            if (List.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Targeting.Action.FindNothing", ("onlooker", _onlooker)), combineDuplicates: true);
                Finish(new Result(null));
            }
        }

        public override void OnQueryFinish()
        {
            _field.Camera.CenterOnTilePos(_gameSession.Player);
        }

        public void UpdateTargetText()
        {
            if (List.SelectedCell == null)
            {
                TextTarget.Text = String.Empty;
                return;
            }

            var canSee = _targetText.GetTargetText(_onlooker, List.SelectedCell.Data.TilePosition, out var text, visibleOnly: true);
            TextTarget.Text = text;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            List.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            List.SetPosition(0, 0);
            TextTarget.SetPosition(100, Height - Constants.INF_MSGH - 45 - TextTarget.Height);
        }

        public override void Update(float dt)
        {
            List.Update(dt);
            TextTarget.Update(dt);
        }

        public override void Draw()
        {
            List.Draw();
            TextTarget.Draw();
        }
    }
}
