using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.PCCs;
using OpenNefia.Core.Log;
using OpenNefia.Content.CharaAppearance;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.AppearanceSelect")]
    public class CharaMakeAppearanceLayer : CharaMakeLayer
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IPCCSystem _pccs = default!;

        public const string ResultName = "appearance";

        private CharaAppearanceControl AppearanceControl = new();

        public CharaMakeAppearanceLayer()
        {
            AddChild(AppearanceControl);

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
            if (!Data.TryGetValue<Gender>(CharaMakeGenderSelectLayer.ResultName, out var gender))
            {
                gender = Gender.Female;
                Logger.WarningS("charamake", $"No '{CharaMakeGenderSelectLayer.ResultName}' result in CharaMakeData");
            }

            if (!Data.TryGetValue<RacePrototype>(CharaMakeRaceSelectLayer.ResultName, out var race))
            {
                Logger.WarningS("charamake", $"No '{CharaMakeRaceSelectLayer.ResultName}' result in CharaMakeData");
                return Protos.Chip.Default;
            }

            switch (gender)
            {
                case Gender.Male:
                    return race.ChipMale;
                case Gender.Female:
                default:
                    return race.ChipFemale;
            }
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

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            AppearanceControl.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size, out bounds, yOffset: -15);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            AppearanceControl.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
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

        public override void ApplyStep(EntityUid entity)
        {
            base.ApplyStep(entity);
            if (!Data.TryGetValue<CharaAppearanceData>(ResultName, out var appearance))
            {
                Logger.WarningS("charamake", $"No '{ResultName}' result in CharaMakeData");
                return;
            }

            var portrait = EntityManager.EnsureComponent<PortraitComponent>(entity);
            portrait.PortraitID = appearance.PortraitProto.GetStrongID();

            if (appearance.UsePCC)
            {
                var pccComp = EntityManager.EnsureComponent<PCCComponent>(entity);
                _pccs.SetPCCParts(entity, appearance.PCCDrawable.Parts, pccComp);
            }
            else
            {
                // TODO: TryRemoveComponent?
                if (EntityManager.HasComponent<PCCComponent>(entity))
                    EntityManager.RemoveComponent<PCCComponent>(entity);
            }
        }
    }
}
