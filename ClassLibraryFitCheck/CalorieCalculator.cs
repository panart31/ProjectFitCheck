using System;
using System.Diagnostics;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Калькулятор суточных норм калорий и БЖУ на основе профиля пользователя
    /// Реализует расчет по формуле Харриса-Бенедикта с поддержкой WP-верификации
    /// </summary>
    public class CalorieCalculator
    {
        private readonly UserProfile _user;

        /// <summary>
        /// Инициализирует калькулятор с профилем пользователя
        /// </summary>
        /// <param name="user">Профиль пользователя с данными для расчета</param>
        public CalorieCalculator(UserProfile user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        /// <summary>
        /// Вычисляет суточную норму калорий и распределение БЖУ для пользователя
        /// Использует формулу Харриса-Бенедикта с распределением: 35% белки, 15% жиры, 50% углеводы
        /// </summary>
        /// <returns>Кортеж с калориями, белками, жирами и углеводами</returns>
        public (double Calories, double Protein, double Fat, double Carbs) Calculate()
        {
            // PRECONDITION: расширенная проверка входных данных
            if (_user.Age < 14 || _user.Age > 100)
                throw new ArgumentException("Возраст должен быть от 14 до 100 лет");
            if (_user.Height < 130 || _user.Height > 200)
                throw new ArgumentException("Рост должен быть от 130 до 200 см");
            if (_user.Weight < 45 || _user.Weight > 110)
                throw new ArgumentException("Вес должен быть от 45 до 110 кг");

            // Расчет базового метаболизма (BMR) по формуле Харриса-Бенедикта
            double bmr;
            if (_user.Gender == Gender.Male)
                bmr = 88.36 + (13.4 * _user.Weight) + (4.8 * _user.Height) - (5.7 * _user.Age);
            else
                bmr = 447.6 + (9.2 * _user.Weight) + (3.1 * _user.Height) - (4.3 * _user.Age);

            // Распределение БЖУ (35% белки, 15% жиры, 50% углеводы)
            double protein = (bmr * 0.35) / 4;
            double fat = (bmr * 0.15) / 9;
            double carbs = (bmr * 0.50) / 4;

            // POSTCONDITION: проверка результатов
            if (bmr <= 0)
                throw new InvalidOperationException("Ошибка расчета: суточная калорийность должна быть положительной");
            if (protein <= 0 || fat <= 0 || carbs <= 0)
                throw new InvalidOperationException("Ошибка расчета: БЖУ должны быть положительными");

            return (Math.Round(bmr, 2), Math.Round(protein, 2), Math.Round(fat, 2), Math.Round(carbs, 2));
        }

        // ==================== WP-МЕТОДЫ ДЛЯ АНАЛИЗА ====================

        /// <summary>
        /// Выполняет WP-анализ достижимости целей расчета калорий
        /// Проверяет корректность алгоритма расчета и выполнимость постусловий
        /// </summary>
        /// <param name="isSecondTab">Флаг указывающий на анализ для второй вкладки (всегда достижимо)</param>
        /// <returns>Результат WP-анализа с информацией о достижимости целей</returns>
        public WpEngine.AnalysisResult AnalyzeGoalAchievability(bool isSecondTab = false)
        {
            string code = GetWpAnalysisCode();
            string postCondition = GetWpPostCondition();

            return WpEngine.AnalyzeCode(code, postCondition, _user, isSecondTab);
        }

        /// <summary>
        /// Генерирует код алгоритма расчета для WP-анализа
        /// Представляет расчет калорий в упрощенном псевдокоде
        /// </summary>
        /// <returns>Строка с кодом алгоритма расчета калорий</returns>
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

        /// <summary>
        /// Генерирует постусловие для WP-анализа расчета калорий
        /// Проверяет что все рассчитанные значения положительны и реалистичны
        /// </summary>
        /// <returns>Строка с постусловием для верификации</returns>
        private string GetWpPostCondition()
        {
            return "bmr > 0 AND protein > 0 AND fat > 0 AND carbs > 0 AND bmr < 10000";
        }
    }
}