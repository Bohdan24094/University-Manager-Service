﻿using DesktopApplication.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using CsvHelper;
using System.IO;
using iText.Kernel.Pdf;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Serilog;
using DesktopApplication.ViewModels;
using System.Windows;

namespace DesktopApplication.Services
{
    public class GroupManager
    {
        private readonly ILogger _logger;
        private  readonly UniversityContext _context;

        public GroupManager(UniversityContext context, ILogger logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            return await _context.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Include(g => g.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            return await _context.Teachers.ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task CreateGroupAsync(string groupName, int courseId, int teacherId)
        {
            _logger.Information("Creating new group: {GroupName}", groupName);

            var existingGroup = await _context.Groups
                                              .FirstOrDefaultAsync(g => g.Name == groupName && g.CourseId == courseId);
            var groupWithSameTeacher = await _context.Groups
                .Where(g =>  g.TeacherId == teacherId )
                .FirstOrDefaultAsync();
            if (existingGroup != null)
            {
                _logger.Warning("A group with the same name '{GroupName}' already exists in course '{CourseId}'", groupName, courseId);
                throw new Exception($"A group with the same name currently exists in this course. You can change the name or create it in another course.");
            }
            else if (groupWithSameTeacher != null)
            {
                _logger.Warning("A group with TeacherId '{TeacherId}' already exists", teacherId);
                throw new Exception("The teacher is already assigned to another group. Please choose another teacher.");
            }

            var newGroup = new Group
            {
                Name = groupName,
                CourseId = courseId,
                TeacherId = teacherId
            };

            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();

            _logger.Information("Group {GroupName} created successfully", groupName);
        }

        public async Task UpdateGroupAsync(int groupId, string groupName, int courseId, int teacherId)
        {
            _logger.Information("Updating group with GroupId: {GroupId}", groupId);

            var groupWithSameName = await _context.Groups
                .Where(g => g.CourseId == courseId && g.Name == groupName && g.GroupId != groupId)
                .FirstOrDefaultAsync();

            var groupWithSameTeacher = await _context.Groups
                .Where(g => g.TeacherId == teacherId && g.GroupId != groupId)
                .FirstOrDefaultAsync();

            if (groupWithSameName != null)
            {
                _logger.Warning("A group with the same name '{GroupName}' already exists in the course '{CourseId}'", groupName, courseId);
                throw new Exception("A group with the same name currently exists in this course. Please choose another name.");
            }

            if (groupWithSameTeacher != null)
            {
                _logger.Warning("A group with TeacherId '{TeacherId}' already exists", teacherId);
                throw new Exception("The teacher is already assigned to another group. Please choose another teacher.");
            }

            var selectedGroup = await _context.Groups.FindAsync(groupId);
            if (selectedGroup != null)
            {
                selectedGroup.Name = groupName;
                selectedGroup.CourseId = courseId;
                selectedGroup.TeacherId = teacherId;
                await _context.SaveChangesAsync();
                string successString = "Group {GroupName} updated successfully";
                _logger.Information(successString, groupName);
            }
            else
            {
                string errorString = "The specified group could not be found.";
                _logger.Warning(errorString, groupId);
                throw new Exception(errorString);
            }
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            string errorMessage = "Cannot delete group {GroupName} because it has students";
            _logger.Information("Deleting group {GroupId}", groupId);
            var groupName = "";
            var selectedGroup = _context.Groups
                                        .Include(g => g.Students)
                                        .FirstOrDefault(g => g.GroupId == groupId);
            if (selectedGroup != null && !selectedGroup.Students.Any())
            {
                _context.Groups.Remove(selectedGroup);
                await _context.SaveChangesAsync();
                _logger.Information("Group {GroupName} deleted successfully", groupName);
            }
            else
            {
                _logger.Warning(errorMessage, groupName);
                throw new Exception("Cannot delete group {GroupName} because it has students");
            }
        }

        public async Task ClearGroupAsync(int groupId)
        {
            _logger.Information("Clearing students in Group ID: {GroupId}", groupId);
            string errorMessage = "No students found to clear for Group";
            var group = await _context.Groups.Include(g => g.Students)
                                             .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group != null && group.Students.Any())
            {
                _context.Students.RemoveRange(group.Students);
                await _context.SaveChangesAsync();
                _logger.Information("Students cleared for Group ID: {GroupId}", groupId);
            }
            else
            {
                _logger.Information(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public void ExportStudents(int groupId, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                ExportStudentsToStream(groupId, writer);
            }
        }

        // New method for testing
        public void ExportStudentsToStream(int groupId, TextWriter writer)
        {
            _logger.Information("Exporting students for group {GroupId}", groupId);

            var selectedGroup = _context.Groups
                                        .Include(g => g.Students)
                                        .FirstOrDefault(g => g.GroupId == groupId);
            if (selectedGroup == null)
            {
                return;
            }
            var exportedStudents = selectedGroup.Students.Select(student => new StudentExport
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                GroupName = selectedGroup.Name
            }).ToList();

            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(exportedStudents);
            }
            _logger.Information("Students exported successfully for group {GroupId}", groupId);
        }

        public async Task ImportStudentsAsync(int groupId, string filePath)
        {
            _logger.Information("Importing students asynchronously for Group ID: {GroupId}", groupId);

            var selectedGroup = await _context.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (selectedGroup == null)
            {
                _logger.Warning("Group ID: {GroupId} does not exist. Aborting import.", groupId);
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var studentRecords = csv.GetRecords<PersonRecord>().ToList();

                        foreach (var record in studentRecords)
                        {
                            var newStudent = new Student
                            {
                                FirstName = record.FirstName,
                                LastName = record.LastName,
                                Group = selectedGroup
                            };

                            _context.Students.Add(newStudent);
                            _logger.Information("Adding student: {FirstName} {LastName} to Group: {GroupId}", newStudent.FirstName, newStudent.LastName, selectedGroup.GroupId);
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    _logger.Information("Students imported successfully for Group ID: {GroupId}", groupId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.Error(ex, "Error importing students for Group ID: {GroupId}", groupId);
                    throw;
                }
            }
        }

        public void GenerateDocx(int groupId, string filePath)
        {
            _logger.Information("Generating DOCX for group {GroupId} at {FilePath}", groupId, filePath);

            var selectedGroup = _context.Groups
                                        .Include(g => g.Course)
                                        .Include(g => g.Students)
                                        .FirstOrDefault(g => g.GroupId == groupId);
            if (selectedGroup != null)
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    Body body = new Body();
                    mainPart.Document.Append(body);

                    DocumentFormat.OpenXml.Wordprocessing.Paragraph titleParagraph = new Paragraph();
                    Run titleRun = new Run();
                    titleRun.Append(new Text($"Course: {selectedGroup.Course.Name}"));
                    titleRun.Append(new Break());
                    titleRun.Append(new Text($"Group: {selectedGroup.Name}"));
                    titleParagraph.Append(titleRun);
                    body.Append(titleParagraph);

                    Paragraph studentsParagraph = new Paragraph();
                    Run studentsRun = new Run();
                    int index = 1;
                    foreach (var student in selectedGroup.Students)
                    {
                        studentsRun.Append(new Text($"{index}. {student.FirstName} {student.LastName}"));
                        studentsRun.Append(new Break());
                        index++;
                    }
                    studentsParagraph.Append(studentsRun);
                    body.Append(studentsParagraph);
                }
                _logger.Information("DOCX generated successfully for group {GroupId}", groupId);
            }
            else
            {
                _logger.Warning("Group {GroupId} not found for DOCX generation", groupId);
            }
        }

