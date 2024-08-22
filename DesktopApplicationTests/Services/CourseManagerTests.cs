using Serilog;
using DesktopApplication.Models;
using DesktopApplicationTests.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DesktopApplication.Services.Tests
{
    public class CourseManagerTests
    {
        private readonly Mock<UniversityContext> _mockContext;
        private readonly Mock<DbSet<Course>> _mockCourseSet;
        private readonly Mock<DbSet<Group>> _mockGroupSet;
        private readonly Mock<ILogger> _mockLogger;
        private readonly CourseManager _courseManager;
        private List<Course> _courses;
        private List<Group> _groups;

        public CourseManagerTests()
        {
            _mockContext = new Mock<UniversityContext>();
            _mockLogger = new Mock<ILogger>();
            _mockCourseSet = new Mock<DbSet<Course>>();
            _mockGroupSet = new Mock<DbSet<Group>>();

            SetupMockContext();

            _courseManager = new CourseManager(_mockContext.Object, _mockLogger.Object);
        }

        private void SetupMockContext()
        {
            _courses = new List<Course>
        {
            new Course { CourseId = 1, Name = "Biology 101", Description = "Introduction to Biology" },
            new Course { CourseId = 2, Name = "Chemistry 101", Description = "Introduction to Chemistry" }
        };

            _groups = new List<Group>
        {
            new Group { GroupId = 1, CourseId = 1, Name = "Spring 2021 Biology" }
        };

            var coursesQueryable = _courses.AsQueryable();
            var groupsQueryable = _groups.AsQueryable();

            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Course>(coursesQueryable.Provider));
            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.Expression).Returns(coursesQueryable.Expression);
            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.ElementType).Returns(coursesQueryable.ElementType);
            _mockCourseSet.As<IQueryable<Course>>().Setup(m => m.GetEnumerator()).Returns(coursesQueryable.GetEnumerator());
            _mockCourseSet.As<IAsyncEnumerable<Course>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Course>(coursesQueryable.GetEnumerator()));
            _mockCourseSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns<object[]>(ids => new ValueTask<Course>(_courses.SingleOrDefault(c => c.CourseId == (int)ids[0])));

            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Group>(groupsQueryable.Provider));
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.Expression).Returns(groupsQueryable.Expression);
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.ElementType).Returns(groupsQueryable.ElementType);
            _mockGroupSet.As<IQueryable<Group>>().Setup(m => m.GetEnumerator()).Returns(groupsQueryable.GetEnumerator());
            _mockGroupSet.As<IAsyncEnumerable<Group>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Group>(groupsQueryable.GetEnumerator()));

            _mockContext.Setup(c => c.Courses).Returns(_mockCourseSet.Object);
            _mockContext.Setup(c => c.Groups).Returns(_mockGroupSet.Object);
        }

        [Fact]
        public async Task GetAllCoursesAsync_ShouldReturnAllCourses()
        {
            // Act
            var courses = await _courseManager.GetAllCoursesAsync();

            // Assert
            Xunit.Assert.Equal(2, courses.Count());
            Xunit.Assert.Contains(courses, c => c.Name == "Biology 101" && c.Description == "Introduction to Biology");
            Xunit.Assert.Contains(courses, c => c.Name == "Chemistry 101" && c.Description == "Introduction to Chemistry");
        }

        [Fact]
        public async Task AddCourseAsync_ShouldAddCourse()
        {
            // Arrange
            var courseName = "Physics 101";
            var description = "Introduction to Physics";

            // Act
            await _courseManager.AddCourseAsync(courseName, description);

            // Assert
            _mockCourseSet.Verify(m => m.Add(It.Is<Course>(c => c.Name == courseName && c.Description == description)), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateCourseAsync_ShouldUpdateCourse()
        {
            // Arrange
            int courseId = 1;
            var newName = "Advanced Biology";
            var newDescription = "Advanced Concepts in Biology";

            // Act
            await _courseManager.UpdateCourseAsync(courseId, newName, newDescription);

            // Assert
            var updatedCourse = _courses.FirstOrDefault(c => c.CourseId == courseId);
            Xunit.Assert.NotNull(updatedCourse);
            Xunit.Assert.Equal(newName, updatedCourse.Name);
            Xunit.Assert.Equal(newDescription, updatedCourse.Description);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteCourseAsync_ShouldDeleteCourse()
        {
            // Arrange
            int courseId = 2;  

            // Act
            await _courseManager.DeleteCourseAsync(courseId);

            // Assert
            _mockCourseSet.Verify(m => m.Remove(It.Is<Course>(c => c.CourseId == courseId)), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteCourseAsync_CourseNotFound_ShouldThrowException()
        {
            // Arrange
            int nonExistentCourseId = 99;

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _courseManager.DeleteCourseAsync(nonExistentCourseId));
        }

        [Fact]
        public async Task DeleteCourseAsync_CourseAssignedToGroups_ShouldThrowException()
        {
            // Arrange
            int courseIdAssignedToGroups = 1;  

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<Exception>(async () =>
                await _courseManager.DeleteCourseAsync(courseIdAssignedToGroups));
        }
    }
}