using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Представляет запись о пищевом продукте с его пищевой ценностью
    /// </summary>
    public class FoodEntry
    {
        public string FoodName { get; set; } = "";
        public double Grams { get; set; }
        public double CaloriesPer100g { get; set; }
        public double ProteinPer100g { get; set; }
        public double FatPer100g { get; set; }
        public double CarbsPer100g { get; set; }
        public double TotalCalories => (Grams / 100) * CaloriesPer100g;
        public double TotalProtein => (Grams / 100) * ProteinPer100g;
        public double TotalFat => (Grams / 100) * FatPer100g;
        public double TotalCarbs => (Grams / 100) * CarbsPer100g;
    }

    /// <summary>
    /// Категории пищевых продуктов для классификации
    /// </summary>
    public enum FoodCategory
    {
        Bakery, Dairy, Meat, Fish, Vegetables, Fruits, Grains, Other
    }

    /// <summary>
    /// Статус питания относительно целевых показателей
    /// </summary>
    public enum NutritionStatus
    {
        CriticalLow, Low, Optimal, High, CriticalHigh
    }

    /// <summary>
    /// Представляет пищевой продукт с его характеристиками
    /// </summary>
    public class FoodProduct
    {
        public string Name { get; set; }
        public FoodCategory Category { get; set; }
        public double CaloriesPer100g { get; set; }
        public double ProteinPer100g { get; set; }
        public double FatPer100g { get; set; }
        public double CarbsPer100g { get; set; }

        public FoodProduct(string name, FoodCategory category, double calories,
                          double protein, double fat, double carbs)
        {
            Name = name;
            Category = category;
            CaloriesPer100g = calories;
            ProteinPer100g = protein;
            FatPer100g = fat;
            CarbsPer100g = carbs;
        }

        public FoodEntry CreatePortion(double grams)
        {
            return new FoodEntry
            {
                FoodName = this.Name,
                Grams = grams,
                CaloriesPer100g = this.CaloriesPer100g,
                ProteinPer100g = this.ProteinPer100g,
                FatPer100g = this.FatPer100g,
                CarbsPer100g = this.CarbsPer100g
            };
        }
    }

    /// <summary>
    /// База данных пищевых продуктов с предустановленными значениями
    /// </summary>
    public static class FoodDatabase
    {
        public static List<FoodProduct> GetDefaultProducts()
        {
            return new List<FoodProduct>
            {
                new FoodProduct("Зерновой хлеб", FoodCategory.Bakery, 250, 8.5, 3.2, 42.0),
                new FoodProduct("Бородинский хлеб", FoodCategory.Bakery, 210, 6.8, 1.3, 41.0),
                new FoodProduct("Творог 5%", FoodCategory.Dairy, 121, 17.2, 5.0, 1.8),
                new FoodProduct("Молоко 2.5%", FoodCategory.Dairy, 52, 2.8, 2.5, 4.7),
                new FoodProduct("Греческий йогурт", FoodCategory.Dairy, 59, 10.0, 0.4, 3.6),
                new FoodProduct("Куриная грудка", FoodCategory.Meat, 165, 31.0, 3.6, 0.0),
                new FoodProduct("Говядина", FoodCategory.Meat, 250, 26.0, 16.0, 0.0),
                new FoodProduct("Огурцы", FoodCategory.Vegetables, 15, 0.8, 0.1, 2.8),
                new FoodProduct("Помидоры", FoodCategory.Vegetables, 18, 0.9, 0.2, 3.9),
                new FoodProduct("Гречка", FoodCategory.Grains, 343, 12.6, 3.3, 62.1),
                new FoodProduct("Овсянка", FoodCategory.Grains, 366, 11.9, 7.2, 69.3)
            };
        }

        public static FoodProduct? FindProduct(string name)
        {
            return GetDefaultProducts().FirstOrDefault(p =>
                p.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}