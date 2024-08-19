using DesktopApplication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;
using CsvHelper;
using System.Globalization;
using DesktopApplication.Models;
using Serilog;
using DesktopApplication.ViewModels;
using Xunit;

namespace DesktopApplication.Services.Tests
{
    public class GroupManagerTests
    {
        private readonly Mock<UniversityContext> _mockContext;
        private readonly Mock<ILogger> _mockLogger;
        private readonly GroupManager _groupManager;
        private readonly string _testDirectory;
        private readonly string _testFilePath;

        public GroupManagerTests()
        {
            _mockContext = new Mock<UniversityContext>();
            _mockLogger = new Mock<ILogger>();

            // Setup mock database sets
            SetupMockContext();

            _groupManager = new GroupManager(_mockContext.Object, _mockLogger.Object);

            // Create temporary directory and file for testing
            _testDirectory = Path.Combine(Path.GetTempPath(), "GroupManagerTests");
            Directory.CreateDirectory(_testDirectory);
            _testFilePath = Path.Combine(_testDirectory, "students.csv");
        }

        private void SetupMockContext()
        {
            var groupId = 1;
            var groups = new List<Group>
        {
            new Group
            {
                GroupId = groupId,
                Name = "Test Group",
                Students = new List<Student>
                {
                    new Student { StudentId = 1, FirstName = "John", LastName = "Doe" },
                    new Student { StudentId = 2, FirstName = "Jane", LastName = "Smith" }
                }
            }
        }.AsQueryable();

            var mockGroupSet = new Mock<DbSet<Group>>();
            mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Provider).Returns(groups.Provider);
            mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Expression).Returns(groups.Expression);
            mockGroupSet.As<IQueryable<Group>>().Setup(m => m.ElementType).Returns(groups.ElementType);
            mockGroupSet.As<IQueryable<Group>>().Setup(m => m.GetEnumerator()).Returns(groups.GetEnumerator());

            _mockContext.Setup(c => c.Groups).Returns(mockGroupSet.Object);
        }

        // Clean up the test file before each test
        [Fact]
        public void ExportStudents_Should_WriteCorrectCsv()
        {
            // Arrange
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }

            var groupId = 1;

            // Act
            _groupManager.ExportStudents(groupId, _testFilePath);

            // Assert
            Xunit.Assert.True(File.Exists(_testFilePath));

            using var reader = new StreamReader(_testFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<StudentExport>().ToList();

            Xunit.Assert.Equal(2, records.Count);
            Xunit.Assert.Equal("John", records[0].FirstName);
            Xunit.Assert.Equal("Doe", records[0].LastName);
            Xunit.Assert.Equal("Jane", records[1].FirstName);
            Xunit.Assert.Equal("Smith", records[1].LastName);
        }
    }
}