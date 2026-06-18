using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public enum ToastKind
    {
        Info,
        Success,
        Warning,
        Error
    }

    public static class UiEffects
    {
        private class ButtonEffectState
        {
            public int OriginalTop;
            public Color OriginalBorderColor;
            public bool Hovered;
            public bool Pressed;
        }

        private static readonly Dictionary<Button, ButtonEffectState> buttonStates = new Dictionary<Button, ButtonEffectState>();
        private static readonly Dictionary<DataGridView, int> gridHoverRows = new Dictionary<DataGridView, int>();

        public static void EnableFormFade(Form form)
        {
            if (form == null)
                return;

            try
            {
                if (!form.Visible)
                    form.Opacity = 0;
            }
            catch
            {
            }

            form.Shown -= Form_Shown_FadeIn;
            form.Shown += Form_Shown_FadeIn;
        }

        private static void Form_Shown_FadeIn(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (form == null || form.IsDisposed)
                return;

            try
            {
                form.Opacity = 0;
            }
            catch
            {
                return;
            }

            Timer timer = new Timer();
            timer.Interval = 15;
            timer.Tick += delegate
            {
                if (form.IsDisposed)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

                try
                {
                    if (form.Opacity < 1)
                        form.Opacity = Math.Min(1, form.Opacity + 0.08);
                    else
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                }
                catch
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        public static void EnableButtonEffect(Button button)
        {
            if (button == null)
                return;

            if (!buttonStates.ContainsKey(button))
            {
                ButtonEffectState state = new ButtonEffectState();
                state.OriginalTop = button.Top;
                state.OriginalBorderColor = button.FlatAppearance.BorderColor;
                buttonStates[button] = state;
            }

            button.MouseEnter -= Button_MouseEnter;
            button.MouseLeave -= Button_MouseLeave;
            button.MouseDown -= Button_MouseDown;
            button.MouseUp -= Button_MouseUp;

            button.MouseEnter += Button_MouseEnter;
            button.MouseLeave += Button_MouseLeave;
            button.MouseDown += Button_MouseDown;
            button.MouseUp += Button_MouseUp;
        }

        private static void Button_MouseEnter(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !buttonStates.ContainsKey(button))
                return;

            ButtonEffectState state = buttonStates[button];
            if (!state.Hovered)
            {
                state.OriginalTop = button.Top;
                state.OriginalBorderColor = button.FlatAppearance.BorderColor;
                state.Hovered = true;
                button.Top = Math.Max(0, button.Top - 1);
                button.FlatAppearance.BorderColor = Color.FromArgb(80, 135, 215);
            }
        }

        private static void Button_MouseLeave(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !buttonStates.ContainsKey(button))
                return;

            ButtonEffectState state = buttonStates[button];
            button.Top = state.OriginalTop;
            button.FlatAppearance.BorderColor = state.OriginalBorderColor;
            state.Hovered = false;
            state.Pressed = false;
        }

        private static void Button_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !buttonStates.ContainsKey(button))
                return;

            ButtonEffectState state = buttonStates[button];
            if (!state.Pressed)
            {
                button.Top = state.OriginalTop + 1;
                state.Pressed = true;
            }
        }

        private static void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !buttonStates.ContainsKey(button))
                return;

            ButtonEffectState state = buttonStates[button];
            state.Pressed = false;
            button.Top = state.Hovered ? Math.Max(0, state.OriginalTop - 1) : state.OriginalTop;
        }

        public static void EnableGridHover(DataGridView grid)
        {
            if (grid == null)
                return;

            if (!gridHoverRows.ContainsKey(grid))
                gridHoverRows[grid] = -1;

            grid.CellMouseEnter -= Grid_CellMouseEnter;
            grid.CellMouseLeave -= Grid_CellMouseLeave;
            grid.MouseLeave -= Grid_MouseLeave;

            grid.CellMouseEnter += Grid_CellMouseEnter;
            grid.CellMouseLeave += Grid_CellMouseLeave;
            grid.MouseLeave += Grid_MouseLeave;
        }

        private static void Grid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid == null || e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count)
                return;

            RestoreGridHover(grid);
            gridHoverRows[grid] = e.RowIndex;
            if (!grid.Rows[e.RowIndex].Selected)
                grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(238, 246, 255);
        }

        private static void Grid_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            // 留給 MouseLeave 與下一個 CellMouseEnter 統一處理，避免滑過欄位時閃爍。
        }

        private static void Grid_MouseLeave(object sender, EventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            RestoreGridHover(grid);
        }

        private static void RestoreGridHover(DataGridView grid)
        {
            if (grid == null || !gridHoverRows.ContainsKey(grid))
                return;

            int rowIndex = gridHoverRows[grid];
            if (rowIndex >= 0 && rowIndex < grid.Rows.Count && !grid.Rows[rowIndex].Selected)
                grid.Rows[rowIndex].DefaultCellStyle.BackColor = grid.Rows[rowIndex].Index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
            gridHoverRows[grid] = -1;
        }

        public static void ShowToast(Form form, string message, ToastKind kind)
        {
            if (form == null || form.IsDisposed || string.IsNullOrWhiteSpace(message))
                return;

            Color backColor = Color.FromArgb(31, 63, 109);
            string title = "提醒";
            if (kind == ToastKind.Success)
            {
                backColor = Color.FromArgb(43, 135, 92);
                title = "完成";
            }
            else if (kind == ToastKind.Warning)
            {
                backColor = Color.FromArgb(205, 130, 35);
                title = "注意";
            }
            else if (kind == ToastKind.Error)
            {
                backColor = Color.FromArgb(190, 65, 65);
                title = "錯誤";
            }

            Panel toast = new Panel();
            toast.Width = Math.Min(380, Math.Max(260, form.ClientSize.Width - 48));
            toast.Height = 64;
            toast.BackColor = backColor;
            toast.Left = form.ClientSize.Width + 8;
            toast.Top = Math.Max(20, form.ClientSize.Height - toast.Height - 28);
            toast.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            toast.Padding = new Padding(14, 8, 12, 8);

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.ForeColor = Color.White;
            titleLabel.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
            titleLabel.Location = new Point(14, 8);
            titleLabel.Size = new Size(toast.Width - 28, 20);
            toast.Controls.Add(titleLabel);

            Label messageLabel = new Label();
            messageLabel.Text = message;
            messageLabel.ForeColor = Color.FromArgb(235, 242, 255);
            messageLabel.Font = new Font("Microsoft JhengHei UI", 9f);
            messageLabel.Location = new Point(14, 30);
            messageLabel.Size = new Size(toast.Width - 28, 24);
            toast.Controls.Add(messageLabel);

            form.Controls.Add(toast);
            toast.BringToFront();

            int targetLeft = form.ClientSize.Width - toast.Width - 24;
            int waitTicks = 0;
            int stage = 0;
            Timer timer = new Timer();
            timer.Interval = 15;
            timer.Tick += delegate
            {
                if (form.IsDisposed || toast.IsDisposed)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

                if (stage == 0)
                {
                    toast.Left -= 32;
                    if (toast.Left <= targetLeft)
                    {
                        toast.Left = targetLeft;
                        stage = 1;
                    }
                }
                else if (stage == 1)
                {
                    waitTicks++;
                    if (waitTicks >= 145)
                        stage = 2;
                }
                else
                {
                    toast.Left += 32;
                    if (toast.Left >= form.ClientSize.Width + 8)
                    {
                        timer.Stop();
                        timer.Dispose();
                        form.Controls.Remove(toast);
                        toast.Dispose();
                    }
                }
            };
            timer.Start();
        }
    }
}
