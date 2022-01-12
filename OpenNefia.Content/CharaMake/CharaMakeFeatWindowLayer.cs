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
        private FeatWindow FeatWindow = default!;
        private readonly Dictionary<PrototypeId<FeatPrototype>, int> SelectedFeats = new();
        private int FeatCount;

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            Reset();
        }

        private void AddFeat(FeatWindow.FeatNameAndDesc.Feat feat)
        {
            FeatCount--;
            var protoId = feat.Prototype.GetStrongID();
            SelectedFeats.TryGetValue(protoId, out var level);
            SelectedFeats[protoId] = level + 1;

            if (FeatCount <= 0)
            {
                Finish(new CharaMakeResult(new Dictionary<string, object>
                {
                    { ResultName, SelectedFeats }
                }));
            }
        }

        private void Reset()
        {
            FeatWindow?.Dispose();
            SelectedFeats.Clear();
            FeatCount = 3;
            if (Data.TryGetValue(CharaMakeRaceSelectLayer.ResultName, out RacePrototype? race))
            {
                foreach (var feat in race.InitialFeats)
                    SelectedFeats[feat.Key] = feat.Value;
            }
            FeatWindow = new FeatWindow(() => SelectedFeats, AddFeat, () => FeatCount);
            AddChild(FeatWindow);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Feat);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            FeatWindow.GrabControlFocus();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            FeatWindow.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(FeatWindow, 10);
        }

        public override void Draw()
        {
            base.Draw();
            FeatWindow.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            FeatWindow.Update(dt);
        }
    }
}

