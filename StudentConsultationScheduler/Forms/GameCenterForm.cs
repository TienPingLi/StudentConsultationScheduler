using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentConsultationScheduler.Forms
{
    public class GameCenterForm : Form
    {
        public GameCenterForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "小遊戲中心";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(780, 480);
            MinimumSize = new Size(760, 460);
            BackColor = Color.FromArgb(242, 246, 252);

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 86;
            header.BackColor = Color.FromArgb(31, 63, 109);
            Controls.Add(header);

            Label title = new Label();
            title.Text = "小遊戲中心";
            title.Font = new Font("Microsoft JhengHei UI", 22, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(26, 14);
            title.Size = new Size(260, 38);
            header.Controls.Add(title);

            Label sub = new Label();
            sub.Text = "等待諮詢空檔可以玩小遊戲，也能當作專題展示的延伸功能。";
            sub.Font = new Font("Microsoft JhengHei UI", 10);
            sub.ForeColor = Color.FromArgb(222, 232, 246);
            sub.Location = new Point(29, 54);
            sub.Size = new Size(620, 24);
            header.Controls.Add(sub);

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.Padding = new Padding(24);
            grid.ColumnCount = 3;
            grid.RowCount = 1;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            Controls.Add(grid);
            grid.BringToFront();

            grid.Controls.Add(MakeGameCard("五張撲克", "下注、發牌、保留牌、換牌，最後依牌型計算獎金。", "開啟撲克", delegate { using (PokerGameForm f = new PokerGameForm()) f.ShowDialog(this); }), 0, 0);
            grid.Controls.Add(MakeGameCard("西洋棋", "8x8 棋盤、輪流移動、合法步提示、吃子與悔棋。", "開啟西洋棋", delegate { using (ChessMiniGameForm f = new ChessMiniGameForm()) f.ShowDialog(this); }), 1, 0);
            grid.Controls.Add(MakeGameCard("簡易電子琴", "Do Re Mi Fa Sol La Si Do 八個琴鍵，支援滑鼠與鍵盤演奏。", "開啟電子琴", delegate { using (BeepPlayerForm f = new BeepPlayerForm()) f.ShowDialog(this); }), 2, 0);
            UiTheme.Apply(this);
        }

        private Control MakeGameCard(string title, string description, string buttonText, EventHandler click)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(8);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(18);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Microsoft JhengHei UI", 17, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 63, 109);
            lblTitle.Location = new Point(18, 24);
            lblTitle.Size = new Size(200, 42);
            card.Controls.Add(lblTitle);

            Label lblDesc = new Label();
            lblDesc.Text = description;
            lblDesc.Font = new Font("Microsoft JhengHei UI", 10);
            lblDesc.ForeColor = Color.DimGray;
            lblDesc.Location = new Point(20, 82);
            lblDesc.Size = new Size(190, 118);
            card.Controls.Add(lblDesc);

            Button btn = new Button();
            btn.Text = buttonText;
            btn.Font = new Font("Microsoft JhengHei UI", 11, FontStyle.Bold);
            btn.Size = new Size(170, 42);
            btn.Location = new Point(22, 235);
            btn.BackColor = Color.FromArgb(36, 105, 190);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += click;
            card.Controls.Add(btn);

            return card;
        }
    }
}
