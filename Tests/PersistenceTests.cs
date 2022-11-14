using ManyToManyAddExistingEntity.Data;
using ManyToManyAddExistingEntity.Models;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
namespace ManyToManyAddExistingEntity.Tests;

public class PersistenceTests
{
    private const string ExpectedCourseName = "Data Structures 101";
    private const string MathsCourseName = "Maths 101";
    private const string PhysicsCourseName = "Physics 101";

    private static DbContextOptions<SchoolContext> BuildOptions() => new DbContextOptionsBuilder<SchoolContext>()
        .UseInMemoryDatabase("InMemoryDB")
        .Options;

    [Fact]
    public async Task ManyToMany_AddExistingEntity_IsNotFixedUpByEFCore()
    {
        Student? student = null;

        await using (var dbContext = new SchoolContext(BuildOptions()))
        {
            student = new Student() { FirstMidName = "John", LastName = "Doe" };

            var course = new Course() { Title = PhysicsCourseName, Credits = 20 };

            student.Courses = new List<Course>() { course };
            dbContext.Students.Add(student);

            await dbContext.SaveChangesAsync();

            // EF Core fixes up the relationship as expected when it's a new
            // entity.
            student.StudentCourses.ShouldContain(x => x.Course.Title == PhysicsCourseName);
        }

        await using (var dbContext = new SchoolContext(BuildOptions()))
        {
            var persistedStudent = await dbContext.Students
                .Include(x => x.StudentCourses)
                .Include(x => x.Courses)
                .SingleAsync(x => x.Id == student.Id);

            var newCourse = new Course() { Title = MathsCourseName, Credits = 20 };

            // Add the new Math course to the student. The student already has
            // one course, Physics.
            persistedStudent.Courses.Add(newCourse);

            // EF Core tracker hasn't detected changes yet, so StudentCourses
            // will still only contain the original Physics course.
            persistedStudent.StudentCourses.ShouldSatisfyAllConditions(
                sc => sc.Count.ShouldBe(1),
                sc => sc.ShouldContain(x => x.Course.Title == PhysicsCourseName)
            );

            // This triggers change detection.
            await dbContext.SaveChangesAsync();

            // And StudentCourses now correctly has Physics and Math.
            persistedStudent.StudentCourses.ShouldSatisfyAllConditions(
                sc => sc.Count.ShouldBe(2),
                sc => sc.ShouldContain(x => x.Course.Title == PhysicsCourseName),
                sc => sc.ShouldContain(x => x.Course.Title == MathsCourseName)
            );

            // Create a course in the database, but don't add it to the student
            // yet.
            var existingCourse = new Course { Title = ExpectedCourseName, Credits = 20 };
            dbContext.Courses.Add(existingCourse);
            await dbContext.SaveChangesAsync();

            // Add the previously existing course, Data Structures, to the
            // student.
            persistedStudent.Courses.Add(existingCourse);

            // Change detection hasn't happened, so StudentCourses still has
            // two courses.
            persistedStudent.StudentCourses.ShouldSatisfyAllConditions(
                sc => sc.Count.ShouldBe(2),
                sc => sc.ShouldContain(x => x.Course.Title == MathsCourseName),
                sc => sc.ShouldContain(x => x.Course.Title == PhysicsCourseName)
            );

            // This triggers change detection.
            await dbContext.SaveChangesAsync();

            // StudentCourses should now have the extra course, Data
            // Structures, but doesn't.
            persistedStudent.StudentCourses.ShouldSatisfyAllConditions(
                sc => sc.Count.ShouldBe(3),
                sc => sc.ShouldContain(x => x.Course.Title == ExpectedCourseName),
                sc => sc.ShouldContain(x => x.Course.Title == MathsCourseName),
                sc => sc.ShouldContain(x => x.Course.Title == PhysicsCourseName)
            );
        }
    }

    [Fact]
    public async Task ManyToMany_AddNewEntity_IsFixedUpByEFCore()
    {
        Student? student = null;

        await using var dbContext = new SchoolContext(BuildOptions());
        student = new Student() { FirstMidName = "John", LastName = "Doe" };

        var course = new Course() { Title = PhysicsCourseName, Credits = 20 };

        student.Courses = new List<Course>() { course };
        dbContext.Students.Add(student);

        await dbContext.SaveChangesAsync();

        // EF Core fixes up the relationship as expected when it's a new
        // entity.
        student.StudentCourses.ShouldSatisfyAllConditions(
            sc => sc.Count.ShouldBe(1),
            sc => sc.ShouldContain(x => x.Course.Title == PhysicsCourseName)
        );
    }
}
