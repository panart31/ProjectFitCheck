using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Движок для анализа циклических алгоритмов
    /// </summary>
    public static class LoopAnalysisEngine
    {
        private static readonly Dictionary<string, ILoopAlgorithm> _algorithms = new()
        {
            { "PrefixMax", new PrefixMaxAlgorithm() }
        };

        public static ILoopAlgorithm GetAlgorithm(string name)
        {
            return _algorithms.TryGetValue(name, out var algorithm) ? algorithm : _algorithms["PrefixMax"];
        }

        public static List<string> GetAlgorithmNames()
        {
            return _algorithms.Keys.ToList();
        }

        public static WpEngine.AnalysisResult AnalyzeLoopInvariant(ILoopAlgorithm algorithm)
        {
            try
            {
                string code = algorithm.GenerateWpCode();
                string postCondition = algorithm.GenerateWpPostCondition();
                return WpEngine.AnalyzeCode(code, postCondition, null, false);
            }
            catch (Exception ex)
            {
                return new WpEngine.AnalysisResult
                {
                    IsAchievable = false,
                    Reason = $"Ошибка анализа: {ex.Message}",
                    HumanReadablePrecondition = "Не удалось выполнить анализ"
                };
            }
        }
    }
}