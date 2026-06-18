using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;

namespace StudentConsultationScheduler.Forms
{
    public class MainForm : Form
    {
        private readonly string currentUser;

        private DateTimePicker dtpWeek;
        private ComboBox cmbSubject;
        private ComboBox cmbTeacher;
        private ComboBox cmbBookedStatus;
        private ComboBox cmbStatusFilter;
        private TextBox txtAllSearch;
        private ComboBox cmbAllStatusFilter;
        private ConsultationScheduleView scheduleView;
        private FlowLayoutPanel legendPanel;
        private Label lblInfo;
        private Label lblStats;
        private DataGridView dgvWeekAppointments;
        private DataGridView dgvAllAppointments;
        private TabControl rightTabs;
        private TabPage tabWeekAppointments;
        private TabPage tabAllAppointments;
        private SplitContainer mainSplit;
        private Button btnMusic;
        private ComboBox cmbMusic;
        private SoundPlayer backgroundPlayer;
        private bool isMusicPlaying;
        private List<MusicItem> musicItems = new List<MusicItem>();

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

        public MainForm(string username)
        {
            currentUser = username;
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "學生諮詢預約系統";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 760);
            ClientSize = new Size(1340, 840);
            BackColor = Color.FromArgb(242, 246, 252);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 3;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.Margin = Padding.Empty;
            header.BackColor = Color.FromArgb(31, 63, 109);
            root.Controls.Add(header, 0, 0);

            Label title = new Label();
            title.Text = "學生諮詢預約系統";
            title.Font = new Font("Microsoft JhengHei UI", 20, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(24, 12);
            title.Size = new Size(420, 38);
            header.Controls.Add(title);

            Label subTitle = new Label();
            subTitle.Text = "快速預約、查詢與狀態管理";
            subTitle.Font = new Font("Microsoft JhengHei UI", 9);
            subTitle.ForeColor = Color.FromArgb(220, 230, 245);
            subTitle.Location = new Point(27, 50);
            subTitle.Size = new Size(650, 20);
            header.Controls.Add(subTitle);

            Label user = new Label();
            user.Text = "登入者：" + currentUser;
            user.Font = new Font("Microsoft JhengHei UI", 10, FontStyle.Bold);
            user.ForeColor = Color.WhiteSmoke;
            user.TextAlign = ContentAlignment.MiddleRight;
            user.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            user.Location = new Point(1030, 24);
            user.Size = new Size(280, 26);
            header.Controls.Add(user);

            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.Margin = Padding.Empty;
            toolbar.BackColor = Color.White;
            toolbar.Padding = new Padding(18, 12, 18, 10);
            root.Controls.Add(toolbar, 0, 1);

            int x = 20;
            Button btnStudents = MakeButton("學生管理", x, 14, 96, true); x += 104;
            Button btnTeachers = MakeButton("老師管理", x, 14, 96, true); x += 104;
            Button btnAdd = MakeButton("新增預約", x, 14, 96, true); x += 104;
            Button btnDetail = MakeButton("預約詳細", x, 14, 96, false); x += 104;
            Button btnComplete = MakeButton("標記完成", x, 14, 96, false); x += 104;
            Button btnCancelAppointment = MakeButton("取消預約", x, 14, 96, false); x += 104;
            Button btnDelete = MakeButton("刪除預約", x, 14, 96, false); x += 104;
            Button btnRefresh = MakeButton("重新整理", x, 14, 96, false); x += 104;
            btnMusic = MakeButton("音樂：開", x, 14, 92, false); x += 100;
            Button btnGames = MakeButton("小遊戲", x, 14, 92, true); x += 100;

            AddTopLabel(toolbar, "曲目", x, 20, 40);
            cmbMusic = MakeCombo(x + 40, 16, 150);
            toolbar.Controls.Add(cmbMusic);

            toolbar.Controls.Add(btnStudents);
            toolbar.Controls.Add(btnTeachers);
            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnDetail);
            toolbar.Controls.Add(btnComplete);
            toolbar.Controls.Add(btnCancelAppointment);
            toolbar.Controls.Add(btnDelete);
            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnMusic);
            toolbar.Controls.Add(btnGames);

            Button btnPrevWeek = MakeButton("上一週", 20, 66, 78, false);
            toolbar.Controls.Add(btnPrevWeek);

            AddTopLabel(toolbar, "預約週", 108, 72, 55);
            dtpWeek = new DateTimePicker();
            dtpWeek.Font = new Font("Microsoft JhengHei UI", 10);
            dtpWeek.Format = DateTimePickerFormat.Custom;
            dtpWeek.CustomFormat = "yyyy-MM-dd";
            dtpWeek.Location = new Point(163, 68);
            dtpWeek.Size = new Size(125, 25);
            toolbar.Controls.Add(dtpWeek);

