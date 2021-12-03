using Microsoft.CodeAnalysis.Tags;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System.Collections.Immutable;
using AssetPrototypeID = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Rendering.AssetPrototype>;

namespace OpenNefia.Content.UI.Layer.Repl
{
    [PrototypeOfEntries]
    internal static class ReplIconsAssetPrototypeOf
    {
        public static AssetPrototypeID ReplCompletionIcon_Array = new(nameof(ReplCompletionIcon_Array));
        public static AssetPrototypeID ReplCompletionIcon_Boolean = new(nameof(ReplCompletionIcon_Boolean));
        public static AssetPrototypeID ReplCompletionIcon_Class = new(nameof(ReplCompletionIcon_Class));
        public static AssetPrototypeID ReplCompletionIcon_Color = new(nameof(ReplCompletionIcon_Color));
        public static AssetPrototypeID ReplCompletionIcon_Constant = new(nameof(ReplCompletionIcon_Constant));
        public static AssetPrototypeID ReplCompletionIcon_Document = new(nameof(ReplCompletionIcon_Document));
        public static AssetPrototypeID ReplCompletionIcon_EnumeratorMember = new(nameof(ReplCompletionIcon_EnumeratorMember));
        public static AssetPrototypeID ReplCompletionIcon_Enumerator = new(nameof(ReplCompletionIcon_Enumerator));
        public static AssetPrototypeID ReplCompletionIcon_Event = new(nameof(ReplCompletionIcon_Event));
        public static AssetPrototypeID ReplCompletionIcon_Field = new(nameof(ReplCompletionIcon_Field));
        public static AssetPrototypeID ReplCompletionIcon_Folder = new(nameof(ReplCompletionIcon_Folder));
        public static AssetPrototypeID ReplCompletionIcon_Interface = new(nameof(ReplCompletionIcon_Interface));
        public static AssetPrototypeID ReplCompletionIcon_Key = new(nameof(ReplCompletionIcon_Key));
        public static AssetPrototypeID ReplCompletionIcon_Keyword = new(nameof(ReplCompletionIcon_Keyword));
        public static AssetPrototypeID ReplCompletionIcon_Library = new(nameof(ReplCompletionIcon_Library));
        public static AssetPrototypeID ReplCompletionIcon_LocalVariable = new(nameof(ReplCompletionIcon_LocalVariable));
        public static AssetPrototypeID ReplCompletionIcon_Method = new(nameof(ReplCompletionIcon_Method));
        public static AssetPrototypeID ReplCompletionIcon_Misc = new(nameof(ReplCompletionIcon_Misc));
        public static AssetPrototypeID ReplCompletionIcon_Namespace = new(nameof(ReplCompletionIcon_Namespace));
        public static AssetPrototypeID ReplCompletionIcon_Numeric = new(nameof(ReplCompletionIcon_Numeric));
        public static AssetPrototypeID ReplCompletionIcon_Operator = new(nameof(ReplCompletionIcon_Operator));
        public static AssetPrototypeID ReplCompletionIcon_Parameter = new(nameof(ReplCompletionIcon_Parameter));
        public static AssetPrototypeID ReplCompletionIcon_Property = new(nameof(ReplCompletionIcon_Property));
        public static AssetPrototypeID ReplCompletionIcon_Ruler = new(nameof(ReplCompletionIcon_Ruler));
        public static AssetPrototypeID ReplCompletionIcon_Snippet = new(nameof(ReplCompletionIcon_Snippet));
        public static AssetPrototypeID ReplCompletionIcon_String = new(nameof(ReplCompletionIcon_String));
        public static AssetPrototypeID ReplCompletionIcon_Structure = new(nameof(ReplCompletionIcon_Structure));
        public static AssetPrototypeID ReplCompletionIcon_Unlink = new(nameof(ReplCompletionIcon_Unlink));
        public static AssetPrototypeID ReplCompletionIcon_Variable = new(nameof(ReplCompletionIcon_Variable));
    }

    internal class ReplCompletionIcons : IDisposable
    {
        private static IAssetManager _assets => IoCManager.Resolve<IAssetManager>();

