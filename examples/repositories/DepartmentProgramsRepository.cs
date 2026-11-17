// Purpose: Aggregate repository for DepartmentProgram entities, showcasing split queries and transactional bulk upsert synchronization.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRPA.Portfolio.Examples.Models;

namespace PRPA.Portfolio.Examples.Repositories;

/// <summary>
/// Repository managing access and synchronization for the <see cref="DepartmentProgram"/> aggregate root.
/// </summary>
public class DepartmentProgramsRepository(DbContext context)
{
    private readonly DbContext _context = context;

    public async Task<List<DepartmentProgram>> GetAllProgramsAsync()
    {
        return await _context.Set<DepartmentProgram>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DepartmentProgram?> FindProgramByIdAsync(int id)
    {
        return await _context.Set<DepartmentProgram>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Loads a complete program aggregate with its full curriculum schedule, cohorts, and financials.
    /// Uses <see cref="EntityFrameworkQueryableExtensions.AsSplitQuery{TEntity}"/> to prevent Cartesian explosions
    /// across multi-level 1-to-many collections.
    /// </summary>
    public async Task<DepartmentProgram?> FindProgramByIdWithAllChildrenAsync(int id)
    {
        return await _context.Set<DepartmentProgram>()
            .AsNoTracking()
            .AsSplitQuery() // Executes separate, parallel SQL queries per collection to avoid row duplication
            .Include(p => p.AttritionRates)
            .Include(p => p.Cohorts)
            .Include(p => p.ProgramIntakes)
            .Include(p => p.NonInstructionalsLoads)
            .Include(p => p.ProgramFinancials)
            .Include(p => p.DepartmentEnrollments)
            .Include(p => p.DepartmentFinancials)
            .Include(p => p.BaseSalaries)
            .Include(p => p.CourseTypes)
            .Include(p => p.CourseLevels)
            .Include(p => p.Semesters)
                // In production, further ThenIncludes load CourseGroups and Courses
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Creates a new program after verifying that duplicate titles do not exist within the department.
    /// </summary>
    public async Task<DepartmentProgram> CreateProgramAsync(DepartmentProgram program)
    {
        bool exists = await _context.Set<DepartmentProgram>()
            .AnyAsync(p => p.DeptId == program.DeptId && p.ProgramName == program.ProgramName);

        if (exists)
        {
            throw new InvalidOperationException(
                $"A program with the name '{program.ProgramName}' already exists for this department.");
        }

        _context.Set<DepartmentProgram>().Add(program);
        await _context.SaveChangesAsync();
        return program;
    }

    /// <summary>
    /// Synchronizes a full program payload from the single-page editor within a single transaction.
    /// </summary>
    public async Task<DepartmentProgram?> BulkSaveAsync(int programId, DepartmentProgram payload)
    {
        DepartmentProgram? existing = await _context.Set<DepartmentProgram>()
            .AsSplitQuery()
            .Include(p => p.AttritionRates)
            .Include(p => p.Cohorts)
            .Include(p => p.ProgramIntakes)
            .Include(p => p.BaseSalaries)
            .Include(p => p.DepartmentFinancials)
            .Include(p => p.ProgramFinancials)
            .Include(p => p.NonInstructionalsLoads)
            .Include(p => p.DepartmentEnrollments)
            .Include(p => p.CourseTypes)
            .Include(p => p.CourseLevels)
            .Include(p => p.Semesters)
            .FirstOrDefaultAsync(p => p.Id == programId);

        if (existing == null) return null;

        // Synchronize scalar properties
        existing.ProgramName = payload.ProgramName;
        existing.NumberOfYears = payload.NumberOfYears;
        existing.TuitionPerCredit = payload.TuitionPerCredit;
        existing.FacultyLoadPerFtei = payload.FacultyLoadPerFtei;
        if (payload.GaLoadPerFtei.HasValue)
            existing.GaLoadPerFtei = payload.GaLoadPerFtei;
        existing.DeptId = payload.DeptId;

        // Synchronize child collections in memory
        UpsertCohorts(existing, payload.Cohorts, programId);
        UpsertBaseSalaries(existing, payload.BaseSalaries, programId);
        UpsertProgramFinancials(existing, payload.ProgramFinancials, programId);

        // Commit all updates and inserts in one atomic transaction
        await _context.SaveChangesAsync();
        return await FindProgramByIdWithAllChildrenAsync(programId);
    }

    public async Task<bool> DeleteProgramAsync(int id)
    {
        DepartmentProgram? programToDelete = await _context.Set<DepartmentProgram>().FindAsync(id);
        if (programToDelete is null) return false;

        // Foreign keys configured with ON DELETE CASCADE handle child row cleanup automatically
        _context.Set<DepartmentProgram>().Remove(programToDelete);
        await _context.SaveChangesAsync();
        return true;
    }

    // -------------------------------------------------------------------------
    // Private In-Memory Collection Upsert Helpers
    // -------------------------------------------------------------------------

    private static void UpsertCohorts(DepartmentProgram existing, ICollection<Cohort> items, int programId)
    {
        Dictionary<int, Cohort> tracked = existing.Cohorts.ToDictionary(x => x.Id);
        foreach (Cohort item in items)
        {
            Cohort? entry = null;
            if (item.Id > 0) tracked.TryGetValue(item.Id, out entry);
            entry ??= existing.Cohorts.FirstOrDefault(x => x.CurriculumYear == item.CurriculumYear);

            if (entry != null)
            {
                entry.CurrentStudents = item.CurrentStudents;
            }
            else
            {
                item.ProgramId = programId;
                existing.Cohorts.Add(item);
            }
        }
    }

    private static void UpsertBaseSalaries(DepartmentProgram existing, ICollection<BaseSalary> items, int programId)
    {
        Dictionary<int, BaseSalary> tracked = existing.BaseSalaries.ToDictionary(x => x.Id);
        foreach (BaseSalary item in items)
        {
            BaseSalary? entry = null;
            if (item.Id > 0) tracked.TryGetValue(item.Id, out entry);
            entry ??= existing.BaseSalaries.FirstOrDefault(x => x.InstructorType == item.InstructorType);

            if (entry != null)
            {
                entry.AverageSalary = item.AverageSalary;
                entry.FringeBenefits = item.FringeBenefits;
            }
            else
            {
                item.ProgramId = programId;
                existing.BaseSalaries.Add(item);
            }
        }
    }

    private static void UpsertProgramFinancials(DepartmentProgram existing, ICollection<ProgramFinancial> items, int programId)
    {
        Dictionary<int, ProgramFinancial> tracked = existing.ProgramFinancials.ToDictionary(x => x.Id);
        foreach (ProgramFinancial item in items)
        {
            ProgramFinancial? entry = null;
            if (item.Id > 0) tracked.TryGetValue(item.Id, out entry);
            entry ??= existing.ProgramFinancials.FirstOrDefault(x => x.FiscalYear == item.FiscalYear);

            if (entry != null)
            {
                entry.EquipmentCost = item.EquipmentCost;
            }
            else
            {
                item.ProgramId = programId;
                existing.ProgramFinancials.Add(item);
            }
        }
    }
}