       public void GeneratePdf(int groupId, string filePath)
       {
            _logger.Information("Generating PDF for group {GroupId} at {FilePath}", groupId, filePath);

            var selectedGroup = _context.Groups
                                        .Include(g => g.Course)
                                        .Include(g => g.Students)
                                        .FirstOrDefault(g => g.GroupId == groupId);
           if (selectedGroup != null)
           {
               using (PdfWriter writer = new PdfWriter(filePath))
               {
                   PdfDocument pdf = new PdfDocument(writer);
                   iText.Layout.Document document = new iText.Layout.Document(pdf);

                   iText.Layout.Element.Paragraph title = new iText.Layout.Element.Paragraph($"Course: {selectedGroup.Course.Name}")
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                       .SetFontSize(20);
                   document.Add(title);
       
                   iText.Layout.Element.Paragraph groupTitle = new iText.Layout.Element.Paragraph($"Group: {selectedGroup.Name}")
                       .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                       .SetFontSize(16);
                   document.Add(groupTitle);
       
                   int index = 1;
                   foreach (var student in selectedGroup.Students)
                   {
                       document.Add(new iText.Layout.Element.Paragraph($"{index}. {student.FirstName} {student.LastName}"));
                       index++;
                   }
                   document.Close();
               }
                _logger.Information("PDF generated successfully for group {GroupId}", groupId);
            }
           else
            {
                _logger.Warning("Group {GroupId} not found for PDF generation", groupId);   
            }
        }
    }
}