        public static Dictionary<string, AssetPrototypeID> RoslynTagToIconAssetPrototypeID = new Dictionary<string, AssetPrototypeID>()
        {
            { WellKnownTags.Public,            ReplIconsAssetPrototypeOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Protected,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Private,           ReplIconsAssetPrototypeOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Internal,          ReplIconsAssetPrototypeOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.File,              ReplIconsAssetPrototypeOf.ReplCompletionIcon_Document },
            { WellKnownTags.Project,           ReplIconsAssetPrototypeOf.ReplCompletionIcon_Document },
            { WellKnownTags.Folder,            ReplIconsAssetPrototypeOf.ReplCompletionIcon_Folder },
            { WellKnownTags.Assembly,          ReplIconsAssetPrototypeOf.ReplCompletionIcon_Library },
            { WellKnownTags.Class,             ReplIconsAssetPrototypeOf.ReplCompletionIcon_Class },
            { WellKnownTags.Constant,          ReplIconsAssetPrototypeOf.ReplCompletionIcon_Constant },
            { WellKnownTags.Delegate,          ReplIconsAssetPrototypeOf.ReplCompletionIcon_Method },
            { WellKnownTags.Enum,              ReplIconsAssetPrototypeOf.ReplCompletionIcon_Enumerator },
            { WellKnownTags.EnumMember,        ReplIconsAssetPrototypeOf.ReplCompletionIcon_EnumeratorMember },
            { WellKnownTags.Event,             ReplIconsAssetPrototypeOf.ReplCompletionIcon_Event },
            { WellKnownTags.ExtensionMethod,   ReplIconsAssetPrototypeOf.ReplCompletionIcon_Method },
            { WellKnownTags.Field,             ReplIconsAssetPrototypeOf.ReplCompletionIcon_Field },
            { WellKnownTags.Interface,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Interface },
            { WellKnownTags.Intrinsic,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Misc },
            { WellKnownTags.Keyword,           ReplIconsAssetPrototypeOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Label,             ReplIconsAssetPrototypeOf.ReplCompletionIcon_Misc },
            { WellKnownTags.Local,             ReplIconsAssetPrototypeOf.ReplCompletionIcon_LocalVariable },
            { WellKnownTags.Namespace,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Namespace },
            { WellKnownTags.Method,            ReplIconsAssetPrototypeOf.ReplCompletionIcon_Method },
            { WellKnownTags.Module,            ReplIconsAssetPrototypeOf.ReplCompletionIcon_Library },
            { WellKnownTags.Operator,          ReplIconsAssetPrototypeOf.ReplCompletionIcon_Operator },
            { WellKnownTags.Parameter,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Parameter },
            { WellKnownTags.Property,          ReplIconsAssetPrototypeOf.ReplCompletionIcon_Property },
            { WellKnownTags.RangeVariable,     ReplIconsAssetPrototypeOf.ReplCompletionIcon_Variable },
            { WellKnownTags.Reference,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Unlink },
            { WellKnownTags.Structure,         ReplIconsAssetPrototypeOf.ReplCompletionIcon_Structure },
            { WellKnownTags.TypeParameter,     ReplIconsAssetPrototypeOf.ReplCompletionIcon_Parameter },
            { WellKnownTags.Snippet,           ReplIconsAssetPrototypeOf.ReplCompletionIcon_Snippet },
            { WellKnownTags.Error,             ReplIconsAssetPrototypeOf.ReplCompletionIcon_Misc },
            { WellKnownTags.Warning,           ReplIconsAssetPrototypeOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.StatusInformation, ReplIconsAssetPrototypeIDOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.AddReference,      ReplIconsAssetPrototypeIDOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.NuGet,             ReplIconsAssetPrototypeIDOf.ReplCompletionIcon_Misc }
        };

        private Dictionary<AssetPrototypeID, IAssetDrawable> Drawables = new();

        public IAssetDrawable GetIcon(ImmutableArray<string> roslynTags)
        {
            foreach (var tag in roslynTags)
            {
                if (RoslynTagToIconAssetPrototypeID.TryGetValue(tag, out var id))
                {
                    return _assets.GetAsset(id);
                }
            }

            return _assets.GetAsset(ReplIconsAssetPrototypeOf.ReplCompletionIcon_Misc);
        }

        public void Dispose()
        {
            foreach (var drawable in Drawables.Values)
                drawable.Dispose();
        }
    }
}
