using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class AppointmentDetailForm : Form
    {
        private readonly int appointmentId;
        private Label lblTitle;
        private Label lblStatusBadge;
        private TableLayoutPanel detailTable;

        public AppointmentDetailForm(int id)
        {
            appointmentId = id;
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "預約詳細資訊";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(620, 560);
            BackColor = Color.FromArgb(248, 250, 253);

            Panel header = new Panel();
            header.BackColor = Color.FromArgb(31, 63, 109);
            header.Dock = DockStyle.Top;
            header.Height = 78;
            Controls.Add(header);

            lblTitle = new Label();
            lblTitle.Text = "預約詳細資訊";
            lblTitle.Font = new Font("Microsoft JhengHei UI", 17, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(22, 15);
            lblTitle.Size = new Size(370, 34);
            header.Controls.Add(lblTitle);

            Label sub = new Label();
            sub.Text = "查看學生、老師、科目、時間、主題、備註與預約狀態";
            sub.Font = new Font("Microsoft JhengHei UI", 9);
            sub.ForeColor = Color.FromArgb(222, 232, 245);
            sub.Location = new Point(24, 50);
            sub.Size = new Size(470, 20);
            header.Controls.Add(sub);

            lblStatusBadge = new Label();
            lblStatusBadge.TextAlign = ContentAlignment.MiddleCenter;
            lblStatusBadge.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            lblStatusBadge.ForeColor = Color.White;
            lblStatusBadge.Location = new Point(485, 23);
            lblStatusBadge.Size = new Size(105, 32);
            header.Controls.Add(lblStatusBadge);

            Panel card = new Panel();
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Location = new Point(20, 96);
            card.Size = new Size(580, 342);
            card.Padding = new Padding(12);
            Controls.Add(card);

            detailTable = new TableLayoutPanel();
            detailTable.Dock = DockStyle.Fill;
            detailTable.ColumnCount = 2;
            detailTable.RowCount = 12;
            detailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            detailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 12; i++)
                detailTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            card.Controls.Add(detailTable);

            Button btnEdit = MakeButton("修改預約", 20, 458, 120, true);
            Button btnComplete = MakeButton("標記已完成", 150, 458, 120, false);
            Button btnCancelAppointment = MakeButton("取消預約", 280, 458, 120, false);
            Button btnClose = MakeButton("關閉", 480, 458, 120, false);
            Controls.Add(btnEdit);
            Controls.Add(btnComplete);
            Controls.Add(btnCancelAppointment);
            Controls.Add(btnClose);

            Button btnRebook = MakeButton("恢復已預約", 20, 505, 120, false);
            Controls.Add(btnRebook);

            btnEdit.Click += BtnEdit_Click;
            btnComplete.Click += delegate { UpdateStatus("已完成"); };
            btnCancelAppointment.Click += delegate { UpdateStatus("已取消"); };
            btnRebook.Click += delegate { UpdateStatus("已預約"); };
            btnClose.Click += delegate { Close(); };

            CancelButton = btnClose;
            UiTheme.Apply(this);

            Load += delegate { LoadDetail(); };
        }

        private Button MakeButton(string text, int x, int y, int width, bool primary)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            button.Location = new Point(x, y);
            button.Size = new Size(width, 36);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = primary ? Color.FromArgb(31, 92, 165) : Color.FromArgb(200, 207, 220);
            button.BackColor = primary ? Color.FromArgb(36, 105, 190) : Color.White;
            button.ForeColor = primary ? Color.White : Color.FromArgb(35, 45, 60);
            return button;
        }

        private void LoadDetail()
        {
            DataTable table = Db.GetDataTable(@"
SELECT
    a.AppointmentId,
    s.Name AS StudentName,
    s.StudentNo,
    ISNULL(s.ClassName, N'') AS ClassName,
    ISNULL(s.Phone, N'') AS StudentPhone,
    ISNULL(s.Email, N'') AS StudentEmail,
    t.Name AS TeacherName,
    ISNULL(t.Subject, N'') AS Subject,
    ISNULL(t.Office, N'') AS Office,
    ISNULL(t.Email, N'') AS TeacherEmail,
    a.Topic,
    CONVERT(VARCHAR(10), a.AppointmentDate, 120) AS AppointmentDate,
    CONVERT(VARCHAR(5), a.StartTime, 108) AS StartTime,
    CONVERT(VARCHAR(5), a.EndTime, 108) AS EndTime,
    a.Status,
    ISNULL(a.Note, N'') AS Note
FROM dbo.Appointments a
INNER JOIN dbo.Students s ON a.StudentId = s.StudentId
INNER JOIN dbo.Teachers t ON a.TeacherId = t.TeacherId
WHERE a.AppointmentId = @id", Db.P("@id", appointmentId));

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("找不到這筆預約資料。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            DataRow row = table.Rows[0];
            string status = Convert.ToString(row["Status"]);
            lblStatusBadge.Text = status;
            SetStatusBadgeColor(status);
            lblTitle.Text = Convert.ToString(row["TeacherName"]) + " / " + Convert.ToString(row["StudentName"]);

            detailTable.Controls.Clear();
            AddRow(0, "學生", Convert.ToString(row["StudentName"]) + "（" + Convert.ToString(row["StudentNo"]) + "）");
            AddRow(1, "班級", Convert.ToString(row["ClassName"]));
            AddRow(2, "學生電話", Convert.ToString(row["StudentPhone"]));
            AddRow(3, "學生 Email", Convert.ToString(row["StudentEmail"]));
            AddRow(4, "老師", Convert.ToString(row["TeacherName"]));
            AddRow(5, "科目", EmptyToDash(Convert.ToString(row["Subject"])));
            AddRow(6, "辦公室", EmptyToDash(Convert.ToString(row["Office"])));
            AddRow(7, "老師 Email", EmptyToDash(Convert.ToString(row["TeacherEmail"])));
            AddRow(8, "日期", Convert.ToString(row["AppointmentDate"]));
            AddRow(9, "時間", Convert.ToString(row["StartTime"]) + "-" + Convert.ToString(row["EndTime"]));
            AddRow(10, "主題", Convert.ToString(row["Topic"]));
            AddRow(11, "備註", EmptyToDash(Convert.ToString(row["Note"])));
        }

        private void AddRow(int rowIndex, string name, string value)
        {
            Label lblName = new Label();
            lblName.Text = name;
            lblName.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(31, 63, 109);
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            lblName.Dock = DockStyle.Fill;

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Microsoft JhengHei UI", 10);
            lblValue.ForeColor = Color.FromArgb(35, 40, 50);
            lblValue.TextAlign = ContentAlignment.MiddleLeft;
            lblValue.Dock = DockStyle.Fill;
            lblValue.AutoEllipsis = true;

            detailTable.Controls.Add(lblName, 0, rowIndex);
            detailTable.Controls.Add(lblValue, 1, rowIndex);
        }

        private string EmptyToDash(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "—" : text.Trim();
        }

        private void SetStatusBadgeColor(string status)
        {
            if (status == "已完成")
                lblStatusBadge.BackColor = Color.FromArgb(45, 145, 85);
            else if (status == "已取消")
                lblStatusBadge.BackColor = Color.FromArgb(140, 145, 155);
            else
                lblStatusBadge.BackColor = Color.FromArgb(220, 125, 45);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            using (AppointmentForm form = new AppointmentForm(appointmentId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    DialogResult = DialogResult.OK;
                    LoadDetail();
                }
            }
        }

        private bool IsAppointmentStartInPast()
        {
            try
            {
                object result = Db.ExecuteScalar(@"
SELECT COUNT(*)
FROM dbo.Appointments
WHERE AppointmentId = @id
  AND DATEADD(SECOND, DATEDIFF(SECOND, CAST('00:00:00' AS TIME), StartTime), CAST(AppointmentDate AS DATETIME)) <= GETDATE()", Db.P("@id", appointmentId));
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateStatus(string status)
        {
            if (status == "已預約" && IsAppointmentStartInPast())
            {
                MessageBox.Show("此預約時間已經過去，不能恢復成「已預約」。", "不可恢復過去預約", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("確定要將此預約狀態改為「" + status + "」嗎？", "確認狀態變更", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Db.ExecuteNonQuery("UPDATE dbo.Appointments SET Status=@status WHERE AppointmentId=@id", Db.P("@status", status), Db.P("@id", appointmentId));
                DialogResult = DialogResult.OK;
                LoadDetail();
            }
            catch (Exception ex)
            {
                MessageBox.Show("狀態更新失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
