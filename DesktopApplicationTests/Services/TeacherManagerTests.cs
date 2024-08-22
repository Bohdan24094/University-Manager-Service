using DesktopApplication.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Serilog;
using DesktopApplication.ViewModels;
using DesktopApplicationTests.Models;

namespace DesktopApplication.Services.Tests
{
    public class TeacherManagerTests
    {
        private readonly Mock<UniversityContext> _mockContext;
        private readonly Mock<DbSet<Teacher>> _mockTeacherSet;
        private readonly Mock<DbSet<Group>> _mockGroupSet;
        private readonly Mock<ILogger> _mockLogger;
        private readonly TeacherManager _teacherManager;
        private List<Teacher> _teachers;
        private List<Group> _groups;

        public TeacherManagerTests()
        {
            _mockContext = new Mock<UniversityContext>();
            _mockLogger = new Mock<ILogger>();
            _mockTeacherSet = new Mock<DbSet<Teacher>>();
            _mockGroupSet = new Mock<DbSet<Group>>();

            SetupMockContext();

            _teacherManager = new TeacherManager(_mockContext.Object, _mockLogger.Object);
        }

        private void SetupMockContext()
        {
            _teachers = new List<Teacher>
        {
            new Teacher { TeacherId = 1, FirstName = "John", LastName = "Doe" },
            new Teacher { TeacherId = 2, FirstName = "Jane", LastName = "Smith" }
        };

            _groups = new List<Group>
        {
            new Group { GroupId = 1, TeacherId = 1, CourseId = 1, Name = "Biology 101" }
        };

            var teachersQueryable = _teachers.AsQueryable();
            var groupsQueryable = _groups.AsQueryable();

            _mockTeacherSet.As<IQueryable<Teacher>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Teacher>(teachersQueryable.Provider));
            _mockTeacherSet.As<IQueryable<Teacher>>().Setup(m => m.Expression).Returns(teachersQueryable.Expression);
            _mockTeacherSet.As<IQueryable<Teacher>>().Setup(m => m.ElementType).Returns(teachersQueryable.ElementType);
            _mockTeacherSet.As<IQueryable<Teacher>>().Setup(m => m.GetEnumerator()).Returns(teachersQueryable.GetEnumerator());
            _mockTeacherSet.As<IAsyncEnumerable<Teacher>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Teacher>(teachersQueryable.GetEnumerator()));

            _mockTeacherSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(ids => new ValueTask<Teacher>(_teachers.SingleOrDefault(t => t.TeacherId == (int)ids[0])));

            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Group>(groupsQueryable.Provider));
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Expression).Returns(groupsQueryable.Expression);
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.ElementType).Returns(groupsQueryable.ElementType);
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.GetEnumerator()).Returns(groupsQueryable.GetEnumerator());
            _mockGroupSet.As<IAsyncEnumerable<Group>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Group>(groupsQueryable.GetEnumerator()));

            _mockContext.Setup(c => c.Teachers).Returns(_mockTeacherSet.Object);
            _mockContext.Setup(c => c.Groups).Returns(_mockGroupSet.Object);
        }

        [Fact]
        public async Task GetAllTeachersAsync_ShouldReturnAllTeachers()
        {
            // Act
            var teachers = await _teacherManager.GetAllTeachersAsync();

            // Assert
            Xunit.Assert.Equal(2, teachers.Count());
            Xunit.Assert.Contains(teachers, t => t.FirstName == "John" && t.LastName == "Doe");
            Xunit.Assert.Contains(teachers, t => t.FirstName == "Jane" && t.LastName == "Smith");
        }


        [Fact]
        public async Task AddTeacherAsync_ShouldAddTeacher()
        {
            // Arrange
            var teacherRecord = new PersonRecord { FirstName = "Mark", LastName = "Johnson" };

            // Act
            await _teacherManager.AddTeacherAsync(teacherRecord);

            // Assert
            _mockTeacherSet.Verify(m => m.Add(It.Is<Teacher>(t => t.FirstName == "Mark" && t.LastName == "Johnson")), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }


        [Fact]
        public async Task UpdateTeacherAsync_ShouldUpdateTeacher()
        {
            // Arrange
            var teacherRecord = new PersonRecord { FirstName = "Johnny", LastName = "Doeson" };

            // Act
            await _teacherManager.UpdateTeacherAsync(1, teacherRecord);

            // Assert
            var updatedTeacher = _teachers.FirstOrDefault(t => t.TeacherId == 1);
            Xunit.Assert.NotNull(updatedTeacher);
            Xunit.Assert.Equal("Johnny", updatedTeacher.FirstName);
            Xunit.Assert.Equal("Doeson", updatedTeacher.LastName);
        }

        [Fact]
        public async Task UpdateTeacherAsync_TeacherNotFound_ShouldThrowException()
        {
            // Arrange
            var nonExistentTeacherId = 99;
            var teacherRecord = new PersonRecord { FirstName = "Non", LastName = "Existent" };

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _teacherManager.UpdateTeacherAsync(nonExistentTeacherId, teacherRecord));
        }

        [Fact]
        public async Task DeleteTeacherAsync_ShouldDeleteTeacher()
        {
            // Act
            await _teacherManager.DeleteTeacherAsync(2); 

            // Assert
            _mockTeacherSet.Verify(m => m.Remove(It.Is<Teacher>(t => t.TeacherId == 2)), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteTeacherAsync_TeacherNotFound_ShouldThrowException()
        {
            // Arrange
            var nonExistentTeacherId = 99;

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _teacherManager.DeleteTeacherAsync(nonExistentTeacherId));
        }

        [Fact]
        public async Task DeleteTeacherAsync_TeacherAssignedToGroup_ShouldThrowException()
        {
            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _teacherManager.DeleteTeacherAsync(1)); 
        }
    }
}