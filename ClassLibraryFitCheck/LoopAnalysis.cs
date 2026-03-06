using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryFitCheck
{
    /// <summary>
    /// Движок для анализа циклических алгоритмов с поддержкой WP-верификации
    /// Предоставляет доступ к зарегистрированным алгоритмам и выполняет анализ их корректности
    /// </summary>
    public static class LoopAnalysisEngine
    {
        private static readonly Dictionary<string, ILoopAlgorithm> _algorithms = new()
        {
            { "PrefixMax", new PrefixMaxAlgorithm() }
        };

        /// <summary>
        /// Получает алгоритм по имени или возвращает алгоритм по умолчанию
        /// </summary>
        /// <param name="name">Название алгоритма для поиска</param>
        /// <returns>Найденный алгоритм или алгоритм PrefixMax по умолчанию</returns>
        public static ILoopAlgorithm GetAlgorithm(string name)
        {
            return _algorithms.TryGetValue(name, out var algorithm) ? algorithm : _algorithms["PrefixMax"];
        }

        /// <summary>
        /// Возвращает список названий всех зарегистрированных алгоритмов
        /// </summary>
        /// <returns>Список доступных названий алгоритмов</returns>
        public static List<string> GetAlgorithmNames()
        {
            return _algorithms.Keys.ToList();
        }

        /// <summary>
        /// Выполняет WP-анализ инварианта цикла для заданного алгоритма
        /// Проверяет корректность инварианта и варианта цикла
        /// </summary>
        /// <param name="algorithm">Алгоритм для анализа</param>
        /// <returns>Результат WP-анализа с информацией о корректности инварианта</returns>
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
                    HumanReadablePrecondition = "Не удалось выполнить анализ",
                    HoareTriple = "Ошибка",
                    Steps = new List<string>()
                };
            }
        }
    }
}