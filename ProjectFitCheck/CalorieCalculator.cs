using System;
using System.Diagnostics;
using System.Reflection;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Калькулятор суточных норм калорий и БЖУ
    /// </summary>
    public class CalorieCalculator
    {
        private readonly UserProfile _user;

        public CalorieCalculator(UserProfile user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public (double Calories, double Protein, double Fat, double Carbs) Calculate()
        {
            if (_user.Age < 14 || _user.Age > 100)
                throw new ArgumentException("Возраст должен быть от 14 до 100 лет");
            if (_user.Height < 130 || _user.Height > 200)
                throw new ArgumentException("Рост должен быть от 130 до 200 см");
            if (_user.Weight < 45 || _user.Weight > 110)
                throw new ArgumentException("Вес должен быть от 45 до 110 кг");

            double bmr;
            if (_user.Gender == Gender.Male)
                bmr = 88.36 + (13.4 * _user.Weight) + (4.8 * _user.Height) - (5.7 * _user.Age);
            else
                bmr = 447.6 + (9.2 * _user.Weight) + (3.1 * _user.Height) - (4.3 * _user.Age);

            double protein = (bmr * 0.35) / 4;
            double fat = (bmr * 0.15) / 9;
            double carbs = (bmr * 0.50) / 4;

            return (Math.Round(bmr, 2), Math.Round(protein, 2), Math.Round(fat, 2), Math.Round(carbs, 2));
        }

        public WpEngine.AnalysisResult AnalyzeGoalAchievability(bool isSecondTab = false)
        {
            string code = GetWpAnalysisCode();
            string postCondition = GetWpPostCondition();
            return WpEngine.AnalyzeCode(code, postCondition, _user, isSecondTab);
        }

        private string GetWpAnalysisCode()
        {
            return @"
                if (gender == 0)
                    bmr := 88.36 + (13.4 * weight) + (4.8 * height) - (5.7 * age)
                else
                    bmr := 447.6 + (9.2 * weight) + (3.1 * height) - (4.3 * age);
                protein := (bmr * 0.35) / 4;
                fat := (bmr * 0.15) / 9;
                carbs := (bmr * 0.50) / 4;
            ";
        }

        private string GetWpPostCondition()
        {
            return "bmr > 0 AND protein > 0 AND fat > 0 AND carbs > 0 AND bmr < 10000";
        }
    }
}