namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Представляет состояние циклического алгоритма для визуализации и анализа
    /// </summary>
    public class LoopState
    {
        public int[] Array { get; set; } = new int[0];
        public int CurrentIndex { get; set; }
        public int Result { get; set; }
        public int Threshold { get; set; }
        public bool IsFinished { get; set; }
        public string Status { get; set; } = "Не инициализирован";
        public int[] HighlightedIndices => new[] { CurrentIndex };
        public bool InvariantBeforeStep { get; set; }
        public bool InvariantAfterStep { get; set; }
        public int VariantValue { get; set; }
    }
}