            Button btnToday = MakeButton("本週", 298, 66, 70, false);
            toolbar.Controls.Add(btnToday);

            Button btnNextWeek = MakeButton("下一週", 376, 66, 78, false);
            toolbar.Controls.Add(btnNextWeek);

            AddTopLabel(toolbar, "科目", 472, 72, 42);
            cmbSubject = MakeCombo(514, 68, 170);
            toolbar.Controls.Add(cmbSubject);

            AddTopLabel(toolbar, "老師", 698, 72, 42);
            cmbTeacher = MakeCombo(740, 68, 170);
            toolbar.Controls.Add(cmbTeacher);

            AddTopLabel(toolbar, "是否有預約", 925, 72, 86);
            cmbBookedStatus = MakeCombo(1010, 68, 126);
            cmbBookedStatus.Items.Add("全部");
            cmbBookedStatus.Items.Add("只看已預約");
            cmbBookedStatus.Items.Add("只看未預約");
            cmbBookedStatus.SelectedIndex = 0;
            toolbar.Controls.Add(cmbBookedStatus);

            AddTopLabel(toolbar, "狀態", 1150, 72, 42);
            cmbStatusFilter = MakeCombo(1192, 68, 118);
            cmbStatusFilter.Items.Add("全部狀態");
            cmbStatusFilter.Items.Add("已預約");
            cmbStatusFilter.Items.Add("已完成");
            cmbStatusFilter.Items.Add("已取消");
            cmbStatusFilter.SelectedIndex = 0;
            toolbar.Controls.Add(cmbStatusFilter);

            lblInfo = new Label();
            lblInfo.Text = "";
            lblInfo.Font = new Font("Microsoft JhengHei UI", 9);
            lblInfo.ForeColor = Color.DimGray;
            lblInfo.Location = new Point(1246, 18);
            lblInfo.Size = new Size(76, 40);
            toolbar.Controls.Add(lblInfo);

            SplitContainer split = new SplitContainer();
            mainSplit = split;
            split.Dock = DockStyle.Fill;
            split.SplitterWidth = 6;
            split.FixedPanel = FixedPanel.Panel2;
            // 啟動畫面尚未完成排版時 SplitContainer 的寬度可能是 0。
            // 不能在這裡直接設定很大的 SplitterDistance，否則會出現
            //「SplitterDistance 必須介於 Panel1MinSize 和 Width - Panel2MinSize 之間」。
            split.Panel1MinSize = 1;
            split.Panel2MinSize = 1;
            split.Margin = new Padding(12, 10, 12, 12);
            root.Controls.Add(split, 0, 2);

            Panel scheduleCard = MakeCardPanel();
            scheduleCard.Dock = DockStyle.Fill;
            scheduleCard.Padding = new Padding(12);
            split.Panel1.Controls.Add(scheduleCard);

            scheduleView = new ConsultationScheduleView();
            scheduleView.Dock = DockStyle.Fill;
            scheduleView.DayItems = dayItems;
            scheduleView.SlotItems = slotItems;
            scheduleView.SlotClicked += ScheduleView_SlotClicked;
            scheduleCard.Controls.Add(scheduleView);

            TabControl tabs = new TabControl();
            rightTabs = tabs;
            tabs.Dock = DockStyle.Fill;
            tabs.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            split.Panel2.Controls.Add(tabs);

            tabWeekAppointments = new TabPage("本週預約");
            tabWeekAppointments.Padding = new Padding(10);
            tabWeekAppointments.BackColor = Color.FromArgb(242, 246, 252);
            tabs.TabPages.Add(tabWeekAppointments);

            Panel weekCard = MakeCardPanel();
            weekCard.Dock = DockStyle.Fill;
            weekCard.Padding = new Padding(12);
            tabWeekAppointments.Controls.Add(weekCard);

            Label listTitle = new Label();
            listTitle.Text = "本週預約（雙擊看詳細）";
            listTitle.Font = new Font("Microsoft JhengHei UI", 11, FontStyle.Bold);
            listTitle.ForeColor = Color.FromArgb(31, 63, 109);
            listTitle.Dock = DockStyle.Top;
            listTitle.Height = 28;
            weekCard.Controls.Add(listTitle);

            dgvWeekAppointments = MakeAppointmentGrid();
            dgvWeekAppointments.CellDoubleClick += DgvWeekAppointments_CellDoubleClick;
            weekCard.Controls.Add(dgvWeekAppointments);
            dgvWeekAppointments.BringToFront();

