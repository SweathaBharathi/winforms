// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

/// <summary>
///  Specifies how the currently selected paragraph in a <see cref="RichTextBox"/> is aligned.
/// </summary>
/// <remarks>
///  <para>
///   This enum exists separately from <see cref="HorizontalAlignment"/> because
///   <see cref="HorizontalAlignment"/> is shared by several other controls that have no concept of
///   justified text. Adding <see cref="Justify"/> to <see cref="HorizontalAlignment"/> would make it a
///   silently-accepted, but meaningless, value for those other controls.
///  </para>
/// </remarks>
public enum RichTextBoxSelectionAlignment
{
    /// <summary>
    ///  The paragraph is aligned on the left of the control element.
    /// </summary>
    Left = 0,

    /// <summary>
    ///  The paragraph is aligned on the right of the control element.
    /// </summary>
    Right = 1,

    /// <summary>
    ///  The paragraph is aligned in the center of the control element.
    /// </summary>
    Center = 2,

    /// <summary>
    ///  The paragraph is justified, so that it is aligned evenly along both the left and right
    ///  edges of the control element.
    /// </summary>
    Justify = 3,
}
