namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Профиль пользователя с антропометрическими данными и фитнес-целями
    /// </summary>
    public class UserProfile
    {
        public Gender Gender { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public double TargetWeight { get; set; }
        public int TimelineMonths { get; set; } = 1;
        public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Moderate;

        public UserProfile(Gender gender, int age, double height, double weight)
        {
            Gender = gender;
            Age = age;
            Height = height;
            Weight = weight;
            TargetWeight = weight;
        }
    }

    public enum Gender { Male, Female }

    public enum ActivityLevel
    {
        Sedentary, Light, Moderate, Active, VeryActive
    }
}