            tabAllAppointments = new TabPage("全部預約 / 搜尋");
            tabAllAppointments.Padding = new Padding(10);
            tabAllAppointments.BackColor = Color.FromArgb(242, 246, 252);
            tabs.TabPages.Add(tabAllAppointments);

            Panel allCard = MakeCardPanel();
            allCard.Dock = DockStyle.Fill;
            allCard.Padding = new Padding(12);
            tabAllAppointments.Controls.Add(allCard);

            // 使用 TableLayoutPanel 固定搜尋列與表格區塊，避免搜尋列覆蓋第一筆資料列。
            TableLayoutPanel allLayout = new TableLayoutPanel();
            allLayout.Dock = DockStyle.Fill;
            allLayout.Margin = Padding.Empty;
            allLayout.Padding = Padding.Empty;
            allLayout.ColumnCount = 1;
            allLayout.RowCount = 2;
            allLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            allLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            allCard.Controls.Add(allLayout);

            Panel allSearchPanel = new Panel();
            allSearchPanel.Dock = DockStyle.Fill;
            allSearchPanel.Margin = Padding.Empty;
            allSearchPanel.Height = 76;
            allLayout.Controls.Add(allSearchPanel, 0, 0);

            Label allTitle = new Label();
            allTitle.Text = "全部預約查詢";
            allTitle.Font = new Font("Microsoft JhengHei UI", 11, FontStyle.Bold);
            allTitle.ForeColor = Color.FromArgb(31, 63, 109);
            allTitle.Location = new Point(0, 0);
            allTitle.Size = new Size(160, 24);
            allSearchPanel.Controls.Add(allTitle);

            Label allHint = UiTheme.MakeHint("可搜尋：學生、學號、老師、科目、主題、狀態；輸入後按 Enter", 125, 2, 330);
            allSearchPanel.Controls.Add(allHint);

            Label lblAllStatus = new Label();
            lblAllStatus.Text = "狀態";
            lblAllStatus.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            lblAllStatus.Location = new Point(0, 40);
            lblAllStatus.Size = new Size(42, 24);
            allSearchPanel.Controls.Add(lblAllStatus);

            cmbAllStatusFilter = MakeCombo(42, 36, 105);
            cmbAllStatusFilter.Items.Add("全部狀態");
            cmbAllStatusFilter.Items.Add("已預約");
            cmbAllStatusFilter.Items.Add("已完成");
            cmbAllStatusFilter.Items.Add("已取消");
            cmbAllStatusFilter.SelectedIndex = 0;
            allSearchPanel.Controls.Add(cmbAllStatusFilter);

            Label lblAllSearch = new Label();
            lblAllSearch.Text = "搜尋";
            lblAllSearch.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            lblAllSearch.Location = new Point(158, 40);
            lblAllSearch.Size = new Size(42, 24);
            allSearchPanel.Controls.Add(lblAllSearch);

            txtAllSearch = new TextBox();
            txtAllSearch.Font = new Font("Microsoft JhengHei UI", 10);
            txtAllSearch.Location = new Point(200, 36);
            txtAllSearch.Size = new Size(145, 25);
            allSearchPanel.Controls.Add(txtAllSearch);

            Button btnAllSearch = MakeButton("搜尋", 354, 32, 66, false);
            allSearchPanel.Controls.Add(btnAllSearch);

            dgvAllAppointments = MakeAppointmentGrid();
            dgvAllAppointments.CellDoubleClick += DgvAllAppointments_CellDoubleClick;
            allLayout.Controls.Add(dgvAllAppointments, 0, 1);

            dtpWeek.ValueChanged += delegate { LoadWeekAppointments(); RefreshSchedule(); LoadWeekGrid(); };
            cmbSubject.SelectedIndexChanged += CmbSubject_SelectedIndexChanged;
            cmbTeacher.SelectedIndexChanged += CmbTeacher_SelectedIndexChanged;
            cmbBookedStatus.SelectedIndexChanged += delegate { RefreshSchedule(); };
            cmbStatusFilter.SelectedIndexChanged += delegate { RefreshSchedule(); LoadWeekGrid(); };
            cmbAllStatusFilter.SelectedIndexChanged += delegate { LoadAllGrid(); };
            txtAllSearch.KeyDown += TxtAllSearch_KeyDown;
            btnAllSearch.Click += delegate { LoadAllGrid(); };
            btnRefresh.Click += delegate { LoadData(); };
            btnMusic.Click += delegate { ToggleBackgroundMusic(); };
            cmbMusic.SelectedIndexChanged += delegate { ChangeSelectedMusic(); };
            btnGames.Click += delegate
            {
                bool resumeMusicAfterGame = isMusicPlaying;
                StopBackgroundMusic();
                using (GameCenterForm f = new GameCenterForm())
                    f.ShowDialog(this);
                if (resumeMusicAfterGame)
                    StartBackgroundMusic();
            };
            btnPrevWeek.Click += delegate { dtpWeek.Value = dtpWeek.Value.Date.AddDays(-7); };
            btnNextWeek.Click += delegate { dtpWeek.Value = dtpWeek.Value.Date.AddDays(7); };
            btnToday.Click += delegate { dtpWeek.Value = DateTime.Today; };
            btnStudents.Click += delegate { using (StudentForm f = new StudentForm()) f.ShowDialog(this); LoadData(); };
            btnTeachers.Click += delegate { using (TeacherForm f = new TeacherForm()) f.ShowDialog(this); LoadData(); };
            btnAdd.Click += delegate { AddAppointment(); };
            btnDetail.Click += delegate { ShowSelectedAppointmentDetail(); };
            btnComplete.Click += delegate { UpdateSelectedAppointmentStatus("已完成"); };
            btnCancelAppointment.Click += delegate { UpdateSelectedAppointmentStatus("已取消"); };
            btnDelete.Click += delegate { DeleteSelectedAppointment(); };

