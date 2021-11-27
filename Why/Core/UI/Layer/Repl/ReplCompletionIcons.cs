using Microsoft.CodeAnalysis.Tags;
using OpenNefia.Core.Data;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer.Repl
{
    [DefOfEntries]
    internal static class ReplIconsAssetDefOf
    {
        public static AssetDef ReplCompletionIcon_Array = null!;
        public static AssetDef ReplCompletionIcon_Boolean = null!;
        public static AssetDef ReplCompletionIcon_Class = null!;
        public static AssetDef ReplCompletionIcon_Color = null!;
        public static AssetDef ReplCompletionIcon_Constant = null!;
        public static AssetDef ReplCompletionIcon_Document = null!;
        public static AssetDef ReplCompletionIcon_EnumeratorMember = null!;
        public static AssetDef ReplCompletionIcon_Enumerator = null!;
        public static AssetDef ReplCompletionIcon_Event = null!;
        public static AssetDef ReplCompletionIcon_Field = null!;
        public static AssetDef ReplCompletionIcon_Folder = null!;
        public static AssetDef ReplCompletionIcon_Interface = null!;
        public static AssetDef ReplCompletionIcon_Key = null!;
        public static AssetDef ReplCompletionIcon_Keyword = null!;
        public static AssetDef ReplCompletionIcon_Library = null!;
        public static AssetDef ReplCompletionIcon_LocalVariable = null!;
        public static AssetDef ReplCompletionIcon_Method = null!;
        public static AssetDef ReplCompletionIcon_Misc = null!;
        public static AssetDef ReplCompletionIcon_Namespace = null!;
        public static AssetDef ReplCompletionIcon_Numeric = null!;
        public static AssetDef ReplCompletionIcon_Operator = null!;
        public static AssetDef ReplCompletionIcon_Parameter = null!;
        public static AssetDef ReplCompletionIcon_Property = null!;
        public static AssetDef ReplCompletionIcon_Ruler = null!;
        public static AssetDef ReplCompletionIcon_Snippet = null!;
        public static AssetDef ReplCompletionIcon_String = null!;
        public static AssetDef ReplCompletionIcon_Structure = null!;
        public static AssetDef ReplCompletionIcon_Unlink = null!;
        public static AssetDef ReplCompletionIcon_Variable = null!;
    }

    internal class ReplCompletionIcons : IDisposable
    {
        public static Dictionary<string, AssetDef> RoslynTagToIconAssetDef = new Dictionary<string, AssetDef>()
        {
            { WellKnownTags.Public,            ReplIconsAssetDefOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Protected,         ReplIconsAssetDefOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Private,           ReplIconsAssetDefOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Internal,          ReplIconsAssetDefOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.File,              ReplIconsAssetDefOf.ReplCompletionIcon_Document },
            { WellKnownTags.Project,           ReplIconsAssetDefOf.ReplCompletionIcon_Document },
            { WellKnownTags.Folder,            ReplIconsAssetDefOf.ReplCompletionIcon_Folder },
            { WellKnownTags.Assembly,          ReplIconsAssetDefOf.ReplCompletionIcon_Library },
            { WellKnownTags.Class,             ReplIconsAssetDefOf.ReplCompletionIcon_Class },
            { WellKnownTags.Constant,          ReplIconsAssetDefOf.ReplCompletionIcon_Constant },
            { WellKnownTags.Delegate,          ReplIconsAssetDefOf.ReplCompletionIcon_Method },
            { WellKnownTags.Enum,              ReplIconsAssetDefOf.ReplCompletionIcon_Enumerator },
            { WellKnownTags.EnumMember,        ReplIconsAssetDefOf.ReplCompletionIcon_EnumeratorMember },
            { WellKnownTags.Event,             ReplIconsAssetDefOf.ReplCompletionIcon_Event },
            { WellKnownTags.ExtensionMethod,   ReplIconsAssetDefOf.ReplCompletionIcon_Method },
            { WellKnownTags.Field,             ReplIconsAssetDefOf.ReplCompletionIcon_Field },
            { WellKnownTags.Interface,         ReplIconsAssetDefOf.ReplCompletionIcon_Interface },
            { WellKnownTags.Intrinsic,         ReplIconsAssetDefOf.ReplCompletionIcon_Misc },
            { WellKnownTags.Keyword,           ReplIconsAssetDefOf.ReplCompletionIcon_Keyword },
            { WellKnownTags.Label,             ReplIconsAssetDefOf.ReplCompletionIcon_Misc },
            { WellKnownTags.Local,             ReplIconsAssetDefOf.ReplCompletionIcon_LocalVariable },
            { WellKnownTags.Namespace,         ReplIconsAssetDefOf.ReplCompletionIcon_Namespace },
            { WellKnownTags.Method,            ReplIconsAssetDefOf.ReplCompletionIcon_Method },
            { WellKnownTags.Module,            ReplIconsAssetDefOf.ReplCompletionIcon_Library },
            { WellKnownTags.Operator,          ReplIconsAssetDefOf.ReplCompletionIcon_Operator },
            { WellKnownTags.Parameter,         ReplIconsAssetDefOf.ReplCompletionIcon_Parameter },
            { WellKnownTags.Property,          ReplIconsAssetDefOf.ReplCompletionIcon_Property },
            { WellKnownTags.RangeVariable,     ReplIconsAssetDefOf.ReplCompletionIcon_Variable },
            { WellKnownTags.Reference,         ReplIconsAssetDefOf.ReplCompletionIcon_Unlink },
            { WellKnownTags.Structure,         ReplIconsAssetDefOf.ReplCompletionIcon_Structure },
            { WellKnownTags.TypeParameter,     ReplIconsAssetDefOf.ReplCompletionIcon_Parameter },
            { WellKnownTags.Snippet,           ReplIconsAssetDefOf.ReplCompletionIcon_Snippet },
            { WellKnownTags.Error,             ReplIconsAssetDefOf.ReplCompletionIcon_Misc },
            { WellKnownTags.Warning,           ReplIconsAssetDefOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.StatusInformation, ReplIconsAssetDefOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.AddReference,      ReplIconsAssetDefOf.ReplCompletionIcon_Misc },
            // { WellKnownTags.NuGet,             ReplIconsAssetDefOf.ReplCompletionIcon_Misc }
        };

        private Dictionary<AssetDef, AssetDrawable> Drawables = new Dictionary<AssetDef, AssetDrawable>();

        private AssetDrawable GetDrawable(AssetDef def)
        {
            if (this.Drawables.TryGetValue(def, out var drawable))
                return drawable;

            drawable = new AssetDrawable(def);
            this.Drawables[def] = drawable;
            return drawable;
        }

        public AssetDrawable GetIcon(ImmutableArray<string> roslynTags)
        {
            foreach (var tag in roslynTags)
            {
                if (RoslynTagToIconAssetDef.TryGetValue(tag, out var assetDef))
                {
                    return GetDrawable(assetDef);
                }
            }

            return GetDrawable(ReplIconsAssetDefOf.ReplCompletionIcon_Misc);
        }

        public void Dispose()
        {
            foreach (var drawable in Drawables.Values)
                drawable.Dispose();
        }
    }
}
