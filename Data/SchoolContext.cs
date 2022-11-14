using ManyToManyAddExistingEntity.Models;
using Microsoft.EntityFrameworkCore;

namespace ManyToManyAddExistingEntity.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Student>(s =>
            {
                s.HasMany(s => s.Courses)
                    .WithMany(c => c.Students)
                    .UsingEntity<StudentCourse>(
                        j => j
                                .HasOne(sc => sc.Course)
                                .WithMany(c => c.StudentCourses)
                                .HasForeignKey(sc => sc.CourseId),
                        j => j
                                .HasOne(sc => sc.Student)
                                .WithMany(s => s.StudentCourses)
                                .HasForeignKey(sc => sc.StudentId),
                        j =>
                        {
                            j.Property(sc => sc.HasAttended).HasDefaultValue(false);
                            j.HasKey(t => new { t.StudentId, t.CourseId });
                        }
                    );
            });
            modelBuilder.Entity<Student>().ToTable("Student");
        }
    }
}
