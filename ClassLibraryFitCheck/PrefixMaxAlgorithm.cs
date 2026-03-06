using System;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Алгоритм поиска максимального значения в массиве с поддержкой WP-верификации
    /// Реализует поиск максимума в префиксе массива с проверкой инварианта цикла
    /// </summary>
    public class PrefixMaxAlgorithm : ILoopAlgorithm
    {
        /// <summary>
        /// Название алгоритма для отображения в интерфейсе
        /// </summary>
        public string Name => "Поиск максимального веса";

        /// <summary>
        /// Описание алгоритма и его предназначения
        /// </summary>
        public string Description => "Поиск максимального веса в последовательности измерений";

        /// <summary>
        /// Словесное описание инварианта цикла
        /// </summary>
        public string InvariantWords => "res = максимальный вес в измерениях [0..j-1] и 0 ≤ j ≤ n";

        /// <summary>
        /// Формальная запись инварианта цикла
        /// </summary>
        public string InvariantFormula => "res = max(weights[0..j-1]) ∧ 0 ≤ j ≤ n";

        /// <summary>
        /// Функция варианта цикла для гарантии завершаемости
        /// </summary>
        public string VariantFunction => "t = n - j";

        /// <summary>
        /// Псевдокод цикла для демонстрации алгоритма
        /// </summary>
        public string LoopCode => "for (j = 0; j < n; j++) res = max(res, weights[j]);";

        /// <summary>
        /// Инициализирует состояние цикла для работы с заданным массивом
        /// </summary>
        /// <param name="array">Массив значений для поиска максимума</param>
        /// <param name="threshold">Пороговое значение (не используется в этом алгоритме)</param>
        /// <returns>Начальное состояние цикла с установленными начальными значениями</returns>
        public LoopState Initialize(int[] array, int threshold = 0)
        {
            int initialMax = array.Length > 0 ? array[0] : 0;

            return new LoopState
            {
                Array = array,
                CurrentIndex = 0, // НАЧИНАЕМ С ПЕРВОГО ЭЛЕМЕНТА (исправлено)
                Result = initialMax,
                IsFinished = array.Length == 0,
                Status = array.Length > 0 ? $"Инициализация: res = {initialMax}, начинаем с j = 0" : "Массив пуст",
                InvariantBeforeStep = true,
                VariantValue = array.Length
            };
        }

        /// <summary>
        /// Выполняет один шаг алгоритма, обрабатывая текущий элемент массива
        /// </summary>
        /// <param name="current">Текущее состояние цикла перед выполнением шага</param>
        /// <returns>Обновленное состояние цикла после обработки элемента</returns>
        public LoopState ExecuteStep(LoopState current)
        {
            current.InvariantBeforeStep = CheckInvariant(current);

            if (current.IsFinished || current.CurrentIndex >= current.Array.Length)
            {
                current.IsFinished = true;
                current.Status = $"Завершено: максимальный вес = {current.Result}";
                current.InvariantAfterStep = true;
                return current;
            }

            // Обрабатываем текущий элемент
            int currentValue = current.Array[current.CurrentIndex];
            if (currentValue > current.Result)
            {
                current.Result = currentValue;
                current.Status = $"Шаг {current.CurrentIndex + 1}: вес {currentValue} > текущего максимума, новый res = {current.Result}";
            }
            else
            {
                current.Status = $"Шаг {current.CurrentIndex + 1}: вес {currentValue} ≤ текущего максимума {current.Result}";
            }

            // Увеличиваем индекс ПОСЛЕ обработки текущего элемента
            current.CurrentIndex++;

            // Проверяем завершение
            current.IsFinished = current.CurrentIndex >= current.Array.Length;

            current.InvariantAfterStep = CheckInvariant(current);
            current.VariantValue = CalculateVariant(current);

            return current;
        }

        /// <summary>
        /// Проверяет выполнение инварианта цикла для заданного состояния
        /// </summary>
        /// <param name="state">Состояние цикла для проверки инварианта</param>
        /// <returns>True если инвариант выполняется, иначе False</returns>
        public bool CheckInvariant(LoopState state)
        {
            if (state.Array.Length == 0) return true;

            // Вычисляем ожидаемый максимум для обработанной части массива
            int expectedMax = state.Array[0];
            for (int i = 1; i < state.CurrentIndex; i++)
            {
                if (state.Array[i] > expectedMax)
                    expectedMax = state.Array[i];
            }

            return state.Result == expectedMax &&
                   state.CurrentIndex >= 0 &&
                   state.CurrentIndex <= state.Array.Length;
        }

        /// <summary>
        /// Вычисляет значение варианта цикла для оценки прогресса выполнения
        /// </summary>
        /// <param name="state">Состояние цикла для вычисления варианта</param>
        /// <returns>Текущее значение варианта цикла</returns>
        public int CalculateVariant(LoopState state)
        {
            return state.Array.Length - state.CurrentIndex;
        }

        /// <summary>
        /// Генерирует код алгоритма для проведения WP-анализа корректности
        /// </summary>
        /// <returns>Строка с кодом алгоритма в упрощенном формате</returns>
        public string GenerateWpCode()
        {
            return @"
                if (j < n) {
                    if (a[j] > res) 
                        res := a[j];
                    j := j + 1;
                }
            ";
        }

        /// <summary>
        /// Генерирует постусловие для проверки в WP-анализе
        /// </summary>
        /// <returns>Строка с постусловием алгоритма</returns>
        public string GenerateWpPostCondition()
        {
            return "res = max(a[0..j-1]) ∧ 0 ≤ j ≤ n";
        }
    }
}