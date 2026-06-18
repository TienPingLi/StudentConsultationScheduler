using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class AppointmentForm : Form
    {
        private readonly int appointmentId;
        private readonly int presetTeacherId;
        private readonly DateTime? presetDate;
        private readonly string presetTimeSlot;

        private ComboBox cmbStudent;
        private ComboBox cmbTeacher;
        private ComboBox cmbTopic;
        private ComboBox cmbTimeSlot;
        private ComboBox cmbStatus;
        private DateTimePicker dtpDate;
        private TextBox txtNote;
        private Label lblTeacherAvailable;

        private readonly string[] slotItems = new string[]
        {
            "09:00-10:00", "10:00-11:00", "11:00-12:00",
            "13:00-14:00", "14:00-15:00", "15:00-16:00",
            "16:00-17:00", "17:00-18:00", "18:00-19:00"
        };

        private readonly string[] weekDays = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };

        public AppointmentForm()
        {
            appointmentId = 0;
            InitializeUi();
        }

        public AppointmentForm(int id)
        {
            appointmentId = id;
            InitializeUi();
        }

        public AppointmentForm(int teacherId, DateTime date, string timeSlot)
        {
            appointmentId = 0;
            presetTeacherId = teacherId;
            presetDate = date.Date;
            presetTimeSlot = timeSlot;
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = appointmentId == 0 ? "新增諮詢預約" : "修改諮詢預約";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(560, 625);
            BackColor = Color.White;

            Label title = new Label();
            title.Text = appointmentId == 0 ? "新增諮詢預約" : "修改諮詢預約";
            title.Font = new Font("Microsoft JhengHei UI", 16, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(45, 70, 120);
            title.Location = new Point(25, 18);
            title.Size = new Size(490, 35);
            Controls.Add(title);

            Label enterHint = UiTheme.MakeHint("Enter 下一欄；Ctrl+Enter 可在備註換行", 28, 50, 490);
            Controls.Add(enterHint);

            int y = 75;
            cmbStudent = AddCombo("學生", y); y += 52;
            cmbTeacher = AddCombo("老師", y); y += 52;

            lblTeacherAvailable = new Label();
            lblTeacherAvailable.Text = "可諮詢時段：請先選擇老師";
            lblTeacherAvailable.Font = new Font("Microsoft JhengHei UI", 9);
            lblTeacherAvailable.ForeColor = Color.DimGray;
            lblTeacherAvailable.Location = new Point(150, y - 20);
            lblTeacherAvailable.Size = new Size(370, 48);
            Controls.Add(lblTeacherAvailable);

            cmbTopic = AddCombo("諮詢主題", y + 30); y += 82;
            dtpDate = AddDatePicker("日期", y); y += 52;
            cmbTimeSlot = AddCombo("諮詢時段", y); y += 52;
            cmbStatus = AddCombo("狀態", y); y += 52;

            Label lblNote = AddLabel("備註", y);
            txtNote = new TextBox();
            txtNote.Font = new Font("Microsoft JhengHei UI", 10);
            txtNote.Location = new Point(150, y);
            txtNote.Size = new Size(360, 80);
            txtNote.Multiline = true;
            txtNote.ScrollBars = ScrollBars.Vertical;
            Controls.Add(txtNote);
            y += 105;

            Label hint = new Label();
            hint.Text = "注意：只能預約現在之後的整點時段；同一學生同時段不可預約兩位老師。";
            hint.Font = new Font("Microsoft JhengHei UI", 9);
            hint.ForeColor = Color.FromArgb(180, 80, 40);
            hint.Location = new Point(150, y - 12);
            hint.Size = new Size(370, 25);
            Controls.Add(hint);
            y += 25;

            Button btnSave = new Button();
            btnSave.Text = "確定";
            btnSave.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            btnSave.BackColor = Color.FromArgb(45, 105, 180);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Location = new Point(150, y);
            btnSave.Size = new Size(160, 40);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            Button btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            btnCancel.BackColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(350, y);
            btnCancel.Size = new Size(160, 40);
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);

            cmbTeacher.SelectedIndexChanged += CmbTeacher_SelectedIndexChanged;
            dtpDate.ValueChanged += DtpDate_ValueChanged;
            AcceptButton = btnSave;
            CancelButton = btnCancel;
            UiTheme.Apply(this);
            UiTheme.ConfigureEnterFlow(new Control[] { cmbStudent, cmbTeacher, cmbTopic, dtpDate, cmbTimeSlot, cmbStatus, txtNote }, btnSave);

            Load += AppointmentForm_Load;
        }

        private Label AddLabel(string text, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Microsoft JhengHei UI", 10);
            lbl.Location = new Point(35, y + 4);
            lbl.Size = new Size(105, 25);
            Controls.Add(lbl);
            return lbl;
        }

        private ComboBox AddCombo(string label, int y)
        {
            AddLabel(label, y);
            ComboBox combo = new ComboBox();
            combo.Font = new Font("Microsoft JhengHei UI", 10);
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Location = new Point(150, y);
            combo.Size = new Size(360, 25);
            Controls.Add(combo);
            return combo;
        }

        private DateTimePicker AddDatePicker(string label, int y)
        {
            AddLabel(label, y);
            DateTimePicker picker = new DateTimePicker();
            picker.Font = new Font("Microsoft JhengHei UI", 10);
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "yyyy-MM-dd";
            picker.Location = new Point(150, y);
            picker.Size = new Size(360, 25);
            if (appointmentId == 0)
                picker.MinDate = DateTime.Today;
            Controls.Add(picker);
            return picker;
        }

        private void AppointmentForm_Load(object sender, EventArgs e)
        {
            LoadCombos();

            dtpDate.Value = DateTime.Today;
            if (cmbTimeSlot.Items.Count > 0)
                cmbTimeSlot.SelectedIndex = 0;

            if (appointmentId > 0)
                LoadAppointment();
            else
                ApplyPresetValues();

            RefreshTeacherAvailableLabel();
        }

        private void ApplyPresetValues()
        {
            if (presetDate.HasValue)
                dtpDate.Value = presetDate.Value;

            if (presetTeacherId > 0)
                cmbTeacher.SelectedValue = presetTeacherId;

            if (!string.IsNullOrWhiteSpace(presetTimeSlot))
            {
                int index = cmbTimeSlot.Items.IndexOf(presetTimeSlot);
                if (index >= 0)
                    cmbTimeSlot.SelectedIndex = index;
            }
        }

        private void LoadCombos()
        {
            DataTable students = Db.GetDataTable("SELECT StudentId, StudentNo + N' - ' + Name AS DisplayName FROM dbo.Students ORDER BY StudentNo");
            cmbStudent.DataSource = students;
            cmbStudent.DisplayMember = "DisplayName";
            cmbStudent.ValueMember = "StudentId";

            DataTable teachers = Db.GetDataTable("SELECT TeacherId, Name + N' / ' + ISNULL(Subject, N'') AS DisplayName, ISNULL(AvailableTime, N'') AS AvailableTime FROM dbo.Teachers ORDER BY Name");
            cmbTeacher.DataSource = teachers;
            cmbTeacher.DisplayMember = "DisplayName";
            cmbTeacher.ValueMember = "TeacherId";

            cmbTopic.Items.Clear();
            cmbTopic.Items.Add("作業問題");
            cmbTopic.Items.Add("考試複習");
            cmbTopic.Items.Add("專題討論");
            cmbTopic.Items.Add("程式除錯");
            cmbTopic.Items.Add("課程諮詢");
            cmbTopic.Items.Add("其他");
            if (cmbTopic.Items.Count > 0)
                cmbTopic.SelectedIndex = 0;

            cmbTimeSlot.Items.Clear();
            foreach (string slot in slotItems)
                cmbTimeSlot.Items.Add(slot);
            if (cmbTimeSlot.Items.Count > 0)
                cmbTimeSlot.SelectedIndex = 0;

            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("已預約");
            cmbStatus.Items.Add("已完成");
            cmbStatus.Items.Add("已取消");
            if (cmbStatus.Items.Count > 0)
                cmbStatus.SelectedIndex = 0;
        }

        private void LoadAppointment()
        {
            DataTable table = Db.GetDataTable(@"
SELECT AppointmentId, StudentId, TeacherId, Topic, AppointmentDate, StartTime, EndTime, Note, Status
FROM dbo.Appointments
WHERE AppointmentId = @id", Db.P("@id", appointmentId));

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("找不到預約資料。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            DataRow row = table.Rows[0];
            cmbStudent.SelectedValue = Convert.ToInt32(row["StudentId"]);
            cmbTeacher.SelectedValue = Convert.ToInt32(row["TeacherId"]);
            cmbTopic.Text = Convert.ToString(row["Topic"]);
            dtpDate.Value = Convert.ToDateTime(row["AppointmentDate"]);

            TimeSpan start = (TimeSpan)row["StartTime"];
            TimeSpan end = (TimeSpan)row["EndTime"];
            SelectMatchingTimeSlot(start, end);

            txtNote.Text = Convert.ToString(row["Note"]);
            cmbStatus.Text = Convert.ToString(row["Status"]);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            int studentId = Convert.ToInt32(cmbStudent.SelectedValue);
            int teacherId = Convert.ToInt32(cmbTeacher.SelectedValue);
            DateTime date = dtpDate.Value.Date;
            TimeSpan start;
            TimeSpan end;
            GetSelectedSlot(out start, out end);
            string topic = cmbTopic.Text.Trim();
            string status = cmbStatus.Text.Trim();
            string note = txtNote.Text.Trim();

            if (status != "已取消" && Db.HasStudentTimeConflict(studentId, date, start, end, appointmentId))
            {
                MessageBox.Show("這位學生在同一天同一時段已經有其他老師的預約，不能重複預約。", "學生時間衝突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (status != "已取消" && Db.HasAppointmentConflict(teacherId, date, start, end, appointmentId))
            {
                MessageBox.Show("此老師在這個時段已經有預約，請選擇其他時間。", "時間衝突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (appointmentId == 0)
                {
                    Db.ExecuteNonQuery(@"
INSERT INTO dbo.Appointments (StudentId, TeacherId, Topic, AppointmentDate, StartTime, EndTime, Note, Status)
VALUES (@studentId, @teacherId, @topic, @date, @start, @end, @note, @status)",
                        Db.P("@studentId", studentId),
                        Db.P("@teacherId", teacherId),
                        Db.P("@topic", topic),
                        Db.P("@date", date),
                        Db.P("@start", start),
                        Db.P("@end", end),
                        Db.P("@note", note),
                        Db.P("@status", status));
                }
                else
                {
                    Db.ExecuteNonQuery(@"
UPDATE dbo.Appointments
SET StudentId=@studentId, TeacherId=@teacherId, Topic=@topic, AppointmentDate=@date,
    StartTime=@start, EndTime=@end, Note=@note, Status=@status
WHERE AppointmentId=@id",
                        Db.P("@studentId", studentId),
                        Db.P("@teacherId", teacherId),
                        Db.P("@topic", topic),
                        Db.P("@date", date),
                        Db.P("@start", start),
                        Db.P("@end", end),
                        Db.P("@note", note),
                        Db.P("@status", status),
                        Db.P("@id", appointmentId));
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("儲存失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbTeacher_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshTeacherAvailableLabel();
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            RefreshTeacherAvailableLabel();
        }

        private void RefreshTeacherAvailableLabel()
        {
            if (lblTeacherAvailable == null)
                return;
            string available = GetSelectedTeacherAvailableTime();
            if (string.IsNullOrWhiteSpace(available))
                lblTeacherAvailable.Text = "可諮詢時段：未設定，請先到老師管理用課表設定整點時段";
            else
                lblTeacherAvailable.Text = "可諮詢時段：" + available;
        }

        private string GetSelectedTeacherAvailableTime()
        {
            DataRowView rowView = cmbTeacher.SelectedItem as DataRowView;
            if (rowView != null && rowView.Row.Table.Columns.Contains("AvailableTime"))
                return Convert.ToString(rowView["AvailableTime"]);
            return "";
        }

        private void SelectMatchingTimeSlot(TimeSpan start, TimeSpan end)
        {
            string text = start.ToString(@"hh\:mm") + "-" + end.ToString(@"hh\:mm");
            int index = cmbTimeSlot.Items.IndexOf(text);
            cmbTimeSlot.SelectedIndex = index >= 0 ? index : 0;
        }

        private bool GetSelectedSlot(out TimeSpan start, out TimeSpan end)
        {
            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            if (cmbTimeSlot.SelectedItem == null)
                return false;

            string slot = Convert.ToString(cmbTimeSlot.SelectedItem);
            return TryParseSlot(slot, out start, out end);
        }

        private bool TryParseSlot(string slot, out TimeSpan start, out TimeSpan end)
        {
            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(slot) || !slot.Contains("-"))
                return false;

            string[] parts = slot.Split('-');
            if (parts.Length != 2)
                return false;

            return TimeSpan.TryParse(parts[0], out start) && TimeSpan.TryParse(parts[1], out end);
        }

        private bool ValidateInput()
        {
            if (cmbStudent.Items.Count == 0)
            {
                MessageBox.Show("請先建立學生資料。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (cmbTeacher.Items.Count == 0)
            {
                MessageBox.Show("請先建立老師資料。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (cmbStudent.SelectedValue == null || cmbTeacher.SelectedValue == null)
            {
                MessageBox.Show("請選擇學生與老師。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (cmbTopic.Text.Trim().Length == 0)
            {
                MessageBox.Show("請選擇諮詢主題。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            TimeSpan start;
            TimeSpan end;
            if (!GetSelectedSlot(out start, out end))
            {
                MessageBox.Show("請選擇整點諮詢時段。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (start.Minutes != 0 || end.Minutes != 0 || start.Seconds != 0 || end.Seconds != 0)
            {
                MessageBox.Show("諮詢時間只能是整點，例如 09:00-10:00。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string currentStatus = cmbStatus.Text.Trim();
            DateTime appointmentStart = dtpDate.Value.Date.Add(start);
            if ((appointmentId == 0 || currentStatus == "已預約") && appointmentStart <= DateTime.Now)
            {
                MessageBox.Show("不可預約今天以前或已經開始的時段，請選擇現在之後的時間。", "不可預約過去時間", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 取消狀態不需要檢查老師可諮詢時間，避免修改舊資料時被擋住。
            if (currentStatus != "已取消")
            {
                string available = GetSelectedTeacherAvailableTime();
                string selectedDay = GetChineseWeekday(dtpDate.Value.Date);
                string selectedSlot = Convert.ToString(cmbTimeSlot.SelectedItem);
                if (!IsTeacherAvailable(available, selectedDay, selectedSlot))
                {
                    MessageBox.Show("這位老師在「" + selectedDay + " " + selectedSlot + "」沒有開放諮詢，請改選老師或時段。", "老師未開放此時段", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private string GetChineseWeekday(DateTime date)
        {
            return weekDays[(int)date.DayOfWeek];
        }

        private bool IsTeacherAvailable(string available, string day, string slot)
        {
            if (string.IsNullOrWhiteSpace(available))
                return false;

            string[] parts = available.Split(new char[] { '；', ';' }, StringSplitOptions.RemoveEmptyEntries);
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
    }
}
