using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public static class UiTheme
    {
        public static readonly Color Background = Color.FromArgb(242, 246, 252);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceSoft = Color.FromArgb(248, 250, 253);
        public static readonly Color PanelSoft = Color.FromArgb(244, 247, 252);
        public static readonly Color Primary = Color.FromArgb(36, 105, 190);
        public static readonly Color PrimaryDark = Color.FromArgb(31, 63, 109);
        public static readonly Color Danger = Color.FromArgb(205, 70, 70);
        public static readonly Color Text = Color.FromArgb(35, 45, 60);
        public static readonly Color MutedText = Color.FromArgb(95, 105, 120);
        public static readonly Color Border = Color.FromArgb(210, 218, 232);
        public static readonly Color Focus = Color.FromArgb(255, 252, 230);
        public static readonly Font BaseFont = new Font("Microsoft JhengHei UI", 9.5f);

        public static void Apply(Form form)
        {
            if (form == null)
                return;

            form.Font = BaseFont;
            form.BackColor = Background;
            form.KeyPreview = true;
            form.StartPosition = form.StartPosition == FormStartPosition.Manual ? FormStartPosition.CenterScreen : form.StartPosition;
            StyleChildren(form);
            EnableEnterNavigation(form);
            UiEffects.EnableFormFade(form);
        }

        public static void EnableEnterNavigation(Form form)
        {
            if (form == null)
                return;

            List<Control> inputs = new List<Control>();
            CollectInputControls(form, inputs);
            inputs.Sort(delegate(Control a, Control b)
            {
                Point pa = a.FindForm() == null ? a.Location : a.FindForm().PointToClient(a.Parent.PointToScreen(a.Location));
                Point pb = b.FindForm() == null ? b.Location : b.FindForm().PointToClient(b.Parent.PointToScreen(b.Location));
                if (pa.Y != pb.Y)
                    return pa.Y.CompareTo(pb.Y);
                return pa.X.CompareTo(pb.X);
            });

            for (int i = 0; i < inputs.Count; i++)
                inputs[i].TabIndex = i;

            foreach (Control input in inputs)
            {
                input.KeyDown -= Input_KeyDown_MoveNext;
                input.KeyDown += Input_KeyDown_MoveNext;
            }
        }

        private static void Input_KeyDown_MoveNext(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            TextBox tb = sender as TextBox;
            if (tb != null && tb.Multiline && e.Control)
                return;

            Control control = sender as Control;
            if (control == null)
                return;

            Form form = control.FindForm();
            if (form == null)
                return;

            e.SuppressKeyPress = true;
            form.SelectNextControl(control, !e.Shift, true, true, true);
        }

        private static void CollectInputControls(Control parent, List<Control> inputs)
        {
            foreach (Control c in parent.Controls)
            {
                if (IsInputControl(c) && c.TabStop && c.Enabled && c.Visible)
                    inputs.Add(c);
                if (c.HasChildren)
                    CollectInputControls(c, inputs);
            }
        }

        private static bool IsInputControl(Control c)
        {
            return c is TextBox || c is ComboBox || c is DateTimePicker || c is MaskedTextBox || c is NumericUpDown;
        }

        public static void StyleChildren(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                StyleControl(c);
                if (c.HasChildren)
                    StyleChildren(c);
            }
        }

        public static void StyleControl(Control c)
        {
            if (c == null)
                return;

            Button button = c as Button;
            if (button != null)
            {
                StyleButton(button, InferButtonStyle(button.Text));
                return;
            }

            DataGridView grid = c as DataGridView;
            if (grid != null)
            {
                StyleGrid(grid);
                return;
            }

            TextBox textBox = c as TextBox;
            if (textBox != null)
            {
                StyleInput(textBox);
                return;
            }

            ComboBox combo = c as ComboBox;
            if (combo != null)
            {
                StyleInput(combo);
                return;
            }

            DateTimePicker dateTimePicker = c as DateTimePicker;
            if (dateTimePicker != null)
            {
                StyleInput(dateTimePicker);
                return;
            }

            TabControl tab = c as TabControl;
            if (tab != null)
            {
                tab.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
                return;
            }

            Label label = c as Label;
            if (label != null)
            {
                if (label.ForeColor == SystemColors.ControlText || label.ForeColor == Color.Black)
                    label.ForeColor = Text;
                return;
            }
        }

        private static string InferButtonStyle(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "normal";
            if (text.Contains("刪除") || text.Contains("取消預約") || text.Contains("全部清除"))
                return "danger";
            if (text.Contains("新增") || text.Contains("確定") || text.Contains("登入") || text.Contains("搜尋") || text.Contains("儲存") || text.Contains("預約") || text.Contains("重新整理"))
                return "primary";
            return "normal";
        }

        public static void StyleButton(Button button, string style)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
            button.Height = Math.Max(button.Height, 34);

            Color normalBack;
            Color normalFore;
            Color border;
            Color hoverBack;

            if (style == "danger")
            {
                normalBack = Danger;
                normalFore = Color.White;
                border = Color.FromArgb(180, 55, 55);
                hoverBack = Color.FromArgb(185, 55, 55);
            }
            else if (style == "primary")
            {
                normalBack = Primary;
                normalFore = Color.White;
                border = Color.FromArgb(31, 92, 165);
                hoverBack = Color.FromArgb(25, 90, 168);
            }
            else
            {
                normalBack = Surface;
                normalFore = Text;
                border = Border;
                hoverBack = Color.FromArgb(235, 242, 255);
            }

            button.BackColor = normalBack;
            button.ForeColor = normalFore;
            button.FlatAppearance.BorderColor = border;
            button.FlatAppearance.MouseOverBackColor = hoverBack;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(215, 228, 250);
            UiEffects.EnableButtonEffect(button);
        }

        public static void StyleInput(Control input)
        {
            input.Font = new Font("Microsoft JhengHei UI", 10f);
            input.BackColor = Surface;
            TextBox tb = input as TextBox;
            if (tb != null)
            {
                tb.BorderStyle = BorderStyle.FixedSingle;
                if (tb.Multiline)
                    tb.AcceptsReturn = true;
            }

            input.Enter -= Input_Enter;
            input.Leave -= Input_Leave;
            input.Enter += Input_Enter;
            input.Leave += Input_Leave;
        }

        private static void Input_Enter(object sender, EventArgs e)
        {
            Control c = sender as Control;
            if (c != null)
                c.BackColor = Focus;
        }

        private static void Input_Leave(object sender, EventArgs e)
        {
            Control c = sender as Control;
            if (c != null)
                c.BackColor = Surface;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(225, 231, 240);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.Font = new Font("Microsoft JhengHei UI", 9.5f);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 241, 250);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = PrimaryDark;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = Math.Max(grid.ColumnHeadersHeight, 34);
            grid.DefaultCellStyle.BackColor = Surface;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 228, 250);
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, 31);
            UiEffects.EnableGridHover(grid);
        }


        public static void ConfigureEnterFlow(Control[] controls, Button finalButton)
        {
            if (controls == null)
                return;

            for (int i = 0; i < controls.Length; i++)
            {
                Control current = controls[i];
                if (current == null)
                    continue;
                current.TabIndex = i;
                current.KeyDown -= ExplicitEnterFlow_KeyDown;
                current.KeyDown += ExplicitEnterFlow_KeyDown;
                current.Tag = new EnterFlowInfo(controls, i, finalButton);
            }

            if (finalButton != null)
                finalButton.TabIndex = controls.Length;
        }

        private class EnterFlowInfo
        {
            public Control[] Controls;
            public int Index;
            public Button FinalButton;

            public EnterFlowInfo(Control[] controls, int index, Button finalButton)
            {
                Controls = controls;
                Index = index;
                FinalButton = finalButton;
            }
        }

        private static void ExplicitEnterFlow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            TextBox tb = sender as TextBox;
            if (tb != null && tb.Multiline && e.Control)
                return;

            Control current = sender as Control;
            if (current == null)
                return;

            EnterFlowInfo info = current.Tag as EnterFlowInfo;
            if (info == null || info.Controls == null || info.Controls.Length == 0)
                return;

            e.SuppressKeyPress = true;
            int nextIndex = e.Shift ? info.Index - 1 : info.Index + 1;

            if (nextIndex < 0)
            {
                info.Controls[info.Controls.Length - 1].Focus();
                return;
            }

            if (nextIndex >= info.Controls.Length)
            {
                if (info.FinalButton != null)
                    info.FinalButton.Focus();
                else
                    info.Controls[0].Focus();
                return;
            }

            if (info.Controls[nextIndex] != null)
                info.Controls[nextIndex].Focus();
        }

        public static Label MakeHint(string text, int x, int y, int width)
        {
            Label hint = new Label();
            hint.Text = text;
            hint.Font = new Font("Microsoft JhengHei UI", 8.5f);
            hint.ForeColor = MutedText;
            hint.Location = new Point(x, y);
            hint.Size = new Size(width, 22);
            return hint;
        }
    }
}
