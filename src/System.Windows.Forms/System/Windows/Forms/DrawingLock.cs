// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

/// <summary>
///  Represents a scope that locks drawing on a <see cref="Control"/> until it is disposed.
/// </summary>
/// <remarks>
///  <para>
///   Obtain a <see cref="DrawingLock"/> via <see cref="ControlDrawingExtensions.UseDrawingLock(Control)"/>
///   and dispose it (typically with a <see langword="using"/> statement) to unlock drawing on the target
///   control.
///  </para>
/// </remarks>
public ref struct DrawingLock : IDisposable
{
    private Control? _target;

    /// <summary>
    ///  Initializes a new instance of the <see cref="DrawingLock"/> struct, locking drawing on
    ///  <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The control whose drawing should be locked.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public DrawingLock(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.LockDrawing();
        _target = target;
    }

    /// <summary>
    ///  Unlocks drawing on the target control. Disposing more than once has no additional effect.
    /// </summary>
    public void Dispose()
    {
        _target?.UnlockDrawing();
        _target = null;
    }
}
