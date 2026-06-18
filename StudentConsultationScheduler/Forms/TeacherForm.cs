using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class TeacherForm : Form
    {
        private DataGridView dgv;
        private TextBox txtName;
        private TextBox txtSubject;
        private TextBox txtOffice;
        private TextBox txtEmail;
        private Label lblPreview;
        private int currentId;
        private string availableTimeText = "";

        public TeacherForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "老師資料管理";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1160, 690);
            MinimumSize = new Size(1080, 650);
            BackColor = Color.White;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(layout);

            Panel left = new Panel();
            left.Dock = DockStyle.Fill;
            left.Margin = Padding.Empty;
            left.BackColor = Color.FromArgb(245, 247, 250);
            layout.Controls.Add(left, 0, 0);

            Panel right = new Panel();
            right.Dock = DockStyle.Fill;
            right.Margin = Padding.Empty;
            right.Padding = new Padding(14, 12, 14, 12);
            right.BackColor = Color.White;
            layout.Controls.Add(right, 1, 0);

            Label title = new Label();
            title.Text = "老師資料";
            title.Font = new Font("Microsoft JhengHei UI", 16, FontStyle.Bold);
            title.Location = new Point(20, 18);
            title.Size = new Size(320, 35);
            left.Controls.Add(title);

            Label hint = UiTheme.MakeHint("Enter 下一欄，可諮詢時間請用課表設定", 20, 52, 320);
            left.Controls.Add(hint);

            int y = 72;
            txtName = AddInput(left, "老師姓名 *", y); y += 54;
            txtSubject = AddInput(left, "科目", y); y += 54;
            txtOffice = AddInput(left, "辦公室", y); y += 54;
            txtEmail = AddInput(left, "Email", y); y += 54;

            Label avTitle = new Label();
            avTitle.Text = "可諮詢時間";
            avTitle.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            avTitle.Location = new Point(20, y);
            avTitle.Size = new Size(320, 22);
            left.Controls.Add(avTitle);
            y += 25;

            Button btnVisual = AddButton(left, "用課表設定可諮詢時間", 20, y, 323);
            btnVisual.BackColor = Color.FromArgb(235, 242, 255);
            btnVisual.Click += BtnVisual_Click;
            y += 45;

            lblPreview = new Label();
            lblPreview.Text = "已選擇：尚未設定";
            lblPreview.Font = new Font("Microsoft JhengHei UI", 9);
            lblPreview.ForeColor = Color.DimGray;
            lblPreview.Location = new Point(20, y);
            lblPreview.Size = new Size(323, 140);
            lblPreview.BorderStyle = BorderStyle.FixedSingle;
            lblPreview.BackColor = Color.White;
            left.Controls.Add(lblPreview);
            y += 155;

            Button btnAdd = AddButton(left, "新增", 20, y);
            Button btnUpdate = AddButton(left, "修改", 110, y);
            Button btnDelete = AddButton(left, "刪除", 200, y); y += 45;
            Button btnClear = AddButton(left, "清空欄位", 20, y, 323);

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += delegate { ClearInputs(); };

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.ScrollBars = ScrollBars.Both;
            dgv.Font = new Font("Microsoft JhengHei UI", 9);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            dgv.CellClick += Dgv_CellClick;
            right.Controls.Add(dgv);

            UiTheme.Apply(this);
            UiTheme.ConfigureEnterFlow(new Control[] { txtName, txtSubject, txtOffice, txtEmail }, btnVisual);

            Load += delegate { LoadTeachers(); UpdatePreview(); txtName.Focus(); };
        }

        private TextBox AddInput(Control parent, string label, int y)
        {
            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Microsoft JhengHei UI", 9);
            lbl.Location = new Point(20, y);
            lbl.Size = new Size(320, 20);
            parent.Controls.Add(lbl);

            TextBox txt = new TextBox();
            txt.Font = new Font("Microsoft JhengHei UI", 10);
            txt.Location = new Point(20, y + 22);
            txt.Size = new Size(323, 25);
            parent.Controls.Add(txt);
            return txt;
        }

        private Button AddButton(Control parent, string text, int x, int y)
        {
            return AddButton(parent, text, x, y, 75);
        }

        private Button AddButton(Control parent, string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 34);
            btn.BackColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            parent.Controls.Add(btn);
            return btn;
        }

        private void BtnVisual_Click(object sender, EventArgs e)
        {
            using (AvailabilityEditorForm form = new AvailabilityEditorForm(availableTimeText))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    availableTimeText = form.AvailableTimeText;
                    UpdatePreview();
                }
            }
        }

        private void LoadTeachers()
        {
            DataTable table = Db.GetDataTable("SELECT TeacherId, Name, Subject, Office, Email, AvailableTime FROM dbo.Teachers ORDER BY TeacherId DESC");
            dgv.DataSource = table;
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["TeacherId"].Visible = false;
                dgv.Columns["Name"].HeaderText = "老師姓名";
                dgv.Columns["Subject"].HeaderText = "科目";
                dgv.Columns["Office"].HeaderText = "辦公室";
                dgv.Columns["Email"].HeaderText = "Email";
                dgv.Columns["AvailableTime"].HeaderText = "可諮詢時段";

                dgv.Columns["Name"].FillWeight = 95;
                dgv.Columns["Subject"].FillWeight = 120;
                dgv.Columns["Office"].FillWeight = 80;
                dgv.Columns["Email"].FillWeight = 150;
                dgv.Columns["AvailableTime"].FillWeight = 250;

                dgv.Columns["Name"].MinimumWidth = 90;
                dgv.Columns["Subject"].MinimumWidth = 110;
                dgv.Columns["Office"].MinimumWidth = 80;
                dgv.Columns["Email"].MinimumWidth = 150;
                dgv.Columns["AvailableTime"].MinimumWidth = 260;
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.CurrentRow == null)
                return;

            currentId = Convert.ToInt32(dgv.CurrentRow.Cells["TeacherId"].Value);
            txtName.Text = Convert.ToString(dgv.CurrentRow.Cells["Name"].Value);
            txtSubject.Text = Convert.ToString(dgv.CurrentRow.Cells["Subject"].Value);
            txtOffice.Text = Convert.ToString(dgv.CurrentRow.Cells["Office"].Value);
            txtEmail.Text = Convert.ToString(dgv.CurrentRow.Cells["Email"].Value);
            availableTimeText = Convert.ToString(dgv.CurrentRow.Cells["AvailableTime"].Value);
            UpdatePreview();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                Db.ExecuteNonQuery(
                    "INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (@name, @subject, @office, @email, @available)",
                    Db.P("@name", txtName.Text.Trim()),
                    Db.P("@subject", txtSubject.Text.Trim()),
                    Db.P("@office", txtOffice.Text.Trim()),
                    Db.P("@email", txtEmail.Text.Trim()),
                    Db.P("@available", BuildAvailableTimeText()));
                LoadTeachers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("新增失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (currentId == 0)
            {
                MessageBox.Show("請先選擇要修改的老師。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput())
                return;

            try
            {
                Db.ExecuteNonQuery(
                    "UPDATE dbo.Teachers SET Name=@name, Subject=@subject, Office=@office, Email=@email, AvailableTime=@available WHERE TeacherId=@id",
                    Db.P("@name", txtName.Text.Trim()),
                    Db.P("@subject", txtSubject.Text.Trim()),
                    Db.P("@office", txtOffice.Text.Trim()),
                    Db.P("@email", txtEmail.Text.Trim()),
                    Db.P("@available", BuildAvailableTimeText()),
                    Db.P("@id", currentId));
                LoadTeachers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (currentId == 0)
            {
                MessageBox.Show("請先選擇要刪除的老師。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (Db.CountAppointmentsByTeacher(currentId) > 0)
            {
                MessageBox.Show("這位老師已有預約紀錄，請先刪除或修改相關預約後再刪除老師。", "無法刪除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("確定要刪除這位老師嗎？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Db.ExecuteNonQuery("DELETE FROM dbo.Teachers WHERE TeacherId=@id", Db.P("@id", currentId));
                LoadTeachers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("刪除失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePreview()
        {
            string text = BuildAvailableTimeText();
            lblPreview.Text = string.IsNullOrWhiteSpace(text) ? "已選擇：尚未設定\n\n請按「用課表設定可諮詢時間」設定。" : "已選擇：" + text;
        }

        private string BuildAvailableTimeText()
        {
            return availableTimeText == null ? "" : availableTimeText.Trim();
        }

        private bool ValidateInput()
        {
            if (txtName.Text.Trim().Length == 0)
            {
                MessageBox.Show("請輸入老師姓名。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(BuildAvailableTimeText()))
            {
                MessageBox.Show("請用可視化課表至少設定一個可諮詢時段。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private void ClearInputs()
        {
            currentId = 0;
            txtName.Clear();
            txtSubject.Clear();
            txtOffice.Clear();
            txtEmail.Clear();
            availableTimeText = "";
            UpdatePreview();
            txtName.Focus();
        }
    }
}
