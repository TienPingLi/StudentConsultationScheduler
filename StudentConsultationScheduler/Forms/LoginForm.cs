using System;
using System.Drawing;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblMessage;
        private Button btnLogin;

        public LoginForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "學生諮詢預約系統 - 登入";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(420, 300);
            BackColor = Color.White;

            Label lblTitle = new Label();
            lblTitle.Text = "學生諮詢預約系統";
            lblTitle.Font = new Font("Microsoft JhengHei UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(45, 70, 120);
            lblTitle.AutoSize = false;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Location = new Point(0, 25);
            lblTitle.Size = new Size(ClientSize.Width, 45);
            Controls.Add(lblTitle);

            Label lblSubTitle = new Label();
            lblSubTitle.Text = "Student Consultation Appointment System";
            lblSubTitle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblSubTitle.ForeColor = Color.DimGray;
            lblSubTitle.AutoSize = false;
            lblSubTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubTitle.Location = new Point(0, 68);
            lblSubTitle.Size = new Size(ClientSize.Width, 25);
            Controls.Add(lblSubTitle);

            Label lblUser = new Label();
            lblUser.Text = "帳號";
            lblUser.Font = new Font("Microsoft JhengHei UI", 10);
            lblUser.Location = new Point(75, 115);
            lblUser.Size = new Size(70, 25);
            Controls.Add(lblUser);

            txtUsername = new TextBox();
            txtUsername.Font = new Font("Microsoft JhengHei UI", 10);
            txtUsername.Location = new Point(150, 112);
            txtUsername.Size = new Size(190, 25);
            txtUsername.Text = "admin";
            Controls.Add(txtUsername);

            Label lblPass = new Label();
            lblPass.Text = "密碼";
            lblPass.Font = new Font("Microsoft JhengHei UI", 10);
            lblPass.Location = new Point(75, 155);
            lblPass.Size = new Size(70, 25);
            Controls.Add(lblPass);

            txtPassword = new TextBox();
            txtPassword.Font = new Font("Microsoft JhengHei UI", 10);
            txtPassword.Location = new Point(150, 152);
            txtPassword.Size = new Size(190, 25);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Text = "1234";
            txtPassword.KeyDown += TxtPassword_KeyDown;
            Controls.Add(txtPassword);

            btnLogin = new Button();
            btnLogin.Text = "登入";
            btnLogin.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            btnLogin.Location = new Point(150, 198);
            btnLogin.Size = new Size(190, 35);
            btnLogin.BackColor = Color.FromArgb(45, 105, 180);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Click += BtnLogin_Click;
            Controls.Add(btnLogin);

            lblMessage = new Label();
            lblMessage.Text = "預設帳號：admin / 密碼：1234";
            lblMessage.Font = new Font("Microsoft JhengHei UI", 9);
            lblMessage.ForeColor = Color.Gray;
            lblMessage.AutoSize = false;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Location = new Point(0, 245);
            lblMessage.Size = new Size(ClientSize.Width, 28);
            Controls.Add(lblMessage);

            AcceptButton = btnLogin;
            UiTheme.Apply(this);
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(sender, EventArgs.Empty);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                lblMessage.ForeColor = Color.Firebrick;
                lblMessage.Text = "請輸入帳號與密碼。";
                return;
            }

            try
            {
                if (Db.ValidateLogin(username, password))
                {
                    Db.WriteLoginLog(username);
                    Hide();
                    using (MainForm mainForm = new MainForm(username))
                    {
                        mainForm.ShowDialog();
                    }
                    Close();
                }
                else
                {
                    lblMessage.ForeColor = Color.Firebrick;
                    lblMessage.Text = "帳號或密碼錯誤。";
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登入時發生錯誤：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
