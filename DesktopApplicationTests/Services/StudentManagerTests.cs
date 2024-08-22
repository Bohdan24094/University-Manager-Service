using DesktopApplication.Models;
using DesktopApplicationTests.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using Xunit;
using DesktopApplication.ViewModels;

namespace DesktopApplication.Services.Tests
{
    public class StudentManagerTests
    {
        private readonly Mock<UniversityContext> _mockContext;
        private readonly Mock<DbSet<Student>> _mockStudentSet;
        private readonly Mock<DbSet<Group>> _mockGroupSet;
        private readonly Mock<ILogger> _mockLogger;
        private readonly StudentManager _studentManager;
        private List<Student> _students;
        private List<Group> _groups;

        public StudentManagerTests()
        {
            _mockContext = new Mock<UniversityContext>();
            _mockLogger = new Mock<ILogger>();
            _mockStudentSet = new Mock<DbSet<Student>>();
            _mockGroupSet = new Mock<DbSet<Group>>();

            SetupMockContext();

            _studentManager = new StudentManager(_mockContext.Object, _mockLogger.Object);
        }

        private void SetupMockContext()
        {
            _groups = new List<Group>
                 {
                     new Group { GroupId = 1, Name = "Biology 101" }
                 };

            _students = new List<Student>
                 {
                     new Student { StudentId = 1, FirstName = "Alice", LastName = "Smith", Group = _groups[0] },
                     new Student { StudentId = 2, FirstName = "Bob", LastName = "Johnson", Group = _groups[0] }
                 };

            var studentsQueryable = _students.AsQueryable();
            var groupsQueryable = _groups.AsQueryable();

            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Student>(studentsQueryable.Provider));
            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.Expression).Returns(studentsQueryable.Expression);
            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.ElementType).Returns(studentsQueryable.ElementType);
            _mockStudentSet.As<IQueryable<Student>>().Setup(m => m.GetEnumerator()).Returns(studentsQueryable.GetEnumerator());
            _mockStudentSet.As<IAsyncEnumerable<Student>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Student>(studentsQueryable.GetEnumerator()));
            _mockStudentSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns<object[]>(ids => new ValueTask<Student>(_students.SingleOrDefault(s => s.StudentId == (int)ids[0])));

            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Group>(groupsQueryable.Provider));
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Expression).Returns(groupsQueryable.Expression);
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.ElementType).Returns(groupsQueryable.ElementType);
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.GetEnumerator()).Returns(groupsQueryable.GetEnumerator());
            _mockGroupSet.As<IAsyncEnumerable<Group>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Group>(groupsQueryable.GetEnumerator()));
            _mockGroupSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns<object[]>(ids => new ValueTask<Group>(_groups.SingleOrDefault(g => g.GroupId == (int)ids[0])));

            _mockContext.Setup(c => c.Students).Returns(_mockStudentSet.Object);
            _mockContext.Setup(c => c.Groups).Returns(_mockGroupSet.Object);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents()
        {
            // Act
            var students = await _studentManager.GetAllStudentsAsync();

            // Assert
            Xunit.Assert.Equal(2, students.Count());
            Xunit.Assert.Contains(students, s => s.FirstName == "Alice" && s.LastName == "Smith");
            Xunit.Assert.Contains(students, s => s.FirstName == "Bob" && s.LastName == "Johnson");
        }

        [Fact]
        public async Task AddStudentAsync_ShouldAddStudent()
        {
            // Arrange
            var studentRecord = new PersonRecord { FirstName = "Charlie", LastName = "Brown" };
            int groupId = 1;  

            // Act
            await _studentManager.AddStudentAsync(studentRecord, groupId);

            // Assert
            _mockStudentSet.Verify(m => m.Add(It.Is<Student>(s => s.FirstName == "Charlie" && s.LastName == "Brown")), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task AddStudentAsync_GroupNotFound_ShouldThrowException()
        {
            // Arrange
            var studentRecord = new PersonRecord { FirstName = "Charlie", LastName = "Brown" };
            int nonExistentGroupId = 99;  

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _studentManager.AddStudentAsync(studentRecord, nonExistentGroupId));
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldUpdateStudent()
        {
            // Arrange
            var studentRecord = new PersonRecord { FirstName = "Alice", LastName = "Smithers" };
            int studentId = 1;

            // Act
            await _studentManager.UpdateStudentAsync(studentId, studentRecord);

            // Assert
            var updatedStudent = _students.FirstOrDefault(s => s.StudentId == studentId);
            Xunit.Assert.NotNull(updatedStudent);
            Xunit.Assert.Equal("Alice", updatedStudent.FirstName);
            Xunit.Assert.Equal("Smithers", updatedStudent.LastName);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentAsync_StudentNotFound_ShouldThrowException()
        {
            // Arrange
            var studentRecord = new PersonRecord { FirstName = "Nonexistent", LastName = "Person" };
            int nonExistentStudentId = 99;  

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _studentManager.UpdateStudentAsync(nonExistentStudentId, studentRecord));
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldDeleteStudent()
        {
            // Arrange
            int studentId = 2;  

            // Act
            await _studentManager.DeleteStudentAsync(studentId);

            // Assert
            _mockStudentSet.Verify(m => m.Remove(It.Is<Student>(s => s.StudentId == studentId)), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_StudentNotFound_ShouldThrowException()
        {
            // Arrange
            int nonExistentStudentId = 99;  

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _studentManager.DeleteStudentAsync(nonExistentStudentId));
        }

    }
}