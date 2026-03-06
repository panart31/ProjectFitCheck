using System.Linq;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Представляет состояние циклического алгоритма для визуализации и анализа
    /// Содержит текущие данные, прогресс выполнения и результаты проверки инвариантов
    /// </summary>
    public class LoopState
    {
        /// <summary>
        /// Массив данных для обработки алгоритмом
        /// </summary>
        public int[] Array { get; set; } = new int[0];

        /// <summary>
        /// Текущий индекс обрабатываемого элемента в массиве
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// Текущий результат работы алгоритма
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// Пороговое значение для алгоритма (опционально)
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// Флаг завершения работы алгоритма
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// Текстовое описание текущего состояния алгоритма
        /// </summary>
        public string Status { get; set; } = "Не инициализирован";

        // Для визуализации

        /// <summary>
        /// Индексы элементов, которые должны быть выделены при визуализации
        /// </summary>
        public int[] HighlightedIndices => new[] { CurrentIndex };

        // Для проверки инварианта

        /// <summary>
        /// Флаг выполнения инварианта перед выполнением шага алгоритма
        /// </summary>
        public bool InvariantBeforeStep { get; set; }

        /// <summary>
        /// Флаг выполнения инварианта после выполнения шага алгоритма
        /// </summary>
        public bool InvariantAfterStep { get; set; }

        /// <summary>
        /// Текущее значение варианта цикла для оценки прогресса выполнения
        /// </summary>
        public int VariantValue { get; set; }
    }
}