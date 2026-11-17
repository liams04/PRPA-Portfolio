# Showcase: Domain Entity Models

## Overview

The models in this directory represent the core of our relational domain. They live in a shared `Common/` class library, allowing both the ASP.NET Core API and the Blazor WebAssembly client to share identical data structures.

---

## Included Models

### 1. [`DepartmentProgram.cs`](DepartmentProgram.cs) (Aggregate Root)
Represents an academic major or degree offering. 
* Acts as the aggregate root for all curriculum schedules (`Semesters`), course lookup sets (`CourseTypes`, `CourseLevels`), cohort enrollment tracks (`Cohorts`, `ProgramIntakes`), and financial projections (`BaseSalaries`, `ProgramFinancials`).
* Configured with virtual navigation properties enabling Entity Framework Core to manage foreign key relationships and cascade behaviors.

### 2. [`Course.cs`](Course.cs)
Represents an individual course offering within a curriculum delivery schedule.
* Links to the parent `CourseGroup` and resolves its instructional classification via `CourseType` (e.g., Lecture, Lab) and `CourseLevel` (e.g., 100-level, 400-level).
* Distinguishes student credit hours (`Cr`) from faculty workload credits (`CrLoad`), an essential distinction for accurately calculating university instructional load.
