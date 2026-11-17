// Purpose: Aggregate root entity representing an academic degree program and its associated curriculum, cohorts, and financials.

using System.Collections.Generic;

namespace PRPA.Portfolio.Examples.Models;

/// <summary>
/// Represents an academic program (e.g., Computer Science B.S.) offered by a department.
/// Acts as the root entity for all curriculum schedules, enrollment cohorts, and financial assumptions.
/// </summary>
public class DepartmentProgram
{
    public int Id { get; set; }

    public string? ProgramName { get; set; }

    /// <summary>
    /// Curriculum duration in academic years (e.g., 4 for a bachelor's, 2 for a master's).
    /// </summary>
    public int? NumberOfYears { get; set; }

    /// <summary>
    /// Base tuition charged per student credit hour.
    /// </summary>
    public double? TuitionPerCredit { get; set; }

    /// <summary>
    /// Required annual teaching credit load per full-time faculty equivalent (typically 24 credits).
    /// </summary>
    public int? FacultyLoadPerFtei { get; set; }

    /// <summary>
    /// Required teaching load per Graduate Assistant equivalent.
    /// </summary>
    public int? GaLoadPerFtei { get; set; }

    /// <summary>
    /// Foreign key to the parent department.
    /// </summary>
    public int? DeptId { get; set; }

    // --- Navigation Properties ---

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    public virtual ICollection<CourseType> CourseTypes { get; set; } = new List<CourseType>();

    public virtual ICollection<CourseLevel> CourseLevels { get; set; } = new List<CourseLevel>();

    public virtual ICollection<Cohort> Cohorts { get; set; } = new List<Cohort>();

    public virtual ICollection<ProgramIntake> ProgramIntakes { get; set; } = new List<ProgramIntake>();

    public virtual ICollection<AttritionRate> AttritionRates { get; set; } = new List<AttritionRate>();

    public virtual ICollection<BaseSalary> BaseSalaries { get; set; } = new List<BaseSalary>();

    public virtual ICollection<ProgramFinancial> ProgramFinancials { get; set; } = new List<ProgramFinancial>();

    public virtual ICollection<DepartmentEnrollment> DepartmentEnrollments { get; set; } = new List<DepartmentEnrollment>();

    public virtual ICollection<DepartmentFinancial> DepartmentFinancials { get; set; } = new List<DepartmentFinancial>();

    public virtual ICollection<NonInstructionalsLoad> NonInstructionalsLoads { get; set; } = new List<NonInstructionalsLoad>();
}

// Supporting entity stubs for compilation context
public class Semester { public int Id { get; set; } public int ProgramId { get; set; } public int CurriculumYear { get; set; } public bool Fall { get; set; } public bool Spring { get; set; } public bool Summer { get; set; } }
public class CourseType { public int Id { get; set; } public int ProgramId { get; set; } public string? Type { get; set; } }
public class CourseLevel { public int Id { get; set; } public int ProgramId { get; set; } public string? Level { get; set; } }
public class Cohort { public int Id { get; set; } public int ProgramId { get; set; } public int CurrentStudents { get; set; } public int CurriculumYear { get; set; } }
public class ProgramIntake { public int Id { get; set; } public int ProgramId { get; set; } public int FiscalYear { get; set; } public int EnteringStudents { get; set; } }
public class AttritionRate { public int Id { get; set; } public int ProgramId { get; set; } public double? Rate { get; set; } public int CurriculumYear { get; set; } }
public class BaseSalary { public int Id { get; set; } public int ProgramId { get; set; } public string? InstructorType { get; set; } public int AverageSalary { get; set; } public double FringeBenefits { get; set; } }
public class ProgramFinancial { public int Id { get; set; } public int ProgramId { get; set; } public int FiscalYear { get; set; } public int EquipmentCost { get; set; } }
public class DepartmentEnrollment { public int Id { get; set; } public int ProgramId { get; set; } public int EnrolledStudents { get; set; } }
public class DepartmentFinancial { public int Id { get; set; } public int ProgramId { get; set; } public int Overhead { get; set; } }
public class NonInstructionalsLoad { public int Id { get; set; } public int ProgramId { get; set; } public int Load { get; set; } }
