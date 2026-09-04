using System.Drawing;
using System.Windows.Forms;

namespace SneakerShop.WinForms.Styles;

public static class AppTheme
{
    public static readonly Color PrimaryRed =
        Color.FromArgb(198, 40, 40);

    public static readonly Color BrightRed =
        Color.FromArgb(229, 57, 53);

    public static readonly Color DarkRed =
        Color.FromArgb(105, 18, 18);

    public static readonly Color SidebarRed =
        Color.FromArgb(75, 14, 14);

    public static readonly Color HoverRed =
        Color.FromArgb(135, 25, 25);

    public static readonly Color LightRed =
        Color.FromArgb(255, 235, 238);

    public static readonly Color PageBackground =
        Color.FromArgb(250, 247, 247);

    public static readonly Color CardBackground =
        Color.White;

    public static readonly Color TextDark =
        Color.FromArgb(40, 40, 40);

    public static readonly Color TextMuted =
        Color.FromArgb(105, 105, 105);

    public static readonly Color BorderColor =
        Color.FromArgb(235, 210, 210);

    public static readonly Color Success =
        Color.FromArgb(46, 125, 50);

    public static readonly Color Warning =
        Color.FromArgb(245, 124, 0);

    public static readonly Color Danger =
        Color.FromArgb(183, 28, 28);

    public static void StylePrimaryButton(Button button)
    {
        button.BackColor = PrimaryRed;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
    }

    public static void StyleSecondaryButton(Button button)
    {
        button.BackColor = Color.White;
        button.ForeColor = PrimaryRed;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = PrimaryRed;
        button.FlatAppearance.BorderSize = 1;
        button.Cursor = Cursors.Hand;
    }

    public static void StyleDangerButton(Button button)
    {
        button.BackColor = LightRed;
        button.ForeColor = Danger;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Danger;
        button.FlatAppearance.BorderSize = 1;
        button.Cursor = Cursors.Hand;
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = Color.White;
        textBox.ForeColor = TextDark;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = new Font("Segoe UI", 11);
    }

    public static void StyleDataGridView(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;

        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersDefaultCellStyle.BackColor =
            DarkRed;

        grid.ColumnHeadersDefaultCellStyle.ForeColor =
            Color.White;

        grid.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 10, FontStyle.Bold);

        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
            DarkRed;

        grid.ColumnHeadersHeight = 42;

        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = TextDark;
        grid.DefaultCellStyle.SelectionBackColor = LightRed;
        grid.DefaultCellStyle.SelectionForeColor = TextDark;
        grid.DefaultCellStyle.Font =
            new Font("Segoe UI", 10);

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(255, 248, 248);

        grid.RowTemplate.Height = 38;
        grid.GridColor = BorderColor;
    }
}