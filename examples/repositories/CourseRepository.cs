// Purpose: Repository encapsulating CRUD operations and query lookups for instructional courses.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRPA.Portfolio.Examples.Models;

namespace PRPA.Portfolio.Examples.Repositories;

/// <summary>
/// Repository managing database access for <see cref="Course"/> entities.
/// </summary>
public class CourseRepository(DbContext context)
{
    private readonly DbContext _context = context;

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        return await _context.Set<Course>()
            .AsNoTracking()
            .Include(c => c.CourseType)
            .Include(c => c.CourseLevel)
            .Include(c => c.InstructorType)
            .ToListAsync();
    }

    public async Task<Course?> FindCourseByIdAsync(int id)
    {
        return await _context.Set<Course>()
            .Include(c => c.CourseType)
            .Include(c => c.CourseLevel)
            .Include(c => c.InstructorType)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Course>> FindCoursesByGroupIdAsync(int groupId)
    {
        return await _context.Set<Course>()
            .Where(c => c.GroupId == groupId)
            .Include(c => c.CourseType)
            .Include(c => c.CourseLevel)
            .ToListAsync();
    }

    public async Task<Course> CreateCourseAsync(Course course)
    {
        _context.Set<Course>().Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<bool> UpdateCourseAsync(int id, Course updatedCourseInfo)
    {
        Course? existingCourse = await _context.Set<Course>().FindAsync(id);
        if (existingCourse == null) return false;

        // Synchronize updated properties
        existingCourse.CourseTypeId = updatedCourseInfo.CourseTypeId;
        existingCourse.CourseLevelId = updatedCourseInfo.CourseLevelId;
        existingCourse.InstructorTypeId = updatedCourseInfo.InstructorTypeId;
        existingCourse.Cr = updatedCourseInfo.Cr;
        existingCourse.CrLoad = updatedCourseInfo.CrLoad;
        existingCourse.SectionMax = updatedCourseInfo.SectionMax;
        existingCourse.GroupId = updatedCourseInfo.GroupId;
        existingCourse.PercentOfStudents = updatedCourseInfo.PercentOfStudents;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        Course? courseToDelete = await _context.Set<Course>().FindAsync(id);
        if (courseToDelete is null) return false;

        _context.Set<Course>().Remove(courseToDelete);
        await _context.SaveChangesAsync();
        return true;
    }
}
