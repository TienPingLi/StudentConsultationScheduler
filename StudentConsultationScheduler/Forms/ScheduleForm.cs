using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class ScheduleForm : Form
    {
        private DateTimePicker dtpWeek;
        private ComboBox cmbSubject;
        private ComboBox cmbTeacher;
        private ComboBox cmbBookedStatus;
        private ConsultationScheduleView scheduleView;
        private FlowLayoutPanel legendPanel;
        private List<TeacherInfo> allTeachers = new List<TeacherInfo>();
        private List<AppointmentInfo> weekAppointments = new List<AppointmentInfo>();
        private bool loadingFilters;

        private readonly string[] dayItems = new string[] { "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日" };
        private readonly string[] slotItems = new string[]
        {
            "09:00-10:00", "10:00-11:00", "11:00-12:00",
            "13:00-14:00", "14:00-15:00", "15:00-16:00",
            "16:00-17:00", "17:00-18:00", "18:00-19:00"
        };

        private readonly Color[] teacherColors = new Color[]
        {
            Color.FromArgb(255, 235, 153),
            Color.FromArgb(181, 220, 255),
            Color.FromArgb(190, 235, 190),
            Color.FromArgb(255, 205, 205),
            Color.FromArgb(220, 205, 255),
            Color.FromArgb(255, 220, 180),
            Color.FromArgb(195, 240, 235),
            Color.FromArgb(235, 215, 185),
            Color.FromArgb(225, 235, 165),
            Color.FromArgb(210, 225, 255)
        };

        public ScheduleForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "互動式可諮詢時間課表";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1240, 800);
            MinimumSize = new Size(1120, 700);
            BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 3;
            layout.ColumnCount = 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            Controls.Add(layout);

            Panel top = new Panel();
            top.Dock = DockStyle.Fill;
            top.BackColor = Color.FromArgb(245, 247, 250);
            layout.Controls.Add(top, 0, 0);

            Label title = new Label();
            title.Text = "互動式可諮詢時間課表";
            title.Font = new Font("Microsoft JhengHei UI", 16, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(45, 70, 120);
            title.Location = new Point(20, 12);
            title.Size = new Size(360, 34);
            top.Controls.Add(title);

            Label hint = new Label();
            hint.Text = "點擊可預約時段可直接選學生建立預約；點擊已預約時段可修改該預約。";
            hint.Font = new Font("Microsoft JhengHei UI", 9);
            hint.ForeColor = Color.DimGray;
            hint.Location = new Point(400, 18);
            hint.Size = new Size(760, 24);
            top.Controls.Add(hint);

            Label lblWeek = AddTopLabel(top, "預約週：", 20, 58);
            dtpWeek = new DateTimePicker();
            dtpWeek.Font = new Font("Microsoft JhengHei UI", 10);
            dtpWeek.Format = DateTimePickerFormat.Custom;
            dtpWeek.CustomFormat = "yyyy-MM-dd";
            dtpWeek.Location = new Point(85, 55);
            dtpWeek.Size = new Size(130, 25);
            dtpWeek.ValueChanged += DtpWeek_ValueChanged;
            top.Controls.Add(dtpWeek);

            Label lblSubject = AddTopLabel(top, "科目：", 235, 58);
            cmbSubject = new ComboBox();
            cmbSubject.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSubject.Font = new Font("Microsoft JhengHei UI", 10);
            cmbSubject.Location = new Point(290, 55);
            cmbSubject.Size = new Size(220, 25);
            cmbSubject.SelectedIndexChanged += CmbSubject_SelectedIndexChanged;
            top.Controls.Add(cmbSubject);

            Label lblTeacher = AddTopLabel(top, "老師：", 530, 58);
            cmbTeacher = new ComboBox();
            cmbTeacher.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTeacher.Font = new Font("Microsoft JhengHei UI", 10);
            cmbTeacher.Location = new Point(585, 55);
            cmbTeacher.Size = new Size(220, 25);
            cmbTeacher.SelectedIndexChanged += CmbTeacher_SelectedIndexChanged;
            top.Controls.Add(cmbTeacher);

            Label lblBooked = AddTopLabel(top, "是否有預約：", 825, 58);
            cmbBookedStatus = new ComboBox();
            cmbBookedStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBookedStatus.Font = new Font("Microsoft JhengHei UI", 10);
            cmbBookedStatus.Location = new Point(925, 55);
            cmbBookedStatus.Size = new Size(150, 25);
            cmbBookedStatus.Items.Add("全部");
            cmbBookedStatus.Items.Add("只看已預約");
            cmbBookedStatus.Items.Add("只看未預約");
            cmbBookedStatus.SelectedIndex = 0;
            cmbBookedStatus.SelectedIndexChanged += delegate { RefreshSchedule(); };
            top.Controls.Add(cmbBookedStatus);

            Button btnRefresh = new Button();
            btnRefresh.Text = "重新整理";
            btnRefresh.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            btnRefresh.Location = new Point(1095, 52);
            btnRefresh.Size = new Size(105, 32);
            btnRefresh.BackColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += delegate { LoadData(); };
            top.Controls.Add(btnRefresh);

            Label secondHint = new Label();
            secondHint.Text = "格子文字：可預約＝老師有開放且尚未被預約；已預約＝該老師該時段已有學生預約。";
            secondHint.Font = new Font("Microsoft JhengHei UI", 9);
            secondHint.ForeColor = Color.FromArgb(180, 80, 40);
            secondHint.Location = new Point(20, 88);
            secondHint.Size = new Size(900, 24);
            top.Controls.Add(secondHint);

            scheduleView = new ConsultationScheduleView();
            scheduleView.Dock = DockStyle.Fill;
            scheduleView.DayItems = dayItems;
            scheduleView.SlotItems = slotItems;
            scheduleView.SlotClicked += ScheduleView_SlotClicked;
            layout.Controls.Add(scheduleView, 0, 1);

            legendPanel = new FlowLayoutPanel();
            legendPanel.Dock = DockStyle.Fill;
            legendPanel.AutoScroll = true;
            legendPanel.Padding = new Padding(14, 10, 14, 8);
            legendPanel.BackColor = Color.White;
            layout.Controls.Add(legendPanel, 0, 2);

            Load += delegate { dtpWeek.Value = DateTime.Today; LoadData(); };
        }

        private Label AddTopLabel(Control parent, string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Microsoft JhengHei UI", 10);
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(100, 25);
            parent.Controls.Add(lbl);
            return lbl;
        }

        private void LoadData()
        {
            LoadTeachers();
            LoadWeekAppointments();

            loadingFilters = true;
            LoadSubjectFilter();
            LoadTeacherFilter();
            loadingFilters = false;
            RefreshSchedule();
        }

        private void LoadTeachers()
        {
            allTeachers.Clear();
            DataTable table = Db.GetDataTable("SELECT TeacherId, Name, Subject, ISNULL(AvailableTime, N'') AS AvailableTime FROM dbo.Teachers ORDER BY Subject, Name");
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                TeacherInfo teacher = new TeacherInfo();
                teacher.TeacherId = Convert.ToInt32(row["TeacherId"]);
                teacher.Name = Convert.ToString(row["Name"]);
                teacher.Subject = Convert.ToString(row["Subject"]);
                teacher.AvailableTime = Convert.ToString(row["AvailableTime"]);
                teacher.Color = teacherColors[i % teacherColors.Length];
                allTeachers.Add(teacher);
            }
        }

        private void LoadWeekAppointments()
        {
            weekAppointments.Clear();
            DateTime monday = GetWeekStart(dtpWeek.Value.Date);
            DateTime nextMonday = monday.AddDays(7);
            DataTable table = Db.GetDataTable(@"
SELECT
    a.AppointmentId,
    a.TeacherId,
    a.StudentId,
    s.Name AS StudentName,
    s.StudentNo,
    t.Name AS TeacherName,
    t.Subject,
    a.Topic,
    a.AppointmentDate,
    a.StartTime,
    a.EndTime,
    a.Status
FROM dbo.Appointments a
INNER JOIN dbo.Students s ON a.StudentId = s.StudentId
INNER JOIN dbo.Teachers t ON a.TeacherId = t.TeacherId
WHERE a.AppointmentDate >= @startDate
  AND a.AppointmentDate < @endDate
  AND a.Status <> N'已取消'
ORDER BY a.AppointmentDate, a.StartTime, t.Name", Db.P("@startDate", monday), Db.P("@endDate", nextMonday));

            foreach (DataRow row in table.Rows)
            {
                AppointmentInfo appt = new AppointmentInfo();
                appt.AppointmentId = Convert.ToInt32(row["AppointmentId"]);
                appt.TeacherId = Convert.ToInt32(row["TeacherId"]);
                appt.StudentId = Convert.ToInt32(row["StudentId"]);
                appt.StudentName = Convert.ToString(row["StudentName"]);
                appt.StudentNo = Convert.ToString(row["StudentNo"]);
                appt.TeacherName = Convert.ToString(row["TeacherName"]);
                appt.Subject = Convert.ToString(row["Subject"]);
                appt.Topic = Convert.ToString(row["Topic"]);
                appt.Date = Convert.ToDateTime(row["AppointmentDate"]).Date;
                appt.StartTime = (TimeSpan)row["StartTime"];
                appt.EndTime = (TimeSpan)row["EndTime"];
                appt.Status = Convert.ToString(row["Status"]);
                weekAppointments.Add(appt);
            }
        }

        private void LoadSubjectFilter()
        {
            string oldText = cmbSubject.SelectedItem == null ? "全部科目" : cmbSubject.SelectedItem.ToString();
            cmbSubject.Items.Clear();
            cmbSubject.Items.Add("全部科目");

            List<string> subjects = new List<string>();
            foreach (TeacherInfo t in allTeachers)
            {
                string subject = string.IsNullOrWhiteSpace(t.Subject) ? "未設定科目" : t.Subject.Trim();
                if (!subjects.Contains(subject))
                    subjects.Add(subject);
            }
            subjects.Sort();
            foreach (string subject in subjects)
                cmbSubject.Items.Add(subject);

            int index = cmbSubject.Items.IndexOf(oldText);
            cmbSubject.SelectedIndex = index >= 0 ? index : 0;
        }

        private void LoadTeacherFilter()
        {
            string selectedSubject = GetSelectedSubject();
            int oldId = GetSelectedTeacherId();
            cmbTeacher.Items.Clear();
            cmbTeacher.Items.Add(new TeacherFilterItem(0, "全部老師"));

            foreach (TeacherInfo t in allTeachers)
            {
                if (SubjectMatches(t, selectedSubject))
                    cmbTeacher.Items.Add(new TeacherFilterItem(t.TeacherId, t.Name + " / " + EmptyToUnset(t.Subject)));
            }

            int newIndex = 0;
            for (int i = 0; i < cmbTeacher.Items.Count; i++)
            {
                TeacherFilterItem item = cmbTeacher.Items[i] as TeacherFilterItem;
                if (item != null && item.TeacherId == oldId)
                {
                    newIndex = i;
                    break;
                }
            }
            cmbTeacher.SelectedIndex = newIndex;
        }

        private void DtpWeek_ValueChanged(object sender, EventArgs e)
        {
            if (scheduleView == null)
                return;
            LoadWeekAppointments();
            RefreshSchedule();
        }

        private void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingFilters)
                return;
            loadingFilters = true;
            LoadTeacherFilter();
            loadingFilters = false;
            RefreshSchedule();
        }

        private void CmbTeacher_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingFilters)
                return;
            RefreshSchedule();
        }

        private void ScheduleView_SlotClicked(object sender, ScheduleSlotClickEventArgs e)
        {
            if (e == null || e.Item == null)
                return;

            if (e.Item.Appointment != null)
            {
                string msg = "此時段已預約：\n" +
                             e.Item.Teacher.Name + " / " + e.Item.Appointment.StudentName + " / " + e.Item.Appointment.Topic +
                             "\n\n是否開啟修改預約？";
                if (MessageBox.Show(msg, "已預約", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (AppointmentForm form = new AppointmentForm(e.Item.Appointment.AppointmentId))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            LoadWeekAppointments();
                            RefreshSchedule();
                        }
                    }
                }
                return;
            }

            if (IsPastSlot(e.Item.Date, e.Item.Slot))
            {
                MessageBox.Show("不可預約今天以前或已經開始的時段，請選擇現在之後的時間。", "不可預約過去時間", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (AppointmentForm form = new AppointmentForm(e.Item.Teacher.TeacherId, e.Item.Date, e.Item.Slot))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadWeekAppointments();
                    RefreshSchedule();
                }
            }
        }

        private bool IsPastSlot(DateTime date, string slot)
        {
            TimeSpan start;
            if (!TryGetSlotStart(slot, out start))
                return false;
            return date.Date.Add(start) <= DateTime.Now;
        }

        private bool TryGetSlotStart(string slot, out TimeSpan start)
        {
            start = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(slot) || !slot.Contains("-"))
                return false;
            string[] parts = slot.Split('-');
            return parts.Length > 0 && TimeSpan.TryParse(parts[0], out start);
        }

        private void RefreshSchedule()
        {
            if (scheduleView == null)
                return;

            string selectedSubject = GetSelectedSubject();
            int selectedTeacherId = GetSelectedTeacherId();

            List<TeacherInfo> visible = new List<TeacherInfo>();
            foreach (TeacherInfo t in allTeachers)
            {
                if (!SubjectMatches(t, selectedSubject))
                    continue;
                if (selectedTeacherId != 0 && t.TeacherId != selectedTeacherId)
                    continue;
                visible.Add(t);
            }

            scheduleView.WeekStart = GetWeekStart(dtpWeek.Value.Date);
            scheduleView.Teachers = visible;
            scheduleView.Appointments = weekAppointments;
            scheduleView.BookingFilter = cmbBookedStatus.SelectedItem == null ? "全部" : cmbBookedStatus.SelectedItem.ToString();
            scheduleView.Invalidate();
            RefreshLegend(visible);
        }

        private void RefreshLegend(List<TeacherInfo> teachers)
        {
            legendPanel.Controls.Clear();
            if (teachers.Count == 0)
            {
                Label empty = new Label();
                empty.Text = "目前沒有符合條件的老師可諮詢時段。";
                empty.Font = new Font("Microsoft JhengHei UI", 10);
                empty.AutoSize = true;
                legendPanel.Controls.Add(empty);
                return;
            }

            foreach (TeacherInfo teacher in teachers)
            {
                Panel item = new Panel();
                item.Width = 260;
                item.Height = 30;
                item.Margin = new Padding(4, 3, 14, 3);

                Panel swatch = new Panel();
                swatch.BackColor = teacher.Color;
                swatch.BorderStyle = BorderStyle.FixedSingle;
                swatch.Location = new Point(0, 5);
                swatch.Size = new Size(22, 18);
                item.Controls.Add(swatch);

                Label label = new Label();
                label.Text = teacher.Name + " / " + EmptyToUnset(teacher.Subject);
                label.Font = new Font("Microsoft JhengHei UI", 9);
                label.Location = new Point(30, 4);
                label.Size = new Size(225, 22);
                item.Controls.Add(label);

                legendPanel.Controls.Add(item);
            }
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-diff).Date;
        }

        private string GetSelectedSubject()
        {
            if (cmbSubject.SelectedItem == null)
                return "全部科目";
            return cmbSubject.SelectedItem.ToString();
        }

        private int GetSelectedTeacherId()
        {
            TeacherFilterItem item = cmbTeacher.SelectedItem as TeacherFilterItem;
            return item == null ? 0 : item.TeacherId;
        }

        private bool SubjectMatches(TeacherInfo teacher, string selectedSubject)
        {
            if (string.IsNullOrWhiteSpace(selectedSubject) || selectedSubject == "全部科目")
                return true;
            return EmptyToUnset(teacher.Subject) == selectedSubject;
        }

        private string EmptyToUnset(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "未設定科目" : text.Trim();
        }

        private class TeacherFilterItem
        {
            public int TeacherId;
            public string Text;

            public TeacherFilterItem(int teacherId, string text)
            {
                TeacherId = teacherId;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }

    public class TeacherInfo
    {
        public int TeacherId;
        public string Name;
        public string Subject;
        public string AvailableTime;
        public Color Color;
    }

    public class AppointmentInfo
    {
        public int AppointmentId;
        public int TeacherId;
        public int StudentId;
        public string StudentName;
        public string StudentNo;
        public string TeacherName;
        public string Subject;
        public string Topic;
        public DateTime Date;
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public string Status;
        public string Note;

        public string SlotText
        {
            get { return StartTime.ToString(@"hh\:mm") + "-" + EndTime.ToString(@"hh\:mm"); }
        }
    }

    public class ScheduleCellItem
    {
        public TeacherInfo Teacher;
        public AppointmentInfo Appointment;
        public DateTime Date;
        public string Day;
        public string Slot;
    }

    public class ScheduleSlotClickEventArgs : EventArgs
    {
        public ScheduleCellItem Item;
        public ScheduleSlotClickEventArgs(ScheduleCellItem item)
        {
            Item = item;
        }
    }

    public class ConsultationScheduleView : Control
    {
        public string[] DayItems = new string[0];
        public string[] SlotItems = new string[0];
        public List<TeacherInfo> Teachers = new List<TeacherInfo>();
        public List<AppointmentInfo> Appointments = new List<AppointmentInfo>();
        public DateTime WeekStart = DateTime.Today;
        public string BookingFilter = "全部";
        public string StatusFilter = "全部狀態";
        public event EventHandler<ScheduleSlotClickEventArgs> SlotClicked;

        private int hoverDayIndex = -1;
        private int hoverSlotIndex = -1;
        private Rectangle hoverCell = Rectangle.Empty;
        private Rectangle clickPulseCell = Rectangle.Empty;
        private int clickPulseFrame = 0;
        private Timer clickPulseTimer;

        public ConsultationScheduleView()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Font = new Font("Microsoft JhengHei UI", 8.5f);
            Cursor = Cursors.Default;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int dayIndex;
            int slotIndex;
            Rectangle cell;
            bool hit = HitTest(e.Location, out dayIndex, out slotIndex, out cell) && BuildItemsForCell(dayIndex, slotIndex).Count > 0;

            if (hit)
            {
                Cursor = Cursors.Hand;
                if (hoverDayIndex != dayIndex || hoverSlotIndex != slotIndex)
                {
                    Rectangle oldCell = hoverCell;
                    hoverDayIndex = dayIndex;
                    hoverSlotIndex = slotIndex;
                    hoverCell = cell;
                    if (!oldCell.IsEmpty)
                        Invalidate(Rectangle.Inflate(oldCell, 3, 3));
                    Invalidate(Rectangle.Inflate(hoverCell, 3, 3));
                }
            }
            else
            {
                Cursor = Cursors.Default;
                ClearHoverCell();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
            ClearHoverCell();
        }

        private void ClearHoverCell()
        {
            if (hoverDayIndex == -1 && hoverSlotIndex == -1)
                return;
            Rectangle oldCell = hoverCell;
            hoverDayIndex = -1;
            hoverSlotIndex = -1;
            hoverCell = Rectangle.Empty;
            if (!oldCell.IsEmpty)
                Invalidate(Rectangle.Inflate(oldCell, 3, 3));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int dayIndex;
            int slotIndex;
            Rectangle cell;
            if (!HitTest(e.Location, out dayIndex, out slotIndex, out cell))
                return;

            List<ScheduleCellItem> items = BuildItemsForCell(dayIndex, slotIndex);
            if (items.Count == 0)
                return;

            int partWidth = Math.Max(1, cell.Width / items.Count);
            int itemIndex = (e.X - cell.Left) / partWidth;
            if (itemIndex < 0)
                itemIndex = 0;
            if (itemIndex >= items.Count)
                itemIndex = items.Count - 1;

            StartClickPulse(cell);

            EventHandler<ScheduleSlotClickEventArgs> handler = SlotClicked;
            if (handler != null)
                handler(this, new ScheduleSlotClickEventArgs(items[itemIndex]));
        }

        private void StartClickPulse(Rectangle cell)
        {
            clickPulseCell = cell;
            clickPulseFrame = 0;
            if (clickPulseTimer == null)
            {
                clickPulseTimer = new Timer();
                clickPulseTimer.Interval = 22;
                clickPulseTimer.Tick += delegate
                {
                    clickPulseFrame++;
                    if (clickPulseFrame > 9)
                    {
                        clickPulseTimer.Stop();
                        Rectangle oldCell = clickPulseCell;
                        clickPulseCell = Rectangle.Empty;
                        if (!oldCell.IsEmpty)
                            Invalidate(Rectangle.Inflate(oldCell, 8, 8));
                    }
                    else if (!clickPulseCell.IsEmpty)
                    {
                        Invalidate(Rectangle.Inflate(clickPulseCell, 8, 8));
                    }
                };
            }
            clickPulseTimer.Start();
            Invalidate(Rectangle.Inflate(clickPulseCell, 8, 8));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            int timeWidth = 95;
            int headerHeight = 54;
            int margin = 12;
            int width = Math.Max(10, ClientSize.Width - margin * 2);
            int height = Math.Max(10, ClientSize.Height - margin * 2);
            int dayCount = DayItems.Length;
            int slotCount = SlotItems.Length;
            if (dayCount == 0 || slotCount == 0)
                return;

            int gridX = margin;
            int gridY = margin;
            int gridWidth = width;
            int gridHeight = height;
            int colWidth = Math.Max(60, (gridWidth - timeWidth) / dayCount);
            int rowHeight = Math.Max(46, (gridHeight - headerHeight) / slotCount);

            using (Pen gridPen = new Pen(Color.FromArgb(210, 215, 225)))
            using (Pen strongPen = new Pen(Color.FromArgb(130, 140, 155)))
            using (Brush headerBrush = new SolidBrush(Color.FromArgb(245, 247, 250)))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (Brush emptyBrush = new SolidBrush(Color.White))
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
                    DateTime date = WeekStart.AddDays(d);
                    Rectangle header = new Rectangle(gridX + timeWidth + d * colWidth, gridY, colWidth, headerHeight);
                    g.FillRectangle(headerBrush, header);
                    g.DrawRectangle(strongPen, header);
                    g.DrawString(DayItems[d] + "\n" + date.ToString("MM/dd"), headerFont, textBrush, header, center);
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
                        g.FillRectangle(emptyBrush, cell);
                        List<ScheduleCellItem> items = BuildItemsForCell(d, s);
                        if (items.Count > 0)
                            DrawItemsInCell(g, cell, items);
                        g.DrawRectangle(gridPen, cell);
                        if (d == hoverDayIndex && s == hoverSlotIndex)
                            DrawHoverFrame(g, cell);
                        if (!clickPulseCell.IsEmpty && cell == clickPulseCell)
                            DrawClickPulse(g, cell);
                    }
                }
            }
        }

        private void DrawHoverFrame(Graphics g, Rectangle cell)
        {
            Rectangle frame = Rectangle.Inflate(cell, -2, -2);
            using (Pen hoverPen = new Pen(Color.FromArgb(36, 105, 190), 2f))
            {
                g.DrawRectangle(hoverPen, frame);
            }
        }

        private void DrawClickPulse(Graphics g, Rectangle cell)
        {
            int grow = Math.Min(6, clickPulseFrame);
            int alpha = Math.Max(40, 180 - clickPulseFrame * 15);
            Rectangle frame = Rectangle.Inflate(cell, -3 + grow / 2, -3 + grow / 2);
            using (Pen pulsePen = new Pen(Color.FromArgb(alpha, 255, 130, 60), 2.5f))
            {
                g.DrawRectangle(pulsePen, frame);
            }
        }

        private void DrawItemsInCell(Graphics g, Rectangle cell, List<ScheduleCellItem> items)
        {
            int count = items.Count;
            int partWidth = Math.Max(1, cell.Width / count);
            using (Pen splitPen = new Pen(Color.FromArgb(90, 90, 90)))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(25, 25, 25)))
            using (StringFormat center = new StringFormat())
            {
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;
                center.Trimming = StringTrimming.EllipsisCharacter;

                for (int i = 0; i < count; i++)
                {
                    int x = cell.Left + i * partWidth;
                    int w = (i == count - 1) ? cell.Right - x : partWidth;
                    Rectangle part = new Rectangle(x + 1, cell.Top + 1, Math.Max(1, w - 2), Math.Max(1, cell.Height - 2));
                    bool pastSlot = items[i].Appointment == null && IsPastSlot(items[i]);
                    Color fillColor = pastSlot ? Color.FromArgb(230, 232, 236) : items[i].Teacher.Color;
                    using (Brush brush = new SolidBrush(fillColor))
                    {
                        g.FillRectangle(brush, part);
                    }

                    string text;
                    if (items[i].Appointment == null)
                        text = items[i].Teacher.Name + (pastSlot ? "\n已過期" : "\n可預約");
                    else
                        text = items[i].Teacher.Name + "\n" + items[i].Appointment.Status + "：" + items[i].Appointment.StudentName;

                    g.DrawString(text, Font, textBrush, part, center);

                    if (items[i].Appointment != null)
                    {
                        Rectangle inner = new Rectangle(part.Left + 2, part.Top + 2, Math.Max(1, part.Width - 5), Math.Max(1, part.Height - 5));
                        using (Pen statusPen = new Pen(GetStatusBorderColor(items[i].Appointment.Status), 2f))
                        {
                            g.DrawRectangle(statusPen, inner);
                        }
                    }

                    if (i > 0)
                        g.DrawLine(splitPen, part.Left, cell.Top + 2, part.Left, cell.Bottom - 2);
                }
            }
        }

        private bool IsPastSlot(ScheduleCellItem item)
        {
            if (item == null)
                return false;
            TimeSpan start;
            if (!TryGetSlotStart(item.Slot, out start))
                return false;
            return item.Date.Date.Add(start) <= DateTime.Now;
        }

        private bool TryGetSlotStart(string slot, out TimeSpan start)
        {
            start = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(slot) || !slot.Contains("-"))
                return false;
            string[] parts = slot.Split('-');
            return parts.Length > 0 && TimeSpan.TryParse(parts[0], out start);
        }

        private List<ScheduleCellItem> BuildItemsForCell(int dayIndex, int slotIndex)
        {
            List<ScheduleCellItem> items = new List<ScheduleCellItem>();
            if (dayIndex < 0 || slotIndex < 0 || dayIndex >= DayItems.Length || slotIndex >= SlotItems.Length)
                return items;

            DateTime date = WeekStart.AddDays(dayIndex).Date;
            string day = DayItems[dayIndex];
            string slot = SlotItems[slotIndex];

            foreach (TeacherInfo teacher in Teachers)
            {
                if (!TeacherHasSlot(teacher, day, slot))
                    continue;

                AppointmentInfo appt = FindAppointment(teacher.TeacherId, date, slot);

                if (StatusFilter != "全部狀態")
                {
                    if (appt == null || appt.Status != StatusFilter)
                        continue;
                    if (BookingFilter == "只看未預約")
                        continue;
                }
                else
                {
                    if (BookingFilter == "只看已預約" && appt == null)
                        continue;
                    if (BookingFilter == "只看未預約" && appt != null)
                        continue;
                }

                ScheduleCellItem item = new ScheduleCellItem();
                item.Teacher = teacher;
                item.Appointment = appt;
                item.Date = date;
                item.Day = day;
                item.Slot = slot;
                items.Add(item);
            }
            return items;
        }

        private AppointmentInfo FindAppointment(int teacherId, DateTime date, string slot)
        {
            AppointmentInfo canceled = null;
            foreach (AppointmentInfo appt in Appointments)
            {
                if (appt.TeacherId == teacherId && appt.Date.Date == date.Date && appt.SlotText == slot)
                {
                    if (StatusFilter != "全部狀態")
                    {
                        if (appt.Status == StatusFilter)
                            return appt;
                    }
                    else
                    {
                        if (appt.Status != "已取消")
                            return appt;
                        if (canceled == null)
                            canceled = appt;
                    }
                }
            }

            if (StatusFilter == "已取消")
                return canceled;
            return null;
        }

        private Color GetStatusBorderColor(string status)
        {
            if (status == "已完成")
                return Color.FromArgb(45, 145, 85);
            if (status == "已取消")
                return Color.FromArgb(130, 135, 145);
            return Color.FromArgb(210, 95, 55);
        }

        private bool TeacherHasSlot(TeacherInfo teacher, string day, string slot)
        {
            if (teacher == null || string.IsNullOrWhiteSpace(teacher.AvailableTime))
                return false;

            string[] parts = teacher.AvailableTime.Split(new char[] { '；', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (!part.Contains(day))
                    continue;

                MatchCollection matches = Regex.Matches(part, @"\d{2}:\d{2}-\d{2}:\d{2}");
                foreach (Match match in matches)
                {
                    if (match.Value == slot)
                        return true;
                }
            }
            return false;
        }

        private bool HitTest(Point point, out int dayIndex, out int slotIndex, out Rectangle cell)
        {
            dayIndex = -1;
            slotIndex = -1;
            cell = Rectangle.Empty;

            int timeWidth = 95;
            int headerHeight = 54;
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
            int rowHeight = Math.Max(46, (gridHeight - headerHeight) / slotCount);

            int x = point.X - (gridX + timeWidth);
            int y = point.Y - (gridY + headerHeight);
            if (x < 0 || y < 0)
                return false;

            dayIndex = x / colWidth;
            slotIndex = y / rowHeight;
            if (dayIndex < 0 || dayIndex >= dayCount || slotIndex < 0 || slotIndex >= slotCount)
                return false;

            cell = new Rectangle(gridX + timeWidth + dayIndex * colWidth, gridY + headerHeight + slotIndex * rowHeight, colWidth, rowHeight);
            return true;
        }
    }
}
