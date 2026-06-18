-- Student Consultation Appointment System
-- SQL Server Express LocalDB database script
-- 程式會自動建立資料庫與資料表；此檔案提供給報告或手動檢查使用。

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(50) NOT NULL,
    DisplayName NVARCHAR(50) NOT NULL
);

CREATE TABLE dbo.Students
(
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentNo NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(50) NOT NULL,
    ClassName NVARCHAR(50) NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(100) NULL
);

CREATE TABLE dbo.Teachers
(
    TeacherId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Subject NVARCHAR(100) NULL,
    Office NVARCHAR(50) NULL,
    Email NVARCHAR(100) NULL,
    AvailableTime NVARCHAR(500) NULL
);

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

INSERT INTO dbo.Users (Username, Password, DisplayName)
VALUES (N'admin', N'1234', N'系統管理員');


-- 範例學生資料：學號不可重複。
INSERT INTO dbo.Students (StudentNo, Name, ClassName, Phone, Email) VALUES
(N'1131427', N'田秉立', N'資工二A', N'0912-345-678', N'student@example.com'),
(N'1131001', N'王小明', N'資工二A', N'0922-111-222', N'wang@example.com'),
(N'1131002', N'林小華', N'資工二B', N'0933-333-444', N'lin@example.com'),
(N'1131003', N'陳怡君', N'資工二A', N'0918-222-333', N'chen@example.com'),
(N'1131004', N'張庭瑋', N'資工二B', N'0920-555-666', N'chang@example.com'),
(N'1131005', N'李承翰', N'資工二A', N'0930-777-888', N'lee@example.com'),
(N'1131006', N'黃品睿', N'資工二C', N'0911-888-999', N'huang@example.com'),
(N'1131007', N'吳佳蓉', N'資工二B', N'0921-123-456', N'wu@example.com'),
(N'1131008', N'蔡孟軒', N'資工二C', N'0932-234-567', N'tsai@example.com'),
(N'1131009', N'許雅婷', N'資工二A', N'0915-345-678', N'hsu@example.com'),
(N'1131010', N'劉冠宇', N'資工二B', N'0926-456-789', N'liu@example.com');

-- 範例老師資料：可諮詢時段由程式視覺化課表點選後自動產生，不提供手動亂填。
INSERT INTO dbo.Teachers (Name, Subject, Office, Email, AvailableTime) VALUES
(N'黃老師', N'視窗程式設計', N'R1102', N'teacher1@example.com', N'星期三 13:00-14:00、14:00-15:00'),
(N'陳老師', N'資料結構', N'R1203', N'teacher2@example.com', N'星期二 10:00-11:00、11:00-12:00'),
(N'林老師', N'機率與統計', N'R1305', N'teacher3@example.com', N'星期五 14:00-15:00、15:00-16:00'),
(N'張老師', N'演算法', N'R1401', N'chang.teacher@example.com', N'星期二 15:00-16:00、17:00-18:00；星期五 11:00-12:00、13:00-14:00、15:00-16:00；星期六 16:00-17:00'),
(N'王老師', N'資料庫系統', N'R1502', N'wang.teacher@example.com', N'星期一 09:00-10:00、10:00-11:00；星期三 15:00-16:00、16:00-17:00；星期四 11:00-12:00'),
(N'蔡老師', N'計算機組織', N'R1508', N'tsai.teacher@example.com', N'星期一 13:00-14:00、14:00-15:00；星期四 10:00-11:00、13:00-14:00；星期六 09:00-10:00'),
(N'吳老師', N'人工智慧', N'R1603', N'wu.teacher@example.com', N'星期二 09:00-10:00；星期三 10:00-11:00、11:00-12:00；星期五 16:00-17:00、17:00-18:00'),
(N'郭老師', N'網路概論', N'R1610', N'kuo.teacher@example.com', N'星期一 15:00-16:00、16:00-17:00；星期四 14:00-15:00；星期日 10:00-11:00'),
(N'劉老師', N'軟體工程', N'R1706', N'liu.teacher@example.com', N'星期三 09:00-10:00；星期四 15:00-16:00、16:00-17:00；星期日 13:00-14:00、14:00-15:00'),
(N'鄭老師', N'作業系統', N'R1712', N'cheng.teacher@example.com', N'星期二 13:00-14:00、14:00-15:00；星期五 09:00-10:00、10:00-11:00；星期六 11:00-12:00');


-- 若從舊版資料庫升級，請補上預約狀態欄位。
IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL AND COL_LENGTH('dbo.Appointments', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.Appointments ADD Status NVARCHAR(20) NOT NULL DEFAULT N'已預約';
END;
