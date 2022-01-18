using OpenNefia.Content.Feats;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Stats;
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
        [Dependency] private readonly IFeatsSystem _feats = default!;

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

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            FeatWindow.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: 10);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            FeatWindow.GrabFocus();
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            FeatWindow.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            FeatWindow.SetPosition(X, Y);
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

        public override void ApplyStep(EntityUid entity)
        {
            base.ApplyStep(entity);
            if (!Data.TryGetValue<Dictionary<PrototypeId<FeatPrototype>, int>>(ResultName, out var feats))
            {
                Logger.WarningS("charamake", "No attributes in CharaMakeData");
                return;
            }

            if (!EntityManager.TryGetComponent<FeatsComponent>(entity, out var featsComponent))
            {
                Logger.WarningS("charamake", "No FeatsComponent present on entity");
                return;
            }

            foreach (var feat in feats)
            {
                featsComponent.Feats[feat.Key] = new(_feats.Level(featsComponent, feat.Key) + feat.Value);
            }
        }
    }
}

