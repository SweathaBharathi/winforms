// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms;

public partial class DataGridViewButtonCell
{
    private static class DataGridViewButtonCellRenderer
    {
        private static VisualStyleRenderer? s_visualStyleRenderer;

        public static VisualStyleRenderer DataGridViewButtonRenderer
        {
            get
            {
                s_visualStyleRenderer ??= new VisualStyleRenderer(s_buttonElement);

                return s_visualStyleRenderer;
            }
        }

        public static void DrawButton(Graphics g, Rectangle bounds, int buttonState) =>
            DrawButton(g, bounds, buttonState, backColor: SystemColors.Control);

        public static void DrawButton(Graphics g, Rectangle bounds, int buttonState, Color backColor)
        {
            // The "Button" visual style class is always rendered using the light system theme,
            // regardless of the app's dark mode setting. Painting it unconditionally causes
            // button cells to keep their light appearance while the rest of the DataGridView
            // (and the cell's own BackColor/ForeColor) has already switched to dark mode colors.
            // In that case, fall back to a simple themed fill so the cell matches its surroundings.
            if (Application.IsDarkModeEnabled && AppContextSwitches.DataGridViewDarkModeTheming)
            {
                using var backBrush = backColor.GetCachedSolidBrushScope();
                g.FillRectangle(backBrush, bounds);

                PushButtonState pbState = (PushButtonState)(buttonState & ~(int)PushButtonState.Default);
                Color borderColor = pbState switch
                {
                    PushButtonState.Pressed => ControlPaint.Light(backColor, 0.3f),
                    PushButtonState.Hot => ControlPaint.Light(backColor, 0.2f),
                    _ => ControlPaint.Light(backColor, 0.1f)
                };

                using var borderPen = borderColor.GetCachedPenScope();
                g.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

                return;
            }

            DataGridViewButtonRenderer.SetParameters(s_buttonElement.ClassName, s_buttonElement.Part, buttonState);
            DataGridViewButtonRenderer.DrawBackground(g, bounds, Rectangle.Truncate(g.ClipBounds));
        }
    }
}
