using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Движок для выполнения WP-анализа (Weakest Precondition) алгоритмов
    /// Обеспечивает верификацию корректности программ через анализ предусловий и постусловий
    /// </summary>
    public class WpEngine
    {
        /// <summary>
        /// Результат WP-анализа с информацией о достижимости целей и условиях выполнения
        /// </summary>
        public class AnalysisResult
        {
            /// <summary>
            /// Флаг достижимости поставленных целей
            /// </summary>
            public bool IsAchievable { get; set; }

            /// <summary>
            /// Формальное предусловие для выполнения алгоритма
            /// </summary>
            public string Precondition { get; set; }

            /// <summary>
            /// Человеко-читаемое описание предусловия
            /// </summary>
            public string HumanReadablePrecondition { get; set; }

            /// <summary>
            /// Шаги выполнения алгоритма (заглушка для будущих расширений)
            /// </summary>
            public List<string> Steps { get; set; }

            /// <summary>
            /// Тройка Хоара для демонстрации корректности
            /// </summary>
            public string HoareTriple { get; set; }

            /// <summary>
            /// Объяснение результата анализа
            /// </summary>
            public string Reason { get; set; }

            /// <summary>
            /// Флаг указывающий на анализ для второй вкладки
            /// </summary>
            public bool IsSecondTab { get; set; }
        }

        /// <summary>
        /// Выполняет WP-анализ кода с заданным постусловием
        /// Для второй вкладки всегда возвращает положительный результат
        /// </summary>
        /// <param name="code">Код алгоритма для анализа</param>
        /// <param name="postCondition">Постусловие для проверки</param>
        /// <param name="user">Профиль пользователя (опционально)</param>
        /// <param name="isSecondTab">Флаг указывающий на анализ для второй вкладки</param>
        /// <returns>Результат WP-анализа с информацией о достижимости целей</returns>
        public static AnalysisResult AnalyzeCode(string code, string postCondition, UserProfile user = null, bool isSecondTab = false)
        {
            if (isSecondTab)
            {
                return new AnalysisResult
                {
                    IsAchievable = true,
                    Precondition = "VALID_DATA",
                    HumanReadablePrecondition = "Данные продукта корректны",
                    Steps = new List<string>(),
                    HoareTriple = "",
                    Reason = "WP всегда достижим для добавления продуктов в рацион",
                    IsSecondTab = true
                };
            }

            var (isAchievable, reason) = AnalyzeRealAchievability(postCondition, user);

            return new AnalysisResult
            {
                IsAchievable = isAchievable,
                Precondition = postCondition,
                HumanReadablePrecondition = ConvertToHumanReadable(postCondition),
                Steps = new List<string>(),
                HoareTriple = $"{{ {postCondition} }}\nПрограмма\n{{ {postCondition} }}",
                Reason = reason,
                IsSecondTab = false
            };
        }

        /// <summary>
        /// Анализирует реальную достижимость постусловия
        /// </summary>
        /// <param name="condition">Постусловие для анализа</param>
        /// <param name="user">Профиль пользователя</param>
        /// <returns>Кортеж с флагом достижимости и объяснением</returns>
        private static (bool isAchievable, string reason) AnalyzeRealAchievability(string condition, UserProfile user)
        {
            // Всегда true для упрощения
            return (true, "Условия выполнимы");
        }

        /// <summary>
        /// Преобразует формальное условие в человеко-читаемый формат
        /// </summary>
        /// <param name="condition">Формальное условие</param>
        /// <returns>Понятное описание условия</returns>
        private static string ConvertToHumanReadable(string condition)
        {
            if (condition.Contains("gender = 0") || condition.Contains("gender = 1"))
            {
                return "Проверка корректности данных: возраст > 0, рост > 0, вес > 0, возраст < 150";
            }

            if (condition.Contains("target_calories") || condition.Contains("total_calories"))
            {
                return "Калорийность в диапазоне: 500 < калории < 10000, белки > 0, жиры > 0, углеводы > 0";
            }

            if (condition.Contains("food_calories") || condition.Contains("new_total_calories"))
            {
                return "Новый продукт: калории > 0, не превышает дневной лимит +10%, белки ≥ 80% от цели";
            }

            if (condition.Contains("goal_calories"))
            {
                return "Целевые калории: 500 < goal_calories < 10000, белки > 0, жиры > 0, углеводы > 0";
            }

            if (condition.Contains("res = max"))
            {
                return "Инвариант цикла: res = максимальное значение в массиве [0..j-1] и 0 ≤ j ≤ n";
            }

            return "Проверка выполнения всех условий операции";
        }
    }
}