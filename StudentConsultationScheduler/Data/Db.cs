using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace StudentConsultationScheduler.Data
{
    public static class Db
    {
        // 2026-06-17 修正：改用穩定的使用者資料夾存放 LocalDB，避免專案資料夾搬移或重新解壓縮後，舊 MDF 路徑失效。
        public const string DatabaseName = "StudentConsultationSchedulerDB_Stable";

        // 不再放在 bin\Debug\App_Data，改放在使用者 LocalAppData。
        // 例如：C:\Users\使用者\AppData\Local\StudentConsultationScheduler\App_Data
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StudentConsultationScheduler",
            "App_Data");
        private static readonly string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string MdfPath = Path.Combine(AppDataFolder, DatabaseName + ".mdf");
        private static readonly string LdfPath = Path.Combine(AppDataFolder, DatabaseName + "_log.ldf");

        public static string ConnectionString
        {
            get
            {
                return "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=" + DatabaseName + ";Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True";
            }
        }

        private static string MasterConnectionString
        {
            get
            {
                return "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30";
            }
        }

        public static void EnsureDatabase()
        {
            Directory.CreateDirectory(AppDataFolder);
            Directory.CreateDirectory(LogFolder);

            using (SqlConnection conn = new SqlConnection(MasterConnectionString))
            {
                conn.Open();

                bool exists = DatabaseExists(conn, DatabaseName);
                if (!exists)
                {
                    CreateOrAttachDatabase(conn);
                }
                else if (!CanOpenAppDatabase())
                {
                    // 如果同名資料庫存在但無法開啟，通常是 LocalDB 殘留登錄或 MDF 路徑失效。
                    // 先嘗試移除同名資料庫登錄，再重新建立；若移除失敗，會拋出原始錯誤讓畫面顯示。
                    TryDropDatabase(conn, DatabaseName);
                    CreateOrAttachDatabase(conn);
                }
            }

            // 清除連線池，避免 SQL Server 還抓到重建前的舊連線狀態。
            SqlConnection.ClearAllPools();
            EnsureTables();
        }

        private static bool DatabaseExists(SqlConnection conn, string databaseName)
        {
            object result = ExecuteScalar(conn,
                "SELECT COUNT(*) FROM sys.databases WHERE name = @name",
                new SqlParameter("@name", databaseName));
            return Convert.ToInt32(result) > 0;
        }

        private static bool CanOpenAppDatabase()
        {
            try
            {
                using (SqlConnection testConn = new SqlConnection(ConnectionString))
                {
                    testConn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void CreateOrAttachDatabase(SqlConnection conn)
        {
            string escapedMdf = MdfPath.Replace("'", "''");
            string escapedLdf = LdfPath.Replace("'", "''");

            if (File.Exists(MdfPath))
            {
                string attachSql = "CREATE DATABASE [" + DatabaseName + "] ON " +
                                   "(FILENAME = N'" + escapedMdf + "')";
                if (File.Exists(LdfPath))
                    attachSql += ", (FILENAME = N'" + escapedLdf + "') FOR ATTACH";
                else
                    attachSql += " FOR ATTACH_REBUILD_LOG";
                ExecuteNonQuery(conn, attachSql);
            }
            else
            {
                string createSql =
                    "CREATE DATABASE [" + DatabaseName + "] ON PRIMARY " +
                    "(NAME = N'" + DatabaseName + "', FILENAME = N'" + escapedMdf + "') " +
                    "LOG ON (NAME = N'" + DatabaseName + "_log', FILENAME = N'" + escapedLdf + "')";
                ExecuteNonQuery(conn, createSql);
            }
        }

        private static void TryDropDatabase(SqlConnection conn, string databaseName)
        {
            SqlConnection.ClearAllPools();
            string safeName = databaseName.Replace("]", "]]");
            string sql =
                "IF DB_ID(N'" + databaseName.Replace("'", "''") + "') IS NOT NULL BEGIN " +
                "ALTER DATABASE [" + safeName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                "DROP DATABASE [" + safeName + "]; " +
                "END";
            ExecuteNonQuery(conn, sql);
        }

        private static void EnsureTables()
        {
            string sql = @"
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(50) NOT NULL,
        DisplayName NVARCHAR(50) NOT NULL
    );
END;

IF OBJECT_ID('dbo.Students', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        StudentId INT IDENTITY(1,1) PRIMARY KEY,
        StudentNo NVARCHAR(20) NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        ClassName NVARCHAR(50) NULL,
        Phone NVARCHAR(30) NULL,
        Email NVARCHAR(100) NULL
    );
END;

IF OBJECT_ID('dbo.Teachers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Teachers
    (
        TeacherId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL,
        Subject NVARCHAR(100) NULL,
        Office NVARCHAR(50) NULL,
        Email NVARCHAR(100) NULL,
        AvailableTime NVARCHAR(500) NULL
    );
END;



IF OBJECT_ID('dbo.Teachers', 'U') IS NOT NULL AND COL_LENGTH('dbo.Teachers', 'AvailableTime') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Teachers ALTER COLUMN AvailableTime NVARCHAR(500) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Students_StudentNo' AND object_id = OBJECT_ID(N'dbo.Students'))
   AND NOT EXISTS (SELECT StudentNo FROM dbo.Students WHERE LTRIM(RTRIM(StudentNo)) <> N'' GROUP BY StudentNo HAVING COUNT(*) > 1)
BEGIN
    CREATE UNIQUE INDEX UX_Students_StudentNo ON dbo.Students(StudentNo) WHERE StudentNo <> N'';
END;

IF OBJECT_ID('dbo.Appointments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments
    (
        AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
        StudentId INT NOT NULL,
        TeacherId INT NOT NULL,
        Topic NVARCHAR(100) NOT NULL,
        AppointmentDate DATE NOT NULL,
        StartTime TIME(0) NOT NULL,
        EndTime TIME(0) NOT NULL,
        Note NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT N'已預約',
        CONSTRAINT FK_Appointments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
        CONSTRAINT FK_Appointments_Teachers FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers(TeacherId)
    );
END;

IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL AND COL_LENGTH('dbo.Appointments', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.Appointments ADD Status NVARCHAR(20) NOT NULL DEFAULT N'已預約';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
BEGIN
    INSERT INTO dbo.Users (Username, Password, DisplayName)
    VALUES (N'admin', N'1234', N'系統管理員');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Students)
BEGIN
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES
    (N'1131427', N'田秉立', N'資工二A', N'0912-345-678', N'student@example.com'),
    (N'1131001', N'王小明', N'資工二A', N'0922-111-222', N'wang@example.com'),
    (N'1131002', N'林小華', N'資工二B', N'0933-333-444', N'lin@example.com');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Teachers)
BEGIN
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES
    (N'黃老師', N'視窗程式設計', N'R1102', N'teacher1@example.com', N'星期三 13:00-14:00、14:00-15:00'),
    (N'陳老師', N'資料結構', N'R1203', N'teacher2@example.com', N'星期二 10:00-11:00、11:00-12:00'),
    (N'林老師', N'機率與統計', N'R1305', N'teacher3@example.com', N'星期五 14:00-15:00、15:00-16:00');
END;

-- 預設學生與老師補充資料：如果使用者已經建立過資料庫，也會自動補上缺少的範例資料。
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131003')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131003', N'陳怡君', N'資工二A', N'0918-222-333', N'chen@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131004')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131004', N'張庭瑋', N'資工二B', N'0920-555-666', N'chang@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131005')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131005', N'李承翰', N'資工二A', N'0930-777-888', N'lee@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131006')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131006', N'黃品睿', N'資工二C', N'0911-888-999', N'huang@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131007')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131007', N'吳佳蓉', N'資工二B', N'0921-123-456', N'wu@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131008')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131008', N'蔡孟軒', N'資工二C', N'0932-234-567', N'tsai@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131009')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131009', N'許雅婷', N'資工二A', N'0915-345-678', N'hsu@example.com');
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentNo = N'1131010')
    INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES (N'1131010', N'劉冠宇', N'資工二B', N'0926-456-789', N'liu@example.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'張老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'張老師', N'演算法', N'R1401', N'chang.teacher@example.com', N'星期二 15:00-16:00、17:00-18:00；星期五 11:00-12:00、13:00-14:00、15:00-16:00；星期六 16:00-17:00');
IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'王老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'王老師', N'資料庫系統', N'R1502', N'wang.teacher@example.com', N'星期一 09:00-10:00、10:00-11:00；星期三 15:00-16:00、16:00-17:00；星期四 11:00-12:00');
IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'蔡老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'蔡老師', N'計算機組織', N'R1508', N'tsai.teacher@example.com', N'星期一 13:00-14:00、14:00-15:00；星期四 10:00-11:00、13:00-14:00；星期六 09:00-10:00');
IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'吳老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'吳老師', N'人工智慧', N'R1603', N'wu.teacher@example.com', N'星期二 09:00-10:00；星期三 10:00-11:00、11:00-12:00；星期五 16:00-17:00、17:00-18:00');
IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'郭老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'郭老師', N'網路概論', N'R1610', N'kuo.teacher@example.com', N'星期一 15:00-16:00、16:00-17:00；星期四 14:00-15:00；星期日 10:00-11:00');
IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'劉老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'劉老師', N'軟體工程', N'R1706', N'liu.teacher@example.com', N'星期三 09:00-10:00；星期四 15:00-16:00、16:00-17:00；星期日 13:00-14:00、14:00-15:00');
IF NOT EXISTS (SELECT 1 FROM dbo.Teachers WHERE Name = N'鄭老師')
    INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES (N'鄭老師', N'作業系統', N'R1712', N'cheng.teacher@example.com', N'星期二 13:00-14:00、14:00-15:00；星期五 09:00-10:00、10:00-11:00；星期六 11:00-12:00');";

            ExecuteNonQuery(sql);
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                return ExecuteNonQuery(conn, sql, parameters);
            }
        }

        private static int ExecuteNonQuery(SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                return ExecuteScalar(conn, sql, parameters);
            }
        }

        private static object ExecuteScalar(SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        public static bool ValidateLogin(string username, string password)
        {
            object result = ExecuteScalar(
                "SELECT COUNT(*) FROM dbo.Users WHERE Username = @username AND Password = @password",
                P("@username", username), P("@password", password));
            return Convert.ToInt32(result) > 0;
        }

        public static void WriteLoginLog(string username)
        {
            Directory.CreateDirectory(LogFolder);
            string path = Path.Combine(LogFolder, "login_log.txt");
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + username + " 登入系統" + Environment.NewLine;
            File.AppendAllText(path, line, Encoding.UTF8);
        }

        public static DataTable GetAppointments(DateTime? startDate, DateTime? endDate, string keyword)
        {
            string sql = @"
SELECT 
    a.AppointmentId,
    CONVERT(VARCHAR(10), a.AppointmentDate, 120) AS AppointmentDate,
    CONVERT(VARCHAR(5), a.StartTime, 108) AS StartTime,
    CONVERT(VARCHAR(5), a.EndTime, 108) AS EndTime,
    s.Name AS StudentName,
    s.StudentNo,
    t.Name AS TeacherName,
    t.Subject,
    a.Topic,
    a.Status,
    ISNULL(a.Note, N'') AS Note
FROM dbo.Appointments a
INNER JOIN dbo.Students s ON a.StudentId = s.StudentId
INNER JOIN dbo.Teachers t ON a.TeacherId = t.TeacherId
WHERE 1 = 1";

            var list = new System.Collections.Generic.List<SqlParameter>();

            if (startDate.HasValue)
            {
                sql += " AND a.AppointmentDate >= @startDate";
                list.Add(P("@startDate", startDate.Value.Date));
            }
            if (endDate.HasValue)
            {
                sql += " AND a.AppointmentDate < @endDate";
                list.Add(P("@endDate", endDate.Value.Date));
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += @" AND (s.Name LIKE @keyword OR s.StudentNo LIKE @keyword OR t.Name LIKE @keyword OR t.Subject LIKE @keyword OR a.Topic LIKE @keyword OR a.Status LIKE @keyword)";
                list.Add(P("@keyword", "%" + keyword.Trim() + "%"));
            }

            sql += " ORDER BY a.AppointmentDate, a.StartTime";
            return GetDataTable(sql, list.ToArray());
        }

        public static bool HasAppointmentConflict(int teacherId, DateTime date, TimeSpan startTime, TimeSpan endTime, int ignoreAppointmentId)
        {
            string sql = @"
SELECT COUNT(*)
FROM dbo.Appointments
WHERE TeacherId = @teacherId
  AND AppointmentDate = @date
  AND Status <> N'已取消'
  AND AppointmentId <> @ignoreAppointmentId
  AND StartTime < @endTime
  AND EndTime > @startTime";

            object result = ExecuteScalar(sql,
                P("@teacherId", teacherId),
                P("@date", date.Date),
                P("@startTime", startTime),
                P("@endTime", endTime),
                P("@ignoreAppointmentId", ignoreAppointmentId));
            return Convert.ToInt32(result) > 0;
        }

        public static bool HasStudentTimeConflict(int studentId, DateTime date, TimeSpan startTime, TimeSpan endTime, int ignoreAppointmentId)
        {
            string sql = @"
SELECT COUNT(*)
FROM dbo.Appointments
WHERE StudentId = @studentId
  AND AppointmentDate = @date
  AND Status <> N'已取消'
  AND AppointmentId <> @ignoreAppointmentId
  AND StartTime < @endTime
  AND EndTime > @startTime";

            object result = ExecuteScalar(sql,
                P("@studentId", studentId),
                P("@date", date.Date),
                P("@startTime", startTime),
                P("@endTime", endTime),
                P("@ignoreAppointmentId", ignoreAppointmentId));
            return Convert.ToInt32(result) > 0;
        }

        public static bool StudentNoExists(string studentNo, int ignoreStudentId)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return false;

            object result = ExecuteScalar(@"
SELECT COUNT(*)
FROM dbo.Students
WHERE StudentNo = @studentNo
  AND StudentId <> @ignoreStudentId",
                P("@studentNo", studentNo.Trim()),
                P("@ignoreStudentId", ignoreStudentId));
            return Convert.ToInt32(result) > 0;
        }

        public static DataTable GetUpcomingAppointmentsWithinMinutes(int minutes)
        {
            string sql = @"
SELECT TOP 10
    CONVERT(VARCHAR(10), a.AppointmentDate, 120) AS AppointmentDate,
    CONVERT(VARCHAR(5), a.StartTime, 108) AS StartTime,
    s.Name AS StudentName,
    t.Name AS TeacherName,
    a.Topic
FROM dbo.Appointments a
INNER JOIN dbo.Students s ON a.StudentId = s.StudentId
INNER JOIN dbo.Teachers t ON a.TeacherId = t.TeacherId
WHERE a.Status = N'已預約'
  AND DATEADD(SECOND, DATEDIFF(SECOND, CAST('00:00:00' AS TIME), a.StartTime), CAST(a.AppointmentDate AS DATETIME)) >= GETDATE()
  AND DATEADD(SECOND, DATEDIFF(SECOND, CAST('00:00:00' AS TIME), a.StartTime), CAST(a.AppointmentDate AS DATETIME)) <= DATEADD(MINUTE, @minutes, GETDATE())
ORDER BY a.AppointmentDate, a.StartTime";
            return GetDataTable(sql, P("@minutes", minutes));
        }

        public static int CountAppointmentsByStudent(int studentId)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM dbo.Appointments WHERE StudentId = @id", P("@id", studentId));
            return Convert.ToInt32(result);
        }

        public static int CountAppointmentsByTeacher(int teacherId)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM dbo.Appointments WHERE TeacherId = @id", P("@id", teacherId));
            return Convert.ToInt32(result);
        }
    }
}
