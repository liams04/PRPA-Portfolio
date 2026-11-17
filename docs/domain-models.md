# Domain Models & Relational Concepts

## Overview

The domain model represents the operational workflow of academic program planning at **Minnesota State University, Mankato**. Models live in the shared `Common/` class library and are referenced by both the backend API and the Blazor WebAssembly frontend.

---

## Core Domain Models

### 1. `DepartmentProgram` (The Aggregate Root)
The central entity in the system. Represents an academic major, minor, or certificate program belonging to a university department.

* **Key Properties**:
  * `Id` (`int`): Primary key.
  * `DeptId` (`int`): Foreign key to the parent `Department`.
  * `ProgramName` (`string`): The degree title (e.g., *"Computer Science B.S."*).
  * `NumberOfYears` (`int`): Program curriculum duration (1–5 years).
  * `TuitionPerCredit` (`float`): In-state / baseline tuition rate charged per credit hour.
  * `FacultyLoadPerFtei` (`int`): Standard annual teaching credit hours expected per full-time faculty member (typically 24 credits per year).
  * `GaLoadPerFtei` (`int`): Teaching load expected per Graduate Assistant.
* **Child Navigation Collections**:
  * `Semesters`: The curriculum schedule.
  * `CourseTypes` & `CourseLevels`: Program-specific course categories.
  * `Cohorts`: Active student headcounts by curriculum year.
  * `ProgramIntakes`: Annual entering student projections.
  * `BaseSalaries`: Instructor compensation benchmarks.
  * `ProgramFinancials`: Annual equipment and non-tuition revenue budgets.

---

### 2. Curriculum Tree: `Semester` → `CourseGroup` → `Course`

This sub-tree represents how degree requirements are scheduled across the academic calendar.

#### `Semester`
Represents an academic term within a curriculum year.
* `CurriculumYear` (`int`): Year 1 (Freshman) through Year 4 (Senior).
* `Fall`, `Spring`, `Summer` (`bool`): Flags indicating which active terms offer courses for this curriculum stage.

#### `CourseGroup`
Acts as an organizational container inside a semester. Groups can represent:
* Core mandatory courses.
* Elective baskets where students choose 1 of $N$ courses.
* Co-requisite lecture + lab combinations.

#### `Course`
The fundamental unit of academic instruction.
* `Cr` (`int`): Student credit hours earned upon completing the course.
* `CrLoad` (`int`): Faculty workload credit hours counted toward instructor teaching load (a 3-credit lecture with an attached 2-hour lab might have `Cr = 3` but `CrLoad = 4`).
* `SectionMax` (`int`): Maximum seat capacity per section before a new section must be opened.
* `PercentOfStudents` (`float`): The fraction of the student cohort expected to take this course (e.g., `1.0` for required core classes, `0.33` for 1-of-3 electives).
* `CourseTypeId` & `CourseLevelId` (`int`): Foreign keys to lookup tables specifying whether the course is Lecture/Lab/Seminar and 100/200/300/400-level.

---

### 3. Student Projections: `Cohort`, `ProgramIntake`, and `AttritionRate`

These models drive multi-year student enrollment forecasts.

* **`Cohort`**: Holds current student enrollment broken down by curriculum year (Year 1, Year 2, Year 3, Year 4).
* **`ProgramIntake`**: Projects new incoming students for future fiscal years (FY25, FY26, FY27...).
* **`AttritionRate`**: Percentage of students expected to leave or drop out between successive curriculum years. The projection engine uses these rates to model cohort progression from intake through graduation:
  $$\text{Cohort}_{\text{Year } n+1} = \text{Cohort}_{\text{Year } n} \times (1 - \text{AttritionRate}_n)$$

---

### 4. Faculty & Staffing: `BaseSalary`

Models compensation across four instructor categories:
1. Full-time tenured / probationary faculty
2. Departmental staff
3. Adjunct / fixed-term faculty
4. Graduate Assistants (GAs)

Each record stores `AverageSalary` (integer) and `FringeBenefits` (percentage, e.g., `0.33` for 33% benefits), allowing the financial calculation engine to accurately project instructional salary expenses based on calculated section counts.
