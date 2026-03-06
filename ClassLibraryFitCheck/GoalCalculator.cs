using System;
using System.Diagnostics;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Калькулятор целевых показателей калорий и БЖУ для достижения фитнес-целей
    /// Использует формулу Миффлина-Сан Жеора с учетом уровня активности и целевого веса
    /// </summary>
    public class GoalCalculator
    {
        private readonly UserProfile _user;

        /// <summary>
        /// Инициализирует калькулятор целей с профилем пользователя
        /// </summary>
        /// <param name="user">Профиль пользователя с данными для расчета целей</param>
        public GoalCalculator(UserProfile user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        /// <summary>
        /// Вычисляет целевые показатели калорий и БЖУ для достижения заданного веса
        /// Учитывает уровень активности, срок достижения цели и направление изменения веса
        /// </summary>
        /// <returns>Кортеж с целевыми калориями, белками, жирами и углеводами</returns>
        public (double Calories, double Protein, double Fat, double Carbs) CalculateGoalCalories()
        {
            // PRECONDITION: проверка всех параметров
            if (_user.Height <= 0)
                throw new ArgumentException("Рост должен быть положительным числом");
            if (_user.Weight <= 0)
                throw new ArgumentException("Вес должен быть положительным числом");
            if (_user.Age <= 0)
                throw new ArgumentException("Возраст должен быть положительным числом");
            if (_user.TargetWeight <= 0)
                throw new ArgumentException("Целевой вес должен быть положительным числом");
            if (_user.TimelineMonths <= 0)
                throw new ArgumentException("Срок должен быть положительным числом");
            if (_user.TimelineMonths > 60)
                throw new ArgumentException("Срок должен быть реалистичным (до 5 лет)");
            if (Math.Abs(_user.TargetWeight - _user.Weight) < 0.1)
                throw new ArgumentException("Целевой вес должен отличаться от текущего");

            // 1. Расчет базового метаболизма по Миффлину-Сан Жеора
            double bmr = _user.Gender == Gender.Male
                ? 10 * _user.Weight + 6.25 * _user.Height - 5 * _user.Age + 5
                : 10 * _user.Weight + 6.25 * _user.Height - 5 * _user.Age - 161;

            // 2. Учет уровня активности
            double activityMultiplier = _user.ActivityLevel switch
            {
                ActivityLevel.Sedentary => 1.2,
                ActivityLevel.Light => 1.375,
                ActivityLevel.Moderate => 1.55,
                ActivityLevel.Active => 1.725,
                ActivityLevel.VeryActive => 1.9,
                _ => 1.55
            };
            double maintenanceCalories = bmr * activityMultiplier;

            // 3. Расчет дефицита/профицита калорий для цели
            double weightChangeKg = _user.TargetWeight - _user.Weight;
            double totalCalorieChange = weightChangeKg * 7700;
            double dailyCalorieChange = totalCalorieChange / (_user.TimelineMonths * 30);

            double goalCalories = maintenanceCalories + dailyCalorieChange;

            // 4. Адаптивное распределение БЖУ в зависимости от цели
            double proteinRatio = weightChangeKg < 0 ? 0.35 : 0.25;
            double fatRatio = 0.25;
            double carbsRatio = 0.40;

            double protein = (goalCalories * proteinRatio) / 4;
            double fat = (goalCalories * fatRatio) / 9;
            double carbs = (goalCalories * carbsRatio) / 4;

            // POSTCONDITION: проверка результатов
            if (bmr <= 0)
                throw new InvalidOperationException("Ошибка расчета: суточная калорийность должна быть положительной");
            if (protein <= 0 || fat <= 0 || carbs <= 0)
                throw new InvalidOperationException("Ошибка расчета: БЖУ должны быть положительными");
            if (bmr >= 10000)
                throw new InvalidOperationException("Ошибка расчета: калорийность должна быть реалистичной");

            return (Math.Round(goalCalories, 2), Math.Round(protein, 2), Math.Round(fat, 2), Math.Round(carbs, 2));
        }

        // ==================== WP-МЕТОДЫ ДЛЯ АНАЛИЗА ====================

        /// <summary>
        /// Выполняет WP-анализ реалистичности поставленных фитнес-целей
        /// Проверяет корректность алгоритма расчета и достижимость целевых показателей
        /// </summary>
        /// <param name="isSecondTab">Флаг указывающий на анализ для второй вкладки</param>
        /// <returns>Результат WP-анализа с информацией о реалистичности целей</returns>
        public WpEngine.AnalysisResult AnalyzeGoalRealism(bool isSecondTab = false)
        {
            string code = GetGoalRealismCode();
            string postCondition = GetGoalRealismPostCondition();

            return WpEngine.AnalyzeCode(code, postCondition, _user, isSecondTab);
        }

        /// <summary>
        /// Возвращает множитель активности для расчета калорий
        /// </summary>
        /// <returns>Коэффициент умножения базового метаболизма</returns>
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

        /// <summary>
        /// Генерирует код алгоритма расчета целей для WP-анализа
        /// </summary>
        /// <returns>Строка с кодом алгоритма расчета целевых показателей</returns>
        private string GetGoalRealismCode()
        {
            return $@"
        if (gender == 0)
            bmr := 10 * {_user.Weight} + 6.25 * {_user.Height} - 5 * {_user.Age} + 5
        else
            bmr := 10 * {_user.Weight} + 6.25 * {_user.Height} - 5 * {_user.Age} - 161;
        
        activity_multiplier := {GetActivityMultiplier()};
        maintenance_calories := bmr * activity_multiplier;
        
        weight_change := {_user.TargetWeight} - {_user.Weight};
        total_calorie_change := weight_change * 7700;
        daily_calorie_change := total_calorie_change / ({_user.TimelineMonths} * 30);
        
        goal_calories := maintenance_calories + daily_calorie_change;
        
        if (weight_change < 0)
            protein_ratio := 0.35
        else
            protein_ratio := 0.25;
            
        protein := (goal_calories * protein_ratio) / 4;
        fat := (goal_calories * 0.25) / 9;
        carbs := (goal_calories * 0.40) / 4;
    ";
        }

        /// <summary>
        /// Генерирует постусловие для WP-анализа целей
        /// Проверяет что целевые показатели реалистичны и положительны
        /// </summary>
        /// <returns>Строка с постусловием для верификации</returns>
        private string GetGoalRealismPostCondition()
        {
            return "goal_calories > 500 AND goal_calories < 10000 AND " +
                   "protein > 0 AND fat > 0 AND carbs > 0";
        }
    }
}