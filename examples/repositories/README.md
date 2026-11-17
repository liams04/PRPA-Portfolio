# Showcase: Repository Layer & Query Optimization

## Architectural Problem & Context

Academic planning in PRPA centers around a deeply nested aggregate entity: `DepartmentProgram`. When an academic user opens the planning dashboard, the application must load:
* The program header (tuition rate, faculty teaching loads).
* All curriculum semesters (Fall, Spring, Summer across 1 to 5 academic years).
* Course groups and individual courses inside each semester.
* Course type, course level, and instructor salary classifications.
* Cohort headcounts, future intake projections, attrition rates, and financial budgets.

### The Engineering Challenges:
1. **The Cartesian Product Explosion**: Joining 6+ one-to-many collections in a single SQL query produces an exponential Cartesian product. EF Core joins duplicate rows repeatedly, returning thousands of redundant records and ballooning memory.
2. **Network Chattiness vs. Single-Request Atomic Saves**: We needed a way for users to edit the entire curriculum and financial forecast on a single page, save their changes in one click, and have the backend synchronize insertions, updates, and deletions across all child collections in a single database transaction.

---

## Technical Solutions in this Directory

### 1. Eliminating Cartesian Joins with `.AsSplitQuery()`
In [`DepartmentProgramsRepository.cs`](DepartmentProgramsRepository.cs), the `FindProgramByIdWithAllChildrenAsync` method applies `.AsSplitQuery()`:
```csharp
public async Task<DepartmentProgram?> FindProgramByIdWithAllChildrenAsync(int id) {
    return await context.DepartmentPrograms
        .AsNoTracking()
        .AsSplitQuery() // Tells EF Core to execute separate, fast queries per child collection
        .Include(p => p.Dept).ThenInclude(d => d!.College)
        .Include(p => p.Cohorts)
        .Include(p => p.Semesters)
            .ThenInclude(s => s.CourseGroups)
                .ThenInclude(cg => cg.Courses)
        // ...
        .FirstOrDefaultAsync(p => p.Id == id);
}
```
Instead of a single massive join that locks up the database connection and generates thousands of redundant rows, PostgreSQL executes parallel, targeted queries filtered by foreign key.

### 2. Transactional In-Memory Upsert Synchronization (`BulkSaveAsync`)
The `BulkSaveAsync` method accepts the complete planning state from the frontend, queries the existing database state, and performs in-memory synchronization across every collection (matching existing rows by ID or curriculum year, updating changed fields, and inserting new entries). 

All updates are finalized in a single `await context.SaveChangesAsync()`, guaranteeing atomicity without requiring dozens of individual HTTP calls.

---

## Included Files
* [`DepartmentProgramsRepository.cs`](DepartmentProgramsRepository.cs): The primary aggregate repository featuring split queries, multi-level includes, and bulk-save upserts.
* [`CourseRepository.cs`](CourseRepository.cs): Focused CRUD repository demonstrating clean entity operations and foreign key lookups.
