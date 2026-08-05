# 🎓 StudyHub - Nền Tảng Quản Lý & Hỗ Trợ Học Tập Dành Cho Sinh Viên

> **StudyHub** là hệ thống hỗ trợ học tập toàn diện giúp sinh viên quản lý lịch học, lịch thi, danh sách công việc (Tasks/Kanban), đếm thời gian tập trung Pomodoro, tạo nhóm học tập và trợ lý trí tuệ nhân tạo (AI Assistant).

---

## 🛠️ Công Nghệ Sử Dụng

### **Backend**
- **Framework**: .NET 9.0 (ASP.NET Core Web API)
- **Database**: SQL Server + Entity Framework Core 9.0
- **Real-time**: SignalR (Chat nhóm & Thông báo thời gian thực)
- **Authentication**: JWT Bearer Token

### **Frontend**
- **Framework**: Angular 18 (Standalone Components)
- **Styling**: TailwindCSS & PrimeIcons
- **State & Services**: RxJS, HttpClient

---

## 📋 Yêu Cầu Tiền Trạm (Prerequisites)

Trước khi cài đặt, hãy đảm bảo máy tính của bạn đã cài đặt các công cụ sau:
1. [Node.js](https://nodejs.org/) (Phiên bản `>= 18.x`)
2. [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
3. [SQL Server](https://www.microsoft.com/sql-server/) (Hoặc SQL Server Express / LocalDB)
4. [Git](https://git-scm.com/)

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy Dự Án

### **Bước 1: Clone Repository**
Mở **Terminal** hoặc **Command Prompt / PowerShell** trên máy bạn và chạy:
```bash
git clone <URL_REPOSITORY_CUA_BAN>
cd StudyHub
```

---

### **Bước 2: Cấu Hình & Khởi Chạy Backend (.NET 9)**

1. **Cấu hình Chuỗi Kết Nối Database**:
   Mở file `src/StudyHub.Web/appsettings.json` và điều chỉnh chuỗi kết nối `DefaultConnection` phù hợp với máy của bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=StudyHubDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```
   *(Nếu bạn dùng tên instance SQL riêng như `.\SQLEXPRESS`, hãy thay `Server=localhost` thành `Server=.\\SQLEXPRESS`)*.

2. **Tự Động Tạo Database & Khởi Chạy Backend**:
   Chỉ cần chạy lệnh sau:
   ```bash
   dotnet watch --project src/StudyHub.Web/StudyHub.Web.csproj
   ```
   ➔ Backend sẽ **TỰ ĐỘNG TẠO DATABASE `StudyHubDb`** trên SQL Server (Entity Framework Core Auto-Migration) và khởi chạy API tại: `http://localhost:5186`.

---

### **Bước 3: Khởi Chạy Frontend (Angular 18)**

Mở một **Terminal mới** (giữ Terminal Backend đang chạy) tại thư mục gốc `StudyHub`:

1. **Cài đặt các gói phụ thuộc (Dependencies)**:
   ```bash
   npm install
   ```

2. **Chạy ứng dụng Frontend**:
   ```bash
   npm start
   ```
   *(Hoặc `npx ng serve`)*

3. **Truy Cập Web**:
   Mở trình duyệt web và truy cập địa chỉ:
   ```text
   http://localhost:4200
   ```

---

## 📌 Hướng Dẫn Sử Dụng Git Cho Thành Viên Nhóm

Nếu bạn làm việc theo nhóm, hãy tuân thủ luồng làm việc Git sau:

1. **Tạo nhánh riêng khi làm tính năng**:
   ```bash
   git checkout -b feature/ten-tinh-nang-cua-ban
   ```

2. **Lưu thay đổi & Push code lên GitHub**:
   ```bash
   git add .
   git commit -m "Mô tả ngắn gọn công việc đã làm"
   git push origin feature/ten-tinh-nang-cua-ban
   ```

3. **Tạo Pull Request (PR)** trên GitHub để trưởng nhóm xem và gộp code vào nhánh `main`.

---

## 📁 Cấu Trúc Thư Mục & Vị Trí Code Chính

> ⚠️ **LƯU Ý QUAN TRỌNG DÀNH CHO DEV & AI ASSISTANT**:

- 🔹 **Backend (.NET 9 Web API)**:
  - **Dự án chính (Startup Project)**: `src/StudyHub.Web/StudyHub.Web.csproj`
  - Dữ liệu & EF Core Migrations: `src/StudyHub.Infrastructure`
  - Core & Interfaces: `src/StudyHub.Core`

- 🔹 **Frontend (Angular 18 App)**:
  - **Mã nguồn Frontend CHÍNH đang chạy thực tế**: Thư mục `src/app/` (Được khai báo chuẩn trong `angular.json` và thực thi khi gõ `npm start`).
  - **Thư mục `frontend/`**: Là thư mục phụ/sao lưu. Mọi cập nhật code giao diện bắt buộc thực hiện trên `src/app/`.

```text
StudyHub/
├── src/
│   ├── StudyHub.Core/             # Entities, DTOs & Interfaces (Clean Architecture)
│   ├── StudyHub.Infrastructure/   # DbContext, EF Core Repositories & Migrations
│   ├── StudyHub.Web/              # Web API Host (Startup Project)
│   ├── app/                       # ⭐️ MÃ NGUỒN FRONTEND CHÍNH (Angular App)
│   ├── assets/                    # Static Assets (Images, Icons)
│   └── styles.css                 # Global CSS & Tailwind Directives
├── frontend/                      # Thư mục phụ/sao lưu Frontend
├── .gitignore                     # Cấu hình bỏ qua các file rác
├── angular.json                   # Cấu hình Angular Project (trỏ tới src/app)
├── package.json                   # Gói phụ thuộc Frontend
├── StudyHub.sln                   # Visual Studio Solution
└── README.md                      # Hướng dẫn dự án
```

---

## 🌟 Các Tính Năng Chính Trong Hệ Thống

- 🔒 **Xác thực**: Đăng ký, Đăng nhập, Mã OTP kích hoạt Email, Đổi mật khẩu.
- 📋 **Công việc (Tasks)**: Quản lý danh sách task, phân loại ưu tiên, xem dưới dạng Danh sách & Kanban Board.
- 📅 **Lịch học - Lịch thi**: Quản lý lịch học hàng ngày, phòng học, ca thi.
- ⏱️ **Pomodoro / Focus**: Bộ đếm thời gian học tập chuyên sâu, nhạc nền tập trung.
- 👥 **Nhóm học tập**: Tạo nhóm, nhắn tin real-time, chia sẻ tài liệu và lịch họp nhóm.
- 🤖 **AI Assistant**: Trợ lý thông minh hỗ trợ phân tích khối lượng học tập và đưa ra lời khuyên.
- 🔔 **Thông báo Realtime**: Cảnh báo deadline, nhắc lịch học/lịch thi và tin nhắn nhóm qua SignalR.
