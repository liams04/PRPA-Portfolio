# Program Review & Planning Application (PRPA) — Portfolio Showcase

> **Portfolio Note**: This repository is a sanitized technical showcase of the **Program Review and Planning Application (PRPA)**, an internal software solution developed for **Minnesota State University, Mankato**. Full production files and institutional secrets are omitted. This repo highlights my work on **database design, backend architecture, query optimization, and integration testing**.

---

## What is PRPA?

At universities, creating or modifying an academic program (such as proposing a new degree or re-evaluating an existing major) requires extensive forecasting:
* How many faculty FTE (Full-Time Equivalent) will be required across lecture vs. lab sections?
* How many credit hours will be generated across fall, spring, and summer semesters?
* What does student attrition look like across year 1 through year 4 cohorts?
* What are the projected tuition revenues, equipment expenses, and departmental overhead costs over a multi-year fiscal horizon?

Historically, the university handled this using massive, complex multi-tab Excel workbooks. These spreadsheets were fragile: one broken formula, corrupted cell reference, or accidental copy-paste error would silently skew financial and staffing forecasts for entire colleges.

**PRPA** replaced those spreadsheets with a multi-user web application. Academic chairs and deans can configure curriculum delivery schedules, enter cohort projections, assign instructor salary bands, and instantly generate deterministic enrollment, credit hour, and financial forecasts.

---

## Tech Stack

* **Backend**: C#, ASP.NET Core Web API (.NET 8)
* **ORM & Database**: Entity Framework Core, PostgreSQL
* **Frontend**: Blazor WebAssembly, C#, HTML5, CSS
* **Containerization**: Docker & Docker Compose (local development and PostgreSQL hosting)
* **Testing**: xUnit, `Microsoft.AspNetCore.Mvc.Testing` (integration testing), Moq
* **DevOps**: GitHub Actions (automated CI build and test pipelines)

---

## My Role: Database & Backend Engineer

I served as a **Database & Backend Engineer** on the team, focusing on data persistence, relational modeling, API performance, and test reliability.

### Major Contributions & Engineering Challenges:

1. **Relational Database Design & Data Modeling**:
   * Designed and maintained the PostgreSQL database schema spanning organizational hierarchies (`colleges` → `departments` → `department_programs`), curriculum delivery schedules (`semesters` → `course_groups` → `courses`), and multi-year financial projections (`cohorts`, `base_salaries`, `program_financials`).
   * Configured primary/foreign key constraints, cascade behaviors, and lookup table hierarchies (e.g., customizable course types and levels per program with system-wide default fallbacks).

2. **Solving the "Cartesian Product" Query Problem**:
   * A single academic program contains collections across multiple one-to-many relationships (semesters, courses, cohorts, financials). 
   * Naive EF Core queries with multiple `.Include()` calls caused severe SQL Cartesian explosions, joining rows repeatedly and degrading performance.
   * I optimized these queries using `.AsSplitQuery()` and `.AsNoTracking()`, allowing PostgreSQL to execute parallel, efficient queries per collection without ballooning memory or network bandwidth.

3. **Eliminating Redundant Network Requests**:
   * In early iterations, the frontend was making dozens of chatty HTTP roundtrips to fetch individual semesters, course groups, and instructor types one by one.
   * I refactored the backend repositories to expose consolidated, atomic retrieval methods (like `FindProgramByIdWithAllChildrenAsync`), enabling the client to fetch a program's complete hierarchy in a single fast, structured response.

4. **Robust Integration Testing Infrastructure**:
   * Built the testing foundation using `WebApplicationFactory<Program>` to test API endpoints against a real PostgreSQL instance rather than unreliable in-memory mocks.
   * Designed an ID isolation scheme (test entities reserved with `ID >= 99000`) and deterministic teardown routines (`IAsyncLifetime`) so test suites could run safely in parallel without colliding or dirtying local data.

---

## Repository Contents

* **[Database Design & Schema](docs/database-design.md)**: Simplified Entity-Relationship (ER) diagram in Mermaid, table explanations, and constraint design.
* **[Backend Architecture](docs/backend-architecture.md)**: Overview of the layered API architecture, repository pattern, and query optimization strategies.
* **[Domain Models & Relationships](docs/domain-models.md)**: Walkthrough of the core models and how academic curriculum trees are represented.
* **Code Examples**:
  * **[Backend Models](examples/models/)**: Representative entity models (`DepartmentProgram.cs`, `Course.cs`) showcasing relational mappings and navigation properties.
  * **[Repository Layer](examples/repositories/)**: Production repository implementations showcasing `.AsSplitQuery()`, `.AsNoTracking()`, and CRUD data access.
  * **[Integration Tests](examples/tests/)**: The `TestBase` integration framework and comprehensive controller tests verifying HTTP response codes and data integrity.
