using OpenNefia.Content.PCCs;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Portraits;

namespace OpenNefia.Content.CharaAppearance
{
    public static class CharaAppearanceHelpers
    {
        public static CharaAppearanceData MakeDefaultAppearanceData(IPrototypeManager protos, IResourceCache resourceCache)
        {
            ChipPrototype chipProto = protos.Index(Chip.Default);
            PortraitPrototype portraitProto = protos.EnumeratePrototypes<PortraitPrototype>().Where(p => p.GetStrongID() != Portrait.Default).First();
            PCCDrawable pccDrawable = PCCHelpers.CreateDefaultPCCFromLayout(PCCConstants.DefaultPCCPartLayout, protos, resourceCache);

            var appearanceData = new CharaAppearanceData(chipProto, Color.White, portraitProto, pccDrawable, true, false);
            return appearanceData;
        }

        public static CharaAppearanceData MakeAppearanceDataFrom(EntityUid entity,
            IPrototypeManager protos,
            IEntityManager entityManager,
            IResourceCache resourceCache)
        {
            ChipPrototype chipProto = protos.Index(Chip.Default);
            Color chipColor = Color.White;

            if (entityManager.TryGetComponent(entity, out ChipComponent chipComp)
                && protos.TryIndex(chipComp.ChipID, out var chipProtoFound))
            {
                chipProto = chipProtoFound;
                chipColor = chipComp.Color;
            }

            PortraitPrototype portraitProto = protos.Index(Portrait.Default);

            if (entityManager.TryGetComponent(entity, out PortraitComponent portraitComp)
                && protos.TryIndex(portraitComp.PortraitID, out var portraitProtoFound))
            {
                portraitProto = portraitProtoFound;
            }

            var usePCC = false;
            var isFullSize = false;
            PCCDrawable? pccDrawable;

            if (entityManager.TryGetComponent(entity, out PCCComponent pcc))
            {
                // Make a new drawable so the one in-game won't be modified
                pccDrawable = PCCHelpers.CreatePCCDrawable(pcc);

                usePCC = pcc.UsePCC;
                isFullSize = pcc.IsFullSize;
            }
            else
            {
                pccDrawable = PCCHelpers.CreateDefaultPCCFromLayout(PCCConstants.DefaultPCCPartLayout, protos, resourceCache);
            }

            var appearanceData = new CharaAppearanceData(chipProto, chipColor, portraitProto, pccDrawable, usePCC, isFullSize);
            return appearanceData;
        }

        public static void ApplyAppearanceDataTo(EntityUid entity, CharaAppearanceData appearance, 
            IEntityManager _entityManager,
            IPCCSystem pccs)
        {
            var portrait = _entityManager.EnsureComponent<PortraitComponent>(entity);
            portrait.PortraitID = appearance.PortraitProto.GetStrongID();

            var pccComp = _entityManager.EnsureComponent<PCCComponent>(entity);
            pccComp.UsePCC = appearance.UsePCC;
            pccComp.IsFullSize = appearance.IsFullSize;

            var newParts = appearance.PCCDrawable.Parts.ShallowClone();
            pccs.SetPCCParts(entity, newParts, pccComp);

            var ev = new CharaAppearanceChangedEvent(appearance);
            _entityManager.EventBus.RaiseEvent(entity, ev);
        }
    }

    public sealed class CharaAppearanceChangedEvent : EntityEventArgs
    {
        public CharaAppearanceChangedEvent(CharaAppearanceData appearance)
        {
            Appearance = appearance;
        }

        public CharaAppearanceData Appearance { get; }
    }
}