            Load += MainForm_Load;
            FormClosed += delegate { StopBackgroundMusic(); };
            Shown += delegate { SetMainSplitterDistance(); StartBackgroundMusic(); UiEffects.ShowToast(this, "系統已就緒，點擊課表可預約，雙擊表格可查看詳細。", ToastKind.Info); };
            UiTheme.Apply(this);

            Resize += delegate { SetMainSplitterDistance(); };
        }

        private void SetMainSplitterDistance()
        {
            if (mainSplit == null || mainSplit.IsDisposed)
                return;

            int width = mainSplit.ClientSize.Width;
            if (width <= 0)
                return;

            int splitterWidth = mainSplit.SplitterWidth;
            int desiredRightWidth = 540;
            int minLeft = 420;
            int minRight = 320;

            // 視窗被縮小時，動態降低最小寬度，避免 SplitContainer 例外。
            if (width < minLeft + minRight + splitterWidth)
            {
                minLeft = Math.Max(1, width / 2 - splitterWidth);
                minRight = Math.Max(1, width - minLeft - splitterWidth);
            }

            int distance = width - desiredRightWidth - splitterWidth;
            int maxDistance = width - minRight - splitterWidth;
            if (distance < minLeft)
                distance = minLeft;
            if (distance > maxDistance)
                distance = maxDistance;

            try
            {
                // 先放寬限制，再設定距離，最後才設定合理的最小寬度。
                mainSplit.Panel1MinSize = 1;
                mainSplit.Panel2MinSize = 1;
                if (distance > 0 && distance < width - splitterWidth)
                    mainSplit.SplitterDistance = distance;
                mainSplit.Panel1MinSize = minLeft;
                mainSplit.Panel2MinSize = minRight;
            }
            catch
            {
                // 這只是 UI 配置，不應阻止登入或主畫面開啟。
            }
        }

        private Panel MakeCardPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = Color.White;
            panel.Margin = new Padding(0, 0, 0, 10);
            panel.BorderStyle = BorderStyle.FixedSingle;
            return panel;
        }

        private DataGridView MakeAppointmentGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Font = new Font("Microsoft JhengHei UI", 9);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            grid.RowTemplate.Height = 28;
            return grid;
        }

        private Button MakeButton(string text, int x, int y, int width, bool primary)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            button.Location = new Point(x, y);
            button.Size = new Size(width, 34);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = primary ? Color.FromArgb(31, 92, 165) : Color.FromArgb(205, 212, 225);
            button.BackColor = primary ? Color.FromArgb(36, 105, 190) : Color.White;
            button.ForeColor = primary ? Color.White : Color.FromArgb(35, 45, 60);
            return button;
        }

        private Label AddTopLabel(Control parent, string text, int x, int y, int width)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Microsoft JhengHei UI", 9, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(65, 70, 80);
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(width, 24);
            parent.Controls.Add(lbl);
            return lbl;
        }

        private ComboBox MakeCombo(int x, int y, int width)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Font = new Font("Microsoft JhengHei UI", 10);
            combo.Location = new Point(x, y);
            combo.Size = new Size(width, 25);
            return combo;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            dtpWeek.Value = DateTime.Today;
            LoadMusicFiles();
            LoadData();
            CheckUpcomingAppointments();
        }

        private void TxtAllSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                LoadAllGrid();
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
            LoadWeekGrid();
            LoadAllGrid();
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
    a.Status,
    ISNULL(a.Note, N'') AS Note
