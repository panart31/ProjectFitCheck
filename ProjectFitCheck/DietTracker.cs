using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Трекер питания для отслеживания суточного потребления калорий
    /// </summary>
    public class DietTracker
    {
        private readonly List<FoodEntry> _foodEntries = new List<FoodEntry>();
        public double TargetCalories { get; }
        public double TargetProtein { get; }
        public double TargetFat { get; }
        public double TargetCarbs { get; }

        public DietTracker(double targetCalories, double targetProtein, double targetFat, double targetCarbs)
        {
            if (targetCalories <= 0)
                throw new ArgumentException("Целевая калорийность должна быть положительной");

            TargetCalories = targetCalories;
            TargetProtein = targetProtein;
            TargetFat = targetFat;
            TargetCarbs = targetCarbs;
        }

        public void AddFoodFromProduct(string productName, double grams)
        {
            if (string.IsNullOrEmpty(productName))
                throw new ArgumentException("Название продукта не может быть пустым");
            if (grams <= 0 || grams > 5000)
                throw new ArgumentException("Вес порции должен быть от 1 до 5000 грамм");

            var product = FoodDatabase.FindProduct(productName);
            if (product == null)
                throw new ArgumentException($"Продукт '{productName}' не найден");

            var foodEntry = product.CreatePortion(grams);
            AddFoodEntry(foodEntry);
        }

        public void AddFoodEntry(FoodEntry entry)
        {
            if (entry == null)
                throw new ArgumentException("Запись о пище не может быть null");
            if (entry.Grams <= 0)
                throw new ArgumentException("Вес порции должен быть положительным");

            _foodEntries.Add(entry);
        }

        public NutritionStatus GetNutritionStatus()
        {
            var totals = GetTotals();
            double calorieRatio = TargetCalories > 0 ? totals.Calories / TargetCalories : 0;

            if (calorieRatio < 0.7) return NutritionStatus.CriticalLow;
            if (calorieRatio < 0.9) return NutritionStatus.Low;
            if (calorieRatio > 1.3) return NutritionStatus.CriticalHigh;
            if (calorieRatio > 1.1) return NutritionStatus.High;
            return NutritionStatus.Optimal;
        }

        public (double Calories, double Protein, double Fat, double Carbs) GetTotals()
        {
            double calories = _foodEntries.Sum(f => f.TotalCalories);
            double protein = _foodEntries.Sum(f => f.TotalProtein);
            double fat = _foodEntries.Sum(f => f.TotalFat);
            double carbs = _foodEntries.Sum(f => f.TotalCarbs);
            return (Math.Round(calories, 1), Math.Round(protein, 1), Math.Round(fat, 1), Math.Round(carbs, 1));
        }

        public List<string> GetFoodHistory()
        {
            return _foodEntries.Select(entry =>
                $"{entry.FoodName}: {entry.Grams}г (Ккал: {entry.TotalCalories:F0})"
            ).ToList();
        }

        public void Clear() => _foodEntries.Clear();
        public int GetEntryCount() => _foodEntries.Count;
    }
}