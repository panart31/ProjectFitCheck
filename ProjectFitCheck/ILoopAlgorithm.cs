using ProjectFitCheck;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Интерфейс для алгоритмов циклического анализа с поддержкой WP-верификации
    /// </summary>
    public interface ILoopAlgorithm
    {
        string Name { get; }
        string Description { get; }
        string InvariantWords { get; }
        string InvariantFormula { get; }
        string VariantFunction { get; }
        string LoopCode { get; }

        LoopState Initialize(int[] array, int threshold = 0);
        LoopState ExecuteStep(LoopState currentState);
        bool CheckInvariant(LoopState state);
        int CalculateVariant(LoopState state);
        string GenerateWpCode();
        string GenerateWpPostCondition();
    }
}