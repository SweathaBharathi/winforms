// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

/// <summary>
///  Provides extension methods for locking and unlocking drawing on a <see cref="Control"/>.
/// </summary>
/// <remarks>
///  <para>
///   These methods suppress and resume the redrawing of a control by sending
///   <see cref="PInvoke.WM_SETREDRAW"/> to its window. This is useful when performing bulk updates to a
///   control, such as adding many items to a list, to avoid unnecessary flicker and improve performance.
///  </para>
/// </remarks>
public static class ControlDrawingExtensions
{
    /// <summary>
    ///  Suspends the redrawing of the <paramref name="target"/> control until
    ///  <see cref="UnlockDrawing(Control)"/> is called.
    /// </summary>
    /// <param name="target">The control whose drawing should be locked.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public static void LockDrawing(this Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.BeginUpdateInternal();
    }

    /// <summary>
    ///  Resumes the redrawing of the <paramref name="target"/> control that was previously suspended by a
    ///  call to <see cref="LockDrawing(Control)"/>.
    /// </summary>
    /// <param name="target">The control whose drawing should be unlocked.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public static void UnlockDrawing(this Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.EndUpdateInternal();
    }

    /// <summary>
    ///  Locks drawing for the <paramref name="target"/> control and returns a <see cref="DrawingLock"/> that
    ///  unlocks it when disposed.
    /// </summary>
    /// <param name="target">The control whose drawing should be locked.</param>
    /// <returns>A <see cref="DrawingLock"/> that resumes drawing when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    public static DrawingLock UseDrawingLock(this Control target) => new(target);
}
