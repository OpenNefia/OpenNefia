using OpenNefia.Core.ViewVariables.Instances;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp.Controls;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;
using OpenNefia.Core.Utility;
using OpenNefia.Core.UI.Wisp;

namespace OpenNefia.Core.ViewVariables.Traits
{
    internal sealed class ViewVariablesTraitMembers : ViewVariablesTrait
    {
        private BoxContainer _memberList = default!;

        public override void Initialize(ViewVariablesInstanceObject instance)
        {
            base.Initialize(instance);
            _memberList = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 0
            };
            instance.AddTab("Members", _memberList);
        }

        public override async void Refresh()
        {
            _memberList.DisposeAllChildren();

            var first = true;
            foreach (var group in ViewVariablesInstance.LocalPropertyList(Instance.Object,
                Instance.ViewVariablesManager))
            {
                CreateMemberGroupHeader(
                    ref first,
                    TypeAbbreviation.Abbreviate(group.Key),
                    _memberList);

                foreach (var control in group)
                {
                    _memberList.AddChild(control);
                }
            }
        }

        internal static void CreateMemberGroupHeader(ref bool first, string groupName, WispControl container)
        {
            if (!first)
            {
                container.AddChild(new WispControl { MinSize = (0, 16) });
            }

            first = false;
            container.AddChild(new Label { Text = groupName, FontColorOverride = Color.DarkGray });
        }
    }
}
