using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Представляет запись о пищевом продукте с его пищевой ценностью
    /// </summary>
    public class FoodEntry
    {
        /// <summary>
        /// Название пищевого продукта
        /// </summary>
        public string FoodName { get; set; } = "";

        /// <summary>
        /// Вес порции в граммах
        /// </summary>
        public double Grams { get; set; }

        /// <summary>
        /// Калорийность на 100 грамм продукта
        /// </summary>
        public double CaloriesPer100g { get; set; }

        /// <summary>
        /// Содержание белка на 100 грамм продукта
        /// </summary>
        public double ProteinPer100g { get; set; }

        /// <summary>
        /// Содержание жиров на 100 грамм продукта
        /// </summary>
        public double FatPer100g { get; set; }

        /// <summary>
        /// Содержание углеводов на 100 грамм продукта
        /// </summary>
        public double CarbsPer100g { get; set; }

        /// <summary>
        /// Общая калорийность порции
        /// </summary>
        public double TotalCalories => (Grams / 100) * CaloriesPer100g;

        /// <summary>
        /// Общее содержание белка в порции
        /// </summary>
        public double TotalProtein => (Grams / 100) * ProteinPer100g;

        /// <summary>
        /// Общее содержание жиров в порции
        /// </summary>
        public double TotalFat => (Grams / 100) * FatPer100g;

        /// <summary>
        /// Общее содержание углеводов в порции
        /// </summary>
        public double TotalCarbs => (Grams / 100) * CarbsPer100g;
    }

    /// <summary>
    /// Категории пищевых продуктов для классификации
    /// </summary>
    public enum FoodCategory
    {
        Bakery,     // Хлебобулочные
        Dairy,      // Молочные
        Meat,       // Мясо
        Fish,       // Рыба
        Vegetables, // Овощи
        Fruits,     // Фрукты
        Grains,     // Крупы
        Other       // Прочее
    }

    /// <summary>
    /// Статус питания относительно целевых показателей
    /// </summary>
    public enum NutritionStatus
    {
        CriticalLow,    // <70% от нормы - серьезный недобор
        Low,            // 70-90% - небольшой недобор
        Optimal,        // 90-110% - норма
        High,           // 110-130% - небольшой перебор
        CriticalHigh    // >130% - серьезный перебор
    }

    /// <summary>
    /// Представляет пищевой продукт с его характеристиками
    /// </summary>
    public class FoodProduct
    {
        /// <summary>
        /// Название продукта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Категория продукта
        /// </summary>
        public FoodCategory Category { get; set; }

        /// <summary>
        /// Калорийность на 100 грамм
        /// </summary>
        public double CaloriesPer100g { get; set; }

        /// <summary>
        /// Содержание белка на 100 грамм
        /// </summary>
        public double ProteinPer100g { get; set; }

        /// <summary>
        /// Содержание жиров на 100 грамм
        /// </summary>
        public double FatPer100g { get; set; }

        /// <summary>
        /// Содержание углеводов на 100 грамм
        /// </summary>
        public double CarbsPer100g { get; set; }

        /// <summary>
        /// Создает новый пищевой продукт
        /// </summary>
        /// <param name="name">Название продукта</param>
        /// <param name="category">Категория продукта</param>
        /// <param name="calories">Калорийность на 100г</param>
        /// <param name="protein">Белок на 100г</param>
        /// <param name="fat">Жиры на 100г</param>
        /// <param name="carbs">Углеводы на 100г</param>
        public FoodProduct(string name, FoodCategory category, double calories, double protein, double fat, double carbs)
        {
            Name = name;
            Category = category;
            CaloriesPer100g = calories;
            ProteinPer100g = protein;
            FatPer100g = fat;
            CarbsPer100g = carbs;
        }

        /// <summary>
        /// Создает порцию продукта заданного веса
        /// </summary>
        /// <param name="grams">Вес порции в граммах</param>
        /// <returns>Запись о пище с рассчитанной пищевой ценностью</returns>
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
        /// <summary>
        /// Возвращает список продуктов по умолчанию с реалистичными пищевыми ценностями
        /// </summary>
        /// <returns>Список стандартных пищевых продуктов</returns>
        public static List<FoodProduct> GetDefaultProducts()
        {
            return new List<FoodProduct>
            {
                new FoodProduct("Зерновой хлеб 'Волжский пекарь'", FoodCategory.Bakery, 250, 8.5, 3.2, 42.0),
                new FoodProduct("Бородинский хлеб", FoodCategory.Bakery, 210, 6.8, 1.3, 41.0),
                new FoodProduct("Творог 5%", FoodCategory.Dairy, 121, 17.2, 5.0, 1.8),
                new FoodProduct("Молоко 2.5%", FoodCategory.Dairy, 52, 2.8, 2.5, 4.7),
                new FoodProduct("Греческий йогурт", FoodCategory.Dairy, 59, 10.0, 0.4, 3.6),
                new FoodProduct("Куриная грудка", FoodCategory.Meat, 165, 31.0, 3.6, 0.0),
                new FoodProduct("Говядина тушеная", FoodCategory.Meat, 250, 26.0, 16.0, 0.0),
                new FoodProduct("Огурцы свежие", FoodCategory.Vegetables, 15, 0.8, 0.1, 2.8),
                new FoodProduct("Помидоры", FoodCategory.Vegetables, 18, 0.9, 0.2, 3.9),
                new FoodProduct("Гречневая крупа", FoodCategory.Grains, 343, 12.6, 3.3, 62.1),
                new FoodProduct("Овсяные хлопья", FoodCategory.Grains, 366, 11.9, 7.2, 69.3)
            };
        }

        /// <summary>
        /// Находит продукт по названию (без учета регистра)
        /// </summary>
        /// <param name="name">Название продукта для поиска</param>
        /// <returns>Найденный продукт или null если продукт не найден</returns>
        public static FoodProduct? FindProduct(string name)
        {
            return GetDefaultProducts().FirstOrDefault(p => p.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}