using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.FeatSelect")]
    public class CharaMakeFeatWindowLayer : CharaMakeLayer
    {
        public const string ResultName = "feats";
        private FeatWindow Window = default!;
        private Dictionary<FeatPrototype, int> SelectedFeats = default!;
        private int FeatCount;
        public CharaMakeFeatWindowLayer()
        {
            
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            Reset();
        }

        private void Reset()
        {
            Window?.Dispose();
            SelectedFeats = new Dictionary<FeatPrototype, int>();
            FeatCount = 3;
            if (Data.TryGetValue(CharaMakeRaceSelectLayer.ResultName, out RacePrototype? race))
            {
                foreach (var feat in race.BaseFeats)
                    SelectedFeats[feat.ResolvePrototype()] = 1;
            }
            Window = new FeatWindow(() => SelectedFeats, feat =>
            {
                FeatCount--;
                SelectedFeats.TryGetValue(feat.Prototype, out var level);
                SelectedFeats[feat.Prototype] = level + 1;

                if (FeatCount == 0)
                {
                    Finish(new CharaMakeResult(new Dictionary<string, object>
                    {
                        { ResultName, SelectedFeats }
                    }));
                }
            }, () => FeatCount);
            AddChild(Window.List);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Feat);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            Window.List.GrabFocus();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window, 10);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
        }
    }
}

