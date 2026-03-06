using System;
using System.Reflection;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Калькулятор целевых показателей для достижения фитнес-целей
    /// </summary>
    public class GoalCalculator
    {
        private readonly UserProfile _user;

        public GoalCalculator(UserProfile user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public (double Calories, double Protein, double Fat, double Carbs) CalculateGoalCalories()
        {
            ValidateInputs();

            double bmr = _user.Gender == Gender.Male
                ? 10 * _user.Weight + 6.25 * _user.Height - 5 * _user.Age + 5
                : 10 * _user.Weight + 6.25 * _user.Height - 5 * _user.Age - 161;

            double activityMultiplier = GetActivityMultiplier();
            double maintenanceCalories = bmr * activityMultiplier;

            double weightChangeKg = _user.TargetWeight - _user.Weight;
            double totalCalorieChange = weightChangeKg * 7700;
            double dailyCalorieChange = totalCalorieChange / (_user.TimelineMonths * 30);
            double goalCalories = maintenanceCalories + dailyCalorieChange;

            double proteinRatio = weightChangeKg < 0 ? 0.35 : 0.25;
            double protein = (goalCalories * proteinRatio) / 4;
            double fat = (goalCalories * 0.25) / 9;
            double carbs = (goalCalories * 0.40) / 4;

            return (Math.Round(goalCalories, 2), Math.Round(protein, 2),
                    Math.Round(fat, 2), Math.Round(carbs, 2));
        }

        private void ValidateInputs()
        {
            if (_user.Height <= 0) throw new ArgumentException("Рост должен быть положительным");
            if (_user.Weight <= 0) throw new ArgumentException("Вес должен быть положительным");
            if (_user.Age <= 0) throw new ArgumentException("Возраст должен быть положительным");
            if (_user.TargetWeight <= 0) throw new ArgumentException("Целевой вес должен быть положительным");
            if (_user.TimelineMonths <= 0 || _user.TimelineMonths > 60)
                throw new ArgumentException("Срок должен быть от 1 до 60 месяцев");
        }

        private double GetActivityMultiplier()
        {
            return _user.ActivityLevel switch
            {
                ActivityLevel.Sedentary => 1.2,
                ActivityLevel.Light => 1.375,
                ActivityLevel.Moderate => 1.55,
                ActivityLevel.Active => 1.725,
                ActivityLevel.VeryActive => 1.9,
                _ => 1.55
            };
        }
    }
}