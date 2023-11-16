using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using static OpenNefia.Content.Prototypes.Protos;
using Love;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.DebugView;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Random;
using Serilog.Debugging;
using System.Globalization;
using OpenNefia.Core.Utility;
using OpenNefia.Content.DisplayName;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.Home
{
    public sealed class HomeRankLayer : UiLayerWithResult<HomeRankLayer.Args, UINone>
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IAssetManager _assets = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;

        private class RankItemCell : UiElement
        {
            private EntityUid _item;
            [Child] private UiText _rankText;
            [Child] private UiText _itemNameText;
            private EntitySpriteBatch _batch;

            public RankItemCell(EntityUid item, string rankText, string itemName, EntitySpriteBatch batch)
            {
                _item = item;
                _rankText = new UiText(UiFonts.HouseBoardRankText, rankText);
                _itemNameText = new UiText(UiFonts.HouseBoardRankText, itemName);
                _batch = batch;
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                _rankText.SetPreferredSize();
                _itemNameText.SetPreferredSize();
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                _rankText.SetPosition(X + 68, Y + 138);
                _itemNameText.SetPosition(X + 110, Y + 138);
            }

            public override void Update(float dt)
            {
                _rankText.Update(dt);
                _itemNameText.Update(dt);
            }

            public override void Draw()
            {
                _rankText.Draw();
                _itemNameText.Draw();
                _batch.Add(_item, X + 37, Y + 138, centering: BatchCentering.Centered);
            }

            public override void Dispose()
            {
                base.Dispose();
                _rankText.Dispose();
                _itemNameText.Dispose();
            }
        }

        public override int? DefaultZOrder => HudLayer.HudZOrder + 10000;

        private IAssetInstance _assetBG = default!;
        [Child] private EntitySpriteBatch _batch = new();

        private HomeRank _rank = default!;

        [Child] private UiText _textStar = new UiTextShadowed(UiFonts.HouseBoardRankStar);
        [Child] private UiWindow _win = new();
        [Child] private UiTextTopic _textTopicValue;
        [Child] private UiTextTopic _textTopicHeirloomRank;
        private UiText[] _textHeaders;
        private RankItemCell[] _itemCells;

        private static string[] Headers = new[]
        {
            "Elona.Home.Rank.Type.Base",
            "Elona.Home.Rank.Type.Deco",
            "Elona.Home.Rank.Type.Heir",
            "Elona.Home.Rank.Type.Total",
        };

        public HomeRankLayer()
        {
            _textHeaders = Headers.Select(h => new UiText(UiFonts.HouseBoardRankText, Loc.GetString(h))).ToArray();
            foreach (var (text, i) in _textHeaders.WithIndex())
            {
                AddChild(text);
            }

            _itemCells = new RankItemCell[0];

            _textTopicValue = new UiTextTopic(Loc.GetString("Elona.Home.Rank.Window.Topic.Value"));
            _textTopicHeirloomRank = new UiTextTopic(Loc.GetString("Elona.Home.Rank.Window.Topic.HeirloomRank"));
            _win.Title = Loc.GetString("Elona.Home.Rank.Window.Title");
            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public class Args
        {
            public Args(IEnumerable<HomeRankItem> mostValuable, HomeRank rank)
            {
                MostValuable = mostValuable.ToList();
                Rank = rank;
            }

            public IList<HomeRankItem> MostValuable { get; } = new List<HomeRankItem>();
            public HomeRank Rank { get; }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        public override void Initialize(Args args)
        {
            _assetBG = _assets.GetAsset(_rand.Pick(UiUtils.CMBGs));
            _textStar.Text = Loc.GetString("Elona.Home.Rank.Window.Star");
            _rank = args.Rank;
            _win.KeyHints = MakeKeyHints();

            foreach (var cell in _itemCells)
            {
                RemoveChild(cell);
                cell.Dispose();
            }
            _itemCells = args.MostValuable.Take(10)
                .Select((i, place) => new RankItemCell(i.Item.Owner, 
                Loc.GetString("Elona.Home.Rank.Window.Place", ("ordinal", place)),
                _displayNames.GetDisplayName(i.Item.Owner),
                _batch))
                .ToArray();
            foreach (var cell in _itemCells)
            {
                AddChild(cell);
            }
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel || args.Function == EngineKeyFunctions.UISelect)
            {
                Cancel();
                args.Handle();
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _win.SetSize(width, height);
            _textTopicValue.SetPreferredSize();
            _textTopicHeirloomRank.SetPreferredSize();
            _textStar.SetPreferredSize();
            foreach (var text in _textHeaders)
            {
                text.SetPreferredSize();
            }
            foreach (var (cell, i) in _itemCells.WithIndex())
            {
                cell.SetPreferredSize();
            }
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _win.SetPosition(x, y);
            _textTopicValue.SetPosition(X + 28, Y + 36);
            _textTopicHeirloomRank.SetPosition(X + 28, Y + 106);

            for (int i = 0; i < Headers.Length; i++)
            {
                var text = _textHeaders[i];
                text.SetPosition(X + 45 + (i / 2) * 190, Y + 68 + (i % 2) * 18);
            }

            foreach (var (cell, i) in _itemCells.WithIndex())
            {
                cell.SetPosition(X, Y + i * 16);
            }
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(440, 360, out bounds);
        }

        public override void Update(float dt)
        {
            _win.Update(dt);
            _textTopicValue.Update(dt);
            _textTopicHeirloomRank.Update(dt);
            foreach (var text in _textHeaders)
            {
                text.Update(dt);
            }
            foreach (var cell in _itemCells)
            {
                cell.Update(dt);
            }
        }

        public override void Draw()
        {
            _win.Draw();
            _textTopicValue.Draw();
            _textTopicHeirloomRank.Draw();

            Graphics.SetColor(Color.White.WithAlphaB(50));
            _assetBG.Draw(UIScale, X + Width / 4, Y + Height / 2, Width / 5 * 2, Height - 80, centered: true);
            foreach (var (text, i) in _textHeaders.WithIndex())
            {
                text.Draw();

                int value;
                switch (i)
                {
                    case 0:
                        value = _rank.BaseValue;
                        break;
                    case 1:
                        value = _rank.HomeValue;
                        break;
                    case 2:
                        value = _rank.FurnitureValue;
                        break;
                    case 3:
                    default:
                        value = _rank.TotalValue;
                        break;
                }

                var starCount = Math.Clamp(value / 1000, 1, 10);
                for (int j = 0; j < starCount; j++)
                {
                    _textStar.SetPosition(text.X + 35 + _textStar.TextWidth * j, text.Y - 2);
                    _textStar.Draw();
                }
            }
            Graphics.SetColor(Color.White);
            _batch.Clear();
            foreach (var cell in _itemCells)
            {
                cell.Draw();
            }
            _batch.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _batch.Dispose();
        }
    }
}
