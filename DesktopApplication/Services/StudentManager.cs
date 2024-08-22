using Serilog;
using DesktopApplication.Models;
using Microsoft.EntityFrameworkCore;
using DesktopApplication.ViewModels;

namespace DesktopApplication.Services
{
    public class StudentManager
    {
        private readonly UniversityContext _context;
        private readonly ILogger _logger;
        public StudentManager(UniversityContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            _logger.Information("Fetching all students");
            return await _context.Students.Include(s => s.Group).ToListAsync();
        }

        public async Task AddStudentAsync(PersonRecord studentRecord, int groupId)
        {
            _logger.Information("Adding a new student: {FirstName} {LastName}", studentRecord.FirstName, studentRecord.LastName);

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                _logger.Warning("Group ID {GroupId} not found while adding a student", groupId);
                throw new Exception("Group not found");
            }

            var newStudent = new Student();
            PopulateStudentFromRecord(newStudent, studentRecord, group);


            _context.Students.Add(newStudent);
            await _context.SaveChangesAsync();
            _logger.Information("Student {FirstName} {LastName} added successfully", studentRecord.FirstName, studentRecord.LastName);
        }

        public async Task UpdateStudentAsync(int studentId, PersonRecord studentRecord)
        {
            _logger.Information("Updating student with ID: {StudentId}", studentId);

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                _logger.Warning("Student ID {StudentId} not found", studentId);
                throw new Exception("Student not found");
            }

            PopulateStudentFromRecord(student, studentRecord);

            await _context.SaveChangesAsync();
            _logger.Information("Student {StudentId} updated successfully", studentId);
        }

        private void PopulateStudentFromRecord(Student student, PersonRecord studentRecord, Group group = null)
        {
            student.FirstName = studentRecord.FirstName;
            student.LastName = studentRecord.LastName;

            if (group != null)
            {
                student.Group = group;
            }
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            _logger.Information("Deleting student with ID: {StudentId}", studentId);

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                _logger.Warning("Student ID {StudentId} not found", studentId);
                throw new Exception("Student not found");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            _logger.Information("Student {StudentId} deleted successfully", studentId);
        }
    }
}
