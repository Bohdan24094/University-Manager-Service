using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopApplication.Models;
using DesktopApplication.ViewModels;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace DesktopApplication.Services
{
    public class TeacherManager
    {
        private readonly UniversityContext _context;
        private readonly ILogger _logger;
        public TeacherManager(UniversityContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            _logger.Information("Fetching all teachers");
            return await _context.Teachers.ToListAsync();
        }

        public async Task AddTeacherAsync(PersonRecord teacherRecord)
        {
            _logger.Information("Adding a new teacher: {FirstName} {LastName}", teacherRecord.FirstName, teacherRecord.LastName);

            var newTeacher = new Teacher();
            PopulateTeacherFromRecord(newTeacher, teacherRecord);
            

            _context.Teachers.Add(newTeacher);
            await _context.SaveChangesAsync();
            _logger.Information("Teacher {FirstName} {LastName} added successfully", teacherRecord.FirstName, teacherRecord.LastName);
        }

        public async Task UpdateTeacherAsync(int teacherId, PersonRecord teacherRecord)
        {
            _logger.Information("Updating teacher with ID: {TeacherId}", teacherId);

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
            {
                _logger.Warning("Teacher ID {TeacherId} not found", teacherId);
                throw new Exception("Teacher not found");
            }

            PopulateTeacherFromRecord(teacher, teacherRecord);

            await _context.SaveChangesAsync();
            _logger.Information("Teacher {TeacherId} updated successfully", teacherId);
        }

        private void PopulateTeacherFromRecord(Teacher teacher, PersonRecord teacherRecord)
        {
            teacher.FirstName = teacherRecord.FirstName;
            teacher.LastName = teacherRecord.LastName;
        }

        public async Task DeleteTeacherAsync(int teacherId)
        {
            _logger.Information("Deleting teacher with ID: {TeacherId}", teacherId);

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher == null)
            {
                _logger.Warning("Teacher ID {TeacherId} not found", teacherId);
                throw new Exception("Teacher not found");
            }
            var groupsWithTeacher = await _context.Groups
                                                  .Where(g => g.TeacherId == teacherId)
                                                  .ToListAsync();
            if (groupsWithTeacher.Any())
            {
                _logger.Warning("Cannot delete teacher {TeacherId} because they are assigned to groups: {GroupIds}", teacherId, string.Join(", ", groupsWithTeacher.Select(g => g.GroupId)));
                MessageBox.Show($"Cannot delete the teacher '{teacher.FirstName} {teacher.LastName}' because they are assigned to groups. Please reassign those groups to a different teacher before deletion.");
                return; 
            }
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            _logger.Information("Teacher {TeacherId} deleted successfully", teacherId);
        }
    }
}
