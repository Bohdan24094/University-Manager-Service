using DesktopApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.EntityFrameworkCore;
using DesktopApplication.ViewModels;
using System.Windows;

namespace DesktopApplication.Services
{
    public class CourseManager
    {
        private readonly UniversityContext _context;
        private readonly ILogger _logger;

        public CourseManager(UniversityContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Course>> GetAllTeachersAsync()
        {
            _logger.Information("Fetching all courses");
            return await _context.Courses.ToListAsync();
        }

        public async Task AddCourseAsync(string name, string description)
        {
            _logger.Information("Adding a new course");

            var newCourse = new Course
            {
                Name = name,
                Description = description
            };

            _context.Courses.Add(newCourse);
            await _context.SaveChangesAsync();
            _logger.Information("Course added successfully");
        }

        public async Task UpdateCourseAsync(int courseId, string name, string description)
        {
            _logger.Information("Updating course with ID: {courseId}", courseId);

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                _logger.Warning("Course ID {CourseId} not found", courseId);
                throw new Exception("Course not found");
            }
            course.Name = name;
            course.Description = description;
            await _context.SaveChangesAsync();
            _logger.Information("Course ID {CourseId} updated successfully", courseId);

        }

        public async Task DeleteCourseAsync(int courseId)
        {
            _logger.Information("Deleting course with ID: {courseId}", courseId);

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                _logger.Warning("Course ID {courseId} not found", courseId);
                throw new Exception("Course not found");
            }

            // Check for groups associated with this teacher
            var groupsWithCourse = await _context.Groups
                                                  .Where(g => g.CourseId == courseId)
                                                  .ToListAsync();

            if (groupsWithCourse.Any())
            {
                // Notify the user of the association
                _logger.Warning("Cannot delete course {courseId} because they are assigned to groups: {GroupIds}", courseId, string.Join(", ", groupsWithCourse.Select(g => g.GroupId)));

                // Present a message to the user
                MessageBox.Show($"Cannot delete the course because they are assigned to groups. Please reassign those groups to a different teacher before deletion.");

                return; // Exit the method
            }

            // No groups associated, safe to delete
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            _logger.Information("Course {courseId} deleted successfully", courseId);
        }


    }
}
