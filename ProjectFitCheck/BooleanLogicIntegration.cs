using System;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Упрощенный класс для булевых функций в контексте фитнеса
    /// </summary>
    public static class FitnessBooleanLogic
    {
        private static readonly string[] FitnessVariables = { "Диета", "Тренировки", "Сон" };

        public static string AnalyzeFitnessConditions(int functionNumber)
        {
            string result = $"Анализ условий фитнес-программы (номер: {functionNumber}):\n\n";
            result += $"Переменные: {string.Join(", ", FitnessVariables)}\n";
            result += $"Двоичный код: {Convert.ToString(functionNumber, 2).PadLeft(8, '0')}\n\n";

            result += "Таблица условий:\n";
            result += $"{FitnessVariables[0]} | {FitnessVariables[1]} | {FitnessVariables[2]} | Результат\n";
            result += "--------------------------------\n";

            for (int i = 0; i < 8; i++)
            {
                string diet = ((i >> 2) & 1) == 1 ? "Да" : "Нет";
                string training = ((i >> 1) & 1) == 1 ? "Да" : "Нет";
                string sleep = ((i >> 0) & 1) == 1 ? "Да" : "Нет";

                int conditionsMet = ((i >> 2) & 1) + ((i >> 1) & 1) + ((i >> 0) & 1);
                string success = conditionsMet >= 2 ? "Успех" : "Неудача";

                result += $"{diet,-6} | {training,-10} | {sleep,-4} | {success}\n";
            }

            return result;
        }

        public static string AnalyzeFitnessFormula(string formula)
        {
            string fitnessFormula = formula
                .Replace("x1", "Диета")
                .Replace("x2", "Тренировки")
                .Replace("x3", "Сон")
                .Replace("&", " И ")
                .Replace("|", " ИЛИ ")
                .Replace("!", "НЕ ");

            return $"Анализ фитнес-формулы:\n\n" +
                   $"Формула: {fitnessFormula}\n\n" +
                   $"Совет: Для лучших результатов сочетайте Диету И Тренировки И Сон";
        }
    }
}