FROM dbo.Appointments a
INNER JOIN dbo.Students s ON a.StudentId = s.StudentId
INNER JOIN dbo.Teachers t ON a.TeacherId = t.TeacherId
WHERE a.AppointmentDate >= @startDate
  AND a.AppointmentDate < @endDate
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
                appt.Note = Convert.ToString(row["Note"]);
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
                string subject = EmptyToUnset(t.Subject);
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

        private void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingFilters)
                return;
            loadingFilters = true;
            LoadTeacherFilter();
            loadingFilters = false;
            RefreshSchedule();
            LoadWeekGrid();
        }

        private void CmbTeacher_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingFilters)
                return;
            RefreshSchedule();
            LoadWeekGrid();
        }

        private void RefreshSchedule()
        {
            if (scheduleView == null)
                return;

            List<TeacherInfo> visible = GetVisibleTeachers();
            scheduleView.WeekStart = GetWeekStart(dtpWeek.Value.Date);
            scheduleView.Teachers = visible;
            scheduleView.Appointments = weekAppointments;
            scheduleView.BookingFilter = cmbBookedStatus.SelectedItem == null ? "全部" : cmbBookedStatus.SelectedItem.ToString();
            scheduleView.StatusFilter = cmbStatusFilter.SelectedItem == null ? "全部狀態" : cmbStatusFilter.SelectedItem.ToString();
            scheduleView.Invalidate();

            DateTime monday = GetWeekStart(dtpWeek.Value.Date);
            if (lblInfo != null)
                lblInfo.Text = monday.ToString("MM/dd") + " - " + monday.AddDays(6).ToString("MM/dd");
        }

        private List<TeacherInfo> GetVisibleTeachers()
        {
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
            return visible;
        }

        private void RefreshLegend(List<TeacherInfo> teachers)
        {
            legendPanel.Controls.Clear();
            if (teachers.Count == 0)
            {
                Label empty = new Label();
                empty.Text = "目前沒有符合條件的老師。";
                empty.Font = new Font("Microsoft JhengHei UI", 10);
                empty.AutoSize = true;
                legendPanel.Controls.Add(empty);
                return;
            }

            foreach (TeacherInfo teacher in teachers)
            {
                Panel item = new Panel();
                item.Width = 360;
                item.Height = 28;
                item.Margin = new Padding(2, 2, 2, 2);

                Panel swatch = new Panel();
                swatch.BackColor = teacher.Color;
                swatch.BorderStyle = BorderStyle.FixedSingle;
                swatch.Location = new Point(0, 5);
                swatch.Size = new Size(22, 17);
                item.Controls.Add(swatch);

                Label label = new Label();
                label.Text = teacher.Name + " / " + EmptyToUnset(teacher.Subject);
                label.Font = new Font("Microsoft JhengHei UI", 9);
                label.Location = new Point(30, 3);
                label.Size = new Size(310, 22);
                item.Controls.Add(label);

                legendPanel.Controls.Add(item);
            }
        }

        private void RefreshStats(List<TeacherInfo> visibleTeachers)
        {
            int booked = 0;
            int completed = 0;
            int canceled = 0;
            foreach (AppointmentInfo appt in weekAppointments)
            {
                if (!TeacherVisibleById(visibleTeachers, appt.TeacherId))
                    continue;
                if (appt.Status == "已預約")
                    booked++;
                else if (appt.Status == "已完成")
                    completed++;
                else if (appt.Status == "已取消")
                    canceled++;
            }

            int openSlots = 0;
            DateTime monday = GetWeekStart(dtpWeek.Value.Date);
            for (int d = 0; d < dayItems.Length; d++)
            {
                for (int s = 0; s < slotItems.Length; s++)
                {
                    foreach (TeacherInfo teacher in visibleTeachers)
                    {
                        if (TeacherHasSlot(teacher, dayItems[d], slotItems[s]))
                            openSlots++;
                    }
                }
            }

            lblStats.Text = "開放時段：" + openSlots + " 格\n" +
                            "已預約：" + booked + "　已完成：" + completed + "\n" +
                            "已取消：" + canceled + "　老師：" + visibleTeachers.Count;
            lblInfo.Text = monday.ToString("MM/dd") + " - " + monday.AddDays(6).ToString("MM/dd");
        }

        private bool TeacherVisibleById(List<TeacherInfo> teachers, int teacherId)
        {
            foreach (TeacherInfo teacher in teachers)
            {
                if (teacher.TeacherId == teacherId)
                    return true;
            }
            return false;
        }

        private void LoadWeekGrid()
        {
            DateTime monday = GetWeekStart(dtpWeek.Value.Date);
            DateTime nextMonday = monday.AddDays(7);
            DataTable raw = Db.GetAppointments(monday, nextMonday, "");

            string status = cmbStatusFilter == null || cmbStatusFilter.SelectedItem == null ? "全部狀態" : cmbStatusFilter.SelectedItem.ToString();

            DataTable compact = new DataTable();
            compact.Columns.Add("AppointmentId", typeof(int));
            compact.Columns.Add("日期", typeof(string));
            compact.Columns.Add("時間", typeof(string));
            compact.Columns.Add("學生", typeof(string));
            compact.Columns.Add("老師", typeof(string));
            compact.Columns.Add("狀態", typeof(string));

            foreach (DataRow row in raw.Rows)
            {
                string rowStatus = Convert.ToString(row["Status"]);
                if (status != "全部狀態" && rowStatus != status)
                    continue;

                DateTime date = Convert.ToDateTime(row["AppointmentDate"]).Date;
                string dateText = date.ToString("MM/dd") + "(" + GetShortDayName(date) + ")";
                string timeText = FormatTimeRange(row["StartTime"], row["EndTime"]);
                compact.Rows.Add(
                    Convert.ToInt32(row["AppointmentId"]),
                    dateText,
                    timeText,
                    Convert.ToString(row["StudentName"]),
                    Convert.ToString(row["TeacherName"]),
                    rowStatus);
            }

            dgvWeekAppointments.DataSource = compact;
            SetupWeekGridHeaders();
        }

        private void LoadAllGrid()
        {
            if (dgvAllAppointments == null)
                return;

            string keyword = txtAllSearch == null ? "" : txtAllSearch.Text.Trim();
            string status = cmbAllStatusFilter == null || cmbAllStatusFilter.SelectedItem == null ? "全部狀態" : cmbAllStatusFilter.SelectedItem.ToString();
            DataTable raw = Db.GetAppointments(null, null, keyword);

            DataTable compact = new DataTable();
            compact.Columns.Add("AppointmentId", typeof(int));
            compact.Columns.Add("日期", typeof(string));
            compact.Columns.Add("時間", typeof(string));
            compact.Columns.Add("學生", typeof(string));
            compact.Columns.Add("老師", typeof(string));
            compact.Columns.Add("科目", typeof(string));
            compact.Columns.Add("狀態", typeof(string));

            foreach (DataRow row in raw.Rows)
            {
                string rowStatus = Convert.ToString(row["Status"]);
                if (status != "全部狀態" && rowStatus != status)
                    continue;

                DateTime date = Convert.ToDateTime(row["AppointmentDate"]).Date;
                compact.Rows.Add(
                    Convert.ToInt32(row["AppointmentId"]),
                    date.ToString("yyyy/MM/dd") + "(" + GetShortDayName(date) + ")",
                    FormatTimeRange(row["StartTime"], row["EndTime"]),
                    Convert.ToString(row["StudentName"]),
                    Convert.ToString(row["TeacherName"]),
                    Convert.ToString(row["Subject"]),
                    rowStatus);
            }

            dgvAllAppointments.DataSource = compact;
            SetupAllGridHeaders();
        }

        private void SetupWeekGridHeaders()
        {
            if (dgvWeekAppointments.Columns.Count == 0)
                return;

            if (dgvWeekAppointments.Columns.Contains("AppointmentId"))
                dgvWeekAppointments.Columns["AppointmentId"].Visible = false;

            dgvWeekAppointments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWeekAppointments.RowTemplate.Height = 28;
            dgvWeekAppointments.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            if (dgvWeekAppointments.Columns.Contains("日期"))
                dgvWeekAppointments.Columns["日期"].FillWeight = 78;
            if (dgvWeekAppointments.Columns.Contains("時間"))
                dgvWeekAppointments.Columns["時間"].FillWeight = 92;
            if (dgvWeekAppointments.Columns.Contains("學生"))
                dgvWeekAppointments.Columns["學生"].FillWeight = 88;
            if (dgvWeekAppointments.Columns.Contains("老師"))
                dgvWeekAppointments.Columns["老師"].FillWeight = 88;
            if (dgvWeekAppointments.Columns.Contains("狀態"))
                dgvWeekAppointments.Columns["狀態"].FillWeight = 70;
        }

        private void SetupAllGridHeaders()
        {
            if (dgvAllAppointments == null || dgvAllAppointments.Columns.Count == 0)
                return;

            if (dgvAllAppointments.Columns.Contains("AppointmentId"))
                dgvAllAppointments.Columns["AppointmentId"].Visible = false;

            dgvAllAppointments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAllAppointments.RowTemplate.Height = 28;
            dgvAllAppointments.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            SetFillWeight(dgvAllAppointments, "日期", 105);
            SetFillWeight(dgvAllAppointments, "時間", 88);
            SetFillWeight(dgvAllAppointments, "學生", 80);
            SetFillWeight(dgvAllAppointments, "老師", 80);
            SetFillWeight(dgvAllAppointments, "科目", 105);
            SetFillWeight(dgvAllAppointments, "狀態", 70);
        }

        private void SetFillWeight(DataGridView grid, string columnName, float weight)
        {
            if (grid != null && grid.Columns.Contains(columnName))
                grid.Columns[columnName].FillWeight = weight;
        }

        private string FormatTimeRange(object startValue, object endValue)
        {
            return FormatTimeValue(startValue) + "-" + FormatTimeValue(endValue);
        }

        private string FormatTimeValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";
            if (value is TimeSpan)
                return ((TimeSpan)value).ToString(@"hh\:mm");
            DateTime dateTime;
            if (DateTime.TryParse(Convert.ToString(value), out dateTime))
                return dateTime.ToString("HH:mm");
            TimeSpan timeSpan;
            if (TimeSpan.TryParse(Convert.ToString(value), out timeSpan))
                return timeSpan.ToString(@"hh\:mm");
            return Convert.ToString(value);
        }

        private string GetShortDayName(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: return "一";
                case DayOfWeek.Tuesday: return "二";
                case DayOfWeek.Wednesday: return "三";
                case DayOfWeek.Thursday: return "四";
                case DayOfWeek.Friday: return "五";
                case DayOfWeek.Saturday: return "六";
                case DayOfWeek.Sunday: return "日";
                default: return "";
            }
        }

        private void ScheduleView_SlotClicked(object sender, ScheduleSlotClickEventArgs e)
        {
            if (e == null || e.Item == null)
                return;

            if (e.Item.Appointment != null)
            {
                ShowAppointmentDetail(e.Item.Appointment.AppointmentId);
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
                    LoadData();
                    UiEffects.ShowToast(this, "預約已新增，課表與列表已更新。", ToastKind.Success);
                }
            }
        }

        private void AddAppointment()
        {
            using (AppointmentForm form = new AppointmentForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                    UiEffects.ShowToast(this, "預約已新增，課表與列表已更新。", ToastKind.Success);
                }
            }
        }

        private void DgvWeekAppointments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                ShowSelectedAppointmentDetail();
        }

        private void DgvAllAppointments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                ShowSelectedAppointmentDetail();
        }

        private int GetSelectedAppointmentId()
        {
            if (rightTabs != null && rightTabs.SelectedTab == tabAllAppointments)
                return GetSelectedAppointmentIdFromGrid(dgvAllAppointments);
            return GetSelectedAppointmentIdFromGrid(dgvWeekAppointments);
        }

        private int GetSelectedAppointmentIdFromGrid(DataGridView grid)
        {
            if (grid == null || grid.CurrentRow == null)
                return 0;
            object value = grid.CurrentRow.Cells["AppointmentId"].Value;
            if (value == null || value == DBNull.Value)
                return 0;
            return Convert.ToInt32(value);
        }

        private void ShowSelectedAppointmentDetail()
        {
            int id = GetSelectedAppointmentId();
            if (id == 0)
            {
                MessageBox.Show("請先在右側本週/全部預約表選擇一筆資料，或直接點課表上的已預約時段。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ShowAppointmentDetail(id);
        }

        private void ShowAppointmentDetail(int appointmentId)
        {
            using (AppointmentDetailForm form = new AppointmentDetailForm(appointmentId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    LoadData();
            }
        }

        private void UpdateSelectedAppointmentStatus(string status)
        {
            int id = GetSelectedAppointmentId();
            if (id == 0)
            {
                MessageBox.Show("請先在右側預約列表選擇一筆預約。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (status == "已預約" && IsAppointmentStartInPast(id))
            {
                MessageBox.Show("此預約時間已經過去，不能恢復成「已預約」。", "不可恢復過去預約", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("確定要將此預約狀態改為「" + status + "」嗎？", "確認狀態變更", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Db.ExecuteNonQuery("UPDATE dbo.Appointments SET Status=@status WHERE AppointmentId=@id", Db.P("@status", status), Db.P("@id", id));
                LoadData();
                UiEffects.ShowToast(this, "預約狀態已更新為「" + status + "」。", ToastKind.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show("狀態更新失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedAppointment()
        {
            int id = GetSelectedAppointmentId();
            if (id == 0)
            {
                MessageBox.Show("請先在右側預約列表選擇一筆預約。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("確定要永久刪除這筆預約嗎？\n\n建議一般情況使用「取消預約」，保留歷史紀錄。", "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Db.ExecuteNonQuery("DELETE FROM dbo.Appointments WHERE AppointmentId=@id", Db.P("@id", id));
                LoadData();
                UiEffects.ShowToast(this, "預約已刪除。", ToastKind.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show("刪除失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private bool IsAppointmentStartInPast(int appointmentId)
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

        private void LoadMusicFiles()
        {
            musicItems.Clear();
            if (cmbMusic == null)
                return;

            cmbMusic.Items.Clear();
            string assetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            if (!Directory.Exists(assetsDir))
            {
                cmbMusic.Enabled = false;
                cmbMusic.Items.Add("找不到 Assets");
                cmbMusic.SelectedIndex = 0;
                if (btnMusic != null)
                    btnMusic.Text = "音樂：無";
                return;
            }

            string[] files = Directory.GetFiles(assetsDir, "*.wav");
            Array.Sort(files, StringComparer.CurrentCultureIgnoreCase);
            for (int i = 0; i < files.Length; i++)
            {
                MusicItem item = new MusicItem();
                item.DisplayName = Path.GetFileNameWithoutExtension(files[i]);
                item.FilePath = files[i];
                musicItems.Add(item);
                cmbMusic.Items.Add(item);
            }

            if (cmbMusic.Items.Count == 0)
            {
                cmbMusic.Enabled = false;
                cmbMusic.Items.Add("沒有 .wav 檔");
                cmbMusic.SelectedIndex = 0;
                if (btnMusic != null)
                    btnMusic.Text = "音樂：無";
                return;
            }

            cmbMusic.Enabled = true;
            int defaultIndex = 0;
            for (int i = 0; i < musicItems.Count; i++)
            {
                if (string.Equals(Path.GetFileName(musicItems[i].FilePath), "bgm.wav", StringComparison.OrdinalIgnoreCase))
                {
                    defaultIndex = i;
                    break;
                }
            }
            cmbMusic.SelectedIndex = defaultIndex;
        }

        private string GetSelectedMusicPath()
        {
            if (cmbMusic != null && cmbMusic.SelectedItem is MusicItem)
                return ((MusicItem)cmbMusic.SelectedItem).FilePath;

            string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "bgm.wav");
            if (File.Exists(fallback))
                return fallback;
            return string.Empty;
        }

        private void ChangeSelectedMusic()
        {
            // 下拉選單切換曲目時，若目前音樂是開啟狀態，就立即換成新選的曲目。
            // 若音樂是關閉狀態，只記住選擇，不會自動播放。
            if (isMusicPlaying)
                StartBackgroundMusic();
        }

        private void StopCurrentPlayerOnly()
        {
            try
            {
                if (backgroundPlayer != null)
                {
                    backgroundPlayer.Stop();
                    backgroundPlayer.Dispose();
                    backgroundPlayer = null;
                }
            }
            catch
            {
            }
        }

        private void StartBackgroundMusic()
        {
            try
            {
                string musicPath = GetSelectedMusicPath();
                if (string.IsNullOrEmpty(musicPath) || !File.Exists(musicPath))
                {
                    StopCurrentPlayerOnly();
                    isMusicPlaying = false;
                    if (btnMusic != null)
                        btnMusic.Text = "音樂：無";
                    return;
                }

                StopCurrentPlayerOnly();
                backgroundPlayer = new SoundPlayer(musicPath);
                backgroundPlayer.PlayLooping();
                isMusicPlaying = true;
                if (btnMusic != null)
                    btnMusic.Text = "音樂：開";
            }
            catch
            {
                StopCurrentPlayerOnly();
                isMusicPlaying = false;
                if (btnMusic != null)
                    btnMusic.Text = "音樂：關";
            }
        }

        private void StopBackgroundMusic()
        {
            StopCurrentPlayerOnly();
            isMusicPlaying = false;
            if (btnMusic != null)
                btnMusic.Text = "音樂：關";
        }

        private void ToggleBackgroundMusic()
        {
            if (isMusicPlaying)
                StopBackgroundMusic();
            else
                StartBackgroundMusic();
        }

        private void CheckUpcomingAppointments()
        {
            try
            {
                DataTable table = Db.GetUpcomingAppointmentsWithinMinutes(15);
                if (table.Rows.Count == 0)
                    return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("15 分鐘內有即將開始的諮詢預約：");
                sb.AppendLine();
                foreach (DataRow row in table.Rows)
                {
                    sb.AppendLine(row["AppointmentDate"] + " " + row["StartTime"] + "　" + row["TeacherName"] + " / " + row["StudentName"] + " / " + row["Topic"]);
                }

                MessageBox.Show(sb.ToString(), "預約提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                // Reminder is optional; do not block program.
            }
        }

        private class MusicItem
        {
            public string DisplayName { get; set; }
            public string FilePath { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
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
}
