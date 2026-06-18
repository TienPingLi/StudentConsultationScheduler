using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public class BeepPlayerForm : Form
    {
        [DllImport("kernel32.dll")]
        private static extern bool Beep(int frequency, int duration);

        private readonly int[] frequencies = new int[] { 262, 294, 330, 349, 392, 440, 494, 523 };
        private readonly string[] noteNames = new string[] { "Do", "Re", "Mi", "Fa", "Sol", "La", "Si", "Do" };
        private Label lblStatus;

        public BeepPlayerForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "簡易電子琴";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 360);
            BackColor = Color.FromArgb(28, 32, 42);

            Label title = new Label();
            title.Text = "簡易電子琴";
            title.Font = new Font("Microsoft JhengHei UI", 22, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(24, 20);
            title.Size = new Size(260, 45);
            Controls.Add(title);

            Label hint = new Label();
            hint.Text = "點擊琴鍵播放音階；也可以按鍵盤 1 ~ 8。";
            hint.Font = new Font("Microsoft JhengHei UI", 10);
            hint.ForeColor = Color.Gainsboro;
            hint.Location = new Point(28, 70);
            hint.Size = new Size(520, 24);
            Controls.Add(hint);

            Color[] colors = new Color[]
            {
                Color.FromArgb(255, 122, 122), Color.FromArgb(255, 173, 96), Color.FromArgb(255, 222, 89), Color.FromArgb(142, 224, 112),
                Color.FromArgb(94, 194, 255), Color.FromArgb(137, 145, 255), Color.FromArgb(203, 135, 255), Color.FromArgb(255, 143, 210)
            };

            int x = 30;
            for (int i = 0; i < 8; i++)
            {
                Button btn = new Button();
                btn.Text = noteNames[i] + "\n" + frequencies[i] + " Hz";
                btn.Font = new Font("Microsoft JhengHei UI", 12, FontStyle.Bold);
                btn.Location = new Point(x, 125);
                btn.Size = new Size(80, 130);
                btn.Tag = i;
                btn.BackColor = colors[i];
                btn.ForeColor = Color.FromArgb(25, 25, 30);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += NoteButton_Click;
                Controls.Add(btn);
                x += 86;
            }

            lblStatus = new Label();
            lblStatus.Text = "狀態：等待演奏";
            lblStatus.Font = new Font("Microsoft JhengHei UI", 11, FontStyle.Bold);
            lblStatus.ForeColor = Color.WhiteSmoke;
            lblStatus.Location = new Point(30, 292);
            lblStatus.Size = new Size(650, 28);
            Controls.Add(lblStatus);

            KeyPreview = true;
            KeyDown += BeepPlayerForm_KeyDown;
        }

        private void BeepPlayerForm_KeyDown(object sender, KeyEventArgs e)
        {
            int index = -1;
            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D8)
                index = (int)e.KeyCode - (int)Keys.D1;
            if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad8)
                index = (int)e.KeyCode - (int)Keys.NumPad1;
            if (index >= 0)
                PlayNote(index, 300);
        }

        private void NoteButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            PlayNote((int)btn.Tag, 300);
        }

        private void PlayNote(int index, int duration)
        {
            if (index < 0 || index >= frequencies.Length)
                return;
            lblStatus.Text = "狀態：播放 " + noteNames[index] + "（" + frequencies[index] + " Hz）";
            Application.DoEvents();
            try { Beep(frequencies[index], duration); } catch { }
        }
    }
}
