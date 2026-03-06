namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Интерфейс для алгоритмов циклического анализа с поддержкой WP-верификации
    /// Определяет контракт для алгоритмов поиска максимального значения в массиве
    /// </summary>
    public interface ILoopAlgorithm
    {
        /// <summary>
        /// Название алгоритма для отображения в пользовательском интерфейсе
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Описание алгоритма и его предназначения
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Словесное описание инварианта цикла для пользователя
        /// </summary>
        string InvariantWords { get; }

        /// <summary>
        /// Формальная запись инварианта цикла в математической нотации
        /// </summary>
        string InvariantFormula { get; }

        /// <summary>
        /// Функция варианта цикла, гарантирующая его завершаемость
        /// </summary>
        string VariantFunction { get; }

        /// <summary>
        /// Псевдокод цикла для демонстрации алгоритма
        /// </summary>
        string LoopCode { get; }

        /// <summary>
        /// Инициализирует состояние цикла для работы с заданным массивом
        /// </summary>
        /// <param name="array">Массив значений для анализа</param>
        /// <param name="threshold">Пороговое значение для алгоритма (опционально)</param>
        /// <returns>Начальное состояние цикла с установленными начальными значениями</returns>
        LoopState Initialize(int[] array, int threshold = 0);

        /// <summary>
        /// Выполняет один шаг алгоритма, обрабатывая текущий элемент массива
        /// </summary>
        /// <param name="currentState">Текущее состояние цикла перед выполнением шага</param>
        /// <returns>Обновленное состояние цикла после обработки элемента</returns>
        LoopState ExecuteStep(LoopState currentState);

        /// <summary>
        /// Проверяет выполнение инварианта цикла для заданного состояния
        /// </summary>
        /// <param name="state">Состояние цикла для проверки инварианта</param>
        /// <returns>True если инвариант выполняется, иначе False</returns>
        bool CheckInvariant(LoopState state);

        /// <summary>
        /// Вычисляет значение варианта цикла для оценки прогресса выполнения
        /// </summary>
        /// <param name="state">Состояние цикла для вычисления варианта</param>
        /// <returns>Текущее значение варианта цикла</returns>
        int CalculateVariant(LoopState state);

        // Для WP-анализа

        /// <summary>
        /// Генерирует код алгоритма для проведения WP-анализа корректности
        /// </summary>
        /// <returns>Строка с кодом алгоритма в упрощенном формате</returns>
        string GenerateWpCode();

        /// <summary>
        /// Генерирует постусловие для проверки в WP-анализе
        /// </summary>
        /// <returns>Строка с постусловием алгоритма</returns>
        string GenerateWpPostCondition();
    }
}