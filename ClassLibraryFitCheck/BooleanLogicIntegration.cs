using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Упрощенный класс для булевых функций в контексте фитнеса
    /// Предоставляет методы анализа фитнес-условий с использованием булевой логики
    /// </summary>
    public static class FitnessBooleanLogic
    {
        // Фитнес-переменные для n=3
        private static readonly string[] FitnessVariables = { "Диета", "Тренировки", "Сон" };

        /// <summary>
        /// Анализирует комбинации фитнес-условий на основе номера булевой функции
        /// Строит таблицу истинности для трех переменных: диета, тренировки, сон
        /// </summary>
        /// <param name="functionNumber">Номер булевой функции от 0 до 255</param>
        /// <returns>Форматированная строка с анализом условий и таблицей истинности</returns>

        public static string AnalyzeFitnessConditions(int functionNumber)
        {
            try
            {
                // Фиксируем n=3 для простоты
                int n = 3;

                string result = $"Анализ условий фитнес-программы (номер: {functionNumber}):\n\n";
                result += $"Переменные: {string.Join(", ", FitnessVariables)}\n";
                result += $"Двоичный код: {Convert.ToString(functionNumber, 2).PadLeft(8, '0')}\n\n";

                // Упрощенная таблица истинности
                result += "Таблица условий:\n";
                result += $"{FitnessVariables[0]} | {FitnessVariables[1]} | {FitnessVariables[2]} | Результат\n";
                result += "--------------------------------\n";

                // Простая логика для демонстрации
                for (int i = 0; i < 8; i++)
                {
                    string diet = ((i >> 2) & 1) == 1 ? "Да" : "Нет";
                    string training = ((i >> 1) & 1) == 1 ? "Да" : "Нет";
                    string sleep = ((i >> 0) & 1) == 1 ? "Да" : "Нет";

                    // Простая логика: успех если есть хотя бы 2 из 3 условий
                    int conditionsMet = ((i >> 2) & 1) + ((i >> 1) & 1) + ((i >> 0) & 1);
                    string success = conditionsMet >= 2 ? "Успех" : "Неудача";

                    result += $"{diet,-6} | {training,-10} | {sleep,-4} | {success}\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        /// <summary>
        /// Анализирует фитнес-формулу, преобразуя логические операторы в читаемый вид
        /// Поддерживает замену переменных x1, x2, x3 на фитнес-термины
        /// </summary>
        /// <param name="formula">Логическая формула с переменными x1, x2, x3</param>
        /// <returns>Форматированная строка с анализом формулы и рекомендациями</returns>

        public static string AnalyzeFitnessFormula(string formula)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formula))
                    throw new ArgumentNullException(nameof(formula), "Формула не может быть пустой");

                // Заменяем переменные на фитнес-термины
                string fitnessFormula = formula
                    .Replace("x1", "Диета")
                    .Replace("x2", "Тренировки")
                    .Replace("x3", "Сон")
                    .Replace("&", "И")
                    .Replace("|", "ИЛИ")
                    .Replace("!", "НЕ")
                    .Replace("and", "И")
                    .Replace("or", "ИЛИ")
                    .Replace("not", "НЕ");

                // Упрощенный анализ
                return $"Анализ фитнес-формулы:\n\n" +
                       $"Формула: {fitnessFormula}\n\n" +
                       $"Примеры условий:\n" +
                       $"• Диета И Тренировки → Высокий результат\n" +
                       $"• Тренировки ИЛИ Сон → Средний результат\n" +
                       $"• НЕ Диета → Низкий результат\n\n" +
                       $"Совет: Для лучших результатов сочетайте Диету И Тренировки И Сон";
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        /// <summary>
        /// Сравнивает два фитнес-плана на основе ключевых слов и выдает рекомендации
        /// Оценивает полноту планов по наличию основных компонентов фитнес-программы
        /// </summary>
        /// <param name="plan1">Первый фитнес-план для сравнения</param>
        /// <param name="plan2">Второй фитнес-план для сравнения</param>
        /// <returns>Форматированная строка с результатом сравнения и рекомендацией</returns>
        public static string CompareFitnessPlans(string plan1, string plan2)
        {
            try
            {
                // Упрощенное сравнение на основе ключевых слов
                int score1 = CalculatePlanScore(plan1);
                int score2 = CalculatePlanScore(plan2);

                string result = "Сравнение фитнес-планов:\n\n";
                result += $"План 1: {plan1} (оценка: {score1}/10)\n";
                result += $"План 2: {plan2} (оценка: {score2}/10)\n\n";

                if (score1 == score2)
                {
                    result += "Планы РАВНОЦЕННЫ по эффективности";
                }
                else if (score1 > score2)
                {
                    result += "План 1 ЭФФЕКТИВНЕЕ";
                }
                else
                {
                    result += "План 2 ЭФФЕКТИВНЕЕ";
                }

                result += $"\n\nРекомендация: {GetRecommendation(score1, score2)}";

                return result;
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        /// <summary>
        /// Вычисляет оценку фитнес-плана на основе наличия ключевых компонентов
        /// </summary>
        /// <param name="plan">Текст фитнес-плана для оценки</param>
        /// <returns>Оценка от 0 до 10, где 10 - максимальная полнота плана</returns>
        private static int CalculatePlanScore(string plan)
        {
            // Простая оценка плана по ключевым словам
            plan = plan.ToLower();
            int score = 0;

            if (plan.Contains("диет") && plan.Contains("трениров")) score += 8;
            else if (plan.Contains("диет") || plan.Contains("трениров")) score += 5;

            if (plan.Contains("сон")) score += 2;
            if (plan.Contains("и")) score += 1;
            if (plan.Contains("или")) score += 0;

            return Math.Min(10, score);
        }

        /// <summary>
        /// Генерирует рекомендацию на основе оценки фитнес-плана
        /// </summary>
        /// <param name="score1">Оценка первого плана</param>
        /// <param name="score2">Оценка второго плана</param>
        /// <returns>Текст рекомендации по улучшению фитнес-плана</returns>
        private static string GetRecommendation(int score1, int score2)
        {
            int maxScore = Math.Max(score1, score2);

            return maxScore switch
            {
                >= 8 => "Отличный план! Придерживайтесь его для максимальных результатов",
                >= 5 => "Хороший план, но можно улучшить, добавив больше элементов",
                _ => "План нуждается в доработке. Рекомендуется консультация специалиста"
            };
        }

        /// <summary>
        /// Предоставляет демонстрационные примеры анализа фитнес-условий
        /// </summary>
        /// <param name="exampleNumber">Номер примера: 1 - анализ условий, 2 - анализ формулы, 3 - сравнение планов</param>
        /// <returns>Готовый пример анализа для демонстрации</returns>
        public static string GetDemoExample(int exampleNumber)
        {
            return exampleNumber switch
            {
                1 => AnalyzeFitnessConditions(5),  // номер 5
                2 => AnalyzeFitnessFormula("Диета И Тренировки"),
                3 => CompareFitnessPlans("Диета И Тренировки", "Тренировки ИЛИ Сон"),
                _ => "Неизвестный пример"
            };
        }
    }
}