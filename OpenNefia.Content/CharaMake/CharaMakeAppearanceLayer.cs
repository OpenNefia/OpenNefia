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
    public class CharaMakeAppearanceLayer : CharaMakeLayer
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

        private void OnListActivated(object? sender, UiListEventArgs<CharaAppearanceUICellData> args)
        {
            switch (args.SelectedCell.Data)
            {
                case CharaAppearanceUICellData.Done:
                    Finish(new CharaMakeResult(new Dictionary<string, object>
                    {
                        { ResultName, AppearanceControl.AppearanceData }
                    }));
                    break;
            }
        }

        private PrototypeId<ChipPrototype> GetDefaultCharaChip()
        {
            if (!Data.TryGetCharaMakeResult<Gender>(CharaMakeGenderSelectLayer.ResultName, out var gender))
                gender = Gender.Female;

            if (!Data.TryGetCharaMakeResult<RacePrototype>(CharaMakeRaceSelectLayer.ResultName, out var race))
                return Protos.Chip.Default;

            return _charas.GetDefaultCharaChip(race, gender);
        }

        public override void Initialize(CharaMakeData args)
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

        public override void ApplyStep(EntityUid entity, EntityGenArgSet args)
        {
            base.ApplyStep(entity, args);
            if (!Data.TryGetCharaMakeResult<CharaAppearanceData>(ResultName, out var appearance))
                return;

            CharaAppearanceHelpers.ApplyAppearanceDataTo(entity, appearance, EntityManager, _pccs);
        }
    }
}
