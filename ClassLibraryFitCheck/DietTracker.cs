using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Трекер питания для отслеживания суточного потребления калорий и БЖУ
    /// Обеспечивает добавление продуктов, расчет суммарных показателей и анализ достижения целей
    /// </summary>
    public class DietTracker
    {
        private readonly List<FoodEntry> _foodEntries = new List<FoodEntry>();

        /// <summary>
        /// Целевое значение калорийности в сутки
        /// </summary>
        public double TargetCalories { get; }

        /// <summary>
        /// Целевое значение белка в граммах в сутки
        /// </summary>
        public double TargetProtein { get; }

        /// <summary>
        /// Целевое значение жиров в граммах в сутки
        /// </summary>
        public double TargetFat { get; }

        /// <summary>
        /// Целевое значение углеводов в граммах в сутки
        /// </summary>
        public double TargetCarbs { get; }

        /// <summary>
        /// Инициализирует трекер питания с заданными целевыми значениями
        /// </summary>
        /// <param name="targetCalories">Целевая калорийность</param>
        /// <param name="targetProtein">Целевое количество белка</param>
        /// <param name="targetFat">Целевое количество жиров</param>
        /// <param name="targetCarbs">Целевое количество углеводов</param>
        public DietTracker(double targetCalories, double targetProtein, double targetFat, double targetCarbs)
        {
            // PRECONDITION: проверка входных данных
            if (targetCalories <= 0)
                throw new ArgumentException("Целевая калорийность должна быть положительной");
            if (targetProtein < 0)
                throw new ArgumentException("Целевой белок не может быть отрицательным");
            if (targetFat < 0)
                throw new ArgumentException("Целевой жир не может быть отрицательным");
            if (targetCarbs < 0)
                throw new ArgumentException("Целевые углеводы не могут быть отрицательными");

            TargetCalories = targetCalories;
            TargetProtein = targetProtein;
            TargetFat = targetFat;
            TargetCarbs = targetCarbs;
        }

        /// <summary>
        /// Добавляет продукт в историю питания по названию и весу порции
        /// </summary>
        /// <param name="productName">Название продукта из базы данных</param>
        /// <param name="grams">Вес порции в граммах</param>
        public void AddFoodFromProduct(string productName, double grams)
        {
            // PRECONDITION: проверка входных данных
            if (string.IsNullOrEmpty(productName))
                throw new ArgumentException("Название продукта не может быть пустым");
            if (grams <= 0)
                throw new ArgumentException("Вес порции должен быть положительным числом");
            if (grams > 5000)
                throw new ArgumentException("Вес порции должен быть реалистичным");

            var product = FoodDatabase.FindProduct(productName);
            if (product == null)
                throw new ArgumentException($"Продукт '{productName}' не найден в базе данных");

            var foodEntry = product.CreatePortion(grams);
            AddFoodEntry(foodEntry);
        }

        /// <summary>
        /// Добавляет запись о пище в историю питания с проверкой валидности данных
        /// </summary>
        /// <param name="entry">Запись о пище для добавления</param>
        public void AddFoodEntry(FoodEntry entry)
        {
            // PRECONDITION: проверка входных данных
            if (entry == null)
                throw new ArgumentException("Запись о пище не может быть null");
            if (entry.Grams <= 0)
                throw new ArgumentException("Вес порции должен быть положительным числом");
            if (entry.CaloriesPer100g < 0 || entry.ProteinPer100g < 0 || entry.FatPer100g < 0 || entry.CarbsPer100g < 0)
                throw new ArgumentException("Пищевая ценность не может быть отрицательной");

            _foodEntries.Add(entry);

            // POSTCONDITION: проверка инварианта
            Debug.Assert(_foodEntries.Contains(entry), "Запись должна быть добавлена в список");
            Debug.Assert(GetTotalCalories() >= 0, "Суммарные калории не могут быть отрицательными");
        }

        /// <summary>
        /// Определяет статус питания на основе соотношения текущих и целевых показателей
        /// </summary>
        /// <returns>Статус питания: от критического недобора до критического перебора</returns>
        public NutritionStatus GetNutritionStatus()
        {
            var totals = GetTotals();
            double calorieRatio = TargetCalories > 0 ? totals.Calories / TargetCalories : 0;

            // Определение статуса по проценту от целевой калорийности
            if (calorieRatio < 0.7) return NutritionStatus.CriticalLow;    // <70%
            if (calorieRatio < 0.9) return NutritionStatus.Low;            // 70-90%
            if (calorieRatio > 1.3) return NutritionStatus.CriticalHigh;   // >130%
            if (calorieRatio > 1.1) return NutritionStatus.High;           // 110-130%

            return NutritionStatus.Optimal;                                // 90-110%
        }

        /// <summary>
        /// Вычисляет суммарные показатели питания за день
        /// </summary>
        /// <returns>Кортеж с общими калориями, белками, жирами и углеводами</returns>
        public (double Calories, double Protein, double Fat, double Carbs) GetTotals()
        {
            double calories = _foodEntries.Sum(f => f.TotalCalories);
            double protein = _foodEntries.Sum(f => f.TotalProtein);
            double fat = _foodEntries.Sum(f => f.TotalFat);
            double carbs = _foodEntries.Sum(f => f.TotalCarbs);

            // POSTCONDITION: проверка итогов
            Debug.Assert(calories >= 0, "Итоговые калории не могут быть отрицательными");
            Debug.Assert(protein >= 0 && fat >= 0 && carbs >= 0, "Итоговые БЖУ не могут быть отрицательными");

            return (Math.Round(calories, 1), Math.Round(protein, 1), Math.Round(fat, 1), Math.Round(carbs, 1));
        }

        /// <summary>
        /// Возвращает общее количество калорий за день
        /// </summary>
        /// <returns>Суммарные калории из всех записей питания</returns>
        public double GetTotalCalories() => GetTotals().Calories;

        /// <summary>
        /// Формирует историю питания в читаемом формате
        /// </summary>
        /// <returns>Список строк с описанием каждого приема пищи</returns>
        public List<string> GetFoodHistory()
        {
            return _foodEntries.Select(entry =>
                $"{entry.FoodName}: {entry.Grams}г (Ккал: {entry.TotalCalories:F0} Б: {entry.TotalProtein:F1} Ж: {entry.TotalFat:F1} У: {entry.TotalCarbs:F1})"
            ).ToList();
        }

        /// <summary>
        /// Очищает историю питания
        /// </summary>
        public void Clear()
        {
            _foodEntries.Clear();
        }

        /// <summary>
        /// Возвращает количество записей в истории питания
        /// </summary>
        /// <returns>Количество добавленных продуктов</returns>
        public int GetEntryCount() => _foodEntries.Count;

        // ==================== WP-МЕТОДЫ ДЛЯ АНАЛИЗА ====================

        /// <summary>
        /// Выполняет WP-анализ достижимости целей питания
        /// Проверяет соответствие текущего рациона целевым показателям
        /// </summary>
        /// <returns>Результат WP-анализа с информацией о достижимости целей</returns>
        public WpEngine.AnalysisResult AnalyzeNutritionGoalAchievability()
        {
            string code = BuildNutritionGoalCode();
            string postCondition = GetNutritionGoalPostCondition();

            return WpEngine.AnalyzeCode(code, postCondition, null, true);
        }

        /// <summary>
        /// Выполняет WP-анализ добавления отдельного продукта в рацион
        /// Проверяет не приведет ли добавление продукта к превышению целевых показателей
        /// </summary>
        /// <param name="product">Продукт для добавления</param>
        /// <param name="grams">Вес порции в граммах</param>
        /// <returns>Результат WP-анализа возможности добавления продукта</returns>
        public WpEngine.AnalysisResult AnalyzeSingleFoodAddition(FoodProduct product, double grams)
        {
            string code = GetSingleFoodAdditionCode(product, grams);
            string postCondition = GetSingleFoodAdditionPostCondition();

            return WpEngine.AnalyzeCode(code, postCondition, null, true);
        }

        /// <summary>
        /// Генерирует код для WP-анализа достижения целей питания
        /// </summary>
        /// <returns>Код алгоритма расчета питания</returns>
        private string BuildNutritionGoalCode()
        {
            StringBuilder code = new StringBuilder();

            // Инициализация переменных
            code.AppendLine($"target_calories := {TargetCalories};");
            code.AppendLine($"target_protein := {TargetProtein};");

            code.AppendLine("total_calories := 0;");
            code.AppendLine("total_protein := 0;");

            // Добавляем все продукты из истории
            foreach (var entry in _foodEntries.Take(3))
            {
                string safeName = entry.FoodName.Replace(" ", "_").Replace("'", "");
                code.AppendLine($"calories_{safeName} := {entry.Grams} * {entry.CaloriesPer100g} / 100;");
                code.AppendLine($"protein_{safeName} := {entry.Grams} * {entry.ProteinPer100g} / 100;");

                code.AppendLine($"total_calories := total_calories + calories_{safeName};");
                code.AppendLine($"total_protein := total_protein + protein_{safeName};");
            }

            return code.ToString();
        }

        /// <summary>
        /// Генерирует постусловие для WP-анализа целей питания
        /// </summary>
        /// <returns>Постусловие для верификации</returns>
        private string GetNutritionGoalPostCondition()
        {
            return "total_calories <= target_calories * 1.1 AND " +
                   "total_calories >= target_calories * 0.9 AND " +
                   "total_protein >= target_protein * 0.8";
        }

        /// <summary>
        /// Генерирует код для WP-анализа добавления отдельного продукта
        /// </summary>
        /// <param name="product">Продукт для добавления</param>
        /// <param name="grams">Вес порции</param>
        /// <returns>Код алгоритма добавления продукта</returns>
        private string GetSingleFoodAdditionCode(FoodProduct product, double grams)
        {
            return $@"
                food_calories := {grams} * {product.CaloriesPer100g} / 100;
                food_protein := {grams} * {product.ProteinPer100g} / 100;
                new_total_calories := current_total_calories + food_calories;
                new_total_protein := current_total_protein + food_protein;
            ";
        }

        /// <summary>
        /// Генерирует постусловие для WP-анализа добавления продукта
        /// </summary>
        /// <returns>Постусловие для верификации</returns>
        private string GetSingleFoodAdditionPostCondition()
        {
            return $"new_total_calories <= {TargetCalories} * 1.1 AND " +
                   $"new_total_protein >= {TargetProtein} * 0.8 AND " +
                   $"food_calories > 0";
        }
    }
}