# 學生諮詢預約系統

## Student Consultation Appointment System

本專案為「視窗程式設計 (II)」期末專題，使用 **C# Windows Forms** 開發，並搭配 **SQL Server Express LocalDB** 作為資料庫。

系統主要提供學生諮詢預約管理功能。登入後會直接進入「可視化課表主畫面」，使用者可以透過課表查看老師可諮詢時間、已預約時段與預約狀態，並能直接點擊課表格子新增預約或查看詳細資料。

本專案也加入 UI 美化、動畫效果、背景音樂切換與小遊戲中心，讓整體操作更直覺、流暢且具有互動性。

---

## demo 影片

[![demo影片請開起字幕](<img width="960" height="564" alt="image" src="https://github.com/user-attachments/assets/89c294f3-2b10-4d65-805f-d1fe0bc98295" />
)](https://youtu.be/wAFq3Y0p8aU?si=9b7KTXv5biivXU6x)

---

## 一、開發環境

| 項目        | 內容                         |
| --------- | -------------------------- |
| 開發工具      | Visual Studio 2022         |
| 程式語言      | C#                         |
| 專案類型      | Windows Forms App          |
| Framework | .NET Framework 4.8         |
| 資料庫       | SQL Server Express LocalDB |
| 作業系統      | Windows 10 / Windows 11    |

---

## 二、預設登入帳號

| 帳號    | 密碼   |
| ----- | ---- |
| admin | 1234 |

登入後會進入學生諮詢預約系統主畫面。

---

## 三、系統畫面截圖

> 請將截圖放在 `docs/screenshots/` 資料夾中，並依照下列檔名命名。
> 若檔名不同，請自行修改圖片路徑。

### 1. 登入畫面

<img width="316" height="249" alt="image" src="https://github.com/user-attachments/assets/c88063c0-608a-45fa-8388-f28eb9ef6bb7" />

### 2. 可視化課表主畫面

<img width="960" height="564" alt="image" src="https://github.com/user-attachments/assets/46adfc27-383c-47d5-b322-d9fc6bceb114" />


### 3. 學生管理畫面

<img width="841" height="519" alt="image" src="https://github.com/user-attachments/assets/0954deaa-dacf-4f33-a241-fe3664516c8a" />


### 4. 老師管理畫面

<img width="871" height="542" alt="image" src="https://github.com/user-attachments/assets/62ee8323-cb0d-4f1c-9547-b41eda0377f9" />


### 5. 老師可諮詢時間設定畫面

<img width="691" height="519" alt="image" src="https://github.com/user-attachments/assets/abd6e372-4b38-4597-9d91-145573caeba4" />


### 6. 新增預約畫面

<img width="421" height="493" alt="image" src="https://github.com/user-attachments/assets/3394732c-432b-49b8-936e-1ddcb7ccfc5b" />


### 7. 預約詳細資訊畫面

<img width="466" height="444" alt="image" src="https://github.com/user-attachments/assets/f0cbdf30-417a-4754-b135-db633468c66d" />


### 8. 小遊戲中心

<img width="586" height="384" alt="image" src="https://github.com/user-attachments/assets/95fcb1be-7c52-4f2f-a7c0-ed84c69d3c43" />


---

## 四、主要功能

### 1. 管理員登入

系統提供管理員登入功能，預設帳號為 `admin`，密碼為 `1234`。
登入成功後會進入主畫面，並將登入紀錄寫入：

```text
Logs/login_log.txt
```

---

### 2. 學生資料管理

學生管理功能包含：

* 新增學生資料
* 修改學生資料
* 刪除學生資料
* 顯示學生清單
* 學號不可重複，避免建立兩筆相同學生
* 輸入欄位支援 Enter 快速跳到下一格
* Shift + Enter 可回到上一格

學生資料包含：

* 學號
* 姓名
* 班級
* 電話
* Email

---

### 3. 老師資料管理

老師管理功能包含：

* 新增老師資料
* 修改老師資料
* 刪除老師資料
* 顯示老師清單
* 設定老師科目與辦公室
* 使用視覺化課表設定老師可諮詢時間

老師資料包含：

* 老師姓名
* 科目
* 辦公室
* Email

---

### 4. 老師可諮詢時間設定

老師可諮詢時間使用視覺化課表設定。
管理者可以直接點擊課表格子來開放或取消老師的可諮詢時段。

規則如下：

* 諮詢時間以整點為單位
* 例如：`09:00-10:00`、`10:00-11:00`
* 點擊格子即可切換開放或取消
* 老師沒有開放的時間不可被預約

---

### 5. 可視化課表主畫面

登入後主畫面即為可視化課表，不需要另外開啟課表視窗。

主畫面提供以下資訊：

* 老師可諮詢時段
* 已預約時段
* 預約狀態
* 不同老師以不同顏色顯示
* 同一格若有多位老師，會使用分隔線切開
* 已過期時段會顯示為「已過期」
* 點擊可預約時段可直接新增預約
* 點擊已預約時段可查看預約詳細資料

---

### 6. 篩選與搜尋功能

主畫面上方提供多種篩選功能：

* 預約週
* 科目
* 老師
* 是否有預約
* 預約狀態

另外也支援關鍵字搜尋，可搜尋：

* 學生姓名
* 學號
* 老師姓名
* 科目
* 預約主題
* 預約狀態

---

### 7. 預約新增與限制

使用者可以點擊可預約時段新增預約，系統會自動帶入：

* 老師
* 日期
* 開始時間
* 結束時間

預約限制如下：

* 同一位老師同一天同時段不可重複預約
* 同一位學生同一天同時段不可預約兩位老師
* 老師沒有開放的星期與時段不能新增預約
* 不可新增或修改為過去時間的「已預約」資料
* 已完成或已取消的歷史資料會保留，方便日後查詢

---

### 8. 預約詳細資訊

點擊已預約時段後，會開啟預約詳細資訊彈窗。

詳細資訊包含：

* 學生姓名
* 學號
* 班級
* 老師姓名
* 科目
* 辦公室
* 日期
* 時間
* 主題
* 備註
* 預約狀態

使用者可以在詳細資訊視窗中：

* 修改預約
* 標記為已完成
* 取消預約
* 恢復為已預約

---

### 9. 預約狀態管理

系統提供三種預約狀態：

| 狀態  | 說明     |
| --- | ------ |
| 已預約 | 預約尚未完成 |
| 已完成 | 諮詢已完成  |
| 已取消 | 預約已取消  |

可在詳細資訊彈窗或主畫面右側本週預約列表中快速修改狀態。

---

### 10. 本週預約列表

主畫面右側提供精簡的本週預約列表。

功能包含：

* 顯示本週預約資料
* 雙擊預約可查看詳細資訊
* 可快速標記完成
* 可快速取消預約
* 搭配主畫面課表同步查看預約狀態

---

### 11. 15 分鐘內預約提醒

登入後系統會自動檢查 15 分鐘內即將開始的預約。
如果有即將開始的預約，會提醒使用者注意。

---

### 12. 背景音樂切換

主畫面提供背景音樂功能，並支援切換曲目。

使用方式：

1. 將 `.wav` 音樂檔放入：

```text
StudentConsultationScheduler/Assets/
```

2. 重新建置專案後，程式會自動讀取 `Assets` 資料夾中的 `.wav` 檔。
3. 可在主畫面上方的「曲目」下拉選單切換音樂。
4. 若背景音樂正在播放，切換曲目後會立即更換並循環播放。
5. 若音樂目前是關閉狀態，切換曲目只會記住選擇，不會自動播放。

注意：本功能使用 `System.Media.SoundPlayer`，只支援 `.wav`，不支援 `.mp3`。

---

### 13. 小遊戲中心

主畫面上方提供「小遊戲」按鈕，整合三個 WinForms 小作品：

#### 五張撲克

* 可下注
* 發牌
* 選擇保留牌
* 換牌
* 依牌型計算獎金

#### 西洋棋小遊戲

* 8x8 棋盤
* 輪流移動
* 合法步提示
* 吃子
* 升變
* 悔棋
* 將軍提示

#### 簡易電子琴

* 提供 Do、Re、Mi、Fa、Sol、La、Si、Do 八個琴鍵
* 可用滑鼠點擊演奏
* 可用鍵盤演奏音階
* 可作為背景音樂展示功能

---

## 五、UI 美化與操作優化

本專案針對畫面美觀與操作流暢度進行多項優化：

* 全系統主要表單使用一致的字體、按鈕、表格與輸入框樣式
* 表格加入交錯底色，提升可讀性
* 選取列會高亮顯示
* 表格滑鼠移入列會有提示效果
* 輸入欄位聚焦時會以淡黃色提示
* 按鈕有滑過與按下的微互動效果
* 所有主要視窗加入淡入動畫
* 主畫面課表加入 hover 外框提示
* 點擊課表時加入短暫脈衝框動畫
* 新增預約、更新狀態、刪除預約後會出現右下角滑入式提示
* 學生、老師、預約表單支援 Enter 快速切換欄位
* 多行備註欄可使用 Ctrl + Enter 換行

---

## 六、專案結構

```text
StudentConsultationScheduler/
├── StudentConsultationScheduler.sln
├── StudentConsultationScheduler/
│   ├── Program.cs
│   ├── Data/
│   │   └── Db.cs
│   ├── Assets/
│   │   ├── bgm.wav
│   │   └── 其他 .wav 背景音樂檔
│   ├── Forms/
│   │   ├── LoginForm.cs
│   │   ├── MainForm.cs
│   │   ├── StudentForm.cs
│   │   ├── TeacherForm.cs
│   │   ├── AppointmentForm.cs
│   │   ├── AppointmentDetailForm.cs
│   │   ├── AvailabilityEditorForm.cs
│   │   ├── ScheduleForm.cs
│   │   ├── GameCenterForm.cs
│   │   ├── PokerGameForm.cs
│   │   ├── ChessMiniGameForm.cs
│   │   └── BeepPlayerForm.cs
│   └── App.config
├── database.sql
├── docs/
│   ├── 使用說明書.txt
│   ├── LocalDB錯誤修正說明.txt
│   └── screenshots/
│       ├── 01_login.png
│       ├── 02_main_schedule.png
│       ├── 03_student_manage.png
│       ├── 04_teacher_manage.png
│       ├── 05_availability_editor.png
│       ├── 06_add_appointment.png
│       ├── 07_appointment_detail.png
│       └── 08_game_center.png
├── README.md
└── .gitignore
```

---

## 七、執行方式

### 方法一：使用 Visual Studio 2022 執行

1. 下載或解壓縮本專案。
2. 使用 Visual Studio 2022 開啟：

```text
StudentConsultationScheduler.sln
```

3. 確認電腦已安裝 SQL Server Express LocalDB。
4. 按下 `F5` 執行專案。
5. 第一次執行時，程式會自動建立資料庫與測試資料。
6. 使用預設帳號登入：

```text
帳號：admin
密碼：1234
```

---

## 八、資料庫說明

本專案使用 SQL Server Express LocalDB。

新版資料庫名稱為：

```text
StudentConsultationSchedulerDB_Stable
```

資料庫檔案會自動建立在：

```text
%LOCALAPPDATA%\StudentConsultationScheduler\App_Data
```

這樣可以避免專案重新解壓縮、改名或搬移後，因為舊的 `bin\Debug` 路徑不存在而導致資料庫無法開啟。

常見錯誤如下：

```text
Cannot open database "StudentConsultationDB" requested by the login.
Unable to open the physical file
```

若遇到資料庫路徑錯誤，可刪除舊資料夾後重新執行新版專案。

---

## 九、使用流程

1. 開啟程式後，先進入登入畫面。
2. 使用 `admin / 1234` 登入。
3. 登入後進入可視化課表主畫面。
4. 使用上方篩選功能查看不同週次、科目、老師與預約狀態。
5. 點擊可預約時段，開啟新增預約視窗。
6. 填寫學生、主題與備註後建立預約。
7. 點擊已預約時段，查看預約詳細資訊。
8. 在詳細資訊視窗中可修改預約、標記完成或取消預約。
9. 到學生管理頁面可新增、修改、刪除學生資料。
10. 到老師管理頁面可新增、修改、刪除老師資料。
11. 在老師管理中可使用視覺化課表設定老師可諮詢時間。
12. 需要展示延伸功能時，可點擊主畫面上方的小遊戲中心。

---

## 十、DEMO 影片展示建議流程

建議 DEMO 影片控制在 5 分鐘左右，可依照以下順序展示：

1. 登入系統，展示預設帳號登入。
2. 展示登入後主畫面已經是可視化課表。
3. 展示上方篩選功能：預約週、科目、老師、是否有預約、預約狀態。
4. 進入學生管理，新增學生並示範學號不可重複。
5. 進入老師管理，示範用視覺化課表設定老師可諮詢時間。
6. 回到主畫面，點擊可預約時段快速建立預約。
7. 點擊已預約時段，展示預約詳細資訊彈窗。
8. 在彈窗中把預約標記為已完成，再用狀態篩選查看。
9. 示範取消預約後，該時段可重新預約。
10. 示範同一學生同時段預約另一位老師會被系統阻擋。
11. 展示右側本週預約列表，說明雙擊可查看預約詳細資料。
12. 最後展示背景音樂切換與小遊戲中心。

---

## 十一、GitHub 繳交注意事項

上傳 GitHub 前，請確認不要上傳以下資料夾或檔案：

```text
.vs/
bin/
obj/
.git/
Logs/
*.mdf
*.ldf
```

本專案已提供 `.gitignore`，用來避免編譯檔、暫存檔、資料庫檔與執行紀錄被上傳。

---

## 十二、資料來源與參考

本專題部分延伸功能參考以下資料與作品概念：

* Poker 小遊戲功能概念參考：
  https://github.com/TienPingLi/Poker

* ChessGame 小遊戲功能概念參考：
  https://github.com/TienPingLi/ChessGame

* BeepPlayer 簡易電子琴功能概念參考：
  https://github.com/TienPingLi/BeepPlayer

* Microsoft Learn：Windows Forms 文件
  https://learn.microsoft.com/dotnet/desktop/winforms/

* Microsoft Learn：SQL Server LocalDB 說明
  https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb

---

## 十三、版本更新紀錄

### 2026-06-17 修正

* 修正主畫面啟動時 SplitContainer 尚未完成排版，可能出現 `SplitterDistance 必須介於 Panel1MinSize 和 Width - Panel2MinSize 之間` 的錯誤。
* 主畫面右側改為精簡的本週預約列表，移除過度擁擠的統計區與圖例。
* 新增不可預約過去時間限制。
* 修正 LocalDB 建立位置，避免專案搬移後資料庫路徑失效。
* 音樂檔複製方式改為 `PreserveNewest`，減少建置時間。

### 本版新增功能

* 登入後主畫面直接顯示可視化課表。
* 老師可諮詢時間改為視覺化課表設定。
* 新增預約詳細資訊彈窗。
* 支援預約狀態管理：已預約、已完成、已取消。
* 支援背景音樂切換。
* 新增小遊戲中心：五張撲克、西洋棋小遊戲、簡易電子琴。
* 加入 UI 美化、人性化操作與動畫效果。
* 表單支援 Enter / Shift + Enter 快速切換欄位。

---

## 十四、作者

| 項目   | 內容          |
| ---- | ----------- |
| 學號   | 1131427     |
| 姓名   | 田秉立         |
| 課程   | 視窗程式設計 (II) |
| 專題名稱 | 學生諮詢預約系統    |
