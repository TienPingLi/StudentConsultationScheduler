using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public class AvailabilityEditorForm : Form
    {
        private AvailabilityGridControl grid;
        private Label lblPreview;
        private string availableTimeText;

        private readonly string[] dayItems = new string[] { "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日" };
        private readonly string[] slotItems = new string[]
        {
            "09:00-10:00", "10:00-11:00", "11:00-12:00",
            "13:00-14:00", "14:00-15:00", "15:00-16:00",
            "16:00-17:00", "17:00-18:00", "18:00-19:00"
        };

        public string AvailableTimeText
        {
            get { return availableTimeText; }
        }

        public AvailabilityEditorForm(string originalAvailableTime)
        {
            availableTimeText = originalAvailableTime ?? "";
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "可諮詢時間視覺化設定";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(920, 660);
            MinimumSize = new Size(820, 580);
            BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 3;
            layout.ColumnCount = 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            Controls.Add(layout);

            Panel top = new Panel();
            top.Dock = DockStyle.Fill;
            top.BackColor = Color.FromArgb(245, 247, 250);
            layout.Controls.Add(top, 0, 0);

            Label title = new Label();
            title.Text = "點一下格子即可切換老師可諮詢時間";
            title.Font = new Font("Microsoft JhengHei UI", 15, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(45, 70, 120);
            title.Location = new Point(18, 12);
            title.Size = new Size(560, 34);
            top.Controls.Add(title);

            Label hint = new Label();
            hint.Text = "藍色代表該時段可諮詢；空白代表不開放。時間固定為整點時段。";
            hint.Font = new Font("Microsoft JhengHei UI", 9);
            hint.ForeColor = Color.DimGray;
            hint.Location = new Point(20, 48);
            hint.Size = new Size(660, 22);
            top.Controls.Add(hint);

            Button btnSelectAll = MakeButton("全選", 700, 22, 70);
            Button btnClear = MakeButton("全部清除", 780, 22, 95);
            top.Controls.Add(btnSelectAll);
            top.Controls.Add(btnClear);
            btnSelectAll.Click += delegate { grid.SelectAllCells(); UpdatePreview(); };
            btnClear.Click += delegate { grid.ClearAllCells(); UpdatePreview(); };

            grid = new AvailabilityGridControl();
            grid.Dock = DockStyle.Fill;
            grid.DayItems = dayItems;
            grid.SlotItems = slotItems;
            grid.SelectedCells = ParseSelectedCells(availableTimeText);
            grid.SelectionChanged += delegate { UpdatePreview(); };
            layout.Controls.Add(grid, 0, 1);

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Fill;
            bottom.BackColor = Color.White;
            layout.Controls.Add(bottom, 0, 2);

            lblPreview = new Label();
            lblPreview.Font = new Font("Microsoft JhengHei UI", 9);
            lblPreview.ForeColor = Color.DimGray;
            lblPreview.Location = new Point(18, 10);
            lblPreview.Size = new Size(610, 66);
            lblPreview.BorderStyle = BorderStyle.FixedSingle;
            lblPreview.BackColor = Color.White;
            bottom.Controls.Add(lblPreview);

            Button btnOk = MakeButton("確定", 650, 24, 110);
            Button btnCancel = MakeButton("取消", 780, 24, 110);
            btnOk.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            btnOk.BackColor = Color.FromArgb(45, 105, 180);
            btnOk.ForeColor = Color.White;
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            bottom.Controls.Add(btnOk);
            bottom.Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            UiTheme.Apply(this);

            Load += delegate { UpdatePreview(); };
        }

        private Button MakeButton(string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 34);
            btn.BackColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            return btn;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            availableTimeText = BuildAvailableTimeText(grid.SelectedCells);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdatePreview()
        {
            string text = BuildAvailableTimeText(grid.SelectedCells);
            lblPreview.Text = string.IsNullOrWhiteSpace(text) ? "已選擇：尚未設定" : "已選擇：" + text;
        }

        private HashSet<string> ParseSelectedCells(string text)
        {
            HashSet<string> set = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(text))
                return set;

            string[] parts = text.Split(new char[] { '；', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string day = FindDay(part);
                if (day.Length == 0)
                    continue;

                MatchCollection matches = Regex.Matches(part, @"\d{2}:\d{2}-\d{2}:\d{2}");
                foreach (Match match in matches)
                    set.Add(day + "|" + match.Value);
            }
            return set;
        }

        private string FindDay(string text)
        {
            for (int i = 0; i < dayItems.Length; i++)
            {
                if (text.Contains(dayItems[i]))
                    return dayItems[i];
            }
            return "";
        }

        private string BuildAvailableTimeText(HashSet<string> selected)
        {
            if (selected == null || selected.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            bool firstDay = true;
            for (int d = 0; d < dayItems.Length; d++)
            {
                List<string> slots = new List<string>();
                for (int s = 0; s < slotItems.Length; s++)
                {
                    string key = dayItems[d] + "|" + slotItems[s];
                    if (selected.Contains(key))
                        slots.Add(slotItems[s]);
                }

                if (slots.Count == 0)
                    continue;

                if (!firstDay)
                    sb.Append("；");
                sb.Append(dayItems[d]);
                sb.Append(" ");
                for (int i = 0; i < slots.Count; i++)
                {
                    if (i > 0)
                        sb.Append("、");
                    sb.Append(slots[i]);
                }
                firstDay = false;
            }
            return sb.ToString();
        }
    }

    public class AvailabilityGridControl : Control
    {
        public string[] DayItems = new string[0];
        public string[] SlotItems = new string[0];
        public HashSet<string> SelectedCells = new HashSet<string>();
        public event EventHandler SelectionChanged;

        public AvailabilityGridControl()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Font = new Font("Microsoft JhengHei UI", 9);
        }

        public void SelectAllCells()
        {
            SelectedCells.Clear();
            for (int d = 0; d < DayItems.Length; d++)
            {
                for (int s = 0; s < SlotItems.Length; s++)
                    SelectedCells.Add(DayItems[d] + "|" + SlotItems[s]);
            }
            Invalidate();
            OnSelectionChanged();
        }

        public void ClearAllCells()
        {
            SelectedCells.Clear();
            Invalidate();
            OnSelectionChanged();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int dayIndex;
            int slotIndex;
            if (!HitTest(e.Location, out dayIndex, out slotIndex))
                return;

            string key = DayItems[dayIndex] + "|" + SlotItems[slotIndex];
            if (SelectedCells.Contains(key))
                SelectedCells.Remove(key);
            else
                SelectedCells.Add(key);
            Invalidate();
            OnSelectionChanged();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            int timeWidth = 95;
            int headerHeight = 45;
            int margin = 12;
            int dayCount = DayItems.Length;
            int slotCount = SlotItems.Length;
            if (dayCount == 0 || slotCount == 0)
                return;

            int gridX = margin;
            int gridY = margin;
            int gridWidth = Math.Max(10, ClientSize.Width - margin * 2);
            int gridHeight = Math.Max(10, ClientSize.Height - margin * 2);
            int colWidth = Math.Max(60, (gridWidth - timeWidth) / dayCount);
            int rowHeight = Math.Max(44, (gridHeight - headerHeight) / slotCount);

            using (Pen gridPen = new Pen(Color.FromArgb(210, 215, 225)))
            using (Pen strongPen = new Pen(Color.FromArgb(130, 140, 155)))
            using (Brush headerBrush = new SolidBrush(Color.FromArgb(245, 247, 250)))
            using (Brush selectedBrush = new SolidBrush(Color.FromArgb(170, 210, 255)))
            using (Brush emptyBrush = new SolidBrush(Color.White))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (Font headerFont = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold))
            using (StringFormat center = new StringFormat())
            {
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;

                Rectangle corner = new Rectangle(gridX, gridY, timeWidth, headerHeight);
                g.FillRectangle(headerBrush, corner);
                g.DrawRectangle(strongPen, corner);
                g.DrawString("時間", headerFont, textBrush, corner, center);

                for (int d = 0; d < dayCount; d++)
                {
                    Rectangle header = new Rectangle(gridX + timeWidth + d * colWidth, gridY, colWidth, headerHeight);
                    g.FillRectangle(headerBrush, header);
                    g.DrawRectangle(strongPen, header);
                    g.DrawString(DayItems[d], headerFont, textBrush, header, center);
                }

                for (int s = 0; s < slotCount; s++)
                {
                    Rectangle timeCell = new Rectangle(gridX, gridY + headerHeight + s * rowHeight, timeWidth, rowHeight);
                    g.FillRectangle(headerBrush, timeCell);
                    g.DrawRectangle(gridPen, timeCell);
                    g.DrawString(SlotItems[s], Font, textBrush, timeCell, center);

                    for (int d = 0; d < dayCount; d++)
                    {
                        Rectangle cell = new Rectangle(gridX + timeWidth + d * colWidth, gridY + headerHeight + s * rowHeight, colWidth, rowHeight);
                        string key = DayItems[d] + "|" + SlotItems[s];
                        g.FillRectangle(SelectedCells.Contains(key) ? selectedBrush : emptyBrush, cell);
                        g.DrawRectangle(gridPen, cell);
                        if (SelectedCells.Contains(key))
                            g.DrawString("可諮詢", Font, textBrush, cell, center);
                    }
                }
            }
        }

        private bool HitTest(Point point, out int dayIndex, out int slotIndex)
        {
            dayIndex = -1;
            slotIndex = -1;

            int timeWidth = 95;
            int headerHeight = 45;
            int margin = 12;
            int dayCount = DayItems.Length;
            int slotCount = SlotItems.Length;
            if (dayCount == 0 || slotCount == 0)
                return false;

            int gridX = margin;
            int gridY = margin;
            int gridWidth = Math.Max(10, ClientSize.Width - margin * 2);
            int gridHeight = Math.Max(10, ClientSize.Height - margin * 2);
            int colWidth = Math.Max(60, (gridWidth - timeWidth) / dayCount);
            int rowHeight = Math.Max(44, (gridHeight - headerHeight) / slotCount);

            int x = point.X - (gridX + timeWidth);
            int y = point.Y - (gridY + headerHeight);
            if (x < 0 || y < 0)
                return false;

            dayIndex = x / colWidth;
            slotIndex = y / rowHeight;
            return dayIndex >= 0 && dayIndex < dayCount && slotIndex >= 0 && slotIndex < slotCount;
        }

        private void OnSelectionChanged()
        {
            EventHandler handler = SelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
