// Purpose: Entity model representing an instructional course within a curriculum group.

namespace PRPA.Portfolio.Examples.Models;

/// <summary>
/// Represents an individual course offering within a curriculum delivery schedule.
/// </summary>
public class Course
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the course type lookup (e.g., Lecture, Lab, Seminar).
    /// </summary>
    public int? CourseTypeId { get; set; }

    /// <summary>
    /// Foreign key to the course level lookup (e.g., 100-level, 400-level, Graduate).
    /// </summary>
    public int? CourseLevelId { get; set; }

    /// <summary>
    /// Foreign key to the instructor salary band assigned to teach this course.
    /// </summary>
    public int? InstructorTypeId { get; set; }

    /// <summary>
    /// Student credit hours awarded for completing the course.
    /// </summary>
    public int? Cr { get; set; }

    /// <summary>
    /// Faculty teaching load credits (workload credit hours counted toward instructor FTE).
    /// May differ from student credits (e.g., in laboratory or studio courses).
    /// </summary>
    public int? CrLoad { get; set; }

    /// <summary>
    /// Maximum student enrollment capacity allowed per section.
    /// </summary>
    public int? SectionMax { get; set; }

    /// <summary>
    /// Foreign key to the parent course group inside a semester.
    /// </summary>
    public int? GroupId { get; set; }

    /// <summary>
    /// Expected enrollment percentage of the active cohort (0.0 to 1.0).
    /// Set to 1.0 for mandatory core courses; lower for elective options.
    /// </summary>
    public double? PercentOfStudents { get; set; }

    // --- Navigation Properties ---

    public virtual CourseType? CourseType { get; set; }

    public virtual CourseLevel? CourseLevel { get; set; }

    public virtual BaseSalary? InstructorType { get; set; }
}
