# Database Design & Schema Architecture

## Overview

The database is built on **PostgreSQL** and managed using **Entity Framework Core**. 

The schema mirrors a university's real-world operational structure, divided into four functional domains:
1. **Organizational Hierarchy**: Colleges, departments, and degree programs.
2. **Curriculum Delivery**: Semesters, course groups, courses, and customizable type/level lookups.
3. **Student Projections**: Cohort headcounts, entering intake per fiscal year, and curriculum attrition rates.
4. **Faculty Staffing & Financials**: Salary baselines by instructor type, non-instructional faculty loads, and departmental overhead.

---

## Entity-Relationship Diagram

```mermaid
erDiagram
    COLLEGES ||--o{ DEPARTMENTS : contains
    DEPARTMENTS ||--o{ DEPARTMENT_PROGRAMS : offers

    DEPARTMENT_PROGRAMS ||--o{ SEMESTERS : schedules
    SEMESTERS ||--o{ COURSE_GROUPS : organizes
    COURSE_GROUPS ||--o{ COURSES : contains

    DEPARTMENT_PROGRAMS ||--o{ COURSE_TYPES : defines
    DEPARTMENT_PROGRAMS ||--o{ COURSE_LEVELS : defines
    COURSES }o--|| COURSE_TYPES : categorizes
    COURSES }o--|| COURSE_LEVELS : classifies

    DEPARTMENT_PROGRAMS ||--o{ COHORTS : tracks
    DEPARTMENT_PROGRAMS ||--o{ PROGRAM_INTAKE : projects
    DEPARTMENT_PROGRAMS ||--o{ ATTRITION_RATES : models
    DEPARTMENT_PROGRAMS ||--o{ BASE_SALARIES : establishes
    DEPARTMENT_PROGRAMS ||--o{ PROGRAM_FINANCIALS : budgets

    COLLEGES {
        int id PK
        string college_name
    }

    DEPARTMENTS {
        int id PK
        int college_id FK
        string dept_name
    }

    DEPARTMENT_PROGRAMS {
        int id PK
        int dept_id FK
        string program_name
        int number_of_years
        float tuition_per_credit
        int faculty_load_per_ftei
        int ga_load_per_ftei
    }

    SEMESTERS {
        int id PK
        int program_id FK
        int curriculum_year
        boolean fall
        boolean spring
        boolean summer
    }

    COURSE_GROUPS {
        int id PK
        int semester_id FK
    }

    COURSES {
        int id PK
        int group_id FK
        int course_type_id FK
        int course_level_id FK
        int instructor_type_id
        int cr "Credit Hours"
        int cr_load "Teaching Load Credits"
        int section_max "Max Students Per Section"
        float percent_of_students "Expected Enrollment %"
    }

    COURSE_TYPES {
        int id PK
        int program_id FK
        string type "Lecture, Lab, Seminar"
    }

    COURSE_LEVELS {
        int id PK
        int program_id FK
        string level "100-level, 400-level, Graduate"
    }

    COHORTS {
        int id PK
        int program_id FK
        int curriculum_year
        int current_students
    }

    PROGRAM_INTAKE {
        int id PK
        int program_id FK
        int fiscal_year
        int entering_students
    }

    ATTRITION_RATES {
        int id PK
        int program_id FK
        int curriculum_year
        float attrition_rate
    }

    BASE_SALARIES {
        int id PK
        int program_id FK
        string instructor_type "Faculty, Staff, GA"
        int average_salary
        float fringe_benefits
    }

    PROGRAM_FINANCIALS {
        int id PK
        int program_id FK
        int fiscal_year
        int equipment_cost
        int other_cost
        int non_tuition_revenue
    }
```

---

## Relational Design Decisions

### 1. Curriculum Hierarchy (`DepartmentProgram` → `Semester` → `CourseGroup` → `Course`)
* A degree program is structured into multi-year curriculum cycles (typically 1 to 4 years).
* Each curriculum year can offer courses in **Fall, Spring, and Summer** terms (`semesters`).
* Within a semester, courses are clustered into `course_groups` to represent alternative course tracks or co-requisite clusters.
* Each `course` stores its credit hours (`cr`), faculty workload weight (`cr_load`), maximum capacity per section (`section_max`), and the percentage of students in that cohort expected to enroll (`percent_of_students`).

### 2. Program-Scoped Lookup Tables with Default Templates
* Rather than enforcing rigid, campus-wide course classifications, different departments require different course types (e.g., Nursing needs *"Clinical Rotation"*, while Art needs *"Studio Practice"*).
* We designed `course_types` and `course_levels` to link directly to `program_id`. 
* When a user creates a new program, the system automatically clones rows from `default_course_types` and `default_course_levels` into program-scoped lookup tables, giving each department full flexibility to customize course labels without altering global university defaults.

### 3. Referential Integrity & Cascades
* Deleting a degree program should clean up its underlying curriculum schedule and projection tables without leaving orphan records.
* Foreign keys linking `semesters`, `cohorts`, `base_salaries`, and `program_financials` back to `department_programs` are configured with `ON DELETE CASCADE`.
* Conversely, `colleges` and `departments` use restrictive deletion so administrative units cannot be deleted accidentally if active programs belong to them.
