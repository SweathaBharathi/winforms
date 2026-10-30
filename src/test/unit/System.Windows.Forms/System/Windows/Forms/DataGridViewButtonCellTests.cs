// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
using Microsoft.DotNet.RemoteExecutor;

namespace System.Windows.Forms.Tests;

public class DataGridViewButtonCellTests : IDisposable
{
    private const string DarkModeThemingSwitch = "System.Windows.Forms.DataGridViewDarkModeTheming";
    private readonly DataGridViewButtonCell _dataGridViewButtonCell;

    public void Dispose() => _dataGridViewButtonCell?.Dispose();

    public DataGridViewButtonCellTests() => _dataGridViewButtonCell = new();

    [WinFormsTheory]
    [InlineData(PushButtonState.Normal, 0.1f)]
    [InlineData(PushButtonState.Hot, 0.2f)]
    [InlineData(PushButtonState.Pressed, 0.3f)]
    [InlineData(PushButtonState.Disabled, 0.1f)]
    public void ButtonRenderer_DarkMode_PaintsBackgroundAndStateBorder(PushButtonState state, float lightening)
    {
        using RemoteInvokeHandle handle = RemoteExecutor.Invoke((stateValue, lighteningValue) =>
        {
            EnableDarkMode();
            using Bitmap bitmap = new(60, 50);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Magenta);
            Rectangle bounds = new(5, 7, 40, 30);
            Color backColor = Color.FromArgb(32, 40, 48);
            Color borderColor = ControlPaint.Light(backColor, float.Parse(lighteningValue, CultureInfo.InvariantCulture));

            DrawButton(graphics, bounds, (PushButtonState)int.Parse(stateValue), backColor);

            Assert.Equal(backColor.ToArgb(), bitmap.GetPixel(20, 20).ToArgb());
            Assert.Equal(borderColor.ToArgb(), bitmap.GetPixel(bounds.Left, bounds.Top).ToArgb());
            Assert.Equal(borderColor.ToArgb(), bitmap.GetPixel(bounds.Right - 1, bounds.Bottom - 1).ToArgb());
            Assert.Equal(Color.Magenta.ToArgb(), bitmap.GetPixel(bounds.Right, bounds.Bottom).ToArgb());
        }, ((int)state).ToString(), lightening.ToString(CultureInfo.InvariantCulture));
    }

    [WinFormsTheory]
    [InlineData(FlatStyle.Standard, false, true)]
    [InlineData(FlatStyle.Standard, true, true)]
    [InlineData(FlatStyle.System, false, true)]
    [InlineData(FlatStyle.System, true, true)]
    [InlineData(FlatStyle.Standard, true, false)]
    [InlineData(FlatStyle.System, true, false)]
    public void Paint_DarkMode_UsesCellColors(FlatStyle flatStyle, bool selected, bool paintSelectionBackground)
    {
        using RemoteInvokeHandle handle = RemoteExecutor.Invoke((flatStyleValue, selectedValue, paintSelectionValue) =>
        {
            EnableDarkMode();
            Assert.True(DataGridView.ApplyVisualStylesToInnerCells);
            using DataGridView grid = new() { AllowUserToAddRows = false };
            grid.Columns.Add(new DataGridViewButtonColumn { FlatStyle = (FlatStyle)int.Parse(flatStyleValue) });
            grid.Rows.Add();
            grid.CurrentCell = null;
            using Font font = new(FontFamily.GenericSansSerif, 16, FontStyle.Bold);
            DataGridViewCellStyle cellStyle = new()
            {
                Font = font,
                BackColor = Color.FromArgb(32, 40, 48),
                ForeColor = Color.Lime,
                SelectionBackColor = Color.FromArgb(40, 60, 100),
                SelectionForeColor = Color.Cyan,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };
            using Bitmap bitmap = new(160, 50);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Magenta);
            bool isSelected = bool.Parse(selectedValue);

            PaintButtonCell(grid[0, 0], graphics, cellStyle, isSelected, DataGridViewPaintParts.ContentForeground, "TEST");

            Color expectedForeColor = isSelected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
            Color unexpectedForeColor = isSelected ? cellStyle.ForeColor : cellStyle.SelectionForeColor;
            bool textPainted = false;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int pixel = bitmap.GetPixel(x, y).ToArgb();
                    textPainted |= pixel == expectedForeColor.ToArgb();
                    Assert.NotEqual(unexpectedForeColor.ToArgb(), pixel);
                }
            }

            Assert.True(textPainted);
            graphics.Clear(Color.Magenta);
            bool paintSelection = bool.Parse(paintSelectionValue);
            DataGridViewPaintParts paintParts = DataGridViewPaintParts.Background | DataGridViewPaintParts.ContentBackground;
            if (paintSelection)
            {
                paintParts |= DataGridViewPaintParts.SelectionBackground;
            }

            PaintButtonCell(grid[0, 0], graphics, cellStyle, isSelected, paintParts, string.Empty);

            Color expectedBackColor = isSelected && paintSelection ? cellStyle.SelectionBackColor : cellStyle.BackColor;
            Assert.Equal(expectedBackColor.ToArgb(), bitmap.GetPixel(40, 25).ToArgb());
        }, ((int)flatStyle).ToString(), selected.ToString(), paintSelectionBackground.ToString());
    }

    [WinFormsTheory]
    [InlineData(FlatStyle.Standard)]
    [InlineData(FlatStyle.System)]
    public void Paint_DarkMode_FocusedPressedStateIsPreserved(FlatStyle flatStyle)
    {
        using RemoteInvokeHandle handle = RemoteExecutor.Invoke(flatStyleValue =>
        {
            EnableDarkMode();
            Assert.True(DataGridView.ApplyVisualStylesToInnerCells);
            using FocusedDataGridView grid = new() { AllowUserToAddRows = false };
            grid.Columns.Add(new DataGridViewButtonColumn { FlatStyle = (FlatStyle)int.Parse(flatStyleValue) });
            grid.Rows.Add();
            grid.CurrentCell = grid[0, 0];
            Assert.True(grid.Focused);
            Assert.True((bool)grid.TestAccessor.Dynamic.ShowFocusCues);
            grid[0, 0].TestAccessor.Dynamic.ButtonState = ButtonState.Pushed;
            DataGridViewCellStyle cellStyle = grid[0, 0].InheritedStyle;
            cellStyle.BackColor = Color.FromArgb(32, 40, 48);
            using Bitmap bitmap = new(160, 50);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Magenta);

            PaintButtonCell(
                grid[0, 0],
                graphics,
                cellStyle,
                selected: false,
                DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.Focus,
                string.Empty);

            Assert.Equal(ControlPaint.Light(cellStyle.BackColor, 0.3f).ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
        }, ((int)flatStyle).ToString());
    }

    [WinFormsTheory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void ButtonRenderer_DarkModeInactive_UsesVisualStyle(bool darkMode, bool themingEnabled)
    {
        using RemoteInvokeHandle handle = RemoteExecutor.Invoke((darkModeValue, themingEnabledValue) =>
        {
            AppContext.SetSwitch(DarkModeThemingSwitch, bool.Parse(themingEnabledValue));
            Application.SetColorMode(bool.Parse(darkModeValue) ? SystemColorMode.Dark : SystemColorMode.Classic);
            Assert.True(DataGridView.ApplyVisualStylesToInnerCells);
            Rectangle bounds = new(0, 0, 40, 30);
            using Bitmap actual = new(bounds.Width, bounds.Height);
            using Bitmap expected = new(bounds.Width, bounds.Height);
            using Graphics actualGraphics = Graphics.FromImage(actual);
            using Graphics expectedGraphics = Graphics.FromImage(expected);
            VisualStyleRenderer renderer = new(VisualStyleElement.Button.PushButton.Pressed);

            DrawButton(actualGraphics, bounds, PushButtonState.Pressed, Color.Magenta);
            renderer.DrawBackground(expectedGraphics, bounds, Rectangle.Truncate(expectedGraphics.ClipBounds));

            for (int x = 0; x < bounds.Width; x++)
            {
                for (int y = 0; y < bounds.Height; y++)
                {
                    Assert.Equal(expected.GetPixel(x, y), actual.GetPixel(x, y));
                }
            }
        }, darkMode.ToString(), themingEnabled.ToString());
    }

    private static void EnableDarkMode()
    {
        // Isolate process-wide colors and system-event broadcasts using RemoteExecutor.
        AppContext.SetSwitch(DarkModeThemingSwitch, true);
        Application.SetColorMode(SystemColorMode.Dark);
        Assert.True(Application.IsDarkModeEnabled);
    }

    private static void DrawButton(Graphics graphics, Rectangle bounds, PushButtonState state, Color backColor)
    {
        Type rendererType = typeof(DataGridViewButtonCell).GetNestedType("DataGridViewButtonCellRenderer", BindingFlags.NonPublic)!;
        MethodInfo drawButton = rendererType.GetMethod(
            "DrawButton",
            [typeof(Graphics), typeof(Rectangle), typeof(int), typeof(Color)])!;
        drawButton.Invoke(null, [graphics, bounds, (int)state, backColor]);
    }

    private static void PaintButtonCell(
        DataGridViewCell cell,
        Graphics graphics,
        DataGridViewCellStyle cellStyle,
        bool selected,
        DataGridViewPaintParts paintParts,
        string text)
    {
        Rectangle bounds = new(0, 0, 160, 50);
        cell.TestAccessor.Dynamic.Paint(
            graphics,
            bounds,
            bounds,
            cell.RowIndex,
            selected ? DataGridViewElementStates.Selected : DataGridViewElementStates.None,
            text,
            text,
            null,
            cellStyle,
            new DataGridViewAdvancedBorderStyle { All = DataGridViewAdvancedCellBorderStyle.None },
            paintParts);
    }

    /// <summary>
    ///  Simulates focus without showing a window or taking focus from another application.
    /// </summary>
    private sealed class FocusedDataGridView : DataGridView
    {
        public override bool Focused => true;
    }

    [Fact]
    public void FormattedValueType_ReturnsStringType()
    {
        Type result = _dataGridViewButtonCell.FormattedValueType;

        result.Should().Be(typeof(string));
    }

    [WinFormsFact]
    public void UseColumnTextForButtonValue_DefaultValue_IsFalse() =>
        _dataGridViewButtonCell.UseColumnTextForButtonValue.Should().BeFalse();

    [WinFormsFact]
    public void UseColumnTextForButtonValue_SetTrue_ReflectsValue()
    {
        _dataGridViewButtonCell.UseColumnTextForButtonValue = true;
        _dataGridViewButtonCell.UseColumnTextForButtonValue.Should().BeTrue();
    }

    [Fact]
    public void ValueType_DefaultValue_IsObject() =>
        _dataGridViewButtonCell.ValueType.Should().Be(typeof(object));

    [Fact]
    public void ValueType_WhenBaseValueTypeIsSet_ReturnsBaseValueType()
    {
        Type customType = typeof(int);
        _dataGridViewButtonCell.GetType().BaseType!
            .GetProperty("ValueType")!
            .SetValue(_dataGridViewButtonCell, customType);

        _dataGridViewButtonCell.ValueType.Should().Be(customType);
    }

    [WinFormsFact]
    public void GetContentBounds_ReturnsEmpty_WhenDataGridViewIsNull()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetContentBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetContentBounds_ReturnsEmpty_WhenRowIndexIsNegative()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetContentBounds(g, dataGridViewCellStyle, -1);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetContentBounds_ReturnsEmpty_WhenOwningColumnIsNull()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        dataGridView.Columns.Clear();
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetContentBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetErrorIconBounds_ReturnsEmpty_WhenDataGridViewIsNull()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetErrorIconBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetErrorIconBounds_ReturnsEmpty_WhenRowIndexIsNegative()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetErrorIconBounds(g, dataGridViewCellStyle, -1);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetErrorIconBounds_ReturnsEmpty_WhenOwningColumnIsNull()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        dataGridView.Columns.Clear();
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetErrorIconBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetErrorIconBounds_ReturnsEmpty_WhenShowCellErrorsIsFalse()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView.ShowCellErrors = false;
        dataGridView[0, 0] = _dataGridViewButtonCell;
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetErrorIconBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetErrorIconBounds_ReturnsEmpty_WhenErrorTextIsEmpty()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetErrorIconBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().Be(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetErrorIconBounds_ReturnsNonEmpty_WhenErrorTextIsSet()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new();
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        _dataGridViewButtonCell.ErrorText = "Error!";

        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetErrorIconBounds(g, dataGridViewCellStyle, 0);

        ((Rectangle)result).Should().NotBe(Rectangle.Empty);
    }

    [WinFormsFact]
    public void GetPreferredSize_ReturnsMinusOne_WhenDataGridViewIsNull()
    {
        DataGridViewCellStyle dataGridViewCellStyle = new()
        {
            Font = SystemFonts.DefaultFont
        };
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetPreferredSize(g, dataGridViewCellStyle, 0, new Size(100, 100));

        ((Size)result).Should().Be(new Size(-1, -1));
    }

    [WinFormsFact]
    public void GetPreferredSize_ThrowsNullReferenceException_WhenCellStyleIsNull()
    {
        using Graphics g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        Action action = () => _dataGridViewButtonCell.TestAccessor.Dynamic.GetPreferredSize(g, null, 0, new Size(100, 100));
        action.Should().Throw<NullReferenceException>();
    }

    [WinFormsFact]
    public void GetPreferredSize_UsesThemeMargins_WhenVisualStylesApplied()
    {
        using Graphics g = Graphics.FromImage(new Bitmap(100, 100));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        dataGridView.Rows[0].Cells[0].Value = "Test";
        dataGridView.Rows[0].Cells[0].Style.Font = SystemFonts.DefaultFont;
        dataGridView.Rows[0].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridView.Rows[0].Cells[0].Style.Padding = Padding.Empty;
        dataGridView.Rows[0].Cells[0].Style.WrapMode = DataGridViewTriState.False;

        if (!DataGridView.ApplyVisualStylesToInnerCells)
            return;

        var result = _dataGridViewButtonCell.TestAccessor.Dynamic.GetPreferredSize(
            g, dataGridView.Rows[0].Cells[0].Style, 0, Size.Empty);

        ((Size)result).Width.Should().BeGreaterThan(0);
        ((Size)result).Height.Should().BeGreaterThan(0);
    }

    [WinFormsFact]
    public void GetValue_ReturnsColumnText_WhenUseColumnTextForButtonValueIsTrue()
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new() { Text = "ColumnText" };
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        _dataGridViewButtonCell.UseColumnTextForButtonValue = true;
        dataGridView[0, 0] = _dataGridViewButtonCell;

        var value = _dataGridViewButtonCell.TestAccessor.Dynamic.GetValue(0);

        ((string)value).Should().Be("ColumnText");
    }

    [WinFormsFact]
    public void GetValue_ReturnsBaseValue_WhenUseColumnTextForButtonValueIsFalse()
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        _dataGridViewButtonCell.UseColumnTextForButtonValue = false;
        dataGridView[0, 0] = _dataGridViewButtonCell;
        _dataGridViewButtonCell.Value = "CellValue";

        var value = _dataGridViewButtonCell.TestAccessor.Dynamic.GetValue(0);

        ((string)value).Should().Be("CellValue");
    }

    [WinFormsFact]
    public void GetValue_ReturnsBaseValue_WhenOwningColumnIsNotButtonColumn()
    {
        using DataGridView dataGridView = new();
        using DataGridViewTextBoxColumn dataGridViewTextBoxColumn = new();
        dataGridView.Columns.Add(dataGridViewTextBoxColumn);
        dataGridView.Rows.Add();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new() { Text = "CellValue" };
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView[0, 0] = _dataGridViewButtonCell;
        _dataGridViewButtonCell.Value = "CellValue";

        var value = _dataGridViewButtonCell.TestAccessor.Dynamic.GetValue(0);

        ((string)value).Should().Be("CellValue");
    }

    [Theory]
    [InlineData(Keys.Space, false, false, false, true)]
    [InlineData(Keys.Space, true, false, false, false)]
    [InlineData(Keys.Space, false, true, false, false)]
    [InlineData(Keys.Space, false, false, true, false)]
    [InlineData(Keys.A, false, false, false, false)]
    public void KeyDownUnsharesRow_ReturnsExpected(Keys key, bool alt, bool control, bool shift, bool expected)
    {
        Keys keyData = key;
        if (alt)
            keyData |= Keys.Alt;
        if (control)
            keyData |= Keys.Control;
        if (shift)
            keyData |= Keys.Shift;
        KeyEventArgs e = new(keyData);

        bool result = _dataGridViewButtonCell.TestAccessor.Dynamic.KeyDownUnsharesRow(e, 0);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Keys.Space, true)]
    [InlineData(Keys.A, false)]
    public void KeyUpUnsharesRow_ReturnsExpected(Keys key, bool expected)
    {
        KeyEventArgs e = new(key);

        bool result = _dataGridViewButtonCell.TestAccessor.Dynamic.KeyUpUnsharesRow(e, 0);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MouseButtons.Left, true)]
    [InlineData(MouseButtons.Right, false)]
    [InlineData(MouseButtons.Middle, false)]
    [InlineData(MouseButtons.None, false)]
    public void MouseDownUnsharesRow_ReturnsExpected(MouseButtons button, bool expected)
    {
        DataGridViewCellMouseEventArgs e = new(0, 0, 0, 0, new MouseEventArgs(button, 1, 0, 0, 0));

        bool result = _dataGridViewButtonCell.TestAccessor.Dynamic.MouseDownUnsharesRow(e);
        result.Should().Be(expected);
    }

    [WinFormsTheory]
    [InlineData(ButtonState.Pushed, true)]
    [InlineData(ButtonState.Normal, false)]
    [InlineData(ButtonState.Checked, false)]
    [InlineData(ButtonState.Pushed | ButtonState.Checked, true)]
    public void MouseLeaveUnsharesRow_ReturnsExpected(ButtonState buttonState, bool expected)
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState = buttonState;

        bool result = _dataGridViewButtonCell.TestAccessor.Dynamic.MouseLeaveUnsharesRow(0);
        result.Should().Be(expected);
    }

    [WinFormsTheory]
    [InlineData(MouseButtons.Left, true)]
    [InlineData(MouseButtons.Right, false)]
    [InlineData(MouseButtons.Middle, false)]
    [InlineData(MouseButtons.None, false)]
    public void MouseUpUnsharesRow_ReturnsExpected(MouseButtons button, bool expected)
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        DataGridViewCellMouseEventArgs dataGridViewCellMouseEventArgs = new(0, 0, 0, 0, new MouseEventArgs(button, 1, 0, 0, 0));

        bool result = _dataGridViewButtonCell.TestAccessor.Dynamic.MouseUpUnsharesRow(dataGridViewCellMouseEventArgs);
        result.Should().Be(expected);
    }

    [WinFormsFact]
    public void OnKeyDown_SetsCheckedStateAndHandled_WhenSpacePressedWithoutModifiers()
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        KeyEventArgs keyEventArgs = new(Keys.Space);

        ButtonState buttonState = _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState;
        buttonState.Should().Be(ButtonState.Normal);

        _dataGridViewButtonCell.TestAccessor.Dynamic.OnKeyDown(keyEventArgs, 0);
        buttonState = _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState;

        buttonState.HasFlag(ButtonState.Checked).Should().BeTrue();
        keyEventArgs.Handled.Should().BeTrue();
    }

    [WinFormsTheory]
    [InlineData(Keys.Space | Keys.Alt)]
    [InlineData(Keys.Space | Keys.Control)]
    [InlineData(Keys.Space | Keys.Shift)]
    [InlineData(Keys.A)]
    public void OnKeyDown_DoesNotSetCheckedStateOrHandled_WhenModifiersOrOtherKey(Keys keyData)
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        KeyEventArgs keyEventArgs = new(keyData);

        ButtonState buttonState = _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState;
        buttonState.Should().Be(ButtonState.Normal);

        _dataGridViewButtonCell.TestAccessor.Dynamic.OnKeyDown(keyEventArgs, 0);
        buttonState = _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState;

        (buttonState & ButtonState.Checked).Should().Be(0);
        keyEventArgs.Handled.Should().BeFalse();
    }

    [WinFormsTheory]
    [InlineData(Keys.Space, true)]
    [InlineData(Keys.Space | Keys.Alt, false)]
    [InlineData(Keys.Space | Keys.Control, false)]
    [InlineData(Keys.Space | Keys.Shift, false)]
    public void OnKeyUp_RemovesCheckedState_AndSetsHandledAsExpected(Keys keyData, bool expectedHandled)
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState = ButtonState.Checked;

        KeyEventArgs keyEventArgs = new(keyData);
        _dataGridViewButtonCell.TestAccessor.Dynamic.OnKeyUp(keyEventArgs, 0);

        ButtonState buttonState = _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState;
        buttonState.HasFlag(ButtonState.Checked).Should().BeFalse();
        keyEventArgs.Handled.Should().Be(expectedHandled);
    }

    [WinFormsFact]
    public void OnLeave_ResetsButtonState_WhenNotNormal()
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState = ButtonState.Pushed | ButtonState.Checked;

        _dataGridViewButtonCell.TestAccessor.Dynamic.OnLeave(0, false);

        ButtonState buttonState = _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState;
        buttonState.Should().Be(ButtonState.Normal);
    }

    [WinFormsFact]
    public void OnMouseUp_UpdatesButtonState_WhenLeftButton()
    {
        using DataGridView dataGridView = new();
        using DataGridViewButtonColumn dataGridViewButtonColumn = new();
        dataGridView.Columns.Add(dataGridViewButtonColumn);
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        _dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState = ButtonState.Pushed;

        DataGridViewCellMouseEventArgs dataGridViewCellMouseEventArgs = new(0, 0, 0, 0, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        _dataGridViewButtonCell.TestAccessor.Dynamic.OnMouseUp(dataGridViewCellMouseEventArgs);

        ((ButtonState)_dataGridViewButtonCell.TestAccessor.Dynamic.ButtonState).HasFlag(ButtonState.Pushed).Should().BeFalse();
    }

    [WinFormsFact]
    public void Paint_CallsPaintPrivate_WithExpectedParameters()
    {
        using var g = Graphics.FromImage(new Bitmap(10, 10));
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;
        DataGridViewCellStyle dataGridViewCellStyle = new() { Font = SystemFonts.DefaultFont };
        DataGridViewAdvancedBorderStyle advancedBorderStyle = new();
        _dataGridViewButtonCell.TestAccessor.Dynamic.Paint(
            g,
            new Rectangle(0, 0, 10, 10),
            new Rectangle(0, 0, 10, 10),
            0,
            DataGridViewElementStates.Selected,
            "value",
            "formatted",
            "error",
            dataGridViewCellStyle,
            advancedBorderStyle,
            DataGridViewPaintParts.All);

        _dataGridViewButtonCell.DataGridView.Should().BeSameAs(dataGridView);
        dataGridViewCellStyle.Font.Should().Be(SystemFonts.DefaultFont);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat_WithDefaultIndices()
    {
        string result = _dataGridViewButtonCell.ToString();
        result.Should().Be("DataGridViewButtonCell { ColumnIndex=-1, RowIndex=-1 }");
    }

    [WinFormsFact]
    public void ToString_ReturnsExpectedFormat_WhenAddedToDataGridView()
    {
        using DataGridView dataGridView = new();
        dataGridView.Columns.Add(new DataGridViewButtonColumn());
        dataGridView.Rows.Add();
        dataGridView[0, 0] = _dataGridViewButtonCell;

        _dataGridViewButtonCell.ToString().Should().Be("DataGridViewButtonCell { ColumnIndex=0, RowIndex=0 }");
    }
}
