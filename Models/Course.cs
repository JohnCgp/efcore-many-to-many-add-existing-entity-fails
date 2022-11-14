namespace ManyToManyAddExistingEntity.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }

        public ICollection<Student> Students { get; set; }
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}
