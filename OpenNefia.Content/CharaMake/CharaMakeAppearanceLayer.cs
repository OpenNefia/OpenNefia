using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Maths;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.PCCs;
using OpenNefia.Core.Log;
using OpenNefia.Content.CharaAppearance;
using OpenNefia.Content.UI;
using OpenNefia.Core.UI;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.AppearanceSelect")]
    public class CharaMakeAppearanceLayer : CharaMakeLayer<CharaMakeAppearanceLayer.ResultData>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IPCCSystem _pccs = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;

        public const string ResultName = "appearance";

        [Child] private CharaAppearanceControl AppearanceControl = new();

        public CharaMakeAppearanceLayer()
        {
            AppearanceControl.List_OnActivated += OnListActivated;
        }

        public sealed class ResultData : CharaMakeResult
        {
            [Dependency] private readonly IPCCSystem _pccs = default!;

            public CharaAppearanceData AppearanceData { get; set; }

            public ResultData(CharaAppearanceData appearanceData)
            {
                AppearanceData = appearanceData;
            }

            public override void ApplyStep(EntityUid entity, EntityGenArgSet args)
            {
                CharaAppearanceHelpers.ApplyAppearanceDataTo(entity, AppearanceData, EntityManager, _pccs);
            }
        }

        private void OnListActivated(object? sender, UiListEventArgs<CharaAppearanceUICellData> args)
        {
            switch (args.SelectedCell.Data)
            {
                case CharaAppearanceUICellData.Done:
                    Finish(new CharaMakeUIResult(new ResultData(AppearanceControl.AppearanceData)));
                    break;
            }
        }

        private PrototypeId<ChipPrototype> GetDefaultCharaChip()
        {
            Gender gender;

            if (Results.TryGet<CharaMakeGenderSelectLayer.ResultData>(out var genderResult))
                gender = genderResult.Gender;
            else
                gender = Gender.Female;

            if (!Results.TryGet<CharaMakeRaceSelectLayer.ResultData>(out var raceResult))
                return Protos.Chip.Default;

            return _charas.GetDefaultCharaChip(raceResult.RaceID, gender);
        }

        public override void Initialize(CharaMakeResultSet args)
        {
            base.Initialize(args);

            var appearanceData = CharaAppearanceHelpers.MakeDefaultAppearanceData(_protos, _resourceCache);

            appearanceData.ChipProto = _protos.Index(GetDefaultCharaChip());

            AppearanceControl.Initialize(appearanceData);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Port);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            AppearanceControl.GrabFocus();
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            AppearanceControl.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size, out bounds, yOffset: -15);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            AppearanceControl.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            AppearanceControl.SetPosition(X, Y);
        }

        public override void Draw()
        {
            base.Draw();
            AppearanceControl.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            AppearanceControl.Update(dt);
        }

        public override void Dispose()
        {
            base.Dispose();
            AppearanceControl.Dispose();
        }
    }
}
