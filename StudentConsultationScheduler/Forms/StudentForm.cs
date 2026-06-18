using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class StudentForm : Form
    {
        private DataGridView dgv;
        private TextBox txtStudentNo;
        private TextBox txtName;
        private TextBox txtClassName;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private Label lblMode;
        private int currentId;

        public StudentForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "學生管理";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1120, 660);
            MinimumSize = new Size(1040, 620);
            BackColor = UiTheme.Background;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Padding = new Padding(16);
            Controls.Add(layout);

            Panel leftCard = MakeCard();
            leftCard.Dock = DockStyle.Fill;
            leftCard.Padding = new Padding(20);
            layout.Controls.Add(leftCard, 0, 0);

            Panel rightCard = MakeCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Padding = new Padding(18);
            layout.Controls.Add(rightCard, 1, 0);

            Label title = new Label();
            title.Text = "學生資料";
            title.Font = new Font("Microsoft JhengHei UI", 18, FontStyle.Bold);
            title.ForeColor = UiTheme.PrimaryDark;
            title.Location = new Point(22, 18);
            title.Size = new Size(250, 36);
            leftCard.Controls.Add(title);

            Label hint = UiTheme.MakeHint("填完一格按 Enter 到下一格，Shift+Enter 回上一格", 24, 56, 252);
            leftCard.Controls.Add(hint);

            lblMode = new Label();
            lblMode.Text = "目前模式：新增學生";
            lblMode.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
            lblMode.ForeColor = Color.FromArgb(70, 105, 160);
            lblMode.BackColor = Color.FromArgb(232, 241, 255);
            lblMode.TextAlign = ContentAlignment.MiddleLeft;
            lblMode.Location = new Point(22, 84);
            lblMode.Size = new Size(252, 30);
            lblMode.Padding = new Padding(8, 0, 0, 0);
            leftCard.Controls.Add(lblMode);

            int y = 132;
            txtStudentNo = AddInput(leftCard, "學號 *", y, "例如：1131427"); y += 66;
            txtName = AddInput(leftCard, "姓名 *", y, "例如：田秉立"); y += 66;
            txtClassName = AddInput(leftCard, "班級", y, "例如：資工二A"); y += 66;
            txtPhone = AddInput(leftCard, "電話", y, "例如：0912-345-678"); y += 66;
            txtEmail = AddInput(leftCard, "Email", y, "例如：student@example.com"); y += 74;

            Button btnAdd = AddButton(leftCard, "新增", 22, y, 76);
            Button btnUpdate = AddButton(leftCard, "修改", 110, y, 76);
            Button btnDelete = AddButton(leftCard, "刪除", 198, y, 76); y += 48;
            Button btnClear = AddButton(leftCard, "清空欄位", 22, y, 252);

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += delegate { ClearInputs(); };

            TableLayoutPanel rightLayout = new TableLayoutPanel();
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.RowCount = 2;
            rightLayout.ColumnCount = 1;
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rightLayout.Margin = Padding.Empty;
            rightCard.Controls.Add(rightLayout);

            Panel listHeader = new Panel();
            listHeader.Dock = DockStyle.Fill;
            listHeader.BackColor = Color.White;
            rightLayout.Controls.Add(listHeader, 0, 0);

            Label listTitle = new Label();
            listTitle.Text = "學生清單";
            listTitle.Font = new Font("Microsoft JhengHei UI", 14, FontStyle.Bold);
            listTitle.ForeColor = UiTheme.PrimaryDark;
            listTitle.Location = new Point(0, 4);
            listTitle.Size = new Size(160, 30);
            listHeader.Controls.Add(listTitle);

            Label listHint = UiTheme.MakeHint("單擊資料列可帶入左側欄位，方便修改或刪除", 150, 9, 360);
            listHeader.Controls.Add(listHint);

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoGenerateColumns = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.ScrollBars = ScrollBars.Both;
            dgv.CellClick += Dgv_CellClick;
            AddStudentColumns();
            rightLayout.Controls.Add(dgv, 0, 1);

            UiTheme.Apply(this);
            UiTheme.ConfigureEnterFlow(new Control[] { txtStudentNo, txtName, txtClassName, txtPhone, txtEmail }, btnAdd);

            Load += delegate { LoadStudents(); ClearInputs(); };
            Shown += delegate { txtStudentNo.Focus(); };
        }

        private Panel MakeCard()
        {
            Panel panel = new Panel();
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Margin = new Padding(0, 0, 14, 0);
            return panel;
        }

        private void AddStudentColumns()
        {
            dgv.Columns.Clear();
            AddTextColumn("StudentId", "StudentId", "ID", 40, 0, false);
            AddTextColumn("StudentNo", "StudentNo", "學號", 115, 105, true);
            AddTextColumn("Name", "Name", "姓名", 120, 115, true);
            AddTextColumn("ClassName", "ClassName", "班級", 110, 100, true);
            AddTextColumn("Phone", "Phone", "電話", 135, 120, true);
            AddTextColumn("Email", "Email", "Email", 220, 185, true);
        }

        private void AddTextColumn(string name, string dataPropertyName, string headerText, int minWidth, float fillWeight, bool visible)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = name;
            col.DataPropertyName = dataPropertyName;
            col.HeaderText = headerText;
            col.MinimumWidth = minWidth;
            col.FillWeight = fillWeight <= 0 ? 1 : fillWeight;
            col.Visible = visible;
            col.SortMode = DataGridViewColumnSortMode.Automatic;
            dgv.Columns.Add(col);
        }

        private TextBox AddInput(Control parent, string label, int y, string placeholder)
        {
            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
            lbl.ForeColor = UiTheme.Text;
            lbl.Location = new Point(22, y);
            lbl.Size = new Size(252, 20);
            parent.Controls.Add(lbl);

            TextBox txt = new TextBox();
            txt.Font = new Font("Microsoft JhengHei UI", 10.5f);
            txt.Location = new Point(22, y + 24);
            txt.Size = new Size(252, 28);
            txt.Tag = placeholder;
            parent.Controls.Add(txt);
            return txt;
        }

        private Button AddButton(Control parent, string text, int x, int y, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Microsoft JhengHei UI", 9.5f, FontStyle.Bold);
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 38);
            btn.FlatStyle = FlatStyle.Flat;
            parent.Controls.Add(btn);
            return btn;
        }

        private void LoadStudents()
        {
            DataTable table = Db.GetDataTable("SELECT StudentId, StudentNo, Name, ClassName, Phone, Email FROM dbo.Students ORDER BY StudentId DESC");
            dgv.DataSource = table;
            dgv.RowHeadersVisible = false;
            if (dgv.Columns.Contains("StudentId"))
                dgv.Columns["StudentId"].Visible = false;
            if (dgv.Columns.Contains("StudentNo"))
                dgv.FirstDisplayedScrollingColumnIndex = dgv.Columns["StudentNo"].Index;
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.CurrentRow == null)
                return;

            currentId = Convert.ToInt32(dgv.CurrentRow.Cells["StudentId"].Value);
            txtStudentNo.Text = Convert.ToString(dgv.CurrentRow.Cells["StudentNo"].Value);
            txtName.Text = Convert.ToString(dgv.CurrentRow.Cells["Name"].Value);
            txtClassName.Text = Convert.ToString(dgv.CurrentRow.Cells["ClassName"].Value);
            txtPhone.Text = Convert.ToString(dgv.CurrentRow.Cells["Phone"].Value);
            txtEmail.Text = Convert.ToString(dgv.CurrentRow.Cells["Email"].Value);
            lblMode.Text = "目前模式：修改學生";
            txtStudentNo.Focus();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                Db.ExecuteNonQuery(
                    "INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (@no, @name, @class, @phone, @email)",
                    Db.P("@no", txtStudentNo.Text.Trim()),
                    Db.P("@name", txtName.Text.Trim()),
                    Db.P("@class", txtClassName.Text.Trim()),
                    Db.P("@phone", txtPhone.Text.Trim()),
                    Db.P("@email", txtEmail.Text.Trim()));
                LoadStudents();
                ClearInputs();
                MessageBox.Show("學生資料已新增。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("請先選擇要修改的學生。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput())
                return;

            try
            {
                Db.ExecuteNonQuery(
                    "UPDATE dbo.Students SET StudentNo=@no, Name=@name, ClassName=@class, Phone=@phone, Email=@email WHERE StudentId=@id",
                    Db.P("@no", txtStudentNo.Text.Trim()),
                    Db.P("@name", txtName.Text.Trim()),
                    Db.P("@class", txtClassName.Text.Trim()),
                    Db.P("@phone", txtPhone.Text.Trim()),
                    Db.P("@email", txtEmail.Text.Trim()),
                    Db.P("@id", currentId));
                LoadStudents();
                ClearInputs();
                MessageBox.Show("學生資料已修改。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("請先選擇要刪除的學生。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (Db.CountAppointmentsByStudent(currentId) > 0)
            {
                MessageBox.Show("這位學生已有預約紀錄，請先刪除或修改相關預約後再刪除學生。", "無法刪除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("確定要刪除這位學生嗎？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Db.ExecuteNonQuery("DELETE FROM dbo.Students WHERE StudentId=@id", Db.P("@id", currentId));
                LoadStudents();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("刪除失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (txtStudentNo.Text.Trim().Length == 0)
            {
                MessageBox.Show("請輸入學號。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtStudentNo.Focus();
                return false;
            }
            if (txtName.Text.Trim().Length == 0)
            {
                MessageBox.Show("請輸入學生姓名。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Focus();
                return false;
            }
            if (Db.StudentNoExists(txtStudentNo.Text.Trim(), currentId))
            {
                MessageBox.Show("這個學號已經存在，不能建立重複學號。", "學號重複", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentNo.Focus();
                return false;
            }
            return true;
        }

        private void ClearInputs()
        {
            currentId = 0;
            txtStudentNo.Clear();
            txtName.Clear();
            txtClassName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            lblMode.Text = "目前模式：新增學生";
            txtStudentNo.Focus();
        }
    }
}
