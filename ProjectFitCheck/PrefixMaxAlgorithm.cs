using System;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Алгоритм поиска максимального значения в массиве
    /// </summary>
    public class PrefixMaxAlgorithm : ILoopAlgorithm
    {
        public string Name => "Поиск максимального веса";
        public string Description => "Поиск максимального веса в последовательности измерений";
        public string InvariantWords => "res = максимальный вес в измерениях [0..j-1] и 0 ≤ j ≤ n";
        public string InvariantFormula => "res = max(weights[0..j-1]) ∧ 0 ≤ j ≤ n";
        public string VariantFunction => "t = n - j";
        public string LoopCode => "for (j = 0; j < n; j++) res = max(res, weights[j]);";

        public LoopState Initialize(int[] array, int threshold = 0)
        {
            int initialMax = array.Length > 0 ? array[0] : 0;

            return new LoopState
            {
                Array = array,
                CurrentIndex = 0,
                Result = initialMax,
                IsFinished = array.Length == 0,
                Status = array.Length > 0 ?
                    $"Инициализация: res = {initialMax}, начинаем с j = 0" : "Массив пуст",
                InvariantBeforeStep = true,
                VariantValue = array.Length
            };
        }

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

            int currentValue = current.Array[current.CurrentIndex];
            if (currentValue > current.Result)
            {
                current.Result = currentValue;
                current.Status = $"Шаг {current.CurrentIndex + 1}: новый максимум = {current.Result}";
            }
            else
            {
                current.Status = $"Шаг {current.CurrentIndex + 1}: значение {currentValue} ≤ {current.Result}";
            }

            current.CurrentIndex++;
            current.IsFinished = current.CurrentIndex >= current.Array.Length;
            current.InvariantAfterStep = CheckInvariant(current);
            current.VariantValue = CalculateVariant(current);

            return current;
        }

        public bool CheckInvariant(LoopState state)
        {
            if (state.Array.Length == 0) return true;

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

        public int CalculateVariant(LoopState state)
        {
            return state.Array.Length - state.CurrentIndex;
        }

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

        public string GenerateWpPostCondition()
        {
            return "res = max(a[0..j-1]) ∧ 0 ≤ j ≤ n";
        }
    }
}