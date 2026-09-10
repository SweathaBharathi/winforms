// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace System.Windows.Forms.Tests;

public class DataGridViewTextBoxCellTests : IDisposable
{
    private readonly DataGridViewTextBoxCell _dataGridViewTextBoxCell;

    public DataGridViewTextBoxCellTests() => _dataGridViewTextBoxCell = new();

    public void Dispose() => _dataGridViewTextBoxCell.Dispose();

    [WinFormsFact]
    public void Paint_InvokesOverriddenCellPaint()
    {
        using CustomDataGridViewTextBoxCell cell = new();
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = cell;
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        DataGridViewCellStyle cellStyle = new() { Font = SystemFonts.DefaultFont };

        cell.TestAccessor.Dynamic.Paint(
            g,
            new Rectangle(0, 0, 10, 10),
            new Rectangle(0, 0, 10, 10),
            0,
            DataGridViewElementStates.Selected,
            "value",
            "formatted",
            "error",
            cellStyle,
            new DataGridViewAdvancedBorderStyle(),
            DataGridViewPaintParts.All);

        cell.CellPaintCalled.Should().BeTrue();
    }

    private class CustomDataGridViewTextBoxCell : DataGridViewTextBoxCell
    {
        public bool CellPaintCalled { get; private set; }

        protected override void CellPaint(
            Graphics graphics,
            Rectangle valBounds,
            string formattedString,
            DataGridViewCellStyle cellStyle,
            bool cellSelected,
            TextFormatFlags flags)
        {
            CellPaintCalled = true;
            base.CellPaint(graphics, valBounds, formattedString, cellStyle, cellSelected, flags);
        }
    }
}
