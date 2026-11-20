# Acadex - Role-Based Assignment & Submission Management System

[![Acadex CI/CD Pipeline](https://github.com/rafiulshafin/Acadex/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/rafiulshafin/Acadex/actions/workflows/ci-cd.yml)
[![Docker](https://img.shields.io/badge/Docker-Multi--Container-blue?logo=docker)](https://www.docker.com/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20Web%20API-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-14%20App%20Router-black?logo=next.js)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL%2016-336791?logo=postgresql)](https://www.postgresql.org/)

**Acadex** is a full-stack, enterprise-grade, role-based educational management system engineered for schools, colleges, and training academies. It streamlines the lifecycle of coursework: allowing administrators to allocate teachers and manage cohorts, instructors to author assignments and evaluate submissions with quantitative marks and qualitative critique, and students to track deadlines and submit deliverables.

---

## 🚀 Quick Start (Single-Tap Launch)

Acadex is configured for immediate evaluation. A single command or script tap provisions PostgreSQL 16, executes EF Core database migrations, seeds demonstration accounts, builds the ASP.NET Core API, compiles the Next.js frontend, and launches the application in your default browser.

### Windows (Single-Tap)
Double-click `start.bat` in the project root, or execute:
```cmd
start.bat
```

### Linux / macOS (Single-Tap)
Execute the launch script:
```bash
chmod +x start.sh
./start.sh
```

### Direct Docker Compose
```bash
docker compose up -d --build
```
*Note: If your system uses Docker Compose v1, use `docker-compose up -d --build`.*

- **Frontend Application:** [http://localhost:3000](http://localhost:3000)
- **Backend API & Swagger Documentation:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **API Health Check:** [http://localhost:5000/health](http://localhost:5000/health)

---

## 🔑 Demo Login Credentials

The database is pre-seeded with verified demonstration accounts across all three required roles. You can log in manually or use the **Fast Demo Accounts (One-Click Login)** buttons directly on the login screen.

| Role | Email Address | Password | Permissions & Scope |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@acadex.edu` | `Admin@12345` | Global system control, user provisioning, class/subject catalog, teacher allocations, global submission audit. |
| **Teacher** | `teacher@acadex.edu` | `Teacher@12345` | Create/edit assignments for allocated classes, view student submissions, assign marks (0–Max), provide feedback, update submission statuses. |
| **Student** | `student@acadex.edu` | `Student@12345` | Enrolled in *Computer Science 2026 (Section A)*. View class assignments, submit text & repo links, update submissions before deadline, inspect grades and feedback. |

*Additional Seeded Accounts:*
- **Secondary Teacher:** `sarah.teacher@acadex.edu` / `Teacher@12345`
- **Secondary Student:** `alex.student@acadex.edu` / `Student@12345`

---

## 🏛️ System Architecture

```
                    +------------------------------------------+
                    |           Web Browser / Client           |
                    |       (Next.js 14 App Router, TS)        |
                    +--------------------+---------------------+
                                         |
                                         | HTTP / REST (JWT Bearer)
                                         v
                    +--------------------+---------------------+
                    |       ASP.NET Core 8 Web API             |
                    |  - JWT Authentication & Role Authorization|
                    |  - Fluent / DataAnnotation Validation    |
                    |  - Global RFC 7807 Error Middleware      |
                    |  - Interactive Swagger / OpenAPI UI      |
                    +--------------------+---------------------+
                                         |
                                         | Entity Framework Core
                                         v
                    +--------------------+---------------------+
                    |       PostgreSQL 16 Database             |
                    |  - Automatic Schema Migration & Seeding  |
                    |  - Normalized Relational Tables          |
                    |  - Unique & Composite Index Constraints  |
                    +------------------------------------------+
```

---

## 👥 Role Permissions Matrix

| Feature / Action | Admin | Teacher | Student |
| :--- | :---: | :---: | :---: |
| **Login & JWT Token Authentication** | ✅ | ✅ | ✅ |
| **View Role-Tailored Dashboard & Live Stats** | ✅ | ✅ | ✅ |
| **Create / Update / Deactivate User Accounts** | ✅ | ❌ | ❌ |
| **Manage Classes, Sections & Batches** | ✅ | ❌ | ❌ |
| **Manage Subjects & Curriculum Codes** | ✅ | ❌ | ❌ |
| **Assign Teachers to Class & Subject** | ✅ | ❌ | ❌ |
| **Create & Publish Assignments** | ✅ | ✅ (Allocated) | ❌ |
| **Draft / Hide Unfinished Assignments** | ✅ | ✅ | ❌ |
| **Delete Assignments** | ✅ | ✅ (Owned) | ❌ |
| **View Student Submissions** | ✅ (All) | ✅ (Allocated) | ✅ (Own Only) |
| **Submit Solution & Attachment URLs** | ❌ | ❌ | ✅ |
| **Update Submission Prior to Deadline** | ❌ | ❌ | ✅ |
| **Grade Submissions & Provide Feedback** | ✅ | ✅ (Allocated) | ❌ |
| **Change Submission Status** | ✅ | ✅ (Allocated) | ❌ |

---

## 🛠️ Technology Stack

### Backend
- **Framework:** ASP.NET Core 8.0 Web API (C#)
- **Data Access:** Entity Framework Core 8.0 with Npgsql (PostgreSQL provider)
- **Authentication & Security:** JWT Bearer tokens, BCrypt password hashing, role-based authorization policies
- **Documentation:** Swagger / OpenAPI via Swashbuckle with JWT Bearer authorization injection
- **Logging & Diagnostics:** ASP.NET Core structured logging, custom global exception handling middleware, health check probes (`/health`)

### Frontend
- **Framework:** Next.js 14 (App Router)
- **Language:** TypeScript 5
- **UI Styling:** Tailwind CSS 3.4
- **Icons:** Lucide React
- **HTTP Client:** Axios with JWT request interceptor and automatic session management

### Testing
- **Framework:** xUnit
- **Assertion Library:** FluentAssertions
- **Mocking:** Moq
- **Database Isolation:** Microsoft.EntityFrameworkCore.InMemory
- **Coverage Tool:** Coverlet collector

### DevOps & Containerization
- **Container Engine:** Docker & Docker Compose
- **Base Images:** `mcr.microsoft.com/dotnet/aspnet:8.0`, `node:20-alpine`, `postgres:16-alpine`
- **CI/CD Pipeline:** GitHub Actions (`.github/workflows/ci-cd.yml`) with automated testing, linting, type-checking, and Docker container verification

---

## 📁 Repository Structure

```
Acadex/
├── .github/
│   └── workflows/
│       └── ci-cd.yml                # Automated GitHub Actions CI/CD pipeline
├── backend/
│   ├── Acadex.Api/
│   │   ├── Controllers/             # RESTful API Controllers
│   │   ├── Data/                    # DbContext & Automatic DbInitializer
│   │   ├── Middleware/              # Global RFC 7807 Exception Middleware
│   │   ├── Models/
│   │   │   ├── DTOs/                # Strongly-typed request/response models
│   │   │   ├── Entities/            # Relational domain entities
│   │   │   └── Enums/               # UserRole, AssignmentStatus, SubmissionStatus
│   │   ├── Services/                # Encapsulated business logic services
│   │   ├── Dockerfile               # Multi-stage ASP.NET Core build
│   │   └── Program.cs               # API application pipeline & DI container
│   ├── Acadex.Tests/
│   │   ├── UnitTests/               # xUnit unit tests for business rules & auth
│   │   └── TestHelpers/             # In-memory test context utilities
│   └── Acadex.sln                   # Visual Studio Solution
├── frontend/
│   ├── src/
│   │   ├── app/                     # Next.js App Router (Dashboard, Pages, Auth)
│   │   ├── components/              # Reusable UI components, Modals, Navbar, Sidebar
│   │   ├── contexts/                # AuthContext with token persistence
│   │   ├── lib/                     # Axios instance & utility functions
│   │   └── types/                   # Unified TypeScript interfaces
│   ├── Dockerfile                   # Multi-stage Next.js standalone runner
│   ├── package.json
│   └── tailwind.config.js
├── docker-compose.yml               # Multi-container orchestration (Postgres, API, Web)
├── .env.example                     # Environment variables template
├── start.bat                        # Windows 1-tap launcher
├── start.sh                         # Linux/macOS 1-tap launcher
└── README.md                        # Project documentation
```

---

## 💻 Manual Local Setup (Without Docker)

If you prefer to execute the services natively on your host machine:

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+ and npm](https://nodejs.org/)
- Running PostgreSQL instance (or Docker Postgres container)

### 2. Database Configuration
Ensure PostgreSQL is active. In `backend/Acadex.Api/appsettings.json`, confirm or adjust the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=acadexdb;Username=postgres;Password=postgres"
}
```

### 3. Run the Backend API
```bash
cd backend/Acadex.Api
dotnet run
```
The API will start at `http://localhost:5000`. Database tables and initial seed data will automatically be created on startup! Explore the endpoints at `http://localhost:5000/swagger`.

### 4. Run the Frontend Web Application
In a separate terminal:
```bash
cd frontend
npm install
npm run dev
```
Open [http://localhost:3000](http://localhost:3000) in your browser.

---

## 🧪 Running Unit Tests

The test suite thoroughly verifies core business rules, permission boundaries, submission deadlines, and grading integrity.

Execute tests using the .NET CLI:
```bash
dotnet test backend/Acadex.Tests/Acadex.Tests.csproj --verbosity normal
```

### Covered Test Cases:
1. **Authentication:**
   - Successful login with BCrypt password verification and JWT token issuance.
   - Rejection of invalid credentials with `UnauthorizedException`.
   - Rejection of deactivated/inactive accounts.
2. **Assignments:**
   - Teacher creation allowed when allocated to subject & class.
   - Teacher creation rejected with `ForbiddenException` when not allocated.
   - Student queries strictly return published assignments for their enrolled cohort.
   - Draft assignments hidden from student views.
3. **Submissions:**
   - Submission permitted prior to deadline.
   - Rejection of submissions past deadline when `AllowLateSubmissions = false`.
   - Update of submission permitted before deadline or with `ResubmissionAllowed`.
   - Prohibition of submission updates once deadline expires or when already graded.
   - Grading validation: rejection of marks exceeding `MaxMarks` or negative marks.
4. **User Management:**
   - Prevention of duplicate email addresses with `ConflictException`.
   - Safe deactivation preserves historical referential integrity.

---

## 🌐 API Endpoints Reference

| Method | Endpoint | Authorized Roles | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Public | Authenticates credentials and returns JWT bearer token. |
| `GET` | `/api/auth/me` | Authenticated | Returns current authenticated user profile. |
| `GET` | `/api/users` | Admin | Search and filter users by role and keyword. |
| `POST` | `/api/users` | Admin | Provisions a new user account. |
| `DELETE` | `/api/users/{id}` | Admin | Deactivates a user account. |
| `GET` | `/api/classes` | Authenticated | Lists all academic classes and student cohorts. |
| `POST` | `/api/classes` | Admin | Creates a new academic class/course. |
| `GET` | `/api/subjects` | Authenticated | Lists all registered curriculum subjects. |
| `POST` | `/api/subjects` | Admin | Creates a new curriculum subject code. |
| `GET` | `/api/classsubjects` | Authenticated | Lists teacher-to-course allocations. |
| `POST` | `/api/classsubjects` | Admin | Assigns a teacher to a class & subject. |
| `GET` | `/api/assignments` | Authenticated | Lists assignments (scoped by role & class). |
| `POST` | `/api/assignments` | Teacher, Admin | Creates a new assignment. |
| `PUT` | `/api/assignments/{id}` | Teacher, Admin | Updates an assignment. |
| `DELETE` | `/api/assignments/{id}`| Teacher, Admin | Deletes an assignment and associated submissions. |
| `GET` | `/api/submissions` | Authenticated | Lists submissions (scoped by role). |
| `POST` | `/api/submissions` | Student | Submits an assignment response and link. |
| `PUT` | `/api/submissions/{id}` | Student | Updates submission before deadline. |
| `POST` | `/api/submissions/{id}/grade` | Teacher, Admin | Grades a submission and saves feedback. |
| `GET` | `/api/dashboard/admin` | Admin | Retrieves system-wide operational metrics. |
| `GET` | `/api/dashboard/teacher` | Teacher | Retrieves instructor-specific stats. |
| `GET` | `/api/dashboard/student` | Student | Retrieves student coursework stats and average. |

---

## 🔄 CI/CD Pipeline Workflow

The repository includes a production-grade GitHub Actions CI/CD configuration (`.github/workflows/ci-cd.yml`):

1. **Backend CI Job:** Restores NuGet packages, builds the solution in Release configuration, and runs all xUnit unit tests with Coverlet code coverage.
2. **Frontend CI Job:** Validates TypeScript types (`tsc --noEmit`), installs dependencies via `npm ci`, and builds the Next.js production bundle.
3. **Docker Compose CI Job:** Validates Docker Compose schema syntax and builds multi-stage container images for both backend and frontend using Docker Buildx.
4. **Delivery Verification Job:** Confirms end-to-end readiness for production deployment.

---

## 💡 Assumptions & Design Decisions

1. **Student Class Enrollment:** Each student is enrolled in one primary class/cohort (e.g. *Computer Science 2026 - Section A*). Assignments assigned to that class are automatically routed to the student.
2. **Resubmission Workflow:** Students are permitted to update their submitted content and links prior to the assignment deadline. After the deadline passes, editing is locked unless an instructor explicitly designates the submission status as `ResubmissionAllowed`.
3. **Database Portability:** EF Core `EnsureCreatedAsync` with relational seeding is executed on application startup. Evaluators can launch the application against any empty PostgreSQL instance without having to manually execute SQL scripts or table migrations.
4. **Security & Secrets:** All connection strings and JWT signing secrets are read from environment variables with sensible local fallback defaults for testing, complying with security standards.

---

## 📝 License & Attribution
Developed for the OnnoRokom Projukti Limited Assistant Software Engineer Recruitment Evaluation (August 2026).
All rights reserved.
