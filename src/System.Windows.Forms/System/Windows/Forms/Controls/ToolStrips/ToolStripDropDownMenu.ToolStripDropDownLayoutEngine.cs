// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public partial class ToolStripDropDownMenu
{
    internal sealed class ToolStripDropDownLayoutEngine : FlowLayout
    {
        public static ToolStripDropDownLayoutEngine LayoutInstance = new();

        internal override Size GetPreferredSize(IArrangedElement container, Size proposedConstraints)
        {
            Size preferredSize = base.GetPreferredSize(container, proposedConstraints);
            if (container is ToolStripDropDownMenu dropDownMenu)
            {
                // When one or more items request a column break (ToolStripMenuItem.Break), the base FlowLayout
                // preferred width already accounts for the additional columns (each column is
                // dropDownMenu.MaxItemSize.Width wide). Trim it by the same padding amount that the
                // single-column case applies instead of collapsing back down to a single column's width.
                preferredSize.Width = Math.Max(
                    preferredSize.Width - dropDownMenu.PaddingToTrim,
                    dropDownMenu.MaxItemSize.Width - dropDownMenu.PaddingToTrim);
            }

            return preferredSize;
        }
    }
}
