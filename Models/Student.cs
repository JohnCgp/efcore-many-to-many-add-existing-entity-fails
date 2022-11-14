namespace ManyToManyAddExistingEntity.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstMidName { get; set; }
        public DateTime EnrollmentDate { get; set; }

        public ICollection<Course> Courses { get; set; }
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}
