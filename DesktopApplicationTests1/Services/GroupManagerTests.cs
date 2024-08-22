using Microsoft.VisualStudio.TestTools.UnitTesting;
using DesktopApplication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopApplication.Models;
using Moq;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace DesktopApplication.Services.Tests
{
    [TestClass()]
    public class GroupManagerTests
    {
        private GroupManager CreateGroupManager(UniversityContext context)
        {
            var loggerMock = new Mock<ILogger>();
            return new GroupManager(context, loggerMock.Object);
        }

        private UniversityContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<UniversityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new UniversityContext(options);
            return context;
        }

        [TestMethod()]
        public async Task CreateGroupAsync_ShouldCreateGroup()
        {
            //Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);
            await groupManager.CreateGroupAsync("Chem 101", 1, 2);
            context.SaveChanges();

            //Act
            var result = context.Groups
                                .Where(g => g.Name == "Chem 101")
                                .FirstOrDefault();
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Chem 101", result.Name);
        }

        [TestMethod()]
        public async Task CreateGroupAsync_GroupAlreadyExists_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Courses.Add(new Course { CourseId = 1, Name = "Chemistry" });
            context.Teachers.Add(new Teacher { TeacherId = 2, FirstName = "Alan", LastName = "Turing" });
            context.Groups.Add(new Group { GroupId = 1, Name = "Chem 101", CourseId = 1, TeacherId = 2 });
            context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await groupManager.CreateGroupAsync("Chem 101", 1, 2);
            });
        }

        [TestMethod()]
        public async Task UpdateGroupAsync_ShouldUpdateGroup()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Courses.Add(new Course { CourseId = 1, Name = "Chemistry" });
            context.Teachers.Add(new Teacher { TeacherId = 2, FirstName = "Alan", LastName = "Turing" });
            context.Groups.Add(new Group { GroupId = 1, Name = "Chem 101", CourseId = 1, TeacherId = 2 });
            context.SaveChanges();

            // Act
            await groupManager.UpdateGroupAsync(1, "Advanced Chem 101", 1, 2);
            var result = context.Groups.FirstOrDefault(g => g.GroupId == 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Advanced Chem 101", result.Name);
        }

        [TestMethod()]
        public async Task UpdateGroupAsync_GroupNotFound_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await groupManager.UpdateGroupAsync(4, "Chem 104", 3, 8);
            });
        }

        [TestMethod()]
        public async Task UpdateGroupAsync_GroupWithSameTeacher_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);
            context.Courses.Add(new Course { CourseId = 1, Name = "Chemistry" });
            context.Teachers.Add(new Teacher { TeacherId = 1, FirstName = "Alan", LastName = "Turing" });
            context.Teachers.Add(new Teacher { TeacherId = 2, FirstName = "Albert", LastName = "Einstein" });
            context.Groups.Add(new Group { GroupId = 1, Name = "Chem 101", CourseId = 1, TeacherId = 1 });
            context.Groups.Add(new Group { GroupId = 2, Name = "Chem 102", CourseId = 1, TeacherId = 2 });
            context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await groupManager.UpdateGroupAsync(2, "Chem 102", 1, 1);
            });
        }

        [TestMethod()]
        public async Task UpdateGroupAsync_GroupWithSameName_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);
            context.Courses.Add(new Course { CourseId = 1, Name = "Chemistry" });
            context.Teachers.Add(new Teacher { TeacherId = 1, FirstName = "Alan", LastName = "Turing" });
            context.Teachers.Add(new Teacher { TeacherId = 2, FirstName = "Albert", LastName = "Einstein" });
            context.Groups.Add(new Group { GroupId = 1, Name = "Chem 101", CourseId = 1, TeacherId = 1 });
            context.Groups.Add(new Group { GroupId = 2, Name = "Chem 102", CourseId = 1, TeacherId = 2 });
            context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await groupManager.UpdateGroupAsync(2, "Chem 101", 1, 2);
            });
        }

        [TestMethod()]
        public async Task DeleteGroupAsync_ShouldDeleteGroup()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Courses.Add(new Course { CourseId = 1, Name = "Chemistry" });
            context.Groups.Add(new Group { GroupId = 1, Name = "Chem 101", CourseId = 1 });
            context.SaveChanges();

            // Act
            await groupManager.DeleteGroupAsync(1);
            var result = context.Groups.FirstOrDefault(g => g.GroupId == 1);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod()]
        public async Task DeleteGroupAsync_HasStudents_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Groups.Add(new Group
            {
                GroupId = 1,
                Name = "Chem 101",
                CourseId = 1,
                Students = new List<Student> { new Student { StudentId = 1, FirstName = "Student", LastName = "One" } }
            });
            context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await groupManager.DeleteGroupAsync(1);
            });
        }

        [TestMethod()]
        public async Task ClearGroupAsync_ShouldClearStudents()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Groups.Add(new Group
            {
                GroupId = 1,
                Name = "Chem 101",
                CourseId = 1,
                Students = new List<Student>
                 {
                 new Student { StudentId = 1, FirstName = "Student", LastName = "One" },
                 new Student { StudentId = 2, FirstName = "Student", LastName = "Two" }
                 }
            });
            context.SaveChanges();

            // Act
            await groupManager.ClearGroupAsync(1);
            var result = context.Groups.FirstOrDefault(g => g.GroupId == 1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Students.Any());
        }

        [TestMethod()]
        public void ExportStudents_Should_WriteCorrectCsv()
        {
            // Arrange
            int groupId = 1;
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Groups.Add(new Group
            {
                GroupId = 1,
                Name = "Chem 101",
                CourseId = 1,
                Students = new List<Student>
                 {
                    new Student { StudentId = 1, FirstName = "Alice", LastName = "Smith" },
                    new Student { StudentId = 2, FirstName = "Lindon", LastName = "Johnson" }
                 }
            });
            context.SaveChanges();

            using var memoryStream = new MemoryStream();
            // Act
            try
            {
                using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
                {
                    groupManager.ExportStudentsToStream(groupId, writer);
                    writer.Flush(); // ensure all data is written to stream before dispose
                }

                // Reset position after using the StreamWriter
                memoryStream.Position = 0;

                // Assert
                using var reader = new StreamReader(memoryStream);
                var csvContent = reader.ReadToEnd();

                Assert.IsTrue(csvContent.Contains("Alice,Smith"), "Expected to find 'Alice,Smith' in the CSV content.");
                Assert.IsTrue(csvContent.Contains("Lindon,Johnson"), "Expected to find 'Lindon,Johnson' in the CSV content.");
            }
            finally
            {
                memoryStream.Dispose(); // clean up the memoryStream
            }
        }

        [TestMethod()]
        public async Task ImportStudentsAsync_ShouldImportStudentsCorrectly()
        {
            // Arrange
            int groupId = 1;
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            context.Groups.Add(new Group
            {
                GroupId = groupId,
                Name = "Chem 101",
                CourseId = 1,
                Students = new List<Student>() // Start with an empty student list
            });
            context.SaveChanges();

            // Prepare CSV data for import
            string csvData = "FirstName,LastName\nAlice,Wonderland\nBob,Builder";
            var filePath = "fake_path.csv";

            // Substitute file IO with memory-based operations
            using (var reader = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvData)))
            {
                using (var streamWriter = new StreamWriter(filePath))
                {
                    streamWriter.Write(csvData);
                }
            }

            // Act
            await groupManager.ImportStudentsAsync(groupId, filePath);

            // Assert
            var group = context.Groups.Include(g => g.Students).FirstOrDefault(g => g.GroupId == groupId);
            Assert.IsNotNull(group);
            Assert.AreEqual(2, group.Students.Count); // Expect two new students
            Assert.IsTrue(group.Students.Any(s => s.FirstName == "Alice" && s.LastName == "Wonderland"));
            Assert.IsTrue(group.Students.Any(s => s.FirstName == "Bob" && s.LastName == "Builder"));
        }

        [TestMethod]
        public void GenerateDocx_ShouldCreateFile()
        {
            // Arrange
            int groupId = 1;
            var context = CreateInMemoryDbContext();
            var groupManager = CreateGroupManager(context);

            // Setup in-memory context data
            context.Groups.Add(new Group
            {
                GroupId = 1,
                Name = "Chem 101",
                CourseId = 1,
                Course = new Course { Name = "Biochemistry" }, // Assuming course object setup
                Students = new List<Student>
                 {
                     new Student { StudentId = 1, FirstName = "Alice", LastName = "Smith" },
                     new Student { StudentId = 2, FirstName = "Lindon", LastName = "Johnson" }
                 }
            });
            context.SaveChanges();

            // Use a temporary directory/file for test file creation
            var directory = Path.GetTempPath();
            var filePath = Path.Combine(directory, "test.docx");

            try
            {
                // Act
                groupManager.GenerateDocx(groupId, filePath);

                // Act - Load generated DOCX into memory
                using var wordDoc = WordprocessingDocument.Open(filePath, false);

                // Validate DOCX contents
                var body = wordDoc.MainDocumentPart.Document.Body;
                Assert.IsNotNull(body);

                // Validate Course Title
                var firstParagraph = body.Elements<Paragraph>().FirstOrDefault();
                Assert.IsNotNull(firstParagraph);
                Assert.IsTrue(firstParagraph.InnerText.Contains("Course: Biochemistry"));

                // Validate Group Name
                var secondText = firstParagraph.Elements<Run>().FirstOrDefault()?.Elements<Text>().Skip(1).FirstOrDefault();
                Assert.IsTrue(secondText.InnerText.Contains("Group: Chem 101"));

                // Validate Student List
                var studentParagraph = body.Elements<Paragraph>().Skip(1).FirstOrDefault();
                Assert.IsNotNull(studentParagraph);
                var studentsText = studentParagraph.Elements<Run>().Select(run => run.InnerText);

                int index = 1;
                foreach (var student in context.Groups.First().Students)
                {
                    Assert.IsTrue(studentsText.Any(st => st.Contains($"{index}. {student.FirstName} {student.LastName}")));
                    index++;
                }
            }
            finally
            {
                // Cleanup the file created during the test to ensure it doesn't linger
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }


        [TestMethod]
        public void GeneratePdf_ShouldLogAndGeneratePdf_WhenGroupExists()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger>();
            var groupManager = CreateGroupManager(context);

            // Seed the database with test data
            var course = new Course { CourseId = 1, Name = "Computer Science" };
            var group = new Group
            {
                GroupId = 1,
                Name = "Comp 101",
                CourseId = 1,
                Course = course,
                Students = new List<Student>
                 {
                     new Student {StudentId = 1,FirstName = "John", LastName = "Doe" },
                     new Student {StudentId = 2,FirstName = "Jane", LastName = "Smith" }
                 }
            };
            context.Courses.Add(course);
            context.Groups.Add(group);
            context.SaveChanges();

            // Create a temporary file path for the PDF
            string filePath = Path.GetTempFileName();

            try
            {
                // Act
                groupManager.GeneratePdf(1, filePath);

                // Assert
                // Optionally verify that the file was created
                Assert.IsTrue(File.Exists(filePath));

                // Optionally, verify the contents of the PDF (e.g., using iText to read the file)
                using (var reader = new PdfReader(filePath))
                using (var pdfDoc = new PdfDocument(reader))
                {
                    var text = new StringBuilder();

                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                    {
                        // Extract text from each page using the correct method
                        var pdfPage = pdfDoc.GetPage(page);
                        string pageText = PdfTextExtractor.GetTextFromPage(pdfPage);
                        text.Append(pageText);
                    }

                    string pdfContent = text.ToString();
                    Assert.IsTrue(pdfContent.Contains("Course: Computer Science"));
                    Assert.IsTrue(pdfContent.Contains("Group: Comp 101"));
                    Assert.IsTrue(pdfContent.Contains("1. John Doe"));
                    Assert.IsTrue(pdfContent.Contains("2. Jane Smith"));
                }
            }
            finally
            {
                // Cleanup: Delete the temporary file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
