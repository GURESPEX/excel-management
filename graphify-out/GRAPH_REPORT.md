# Graph Report - .  (2026-09-15)

## Corpus Check
- 162 files · ~200,021 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 973 nodes · 1719 edges · 68 communities (46 shown, 22 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 51 edges (avg confidence: 0.81)
- Token cost: 379,607 input · 162,685 output

## Community Hubs (Navigation)
- Employee/Department DTOs & Queries
- Frontend Dev Dependencies (npm)
- EF Core Database Migrations
- Employee API Integration Tests
- Department Repository & DTOs
- Frontend Runtime Dependencies (npm)
- .NET Solution & Project Files
- Audit Log Application Layer
- Auth Integration Tests
- App Shell & Auth Store
- System Architecture Diagram (overview)
- Employee Import & Row Validation
- Frontend tsconfig (app)
- shadcn/ui Table & Form Components
- Application Layer Namespaces & DI
- Employee Import/Export Tests
- Beads Issue Tracker & CI Publish Pipeline
- shadcn/ui components.json Config
- EF Core AppDbContext & Audit Capture
- Frontend API Hooks (employees/depts/audit)
- Auth Domain & API Wiring
- Frontend tsconfig (node)
- Employee List Page & Export Controls
- shadcn/ui Select & Checkbox Components
- Dev Launch Profiles (launchSettings.json)
- shadcn/ui Alert Dialog Component
- Clean Architecture Diagram (layers)
- JWT Token Service
- Clean Architecture Diagram (API/DB detail)
- Clean Architecture Diagram (endpoints detail)
- Refresh Token Repository
- Clean Architecture Diagram (full stack)
- User Repository & Domain
- Frontend Test Setup & MSW Server
- Audit Log Domain & Change Tracking
- Oxlint Rule Config
- DatePicker Component
- CLAUDE.md/AGENTS.md Beads Instructions
- Employee CSV Writer
- Department & Employee Domain Entities
- Frontend App Entry & Router
- OpenAPI Generated Schema Types
- Password Hasher
- Root tsconfig References
- Department Validator
- Infrastructure DI Registration
- Employee Validator
- CI success() Guard Pattern
- Beads Rust CLI & Viewer Sidecar
- post-checkout Git Hook
- post-merge Git Hook
- pre-commit Git Hook
- pre-push Git Hook
- prepare-commit-msg Git Hook
- Vite React Plugin Choice (Oxc vs SWC)
- Beads JSONL Auto-Export Setting
- Non-Interactive Shell Commands Rule
- Favicon Icon
- Bluesky Icon
- Discord Icon
- Documentation Icon
- GitHub Icon
- Social/Contacts Icon
- X (Twitter) Icon
- Oxlint Configuration (top-level)
- React Compiler Setting

## God Nodes (most connected - your core abstractions)
1. `ExcelManagement.Application.Employees` - 24 edges
2. `AuthTests` - 21 edges
3. `compilerOptions` - 19 edges
4. `ExcelManagement.Infrastructure.Persistence` - 18 edges
5. `ExcelManagement.Application.Departments` - 17 edges
6. `AppDbContext` - 17 edges
7. `EmployeeCrudTests` - 17 edges
8. `EmployeeImportExportTests` - 17 edges
9. `IEmployeeRepository` - 16 edges
10. `ExcelManagement.Domain` - 16 edges

## Surprising Connections (you probably didn't know these)
- `Beads Issue Tracker Section (Managed Block)` --semantically_similar_to--> `Beads Issue Tracker Section`  [INFERRED] [semantically similar]
  AGENTS.md → CLAUDE.md
- `Agent Context Profiles (Conservative/Minimal/Team-maintainer)` --semantically_similar_to--> `Agent Context Profiles (Conservative/Minimal/Team-maintainer)`  [INFERRED] [semantically similar]
  AGENTS.md → CLAUDE.md
- `Session Completion Protocol` --semantically_similar_to--> `Session Completion Protocol`  [INFERRED] [semantically similar]
  AGENTS.md → CLAUDE.md
- `sync.remote (Dolt Git Remote: GURESPEX/excel-management)` --conceptually_related_to--> `Infra Compose Orchestration README`  [INFERRED]
  .beads/config.yaml → infra/README.md
- `api Service (Backend Image)` --shares_data_with--> `excel-management-api GHCR Image`  [INFERRED]
  infra/docker-compose.yml → .github/workflows/backend-ci.yml

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Excel Management Docker Compose Stack (postgres + api + web)** — infra_docker_compose_postgres, infra_docker_compose_api, infra_docker_compose_web [EXTRACTED 1.00]
- **Standardized Beads Agent-Instructions Template (shared across AGENTS.md and CLAUDE.md)** — agents_beads_issue_tracker, claude_beads_issue_tracker, agents_agent_context_profiles, claude_agent_context_profiles [INFERRED 0.90]
- **CI Image Build-and-Publish Pipeline Consumed by Compose Stack** — _github_workflows_backend_ci_push_image, _github_workflows_frontend_ci_push_image, infra_docker_compose_api, infra_docker_compose_web, infra_readme_doc [EXTRACTED 1.00]
- **Clean Architecture Request Flow (Web Client to Database)** — docs_excel_management_api_architecture_visual_check_1440x900_dark_web_client, docs_excel_management_api_architecture_visual_check_1440x900_dark_api_layer, docs_excel_management_api_architecture_visual_check_1440x900_dark_application_layer, docs_excel_management_api_architecture_visual_check_1440x900_dark_infrastructure_layer, docs_excel_management_api_architecture_visual_check_1440x900_dark_database [EXTRACTED 1.00]
- **AI Agent to MCP Server to Infrastructure Integration (Shared DbContext)** — docs_excel_management_api_architecture_visual_check_1440x900_dark_ai_agent, docs_excel_management_api_architecture_visual_check_1440x900_dark_mcp_server, docs_excel_management_api_architecture_visual_check_1440x900_dark_infrastructure_layer [EXTRACTED 1.00]
- **Clean Architecture Layer Grouping** — docs_excel_management_api_architecture_visual_check_1440x900_light_api_layer, docs_excel_management_api_architecture_visual_check_1440x900_light_application_layer, docs_excel_management_api_architecture_visual_check_1440x900_light_domain_layer, docs_excel_management_api_architecture_visual_check_1440x900_light_infrastructure_layer [EXTRACTED 1.00]
- **JWT Authentication Flow** — docs_excel_management_api_architecture_visual_check_1440x900_light_web_client, docs_excel_management_api_architecture_visual_check_1440x900_light_api_layer, docs_excel_management_api_architecture_visual_check_1440x900_light_auth [EXTRACTED 1.00]
- **AI Agent Database Access via MCP** — docs_excel_management_api_architecture_visual_check_1440x900_light_ai_agent, docs_excel_management_api_architecture_visual_check_1440x900_light_mcp_server, docs_excel_management_api_architecture_visual_check_1440x900_light_infrastructure_layer, docs_excel_management_api_architecture_visual_check_1440x900_light_database [INFERRED 0.85]
- **Clean Architecture Request Flow (Web Client through Database)** — docs_excel_management_api_architecture_visual_check_2048x1320_dark_web_client, docs_excel_management_api_architecture_visual_check_2048x1320_dark_api_layer, docs_excel_management_api_architecture_visual_check_2048x1320_dark_application_layer, docs_excel_management_api_architecture_visual_check_2048x1320_dark_domain_layer, docs_excel_management_api_architecture_visual_check_2048x1320_dark_infrastructure_layer, docs_excel_management_api_architecture_visual_check_2048x1320_dark_database [EXTRACTED 1.00]
- **AI Agent to Backend Integration via MCP Server sharing DbContext** — docs_excel_management_api_architecture_visual_check_2048x1320_dark_ai_agent, docs_excel_management_api_architecture_visual_check_2048x1320_dark_mcp_server, docs_excel_management_api_architecture_visual_check_2048x1320_dark_infrastructure_layer [EXTRACTED 1.00]
- **REST API Endpoint Surface (Auth, Employee, Department & Audit)** — docs_excel_management_api_architecture_visual_check_2048x1320_dark_auth_endpoints, docs_excel_management_api_architecture_visual_check_2048x1320_dark_employee_api, docs_excel_management_api_architecture_visual_check_2048x1320_dark_department_audit [INFERRED 0.80]
- **excel-management-api (.NET 9 Clean Architecture) project boundary** — docs_excel_management_api_architecture_visual_check_2048x1320_light_auth, docs_excel_management_api_architecture_visual_check_2048x1320_light_domain_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_api_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_application_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_infrastructure_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_mcp_server [EXTRACTED 0.95]
- **HTTP request pipeline from Web Client to Database** — docs_excel_management_api_architecture_visual_check_2048x1320_light_web_client, docs_excel_management_api_architecture_visual_check_2048x1320_light_api_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_application_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_infrastructure_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_database [EXTRACTED 0.90]
- **AI Agent data access path via MCP Server sharing DbContext** — docs_excel_management_api_architecture_visual_check_2048x1320_light_ai_agent, docs_excel_management_api_architecture_visual_check_2048x1320_light_mcp_server, docs_excel_management_api_architecture_visual_check_2048x1320_light_infrastructure_layer, docs_excel_management_api_architecture_visual_check_2048x1320_light_database [INFERRED 0.80]
- **Social Platform Link Icons (sprite group)** — excel_management_web_public_icons_bluesky, excel_management_web_public_icons_discord, excel_management_web_public_icons_github, excel_management_web_public_icons_x [INFERRED 0.65]

## Communities (68 total, 22 thin omitted)

### Community 0 - "Employee/Department DTOs & Queries"
Cohesion: 0.07
Nodes (33): Departments, Employees, EmployeeDetailDto, EmployeeListItemDto, EmployeeListQuery, Dictionary, CancellationToken, IReadOnlyList (+25 more)

### Community 1 - "Frontend Dev Dependencies (npm)"
Cohesion: 0.04
Nodes (47): devDependencies, jsdom, msw, openapi-typescript, oxlint, tailwindcss, @tailwindcss/vite, @tanstack/router-plugin (+39 more)

### Community 2 - "EF Core Database Migrations"
Cohesion: 0.06
Nodes (20): ExcelManagement.Infrastructure.Persistence.Migrations, MigrationBuilder, ModelBuilder, InitialCreate, MigrationBuilder, ModelBuilder, AddUsers, MigrationBuilder (+12 more)

### Community 3 - "Employee API Integration Tests"
Cohesion: 0.09
Nodes (19): Task, ApiWebApplicationFactory, Fact, HttpClient, Task, EmployeeCrudTests, Fact, HttpClient (+11 more)

### Community 4 - "Department Repository & DTOs"
Cohesion: 0.10
Nodes (16): DepartmentDto, CancellationToken, IReadOnlyList, Task, IDepartmentRepository, CancellationToken, IReadOnlyList, Task (+8 more)

### Community 5 - "Frontend Runtime Dependencies (npm)"
Cohesion: 0.05
Nodes (38): @base-ui/react, class-variance-authority, cn, dependencies, @base-ui/react, class-variance-authority, cn, @fontsource-variable/geist (+30 more)

### Community 6 - ".NET Solution & Project Files"
Cohesion: 0.07
Nodes (28): net9.0, Microsoft.EntityFrameworkCore.Design (9.0.20), net9.0, Microsoft.NET.Sdk, net9.0, Microsoft.NET.Sdk, net9.0, ClosedXML (0.105.1) (+20 more)

### Community 7 - "Audit Log Application Layer"
Cohesion: 0.12
Nodes (16): ExcelManagement.Application.AuditLogs, AuditLogDto, AuditLogQuery, CancellationToken, IReadOnlyList, Task, IAuditLogRepository, CancellationToken (+8 more)

### Community 8 - "Auth Integration Tests"
Cohesion: 0.19
Nodes (9): HttpClient, Task, AuthTestHelper, Fact, HttpClient, Task, AuthTests, HttpMethod (+1 more)

### Community 9 - "App Shell & Auth Store"
Cohesion: 0.14
Nodes (21): Chememan Logo (SVG), AppShell(), bootstrapAuth(), toAuthUser(), useLogin(), useLogout(), requireAdmin(), requireAuth() (+13 more)

### Community 10 - "System Architecture Diagram (overview)"
Cohesion: 0.09
Nodes (27): AI Assistant (MCP Client), API Server Backend (ASP.NET Core 9, JWT Bearer), Backend Stack Card (ASP.NET Core9, EF Core9+Npgsql, ClosedXML, JWT, Swashbuckle), ฐานข้อมูล Database (PostgreSQL 17, :5432), Excel Management System Architecture Diagram, ไฟล์ Excel Import/Export (.xlsx), เว็บแอป Frontend (React 19 + Vite, Nginx :5173), Frontend Stack Card (React19+TS+Vite8, TanStack, Zustand, shadcn/ui+Tailwind4) (+19 more)

### Community 11 - "Employee Import & Row Validation"
Cohesion: 0.10
Nodes (15): Errors, EmployeeImportResult, EmployeeImportRow, ImportRowError, IReadOnlyList, EmployeeImportRowValidator, IReadOnlyList, Stream (+7 more)

### Community 12 - "Frontend tsconfig (app)"
Cohesion: 0.08
Nodes (24): compilerOptions, allowArbitraryExtensions, allowImportingTsExtensions, erasableSyntaxOnly, jsx, lib, module, moduleDetection (+16 more)

### Community 13 - "shadcn/ui Table & Form Components"
Cohesion: 0.19
Nodes (15): Card(), Input(), Label(), Table(), TableBody(), TableCell(), TableHead(), TableHeader() (+7 more)

### Community 14 - "Application Layer Namespaces & DI"
Cohesion: 0.15
Nodes (9): ExcelManagement.Application.Employees, ExcelManagement.McpServer, ExcelManagement.Api.IntegrationTests, ExcelManagement.Application.Departments, CreateDepartmentRequest, UpdateDepartmentRequest, DepartmentTools, CreateEmployeeResult (+1 more)

### Community 15 - "Employee Import/Export Tests"
Cohesion: 0.18
Nodes (11): Department, Fact, HttpClient, Task, EmployeeImportExportTests, HttpContent, IEnumerable, JoinDate (+3 more)

### Community 16 - "Beads Issue Tracker & CI Publish Pipeline"
Cohesion: 0.12
Nodes (22): Beads OpenAI Agent Interface Config, bd CLI, bd prime Command, Beads Skill (SKILL.md), Core CLI Workflow (find/inspect/claim/create/close), sync.remote (Dolt Git Remote: GURESPEX/excel-management), bd dolt push/pull Sync, Beads (AI-Native Issue Tracking) (+14 more)

### Community 17 - "shadcn/ui components.json Config"
Cohesion: 0.09
Nodes (21): aliases, components, hooks, lib, ui, utils, iconLibrary, menuAccent (+13 more)

### Community 18 - "EF Core AppDbContext & Audit Capture"
Cohesion: 0.12
Nodes (13): DbContext, DbContextOptionsBuilder, DbSet, DateTime, RefreshToken, CancellationToken, List, ModelBuilder (+5 more)

### Community 19 - "Frontend API Hooks (employees/depts/audit)"
Cohesion: 0.16
Nodes (14): AuditLogFilters, useCreateDepartment(), useUpdateDepartment(), DepartmentTableRow(), EmployeeFilters, EmployeeFormValues, EmployeeImportResult, useEmployee() (+6 more)

### Community 20 - "Auth Domain & API Wiring"
Cohesion: 0.16
Nodes (8): ExcelManagement.Application.Auth, ExcelManagement.Infrastructure, ExcelManagement.Api.Auth, ExcelManagement.Domain, ExcelManagement.Infrastructure.Persistence, Program, AuthenticatedUserDto, LoginRequest

### Community 21 - "Frontend tsconfig (node)"
Cohesion: 0.10
Nodes (19): compilerOptions, allowImportingTsExtensions, erasableSyntaxOnly, lib, module, moduleDetection, noEmit, noFallthroughCasesInSwitch (+11 more)

### Community 22 - "Employee List Page & Export Controls"
Cohesion: 0.19
Nodes (13): Badge(), badgeVariants, Button(), buttonVariants, exportEmployees(), exportQuery(), useEmployees(), useImportEmployees() (+5 more)

### Community 23 - "shadcn/ui Select & Checkbox Components"
Cohesion: 0.17
Nodes (9): Checkbox(), SelectContent(), SelectItem(), SelectTrigger(), SelectValue(), useDepartments(), EmployeeForm(), toFieldName() (+1 more)

### Community 24 - "Dev Launch Profiles (launchSettings.json)"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 25 - "shadcn/ui Alert Dialog Component"
Cohesion: 0.21
Nodes (11): AlertDialog(), AlertDialogAction(), AlertDialogCancel(), AlertDialogContent(), AlertDialogDescription(), AlertDialogFooter(), AlertDialogHeader(), AlertDialogTitle() (+3 more)

### Community 26 - "Clean Architecture Diagram (layers)"
Cohesion: 0.29
Nodes (14): Excel Management API - Clean Architecture Diagram, AI Agent (Claude / MCP Host), API Layer (ASP.NET Core 9), Application Layer (Use cases, DTOs), Auth (JWT httpOnly Cookie), Auth Endpoints (login/refresh/logout), Database (SQLite / PostgreSQL), Department & Audit Endpoints (+6 more)

### Community 27 - "JWT Token Service"
Cohesion: 0.25
Nodes (8): DateTime, int, AuthPrincipal, JwtTokenService, ExpiresAt, SymmetricSecurityKey, TimeSpan, Token

### Community 28 - "Clean Architecture Diagram (API/DB detail)"
Cohesion: 0.27
Nodes (13): AI Agent (Claude / MCP Host), API Layer (ASP.NET Core 9), Application Layer (Use cases, DTOs), Auth (JWT httpOnly Cookie), Database (SQLite / PostgreSQL), Department & Audit API, Excel Management API — Clean Architecture Diagram, Domain Layer (Entity, Enum) (+5 more)

### Community 29 - "Clean Architecture Diagram (endpoints detail)"
Cohesion: 0.17
Nodes (13): AI Agent (Claude / MCP Host), API Layer (ASP.NET Core 9), Application Layer (Use cases, DTOs), Auth (JWT httpOnly Cookie), Auth Endpoints (login/refresh/logout), Database (SQLite / PostgreSQL), Department & Audit Endpoints, Domain Layer (Entity, Enum) (+5 more)

### Community 30 - "Refresh Token Repository"
Cohesion: 0.21
Nodes (8): CancellationToken, DateTime, Task, IRefreshTokenRepository, CancellationToken, DateTime, Task, RefreshTokenRepository

### Community 31 - "Clean Architecture Diagram (full stack)"
Cohesion: 0.18
Nodes (12): AI Agent (Claude / MCP Host), API Layer (ASP.NET Core 9), Application Layer (Use cases, DTOs), Auth (JWT httpOnly Cookie), Database (SQLite / PostgreSQL), Department & Audit (endpoints: departments CRUD, audit-logs admin-only), Domain Layer (Entity, Enum), Employee API (endpoints: list/create/update/delete, export, import) (+4 more)

### Community 32 - "User Repository & Domain"
Cohesion: 0.18
Nodes (8): CancellationToken, Task, IUserRepository, User, UserRole, CancellationToken, Task, UserRepository

### Community 33 - "Frontend Test Setup & MSW Server"
Cohesion: 0.36
Nodes (3): departments, renderApp(), server

### Community 34 - "Audit Log Domain & Change Tracking"
Cohesion: 0.22
Nodes (6): EntityEntry, AuditAction, DateTime, AuditLog, DateTime, PendingAuditEntry

### Community 35 - "Oxlint Rule Config"
Cohesion: 0.22
Nodes (8): plugins, rules, react/only-export-components, react/rules-of-hooks, $schema, oxc, typescript, warn

### Community 36 - "DatePicker Component"
Cohesion: 0.43
Nodes (5): DatePicker(), DatePickerProps, formatDateValue(), parseDateValue(), PopoverContent()

### Community 37 - "CLAUDE.md/AGENTS.md Beads Instructions"
Cohesion: 0.47
Nodes (6): Agent Context Profiles (Conservative/Minimal/Team-maintainer), Beads Issue Tracker Section (Managed Block), Session Completion Protocol, Agent Context Profiles (Conservative/Minimal/Team-maintainer), Beads Issue Tracker Section, Session Completion Protocol

### Community 38 - "Employee CSV Writer"
Cohesion: 0.40
Nodes (3): IReadOnlyList, string, EmployeeCsvWriter

### Community 39 - "Department & Employee Domain Entities"
Cohesion: 0.33
Nodes (4): Department, DateOnly, DateTime, Employee

### Community 40 - "Frontend App Entry & Router"
Cohesion: 0.33
Nodes (5): index.html Entry Point, queryClient, Register, router, @tanstack/react-router

### Community 41 - "OpenAPI Generated Schema Types"
Cohesion: 0.33
Nodes (5): components, $defs, operations, paths, webhooks

### Community 43 - "Root tsconfig References"
Cohesion: 0.40
Nodes (4): compilerOptions, paths, files, references

### Community 45 - "Infrastructure DI Registration"
Cohesion: 0.50
Nodes (3): DependencyInjection, IConfiguration, IServiceCollection

## Knowledge Gaps
- **225 isolated node(s):** `net9.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)`, `Microsoft.EntityFrameworkCore.Design (9.0.20)`, `Swashbuckle.AspNetCore (10.2.3)`, `Microsoft.NET.Sdk.Web` (+220 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **22 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ExcelManagement.Infrastructure.Persistence` connect `Auth Domain & API Wiring` to `EF Core Database Migrations`, `Employee Import & Row Validation`, `Application Layer Namespaces & DI`, `Audit Log Application Layer`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **Why does `ExcelManagement.Application.Employees` connect `Application Layer Namespaces & DI` to `Employee/Department DTOs & Queries`, `Employee CSV Writer`, `Employee Import & Row Validation`, `Employee Validator`, `Auth Domain & API Wiring`?**
  _High betweenness centrality (0.046) - this node is a cross-community bridge._
- **Why does `ApiWebApplicationFactory` connect `Employee API Integration Tests` to `Employee/Department DTOs & Queries`, `Department Repository & DTOs`, `Audit Log Application Layer`, `Auth Integration Tests`, `Employee Import/Export Tests`, `Auth Domain & API Wiring`?**
  _High betweenness centrality (0.045) - this node is a cross-community bridge._
- **What connects `net9.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)`, `Microsoft.EntityFrameworkCore.Design (9.0.20)` to the rest of the system?**
  _225 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Employee/Department DTOs & Queries` be split into smaller, more focused modules?**
  _Cohesion score 0.07326007326007326 - nodes in this community are weakly interconnected._
- **Should `Frontend Dev Dependencies (npm)` be split into smaller, more focused modules?**
  _Cohesion score 0.041666666666666664 - nodes in this community are weakly interconnected._
- **Should `EF Core Database Migrations` be split into smaller, more focused modules?**
  _Cohesion score 0.05550416281221091 - nodes in this community are weakly interconnected._