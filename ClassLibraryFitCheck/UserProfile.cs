using System;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Профиль пользователя с антропометрическими данными и фитнес-целями
    /// Содержит информацию для расчета норм калорий и планирования тренировок
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Пол пользователя
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Возраст пользователя в годах
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Рост пользователя в сантиметрах
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Текущий вес пользователя в килограммах
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Целевой вес пользователя в килограммах
        /// </summary>
        public double TargetWeight { get; set; }

        /// <summary>
        /// Срок достижения цели в месяцах
        /// </summary>
        public int TimelineMonths { get; set; } = 1;

        /// <summary>
        /// Уровень физической активности пользователя
        /// </summary>
        public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Moderate;

        /// <summary>
        /// Создает профиль пользователя с базовыми антропометрическими данными
        /// </summary>
        /// <param name="gender">Пол пользователя</param>
        /// <param name="age">Возраст пользователя в годах</param>
        /// <param name="height">Рост пользователя в сантиметрах</param>
        /// <param name="weight">Текущий вес пользователя в килограммах</param>
        public UserProfile(Gender gender, int age, double height, double weight)
        {
            Gender = gender;
            Age = age;
            Height = height;
            Weight = weight;
            TargetWeight = weight;
        }
    }

    /// <summary>
    /// Пол пользователя для расчета физиологических показателей
    /// </summary>
    public enum Gender { Male, Female }

    /// <summary>
    /// Уровень физической активности для расчета суточных энергозатрат
    /// </summary>
    public enum ActivityLevel
    {
        Sedentary,
        Light,
        Moderate,
        Active,
        VeryActive
    }
}