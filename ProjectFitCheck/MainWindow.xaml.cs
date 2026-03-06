using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ClassLibraryFitCheck;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace NutritionApp
{
    /// <summary>
    /// Анимация для плавного изменения ширины GridLength с поддержкой easing-функций
    /// Используется для анимированного сворачивания/разворачивания навигационной панели
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        /// <summary>
        /// DependencyProperty для начального значения анимации
        /// </summary>
        public static readonly DependencyProperty FromProperty;

        /// <summary>
        /// DependencyProperty для конечного значения анимации
        /// </summary>
        public static readonly DependencyProperty ToProperty;

        /// <summary>
        /// DependencyProperty для easing-функции анимации
        /// </summary>
        public static readonly DependencyProperty EasingFunctionProperty;

        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));
            ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));
            EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(GridLengthAnimation));
        }

        /// <summary>
        /// Тип целевого свойства для анимации
        /// </summary>
        public override Type TargetPropertyType => typeof(GridLength);

        /// <summary>
        /// Начальное значение анимации
        /// </summary>
        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        /// <summary>
        /// Конечное значение анимации
        /// </summary>
        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        /// <summary>
        /// Easing-функция для плавности анимации
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        /// <summary>
        /// Вычисляет текущее значение анимации на основе прогресса
        /// </summary>
        /// <param name="defaultOriginValue">Значение по умолчанию в начале анимации</param>
        /// <param name="defaultDestinationValue">Значение по умолчанию в конце анимации</param>
        /// <param name="animationClock">Часы анимации для отслеживания прогресса</param>
        /// <returns>Текущее значение GridLength анимации</returns>
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
                return From;

            double progress = animationClock.CurrentProgress.Value;

            if (EasingFunction != null)
            {
                progress = EasingFunction.Ease(progress);
            }

            double fromVal = From.Value;
            double toVal = To.Value;

            if (fromVal > toVal)
            {
                return new GridLength((1 - progress) * (fromVal - toVal) + toVal, GridUnitType.Pixel);
            }
            else
            {
                return new GridLength(progress * (toVal - fromVal) + fromVal, GridUnitType.Pixel);
            }
        }

        /// <summary>
        /// Создает новый экземпляр анимации
        /// </summary>
        /// <returns>Новый экземпляр GridLengthAnimation</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }
    }

    /// <summary>
    /// Главное окно приложения FitCheck - умный фитнес-трекер с WP-анализом
    /// Предоставляет функционал для расчета норм питания, отслеживания прогресса и анализа целей
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Трекер питания для управления дневником калорий и БЖУ
        /// </summary>
        private DietTracker _dietTracker;

        /// <summary>
        /// Алгоритм для анализа циклического прогресса в достижении целей
        /// </summary>
        private ILoopAlgorithm _tab3Algorithm;

        /// <summary>
        /// Текущее состояние алгоритма анализа прогресса
        /// </summary>
        private LoopState _tab3State;

        /// <summary>
        /// Флаг состояния навигационной панели (свернута/развернута)
        /// </summary>
        private bool _isNavCollapsed = false;

        /// <summary>
        /// Флаг открытия панели справки в навигации
        /// </summary>
        private bool _isHelpOpen = false;

        /// <summary>
        /// Список доступных продуктов для выбора в дневнике питания
        /// </summary>
        private List<string> _availableProducts;

        /// <summary>
        /// Список доступных уровней активности для расчета целей
        /// </summary>
        private List<string> _availableActivities;

        /// <summary>
        /// Минимальный допустимый возраст пользователя
        /// </summary>
        private const int MIN_AGE = 14;

        /// <summary>
        /// Максимальный допустимый возраст пользователя
        /// </summary>
        private const int MAX_AGE = 100;

        /// <summary>
        /// Минимальный допустимый вес пользователя в кг
        /// </summary>
        private const double MIN_WEIGHT = 45;

        /// <summary>
        /// Максимальный допустимый вес пользователя в кг
        /// </summary>
        private const double MAX_WEIGHT = 110;

        /// <summary>
        /// Минимальный допустимый рост пользователя в см
        /// </summary>
        private const double MIN_HEIGHT = 130;

        /// <summary>
        /// Максимальный допустимый рост пользователя в см
        /// </summary>
        private const double MAX_HEIGHT = 200;

        /// <summary>
        /// Минимальный допустимый срок достижения цели в месяцах
        /// </summary>
        private const int MIN_MONTHS = 1;

        /// <summary>
        /// Максимальный допустимый срок достижения цели в месяцах
        /// </summary>
        private const int MAX_MONTHS = 24;

        /// <summary>
        /// Максимальный допустимый вес порции продукта в граммах
        /// </summary>
        private const double MAX_GRAMS = 1000;

        /// <summary>
        /// Список данных о прогрессе по неделям для визуализации
        /// </summary>
        private List<WeekProgress> _weeklyProgress;

        /// <summary>
        /// Текущая неделя в процессе достижения цели
        /// </summary>
        private int _currentWeek = 0;

        /// <summary>
        /// Начальный вес пользователя перед началом программы
        /// </summary>
        private double _startWeight;

        /// <summary>
        /// Целевой вес пользователя
        /// </summary>
        private double _targetWeight;

        /// <summary>
        /// Общее количество недель для достижения цели
        /// </summary>
        private int _totalWeeks;

        /// <summary>
        /// Генератор случайных чисел для создания реалистичных данных прогресса
        /// </summary>
        private Random _random = new Random();

        /// <summary>
        /// Временная запись о пищевом продукте для дневника питания
        /// </summary>
        private class FoodEntry
        {
            /// <summary>
            /// Название пищевого продукта
            /// </summary>
            public string ProductName { get; set; }

            /// <summary>
            /// Вес порции в граммах
            /// </summary>
            public double Grams { get; set; }

            /// <summary>
            /// Калорийность порции
            /// </summary>
            public double Calories { get; set; }

            /// <summary>
            /// Содержание белка в порции
            /// </summary>
            public double Protein { get; set; }

            /// <summary>
            /// Содержание жиров в порции
            /// </summary>
            public double Fat { get; set; }

            /// <summary>
            /// Содержание углеводов в порции
            /// </summary>
            public double Carbs { get; set; }
        }

        /// <summary>
        /// Данные о прогрессе за неделю для визуализации в третьей вкладке
        /// </summary>
        private class WeekProgress
        {
            /// <summary>
            /// Порядковый номер недели в отображении (1-8)
            /// </summary>
            public int WeekNumber { get; set; }

            /// <summary>
            /// Прогнозируемый вес на конец недели
            /// </summary>
            public double Weight { get; set; }

            /// <summary>
            /// Изменение веса относительно начального значения
            /// </summary>
            public double WeightChange { get; set; }

            /// <summary>
            /// Сообщение о прогрессе за неделю
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Флаг завершения недели
            /// </summary>
            public bool IsCompleted { get; set; }

            /// <summary>
            /// Совет по питанию или тренировкам на неделю
            /// </summary>
            public string Tip { get; set; }

            /// <summary>
            /// Начальная реальная неделя в общем плане
            /// </summary>
            public int StartRealWeek { get; set; }

            /// <summary>
            /// Конечная реальная неделя в общем плане
            /// </summary>
            public int EndRealWeek { get; set; }

            /// <summary>
            /// Количество реальных недель в этом сегменте отображения
            /// </summary>
            public int WeeksInSegment { get; set; }

            /// <summary>
            /// Реальный номер недели для отображения прогресса
            /// </summary>
            public int RealWeekNumber { get; set; }
        }

        /// <summary>
        /// История добавленных продуктов в дневнике питания
        /// </summary>
        private List<FoodEntry> _foodHistory = new List<FoodEntry>();

        /// <summary>
        /// Инициализирует главное окно приложения
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        /// <summary>
        /// Выполняет начальную настройку приложения: заполняет списки данных,
        /// настраивает обработчики событий и отображает стартовую вкладку
        /// </summary>
        private void InitializeApplication()
        {
            _availableProducts = new List<string>
            {
                "Яблоко", "Банан", "Куриная грудка", "Рис вареный", "Творог",
                "Яйцо", "Овсянка", "Гречка", "Говядина", "Рыба", "Молоко", "Йогурт"
            };

            _availableActivities = new List<string>
            {
                "Сидячий",
                "Легкий",
                "Умеренный",
                "Активный",
                "Очень активный"
            };

            productListBox.ItemsSource = _availableProducts;
            activityListBox.ItemsSource = _availableActivities;

            rbProfile.Checked += NavigationChecked;
            rbDiet.Checked += NavigationChecked;
            rbGoals.Checked += NavigationChecked;
            InitializeProgressData();
            InitializeTab3Loop();
            InitializeCheckboxes();
            ShowTabWithAnimation(panelProfile);

            UpdateNavHelpContent();

            NavExpandButton.Visibility = Visibility.Collapsed;
            ExpandButtonColumn.Width = new GridLength(0);
        }

        /// <summary>
        /// Инициализирует структуры данных для отслеживания прогресса
        /// </summary>
        private void InitializeProgressData()
        {
            _weeklyProgress = new List<WeekProgress>();
            _currentWeek = 0;
        }

        /// <summary>
        /// Отображает результат WP-анализа для целей в читаемом формате
        /// </summary>
        /// <param name="textBlock">Элемент для отображения результата</param>
        /// <param name="analysis">Результат WP-анализа</param>
        private void DisplayGoalAnalysisResult(TextBlock textBlock, WpEngine.AnalysisResult analysis)
        {
            if (analysis == null)
            {
                textBlock.Text = "WP-АНАЛИЗ: Ошибка анализа\n\nАнализ не может быть выполнен";
                textBlock.Foreground = Brushes.Red;
                return;
            }

            string status = analysis.IsAchievable ? "ДОСТИЖИМА" : "НЕДОСТИЖИМА";
            string color = analysis.IsAchievable ? "Green" : "Red";

            textBlock.Text = $"WP-АНАЛИЗ: Цель {status}\n\n" +
                           $"Причина: {analysis.Reason}\n\n" +
                           $"Условия выполнения:\n{analysis.HumanReadablePrecondition}";

            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// Инициализирует алгоритм анализа прогресса с проверкой валидности данных
        /// </summary>
        private void InitializeTab3Loop()
        {
            try
            {
                if (txtTargetWeight == null || txtTimelineMonths == null)
                    return;

                if (!ValidateGoalInputs(out List<string> errors))
                {
                    return;
                }

                _tab3Algorithm = LoopAnalysisEngine.GetAlgorithm("PrefixMax");
                int[] progressArray = CreateProgressArray();
                _tab3State = _tab3Algorithm.Initialize(progressArray);
                UpdateTab3UI();
                UpdateProgressStats();
            }
            catch (Exception ex)
            {
                txtGoalResult.Text = $"Ошибка инициализации прогресса: {ex.Message}";
                txtGoalResult.Foreground = Brushes.Red;
            }
        }

        /// <summary>
        /// Создает массив прогресса по неделям на основе введенных пользователем целей
        /// </summary>
        /// <returns>Массив с прогнозируемыми весами по неделям</returns>
        private int[] CreateProgressArray()
        {
            if (!double.TryParse(txtWeight.Text, out _startWeight) || _startWeight <= 0)
                _startWeight = 70;

            if (!double.TryParse(txtTargetWeight.Text, out _targetWeight) || _targetWeight <= 0)
                _targetWeight = _startWeight;

            if (!int.TryParse(txtTimelineMonths.Text, out int months) || months <= 0)
                months = 1;

            _totalWeeks = months * 4;

            int displayWeeks = 8;

            var progressArray = new int[displayWeeks];
            _weeklyProgress.Clear();

            double totalChange = _targetWeight - _startWeight;
            double baseWeeklyChange = totalChange / _totalWeeks;

            for (int i = 0; i < displayWeeks; i++)
            {
                int weeksInSegment = _totalWeeks / displayWeeks;
                int remainder = _totalWeeks % displayWeeks;

                int actualWeeksInSegment = weeksInSegment + (i < remainder ? 1 : 0);
                int startRealWeek = i * weeksInSegment + Math.Min(i, remainder) + 1;
                int endRealWeek = startRealWeek + actualWeeksInSegment - 1;

                double segmentMiddleWeek = startRealWeek + (actualWeeksInSegment - 1) / 2.0;
                double weekWeight = _startWeight + (baseWeeklyChange * segmentMiddleWeek);

                double randomFactor = (_random.NextDouble() - 0.5) * 0.2;
                weekWeight += randomFactor;

                weekWeight = Math.Max(40, Math.Min(200, weekWeight));

                progressArray[i] = (int)Math.Round(weekWeight);

                _weeklyProgress.Add(new WeekProgress
                {
                    WeekNumber = i + 1,
                    Weight = weekWeight,
                    WeightChange = weekWeight - _startWeight,
                    IsCompleted = false,
                    StartRealWeek = startRealWeek,
                    EndRealWeek = endRealWeek,
                    WeeksInSegment = actualWeeksInSegment,
                    RealWeekNumber = (int)segmentMiddleWeek
                });
            }

            return progressArray;
        }

        /// <summary>
        /// Обновляет визуальную панель с элементами прогресса по неделям
        /// </summary>
        /// <param name="panel">Панель для отображения элементов</param>
        /// <param name="state">Текущее состояние алгоритма прогресса</param>
        private void UpdateArrayPanel(StackPanel panel, LoopState state)
        {
            if (panel == null || state?.Array == null) return;

            panel.Children.Clear();

            var wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            for (int i = 0; i < state.Array.Length; i++)
            {
                bool isCurrent = state.CurrentIndex == i;
                bool isProcessed = i < state.CurrentIndex;

                var weekProgress = _weeklyProgress[i];

                var mainStack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(4)
                };

                string headerText;
                if (weekProgress.WeeksInSegment > 1)
                {
                    headerText = $"Недели\n{weekProgress.StartRealWeek}-{weekProgress.EndRealWeek}";
                }
                else
                {
                    headerText = $"Неделя\n{weekProgress.StartRealWeek}";
                }

                var weekHeader = new TextBlock
                {
                    Text = headerText,
                    FontSize = 9,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = isCurrent ? Brushes.White : Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var weightText = new TextBlock
                {
                    Text = state.Array[i].ToString(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = isCurrent ? Brushes.White : Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                string changeSign = weekProgress.WeightChange >= 0 ? "+" : "";
                var changeText = new TextBlock
                {
                    Text = $"{changeSign}{weekProgress.WeightChange:F1}",
                    FontSize = 8,
                    Foreground = isCurrent ? Brushes.White : (weekProgress.WeightChange >= 0 ? Brushes.Green : Brushes.Red),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 1, 0, 0)
                };

                mainStack.Children.Add(weekHeader);
                mainStack.Children.Add(weightText);
                mainStack.Children.Add(changeText);

                Brush backgroundColor, borderColor;

                if (isCurrent)
                {
                    backgroundColor = new SolidColorBrush(Color.FromRgb(126, 87, 194));
                    borderColor = new SolidColorBrush(Color.FromRgb(94, 53, 177));
                }
                else if (isProcessed)
                {
                    backgroundColor = new SolidColorBrush(Color.FromRgb(200, 230, 201));
                    borderColor = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                }
                else
                {
                    backgroundColor = Brushes.White;
                    borderColor = new SolidColorBrush(Color.FromRgb(189, 189, 189));
                }

                var border = new Border
                {
                    Background = backgroundColor,
                    BorderBrush = borderColor,
                    BorderThickness = new Thickness(1.5),
                    CornerRadius = new CornerRadius(6),
                    Child = mainStack,
                    Cursor = Cursors.Hand,
                    ToolTip = CreateWeekTooltip(i, weekProgress)
                };

                wrapPanel.Children.Add(border);
            }

            panel.Children.Add(wrapPanel);
        }

        /// <summary>
        /// Создает всплывающую подсказку с детальной информацией о прогрессе за неделю
        /// </summary>
        /// <param name="index">Индекс элемента прогресса</param>
        /// <param name="progress">Данные о прогрессе за неделю</param>
        /// <returns>Настроенный элемент ToolTip</returns>
        private ToolTip CreateWeekTooltip(int index, WeekProgress progress)
        {
            var tooltip = new ToolTip
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(126, 87, 194)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12)
            };

            var stackPanel = new StackPanel();

            string headerText = progress.WeeksInSegment > 1 ?
                $"Период: недели {progress.StartRealWeek}-{progress.EndRealWeek}" :
                $"Неделя {progress.StartRealWeek}";

            var header = new TextBlock
            {
                Text = headerText,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(126, 87, 194)),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var weightInfo = new TextBlock
            {
                Text = $"Прогнозируемый вес: {progress.Weight:F1} кг",
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 4)
            };

            string changeSign = progress.WeightChange >= 0 ? "+" : "";
            var changeInfo = new TextBlock
            {
                Text = $"Изменение от начала: {changeSign}{progress.WeightChange:F1} кг",
                FontSize = 12,
                Foreground = progress.WeightChange >= 0 ? Brushes.Green : Brushes.Red,
                Margin = new Thickness(0, 0, 0, 4)
            };

            double totalChange = _targetWeight - _startWeight;
            double progressPercent = totalChange != 0 ? (progress.WeightChange / totalChange) * 100 : 0;

            var progressInfo = new TextBlock
            {
                Text = $"Прогресс цели: {progressPercent:F1}%",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };

            if (progress.WeeksInSegment > 1)
            {
                var segmentInfo = new TextBlock
                {
                    Text = $"Этот период включает {progress.WeeksInSegment} недель",
                    FontSize = 10,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                stackPanel.Children.Add(segmentInfo);
            }

            stackPanel.Children.Add(header);
            stackPanel.Children.Add(weightInfo);
            stackPanel.Children.Add(changeInfo);
            stackPanel.Children.Add(progressInfo);

            tooltip.Content = stackPanel;
            return tooltip;
        }

        /// <summary>
        /// Обновляет статистику прогресса в третьей вкладке
        /// </summary>
        private void UpdateProgressStats()
        {
            if (_weeklyProgress.Count == 0) return;

            double totalChange = _targetWeight - _startWeight;
            double weeklyChange = totalChange / _totalWeeks;
            string trend = totalChange > 0 ? "набор веса" : "похудение";
            string trendIcon = totalChange > 0 ? "▲" : "▼";

            tab3StatsPanel.Children.Clear();

            AddStatisticRow("Цель", $"{trend} {trendIcon}");
            AddStatisticRow("Начальный вес", $"{_startWeight:F1} кг");
            AddStatisticRow("Целевой вес", $"{_targetWeight:F1} кг");
            AddStatisticRow("Общее изменение", $"{totalChange:F1} кг");
            AddStatisticRow("Темп в неделю", $"{Math.Abs(weeklyChange):F2} кг");
            AddStatisticRow("Продолжительность", $"{_totalWeeks} недель");

            string currentProgressText;
            if (_tab3State != null && _tab3State.IsFinished)
            {
                currentProgressText = "100%";
            }
            else if (_currentWeek > 0 && _currentWeek <= _weeklyProgress.Count)
            {
                int currentRealWeek = _weeklyProgress[_currentWeek - 1].RealWeekNumber;
                double currentProgress = (double)currentRealWeek / _totalWeeks * 100;
                currentProgressText = $"{currentProgress:F1}%";
            }
            else
            {
                currentProgressText = "0%";
            }

            AddStatisticRow("Текущий прогресс", currentProgressText);

            string currentWeightText;
            if (_tab3State != null && _tab3State.IsFinished)
            {
                currentWeightText = $"{_targetWeight:F1} кг";
            }
            else if (_currentWeek > 0 && _currentWeek <= _weeklyProgress.Count)
            {
                currentWeightText = $"{_weeklyProgress[_currentWeek].Weight:F1} кг";
            }
            else
            {
                currentWeightText = $"{_startWeight:F1} кг";
            }

            AddStatisticRow("Текущий вес", currentWeightText);
        }

        /// <summary>
        /// Добавляет строку статистики на панель
        /// </summary>
        /// <param name="label">Метка статистики</param>
        /// <param name="value">Значение статистики</param>
        private void AddStatisticRow(string label, string value)
        {
            var grid = new Grid();
            grid.Margin = new Thickness(0, 4, 0, 4);

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                VerticalAlignment = VerticalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            grid.Children.Add(labelText);
            grid.Children.Add(valueText);

            tab3StatsPanel.Children.Add(grid);
        }

        /// <summary>
        /// Добавляет строку статистики в указанную панель
        /// </summary>
        /// <param name="panel">Целевая панель для добавления</param>
        /// <param name="label">Метка статистики</param>
        /// <param name="value">Значение статистики</param>
        private void AddStatistic(StackPanel panel, string label, string value)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            var labelText = new TextBlock
            {
                Text = label + ":",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                VerticalAlignment = VerticalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(labelText, 0);
            Grid.SetColumn(valueText, 1);

            grid.Children.Add(labelText);
            grid.Children.Add(valueText);

            grid.Margin = new Thickness(0, 2, 0, 2);
            panel.Children.Add(grid);
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки перехода к следующей неделе
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void Tab3Step_Click(object sender, RoutedEventArgs e)
        {
            if (_tab3Algorithm == null || _tab3State == null) return;

            _tab3State = _tab3Algorithm.ExecuteStep(_tab3State);
            _currentWeek = _tab3State.CurrentIndex;

            if (_currentWeek < _weeklyProgress.Count)
            {
                _weeklyProgress[_currentWeek].IsCompleted = true;
            }

            UpdateTab3UI();
            UpdateProgressStats();

            if (_currentWeek >= _weeklyProgress.Count - 1 && !_tab3State.IsFinished)
            {
                _tab3State.IsFinished = true;
                UpdateTab3UI();
                UpdateProgressStats();
            }
        }

        /// <summary>
        /// Запускает анимацию празднования достижения цели
        /// </summary>
        private void CelebrateGoalAchievement()
        {
            var colorAnimation = new ColorAnimation
            {
                From = Colors.Green,
                To = Colors.Purple,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            tab3LoopStatus.Foreground = new SolidColorBrush(Colors.Green);
            tab3LoopStatus.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

            var scaleAnimation = new DoubleAnimation
            {
                To = 1.1,
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };

            var scaleTransform = new ScaleTransform();
            tab3LoopStatus.RenderTransform = scaleTransform;
            tab3LoopStatus.RenderTransformOrigin = new Point(0.5, 0.5);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        /// <summary>
        /// Сбрасывает прогресс третьей вкладки к начальному состоянию
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void Tab3Reset_Click(object sender, RoutedEventArgs e)
        {
            _currentWeek = 0;
            InitializeTab3Loop();

            tab3LoopStatus.ClearValue(TextBlock.ForegroundProperty);
            tab3LoopStatus.ClearValue(UIElement.RenderTransformProperty);
        }

        /// <summary>
        /// Обновляет массив прогресса при изменении целей
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void Tab3UpdateArray_Click(object sender, RoutedEventArgs e)
        {
            InitializeTab3Loop();
        }

        /// <summary>
        /// Обновляет пользовательский интерфейс третьей вкладки
        /// </summary>
        private void UpdateTab3UI()
        {
            if (_tab3State == null || _tab3Algorithm == null) return;

            UpdateArrayPanel(tab3ArrayPanel, _tab3State);

            if (_tab3State.IsFinished)
            {
                tab3LoopStatus.Text = $"Цель достигнута! Финальный вес: {_targetWeight} кг";
                tab3LoopStatus.Foreground = Brushes.Green;
                tab3LoopStatus.FontWeight = FontWeights.Bold;

                AnimateProgressBar(tab3VariantProgress, 100);
                tab3VariantText.Text = $"Период {_weeklyProgress.Count} из {_weeklyProgress.Count} (100%)";
            }
            else if (_currentWeek < _weeklyProgress.Count)
            {
                double currentWeight = _weeklyProgress[_currentWeek].Weight;
                int currentRealWeek = _weeklyProgress[_currentWeek].RealWeekNumber;

                tab3LoopStatus.Text = $"Текущая неделя: {currentRealWeek} из {_totalWeeks} | Вес: {currentWeight:F1} кг";
                tab3LoopStatus.Foreground = Brushes.Black;
                tab3LoopStatus.FontWeight = FontWeights.SemiBold;

                double progressPercent = _weeklyProgress.Count > 0 ? (double)(_currentWeek + 1) / _weeklyProgress.Count * 100 : 0;
                AnimateProgressBar(tab3VariantProgress, progressPercent);
                tab3VariantText.Text = $"Период {_currentWeek + 1} из {_weeklyProgress.Count} ({progressPercent:F0}%)";
            }

            tab3InvariantText.Text = "Инвариант прогресса:\n" +
                                   "• current = лучший результат за пройденные периоды\n" +
                                   "• 0 ≤ период ≤ общее_количество_периодов\n" +
                                   "• Каждый период приближает к цели";

            tab3InvariantBefore.Fill = _tab3State.InvariantBeforeStep ? Brushes.Green : Brushes.Red;
            tab3InvariantAfter.Fill = _tab3State.InvariantAfterStep ? Brushes.Green : Brushes.Red;

            tab3StepBtn.IsEnabled = !_tab3State.IsFinished;

            UpdateProgressStats();
        }

        /// <summary>
        /// Обрабатывает сворачивание навигационной панели
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void NavCollapseButton_Click(object sender, RoutedEventArgs e)
        {
            CollapseNavigation();
        }

        /// <summary>
        /// Обрабатывает разворачивание навигационной панели
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void NavExpandButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandNavigation();
        }

        /// <summary>
        /// Сворачивает навигационную панель с анимацией
        /// </summary>
        private void CollapseNavigation()
        {
            if (_isNavCollapsed) return;

            _isNavCollapsed = true;

            var navAnimation = new GridLengthAnimation
            {
                From = new GridLength(300),
                To = new GridLength(0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var buttonAnimation = new GridLengthAnimation
            {
                From = new GridLength(0),
                To = new GridLength(32),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            NavColumn.BeginAnimation(ColumnDefinition.WidthProperty, navAnimation);
            ExpandButtonColumn.BeginAnimation(ColumnDefinition.WidthProperty, buttonAnimation);

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(150);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                NavExpandButton.Visibility = Visibility.Visible;
            };
            timer.Start();
        }

        /// <summary>
        /// Разворачивает навигационную панель с анимацией
        /// </summary>
        private void ExpandNavigation()
        {
            if (!_isNavCollapsed) return;

            _isNavCollapsed = false;

            NavExpandButton.Visibility = Visibility.Collapsed;

            var navAnimation = new GridLengthAnimation
            {
                From = new GridLength(0),
                To = new GridLength(300),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var buttonAnimation = new GridLengthAnimation
            {
                From = new GridLength(32),
                To = new GridLength(0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            NavColumn.BeginAnimation(ColumnDefinition.WidthProperty, navAnimation);
            ExpandButtonColumn.BeginAnimation(ColumnDefinition.WidthProperty, buttonAnimation);
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки справки в навигации
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void NavHelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isHelpOpen)
            {
                CloseNavHelp();
            }
            else
            {
                OpenNavHelp();
            }
        }

        /// <summary>
        /// Открывает панель справки с анимацией
        /// </summary>
        private void OpenNavHelp()
        {
            _isHelpOpen = true;
            NavHelpPanel.Visibility = Visibility.Visible;

            var helpAnimation = new DoubleAnimation
            {
                To = 150,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            NavHelpPanel.BeginAnimation(Border.HeightProperty, helpAnimation);

            UpdateNavHelpContent();
        }

        /// <summary>
        /// Закрывает панель справки с анимацией
        /// </summary>
        private void CloseNavHelp()
        {
            _isHelpOpen = false;

            var helpAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            helpAnimation.Completed += (s, e) =>
            {
                NavHelpPanel.Visibility = Visibility.Collapsed;
            };

            NavHelpPanel.BeginAnimation(Border.HeightProperty, helpAnimation);
        }

        /// <summary>
        /// Обновляет содержимое справки в зависимости от активной вкладки
        /// </summary>
        private void UpdateNavHelpContent()
        {
            string title = "";
            string content = "";

            if (panelProfile.Visibility == Visibility.Visible)
            {
                title = "Мой профиль";
                content = "• Введите свои данные: возраст, рост, вес и пол\n" +
                         "• Нажмите 'Рассчитать норму' для получения рекомендаций\n" +
                         "• WP-анализ покажет достижимость ваших целей\n" +
                         "• Отметьте элементы фитнес-программы для оценки эффективности";
            }
            else if (panelDiet.Visibility == Visibility.Visible)
            {
                title = "Дневник питания";
                content = "• Нажмите 'Выберите продукт...' для открытия списка\n" +
                         "• Выберите продукт из ListBox и укажите вес порции\n" +
                         "• Нажмите 'Добавить продукт' для учета в дневнике\n" +
                         "• Просматривайте историю питания и итоговые показатели\n" +
                         "• WP-анализ оценит сбалансированность вашего рациона";
            }
            else if (panelGoal.Visibility == Visibility.Visible)
            {
                title = "Планирование целей";
                content = "• Установите текущий и целевой вес, срок достижения\n" +
                         "• Нажмите 'Рассчитать прогресс' для получения плана\n" +
                         "• Используйте 'Следующая неделя' для пошагового прогресса\n" +
                         "• Просматривайте статистику и мотивационные советы\n" +
                         "• WP-анализ проверит реалистичность цели\n" +
                         "• Визуализация прогресса по неделям с анимациями";
            }

            NavHelpTitle.Text = title;
            NavHelpContent.Text = content;
        }

        /// <summary>
        /// Анимирует появление элемента с задержкой
        /// </summary>
        /// <param name="element">Элемент для анимации</param>
        /// <param name="delayMilliseconds">Задержка в миллисекундах</param>
        private void AnimateElementWithDelay(FrameworkElement element, int delayMilliseconds)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(delayMilliseconds);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            timer.Start();
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки выбора продукта
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void btnSelectProduct_Click(object sender, RoutedEventArgs e)
        {
            productPopup.IsOpen = true;
        }

        /// <summary>
        /// Обрабатывает изменение выбора продукта в списке
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void ProductListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (productListBox.SelectedItem != null)
            {
                string selectedProduct = productListBox.SelectedItem.ToString();
                btnSelectProduct.Content = selectedProduct;

                productPopup.IsOpen = false;

                CheckDietPrecondition();
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки выбора активности
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void btnSelectActivity_Click(object sender, RoutedEventArgs e)
        {
            activityPopup.IsOpen = true;
        }

        /// <summary>
        /// Обрабатывает изменение выбора активности в списке
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void ActivityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activityListBox.SelectedItem != null)
            {
                string selectedActivity = activityListBox.SelectedItem.ToString();
                btnSelectActivity.Content = selectedActivity;

                activityPopup.IsOpen = false;

                GoalInputChanged(sender, e);
            }
        }

        /// <summary>
        /// Получает выбранный продукт из интерфейса
        /// </summary>
        /// <returns>Название выбранного продукта или текст по умолчанию</returns>
        private string GetSelectedProduct()
        {
            return btnSelectProduct.Content?.ToString() ?? "Выберите продукт...";
        }

        /// <summary>
        /// Получает выбранный уровень активности из интерфейса
        /// </summary>
        /// <returns>Название выбранной активности или текст по умолчанию</returns>
        private string GetSelectedActivity()
        {
            return btnSelectActivity.Content?.ToString() ?? "Выберите активность...";
        }

        /// <summary>
        /// Преобразует строковое представление активности в enum
        /// </summary>
        /// <param name="activity">Строковое название активности</param>
        /// <returns>Соответствующее значение ActivityLevel</returns>
        private ActivityLevel ConvertActivityToEnum(string activity)
        {
            return activity switch
            {
                "Сидячий" => ActivityLevel.Sedentary,
                "Легкий" => ActivityLevel.Light,
                "Умеренный" => ActivityLevel.Moderate,
                "Активный" => ActivityLevel.Active,
                "Очень активный" => ActivityLevel.VeryActive,
                _ => ActivityLevel.Moderate
            };
        }

        /// <summary>
        /// Обрабатывает добавление продукта в дневник питания
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void btnAddFood_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(txtGrams.Text, out double grams) || grams <= 0 || grams > MAX_GRAMS)
                {
                    string errorMessage = $"ОШИБКА: Вес порции должен быть от 1 до {MAX_GRAMS} граммов";

                    txtDietAnalysis.Text = errorMessage;
                    txtDietAnalysis.Foreground = Brushes.Red;

                    dietPreIndicator.Fill = Brushes.Red;
                    dietPostIndicator.Fill = Brushes.Red;

                    AnimateTextAppearance(txtDietAnalysis);
                    return;
                }

                string selectedProduct = GetSelectedProduct();
                if (selectedProduct == "Выберите продукт...")
                {
                    string errorMessage = "ОШИБКА: Выберите продукт из списка";

                    txtDietAnalysis.Text = errorMessage;
                    txtDietAnalysis.Foreground = Brushes.Red;

                    dietPreIndicator.Fill = Brushes.Red;
                    dietPostIndicator.Fill = Brushes.Red;

                    AnimateTextAppearance(txtDietAnalysis);
                    return;
                }

                dietPreIndicator.Fill = Brushes.Green;

                var foodEntry = CreateFoodEntry(selectedProduct, grams);
                _foodHistory.Add(foodEntry);

                UpdateFoodHistory();
                UpdateTotals();

                bool postOk = _foodHistory.Count > 0;
                dietPostIndicator.Fill = postOk ? Brushes.Green : Brushes.Red;

                PerformDietAnalysis();
                AnimateTextAppearance(txtDietAnalysis);

                txtGrams.Text = "";
                btnSelectProduct.Content = "Выберите продукт...";
                productListBox.SelectedItem = null;
            }
            catch (Exception ex)
            {
                dietPostIndicator.Fill = Brushes.Red;

                string errorMessage = $"ОШИБКА: {ex.Message}";
                txtDietAnalysis.Text = errorMessage;
                txtDietAnalysis.Foreground = Brushes.Red;

                AnimateTextAppearance(txtDietAnalysis);
            }
        }

        /// <summary>
        /// Создает запись о продукте на основе названия и веса
        /// </summary>
        /// <param name="productName">Название продукта</param>
        /// <param name="grams">Вес порции в граммах</param>
        /// <returns>Запись о пищевом продукте</returns>
        private FoodEntry CreateFoodEntry(string productName, double grams)
        {
            var nutritionData = new Dictionary<string, (double calories, double protein, double fat, double carbs)>
            {
                {"Яблоко", (52, 0.3, 0.2, 14)},
                {"Банан", (89, 1.1, 0.3, 23)},
                {"Куриная грудка", (165, 31, 3.6, 0)},
                {"Рис вареный", (130, 2.7, 0.3, 28)},
                {"Творог", (98, 11, 4.3, 3.4)},
                {"Яйцо", (155, 13, 11, 1.1)},
                {"Овсянка", (68, 2.4, 1.4, 12)},
                {"Гречка", (92, 3.4, 0.6, 20)},
                {"Говядина", (250, 26, 15, 0)},
                {"Рыба", (206, 22, 13, 0)},
                {"Молоко", (42, 3.4, 1, 5)},
                {"Йогурт", (59, 3.5, 1.5, 6)}
            };

            if (nutritionData.ContainsKey(productName))
            {
                var data = nutritionData[productName];
                double factor = grams / 100.0;

                return new FoodEntry
                {
                    ProductName = productName,
                    Grams = grams,
                    Calories = data.calories * factor,
                    Protein = data.protein * factor,
                    Fat = data.fat * factor,
                    Carbs = data.carbs * factor
                };
            }

            return new FoodEntry
            {
                ProductName = productName,
                Grams = grams,
                Calories = grams * 1.5,
                Protein = grams * 0.1,
                Fat = grams * 0.05,
                Carbs = grams * 0.2
            };
        }

        /// <summary>
        /// Обновляет историю питания в интерфейсе
        /// </summary>
        private void UpdateFoodHistory()
        {
            listFoodHistory.Items.Clear();

            foreach (var entry in _foodHistory)
            {
                var listItem = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(12, 8, 12, 8)
                };

                var stackPanel = new StackPanel();

                var productText = new TextBlock
                {
                    Text = entry.ProductName,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
                    FontSize = 14
                };

                var detailsText = new TextBlock
                {
                    Text = $"{entry.Grams:F0}г • {entry.Calories:F0} ккал • Б: {entry.Protein:F1}г • Ж: {entry.Fat:F1}г • У: {entry.Carbs:F1}г",
                    Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                    FontSize = 12
                };

                stackPanel.Children.Add(productText);
                stackPanel.Children.Add(detailsText);
                listItem.Child = stackPanel;

                listFoodHistory.Items.Add(listItem);
            }

            if (listFoodHistory.Items.Count == 0)
            {
                var emptyMessage = new TextBlock
                {
                    Text = "Нет добавленных продуктов",
                    Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                listFoodHistory.Items.Add(emptyMessage);
            }
        }

        /// <summary>
        /// Обновляет итоговые показатели питания за день
        /// </summary>
        private void UpdateTotals()
        {
            double totalCalories = _foodHistory.Sum(f => f.Calories);
            double totalProtein = _foodHistory.Sum(f => f.Protein);
            double totalFat = _foodHistory.Sum(f => f.Fat);
            double totalCarbs = _foodHistory.Sum(f => f.Carbs);

            txtTotals.Text = $"Итоги за день:\n" +
                            $"Калории: {totalCalories:F0} ккал\n" +
                            $"Белки: {totalProtein:F1} г\n" +
                            $"Жиры: {totalFat:F1} г\n" +
                            $"Углеводы: {totalCarbs:F1} г";
        }

        /// <summary>
        /// Выполняет анализ сбалансированности рациона
        /// </summary>
        private void PerformDietAnalysis()
        {
            double totalCalories = _foodHistory.Sum(f => f.Calories);
            double totalProtein = _foodHistory.Sum(f => f.Protein);

            bool isBalanced = totalCalories > 0 && totalProtein > 0;
            string status = isBalanced ? "СБАЛАНСИРОВАН" : "НЕСБАЛАНСИРОВАН";
            string color = isBalanced ? "Green" : "Red";

            txtDietAnalysis.Text = $"Рацион: {status}\n\n" +
                                 $"Калории: {totalCalories:F0} ккал\n" +
                                 $"Белки: {totalProtein:F1} г\n\n" +
                                 $"Рекомендация: {(isBalanced ? "Продолжайте в том же духе!" : "Добавьте больше белков и калорий")}";

            txtDietAnalysis.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// Проверяет предусловия для добавления продукта в дневник
        /// </summary>
        private void CheckDietPrecondition()
        {
            if (!double.TryParse(txtGrams.Text, out double grams) || grams <= 0 || grams > MAX_GRAMS)
            {
                dietPreIndicator.Fill = Brushes.Red;
            }
            else
            {
                dietPreIndicator.Fill = Brushes.Green;
            }
        }

        /// <summary>
        /// Обрабатывает изменение текста в поле веса порции
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void txtGrams_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDietPrecondition();

            if (!double.TryParse(txtGrams.Text, out double grams) || grams <= 0 || grams > MAX_GRAMS)
            {
                txtDietAnalysis.Text = $"Введите вес порции от 1 до {MAX_GRAMS} граммов";
                txtDietAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
            }
            else if (GetSelectedProduct() == "Выберите продукт...")
            {
                txtDietAnalysis.Text = "Выберите продукт из списка";
                txtDietAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
            }
            else
            {
                txtDietAnalysis.Text = "Добавьте продукты и выполните WP-анализ";
                txtDietAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
            }
        }

        /// <summary>
        /// Обрабатывает переключение между вкладками навигации
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void NavigationChecked(object sender, RoutedEventArgs e)
        {
            if (sender == rbProfile) ShowTabWithAnimation(panelProfile);
            else if (sender == rbDiet) ShowTabWithAnimation(panelDiet);
            else if (sender == rbGoals)
            {
                if (!string.IsNullOrEmpty(txtWeight.Text) && double.TryParse(txtWeight.Text, out double currentWeight))
                {
                    _startWeight = currentWeight;
                    InitializeTab3Loop();
                }
                ShowTabWithAnimation(panelGoal);
            }

            if (_isHelpOpen)
            {
                UpdateNavHelpContent();
            }
        }

        /// <summary>
        /// Показывает указанную вкладку с анимацией
        /// </summary>
        /// <param name="tabToShow">Вкладка для отображения</param>
        private void ShowTabWithAnimation(Grid tabToShow)
        {
            panelProfile.Visibility = Visibility.Collapsed;
            panelDiet.Visibility = Visibility.Collapsed;
            panelGoal.Visibility = Visibility.Collapsed;

            tabToShow.Visibility = Visibility.Visible;

            AnimateTabAppearance(tabToShow);
        }

        /// <summary>
        /// Анимирует появление вкладки
        /// </summary>
        /// <param name="tab">Вкладка для анимации</param>
        private void AnimateTabAppearance(Grid tab)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var slideIn = new ThicknessAnimation
            {
                From = new Thickness(40, 0, -40, 0),
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            tab.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            tab.BeginAnimation(FrameworkElement.MarginProperty, slideIn);

            AnimateCardsInTab(tab);
        }

        /// <summary>
        /// Анимирует появление карточек внутри вкладки с задержкой
        /// </summary>
        /// <param name="tab">Вкладка с карточками</param>
        private void AnimateCardsInTab(Grid tab)
        {
            var cards = FindVisualChildren<Border>(tab).Where(b => b.Style == FindResource("InteractiveCard")).ToList();

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    BeginTime = TimeSpan.FromSeconds(i * 0.15),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                card.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }

        /// <summary>
        /// Анимирует появление текстового элемента
        /// </summary>
        /// <param name="textBlock">Текстовый элемент для анимации</param>
        private void AnimateTextAppearance(TextBlock textBlock)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var slideUp = new ThicknessAnimation
            {
                From = new Thickness(0, 10, 0, 0),
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            textBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            textBlock.BeginAnimation(FrameworkElement.MarginProperty, slideUp);
        }

        /// <summary>
        /// Анимирует появление карточки
        /// </summary>
        /// <param name="card">Карточка для анимации</param>
        private void AnimateCardAppearance(Border card)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var slideUp = new ThicknessAnimation
            {
                From = new Thickness(0, 20, 0, 0),
                To = new Thickness(0, 12, 0, 0),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            card.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            card.BeginAnimation(FrameworkElement.MarginProperty, slideUp);
        }

        /// <summary>
        /// Находит все дочерние элементы указанного типа в визуальном дереве
        /// </summary>
        /// <typeparam name="T">Тип искомых элементов</typeparam>
        /// <param name="depObj">Корневой элемент для поиска</param>
        /// <returns>Перечисление найденных элементов</returns>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// Анимирует изменение значения прогресс-бара
        /// </summary>
        /// <param name="progressBar">Прогресс-бар для анимации</param>
        /// <param name="newValue">Новое значение прогресса</param>
        private void AnimateProgressBar(ProgressBar progressBar, double newValue)
        {
            var animation = new DoubleAnimation
            {
                To = newValue,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        /// <summary>
        /// Обновляет результат оценки эффективности фитнес-программы
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void UpdateSuccessResult(object sender, RoutedEventArgs e)
        {
            bool diet = cbDiet?.IsChecked == true;
            bool training = cbTraining?.IsChecked == true;
            bool sleep = cbSleep?.IsChecked == true;

            int conditionsMet = (diet ? 1 : 0) + (training ? 1 : 0) + (sleep ? 1 : 0);
            bool success = conditionsMet >= 2;

            if (txtSuccessResult != null)
            {
                AnimateSuccessResultChange(txtSuccessResult, success ? "Успех" : "Неудача",
                                         success ? Colors.Green : Colors.Red);
            }
        }

        /// <summary>
        /// Анимирует изменение результата эффективности
        /// </summary>
        /// <param name="textBlock">Текстовый элемент для анимации</param>
        /// <param name="newText">Новый текст</param>
        /// <param name="newColor">Новый цвет</param>
        private void AnimateSuccessResultChange(TextBlock textBlock, string newText, Color newColor)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            fadeOut.Completed += (s, e) =>
            {
                textBlock.Text = newText;
                textBlock.Foreground = new SolidColorBrush(newColor);

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleAnimation = new ScaleTransform(1, 1);
                textBlock.RenderTransform = scaleAnimation;
                var scaleXAnimation = new DoubleAnimation(1.1, 1, TimeSpan.FromSeconds(0.3));
                var scaleYAnimation = new DoubleAnimation(1.1, 1, TimeSpan.FromSeconds(0.3));

                textBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                scaleAnimation.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                scaleAnimation.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
            };

            textBlock.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Обновляет оценку эффективности программы
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void UpdateEfficiencyScore(object sender, RoutedEventArgs e)
        {
            bool diet = cbEfficiencyDiet?.IsChecked == true;
            bool training = cbEfficiencyTraining?.IsChecked == true;
            bool sleep = cbEfficiencySleep?.IsChecked == true;
            bool water = cbEfficiencyWater?.IsChecked == true;

            int score = (diet ? 3 : 0) + (training ? 4 : 0) + (sleep ? 3 : 0) + (water ? 2 : 0);

            if (txtEfficiencyResult != null)
            {
                AnimateEfficiencyScoreChange(txtEfficiencyResult, $"{score}/12", score);
                AnimateProgressBar(efficiencyProgress, score);
            }
        }

        /// <summary>
        /// Анимирует изменение оценки эффективности
        /// </summary>
        /// <param name="textBlock">Текстовый элемент для анимации</param>
        /// <param name="newText">Новый текст</param>
        /// <param name="score">Новая оценка</param>
        private void AnimateEfficiencyScoreChange(TextBlock textBlock, string newText, int score)
        {
            Color newColor = score >= 10 ? Colors.Green :
                            score >= 7 ? Color.FromRgb(255, 165, 0) :
                            score >= 4 ? Color.FromRgb(255, 193, 7) :
                            Colors.Red;

            if (int.TryParse(textBlock.Text.Split('/')[0], out int oldScore))
            {
                AnimateCounterChange(textBlock, oldScore, score, newColor);
            }
            else
            {
                AnimateSimpleTextChange(textBlock, newText, newColor);
            }
        }

        /// <summary>
        /// Анимирует счетчик с постепенным изменением значения
        /// </summary>
        /// <param name="textBlock">Текстовый элемент для анимации</param>
        /// <param name="oldScore">Старое значение</param>
        /// <param name="newScore">Новое значение</param>
        /// <param name="newColor">Новый цвет</param>
        private void AnimateCounterChange(TextBlock textBlock, int oldScore, int newScore, Color newColor)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            int currentValue = oldScore;
            int step = newScore > oldScore ? 1 : -1;

            string originalText = textBlock.Text;

            timer.Tick += (s, e) =>
            {
                currentValue += step;

                if ((step > 0 && currentValue >= newScore) || (step < 0 && currentValue <= newScore))
                {
                    currentValue = newScore;
                    timer.Stop();

                    textBlock.Text = $"{newScore}/12";
                    textBlock.Foreground = new SolidColorBrush(newColor);

                    var scaleTransform = new ScaleTransform(1, 1);
                    textBlock.RenderTransform = scaleTransform;
                    var scaleAnimation = new DoubleAnimation(1.1, 1, TimeSpan.FromSeconds(0.2));
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                }
                else
                {
                    textBlock.Text = $"{currentValue}/12";
                }
            };

            var colorAnimation = new ColorAnimation
            {
                To = newColor,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            textBlock.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            timer.Start();
        }

        /// <summary>
        /// Анимирует простое изменение текста
        /// </summary>
        /// <param name="textBlock">Текстовый элемент для анимации</param>
        /// <param name="newText">Новый текст</param>
        /// <param name="newColor">Новый цвет</param>
        private void AnimateSimpleTextChange(TextBlock textBlock, string newText, Color newColor)
        {
            var fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            fadeOut.Completed += (s, e) =>
            {
                textBlock.Text = newText;
                textBlock.Foreground = new SolidColorBrush(newColor);

                var fadeIn = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleTransform = new ScaleTransform(1, 1);
                textBlock.RenderTransform = scaleTransform;
                var scaleXAnimation = new DoubleAnimation(1.1, 1, TimeSpan.FromSeconds(0.3));
                var scaleYAnimation = new DoubleAnimation(1.1, 1, TimeSpan.FromSeconds(0.3));

                textBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
            };

            textBlock.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Инициализирует состояние чекбоксов при запуске приложения
        /// </summary>
        private void InitializeCheckboxes()
        {
            UpdateSuccessResult(null, null);
            UpdateEfficiencyScore(null, null);
        }

        /// <summary>
        /// Обрабатывает изменение данных в первой вкладке
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void InputChanged(object sender, RoutedEventArgs e)
        {
            CheckPrecondition();

            if (!ValidateInputs(out List<string> errors))
            {
                txtResult.Text = "Введите корректные данные для расчета";
                txtResult.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

                txtCaloriesAnalysis.Text = "WP-анализ будет доступен после ввода корректных данных";
                txtCaloriesAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

                postIndicator.Fill = Brushes.Red;
            }
            else
            {
                txtResult.Text = "Введите данные и нажмите 'Рассчитать норму'";
                txtResult.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

                txtCaloriesAnalysis.Text = "Анализ будет показан здесь";
                txtCaloriesAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
            }
        }

        /// <summary>
        /// Проверяет предусловия для расчета норм питания
        /// </summary>

        private void CheckPrecondition()
        {
            if (preIndicator == null) return;

            bool isValid = ValidateInputs(out List<string> errors);

            if (isValid)
            {
                preIndicator.Fill = Brushes.Green;
            }
            else
            {
                preIndicator.Fill = Brushes.Red;
            }
        }

        /// <summary>
        /// Проверяет валидность введенных пользовательских данных (возраст, рост, вес, пол)
        /// </summary>
        /// <param name="errors">Список для сохранения ошибок валидации</param>
        /// <returns>True если все данные валидны, иначе False</returns>
        private bool ValidateInputs(out List<string> errors)
        {
            errors = new List<string>();

            // Проверка возраста
            if (!int.TryParse(txtAge.Text, out int age))
            {
                errors.Add("Возраст должен быть числом");
            }
            else if (age < MIN_AGE || age > MAX_AGE)
            {
                errors.Add($"Возраст должен быть от {MIN_AGE} до {MAX_AGE} лет");
            }

            // Проверка роста
            if (!double.TryParse(txtHeight.Text, out double height))
            {
                errors.Add("Рост должен быть числом");
            }
            else if (height < MIN_HEIGHT || height > MAX_HEIGHT)
            {
                errors.Add($"Рост должен быть от {MIN_HEIGHT} до {MAX_HEIGHT} см");
            }

            // Проверка веса
            if (!double.TryParse(txtWeight.Text, out double weight))
            {
                errors.Add("Вес должен быть числом");
            }
            else if (weight < MIN_WEIGHT || weight > MAX_WEIGHT)
            {
                errors.Add($"Вес должен быть от {MIN_WEIGHT} до {MAX_WEIGHT} кг");
            }

            // Проверка пола
            if (rbMale.IsChecked != true && rbFemale.IsChecked != true)
            {
                errors.Add("Выберите пол");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки расчета калорий
        /// Выполняет валидацию, расчет и отображение результатов с WP-анализом
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void btnCalc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем валидацию
                if (!ValidateInputs(out List<string> validationErrors))
                {
                    // Показываем ошибки в результатах
                    string errorMessage = "ОШИБКА В ДАННЫХ:\n" + string.Join("\n", validationErrors);

                    txtResult.Text = errorMessage;
                    txtResult.Foreground = Brushes.Red;

                    txtCaloriesAnalysis.Text = "WP-АНАЛИЗ: Предусловия не выполнены\n\n" +
                                             "Не выполненные условия:\n" + string.Join("\n", validationErrors);
                    txtCaloriesAnalysis.Foreground = Brushes.Red;

                    postIndicator.Fill = Brushes.Red;

                    // Анимируем появление ошибки
                    AnimateTextAppearance(txtResult);
                    AnimateTextAppearance(txtCaloriesAnalysis);
                    return;
                }

                // Если данные валидны, продолжаем расчет
                int age = int.Parse(txtAge.Text);
                double height = double.Parse(txtHeight.Text);
                double weight = double.Parse(txtWeight.Text);
                Gender gender = rbMale.IsChecked == true ? Gender.Male : Gender.Female;

                var user = new UserProfile(gender, age, height, weight);
                var calc = new CalorieCalculator(user);

                var (calories, protein, fat, carbs) = calc.Calculate();

                bool postOk = calories > 0 && protein > 0 && fat > 0 && carbs > 0;
                postIndicator.Fill = postOk ? Brushes.Green : Brushes.Red;

                // Обновляем текст результатов
                txtResult.Text = $"Суточная норма:\n" +
                                $"Калории: {calories} ккал\n" +
                                $"Белки: {protein} г\n" +
                                $"Жиры: {fat} г\n" +
                                $"Углеводы: {carbs} г";
                txtResult.Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33));

                // Анимируем появление текста результатов
                AnimateTextAppearance(txtResult);
                AnimateCardAppearance(resultCard);

                // WP-aнализ
                var analysis = calc.AnalyzeGoalAchievability(isSecondTab: false);
                DisplayAnalysisResult(txtCaloriesAnalysis, analysis);

                // Анимируем появление анализа
                AnimateTextAppearance(txtCaloriesAnalysis);
            }
            catch (Exception ex)
            {
                // Обработка исключений из CalorieCalculator
                string errorMessage = $"ОШИБКА РАСЧЕТА: {ex.Message}";

                txtResult.Text = errorMessage;
                txtResult.Foreground = Brushes.Red;

                txtCaloriesAnalysis.Text = "WP-АНАЛИЗ: Ошибка выполнения\n\n" +
                                         $"Причина: {ex.Message}";
                txtCaloriesAnalysis.Foreground = Brushes.Red;

                postIndicator.Fill = Brushes.Red;

                AnimateTextAppearance(txtResult);
                AnimateTextAppearance(txtCaloriesAnalysis);
            }
        }

        /// <summary>
        /// Обрабатывает изменение ввода данных цели в третьей вкладке
        /// Выполняет динамическую валидацию и обновление интерфейса
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void GoalInputChanged(object sender, RoutedEventArgs e)
        {
            if (txtTargetWeight == null || txtTimelineMonths == null || goalPreIndicator == null)
                return;

            // Проверяем валидацию
            if (!ValidateGoalInputs(out List<string> errors))
            {
                goalPreIndicator.Fill = Brushes.Red;

                // Показываем первую ошибку в результатах
                if (errors.Count > 0)
                {
                    txtGoalResult.Text = "Введите корректные данные для расчета";
                    txtGoalResult.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

                    txtGoalAnalysis.Text = $"Ошибка: {errors[0]}";
                    txtGoalAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
                }
            }
            else
            {
                goalPreIndicator.Fill = Brushes.Green;

                // Сбрасываем текст если данные валидны
                txtGoalResult.Text = "Установите цели для расчета";
                txtGoalResult.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

                txtGoalAnalysis.Text = "Выполните анализ для проверки достижимости цели";
                txtGoalAnalysis.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

                // Автоматически пересчитываем если данные валидны
                InitializeTab3Loop();
            }
        }

        /// <summary>
        /// Проверяет валидность введенных данных цели (целевой вес, срок, текущий вес)
        /// </summary>
        /// <param name="errors">Список для сохранения ошибок валидации</param>
        /// <returns>True если все данные цели валидны, иначе False</returns>
        private bool ValidateGoalInputs(out List<string> errors)
        {
            errors = new List<string>();

            // Проверка целевого веса
            if (!double.TryParse(txtTargetWeight.Text, out double targetWeight))
            {
                errors.Add("Целевой вес должен быть числом");
            }
            else if (targetWeight < MIN_WEIGHT || targetWeight > MAX_WEIGHT)
            {
                errors.Add($"Целевой вес должен быть от {MIN_WEIGHT} до {MAX_WEIGHT} кг");
            }

            // Проверка срока
            if (!int.TryParse(txtTimelineMonths.Text, out int months))
            {
                errors.Add("Срок должен быть числом");
            }
            else if (months < MIN_MONTHS || months > MAX_MONTHS)
            {
                errors.Add($"Срок должен быть от {MIN_MONTHS} до {MAX_MONTHS} месяцев");
            }

            // Проверка текущего веса (из первой вкладки)
            if (!double.TryParse(txtWeight.Text, out double currentWeight))
            {
                errors.Add("Сначала укажите ваш текущий вес в разделе 'Мой профиль'");
            }
            else if (currentWeight < MIN_WEIGHT || currentWeight > MAX_WEIGHT)
            {
                errors.Add($"Текущий вес должен быть от {MIN_WEIGHT} до {MAX_WEIGHT} кг");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки установки цели
        /// Выполняет расчет плана достижения цели и анализ реалистичности
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void btnSetGoal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем валидацию
                if (!ValidateGoalInputs(out List<string> validationErrors))
                {
                    // Показываем ошибки в результатах
                    string errorMessage = "ОШИБКА В ДАННЫХ:\n" + string.Join("\n", validationErrors);

                    txtGoalResult.Text = errorMessage;
                    txtGoalResult.Foreground = Brushes.Red;

                    txtGoalAnalysis.Text = "WP-АНАЛИЗ: Предусловия не выполнены\n\n" +
                                         "Не выполненные условия:\n" + string.Join("\n", validationErrors);
                    txtGoalAnalysis.Foreground = Brushes.Red;

                    goalPreIndicator.Fill = Brushes.Red;
                    goalPostIndicator.Fill = Brushes.Red;

                    // Анимируем появление ошибки
                    AnimateTextAppearance(txtGoalResult);
                    AnimateTextAppearance(txtGoalAnalysis);
                    return;
                }

                // Если данные валидны, продолжаем расчет
                double currentWeight = double.Parse(txtWeight.Text);
                double targetWeight = double.Parse(txtTargetWeight.Text);
                int timelineMonths = int.Parse(txtTimelineMonths.Text);

                goalPreIndicator.Fill = Brushes.Green;

                // Обновляем прогресс
                _startWeight = currentWeight;
                _targetWeight = targetWeight;
                InitializeTab3Loop();

                // Показываем результат
                double weightChange = targetWeight - currentWeight;
                string changeType = weightChange > 0 ? "набор веса" : "похудение";
                string arrow = weightChange > 0 ? "↑" : "↓";

                txtGoalResult.Text = $"План на {timelineMonths} месяцев ({timelineMonths * 4} недель):\n" +
                                    $"Текущий вес: {currentWeight} кг\n" +
                                    $"Целевой вес: {targetWeight} кг\n" +
                                    $"Изменение: {Math.Abs(weightChange):F1} кг {arrow}\n" +
                                    $"В неделю: {Math.Abs(weightChange / (timelineMonths * 4)):F2} кг\n" +
                                    $"Тип: {changeType}";
                txtGoalResult.Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33));

                goalPostIndicator.Fill = Brushes.Green;

                // Анализ реалистичности
                var analysis = AnalyzeGoalRealism(currentWeight, targetWeight, timelineMonths);
                DisplayGoalAnalysisResult(txtGoalAnalysis, analysis);

                AnimateTextAppearance(txtGoalAnalysis);
            }
            catch (Exception ex)
            {
                goalPostIndicator.Fill = Brushes.Red;

                string errorMessage = $"ОШИБКА РАСЧЕТА: {ex.Message}";

                txtGoalResult.Text = errorMessage;
                txtGoalResult.Foreground = Brushes.Red;

                txtGoalAnalysis.Text = "WP-АНАЛИЗ: Ошибка выполнения\n\n" +
                                     $"Причина: {ex.Message}";
                txtGoalAnalysis.Foreground = Brushes.Red;

                AnimateTextAppearance(txtGoalResult);
                AnimateTextAppearance(txtGoalAnalysis);
            }
        }

        /// <summary>
        /// Анализирует реалистичность поставленной цели по изменению веса
        /// Проверяет соответствует ли недельное изменение веса безопасным нормам
        /// </summary>
        /// <param name="currentWeight">Текущий вес пользователя</param>
        /// <param name="targetWeight">Целевой вес</param>
        /// <param name="months">Срок достижения в месяцах</param>
        /// <returns>Результат анализа с оценкой достижимости цели</returns>
        private WpEngine.AnalysisResult AnalyzeGoalRealism(double currentWeight, double targetWeight, int months)
        {
            double weightChange = targetWeight - currentWeight;
            double weeklyChange = weightChange / (months * 4);
            bool isRealistic = Math.Abs(weeklyChange) <= 1.0;

            string reason;
            if (isRealistic)
            {
                reason = Math.Abs(weeklyChange) <= 0.5 ?
                    "Отличный темп! Цель легко достижима при соблюдении рекомендаций" :
                    "Хороший темп! Цель реалистична при регулярных тренировках и правильном питании";
            }
            else
            {
                reason = "Слишком быстрый темп! Рекомендуется увеличить срок или скорректировать цель\n" +
                        "Безопасное изменение веса: 0.5-1 кг в неделю";
            }

            return new WpEngine.AnalysisResult
            {
                IsAchievable = isRealistic,
                Reason = reason,
                HumanReadablePrecondition = $"Темп изменения: {Math.Abs(weeklyChange):F2} кг/неделю",
                HoareTriple = "",
                Steps = new List<string>()
            };
        }

        /// <summary>
        /// Отображает результат WP-анализа в текстовом блоке
        /// Форматирует вывод в зависимости от успешности анализа
        /// </summary>
        /// <param name="textBlock">Текстовый блок для отображения результата</param>
        /// <param name="analysis">Результат анализа для отображения</param>
        private void DisplayAnalysisResult(TextBlock textBlock, WpEngine.AnalysisResult analysis)
        {
            if (analysis == null)
            {
                textBlock.Text = "WP-АНАЛИЗ: Ошибка анализа\n\nАнализ не может быть выполнен";
                textBlock.Foreground = Brushes.Red;
                return;
            }

            string status = analysis.IsAchievable ? "УСПЕШНО" : "НЕУДАЧА";
            string color = analysis.IsAchievable ? "Green" : "Red";

            textBlock.Text = $"WP-АНАЛИЗ: {status}\n\n" +
                           $"Результат: {analysis.Reason}\n\n" +
                           $"Условия выполнения:\n{analysis.HumanReadablePrecondition}";

            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// Проверяет валидность базовых данных профиля пользователя
        /// Упрощенная проверка без детализации ошибок
        /// </summary>
        /// <returns>True если данные профиля валидны, иначе False</returns>
        private bool ValidateProfileInput()
        {
            return int.TryParse(txtAge.Text, out int age) && age > 0 &&
                   double.TryParse(txtHeight.Text, out double height) && height > 0 &&
                   double.TryParse(txtWeight.Text, out double weight) && weight > 0;
        }

        /// <summary>
        /// Создает объект профиля пользователя на основе введенных данных
        /// </summary>
        /// <returns>Новый экземпляр UserProfile</returns>
        private UserProfile CreateUserProfile()
        {
            return new UserProfile(
                rbMale.IsChecked == true ? Gender.Male : Gender.Female,
                int.Parse(txtAge.Text),
                double.Parse(txtHeight.Text),
                double.Parse(txtWeight.Text)
            );
        }

        /// <summary>
        /// Создает объект профиля пользователя с учетом установленных целей
        /// Включает целевой вес, срок и уровень активности
        /// </summary>
        /// <returns>Новый экземпляр UserProfile с целями</returns>
        private UserProfile CreateUserProfileWithGoal()
        {
            var user = CreateUserProfile();
            user.TargetWeight = double.Parse(txtTargetWeight.Text);
            user.TimelineMonths = int.Parse(txtTimelineMonths.Text);
            user.ActivityLevel = ConvertActivityToEnum(GetSelectedActivity());
            return user;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки сворачивания окна
        /// </summary>
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки развертывания/восстановления окна
        /// </summary>
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                btnMaximize.Content = "□"; // Значок развернуть
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                btnMaximize.Content = "❐"; // Значок восстановить
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки закрытия приложения с красивым диалогом
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ShowCustomCloseDialog();
        }

        /// <summary>
        /// Показывает красивый кастомный диалог подтверждения закрытия
        /// </summary>
        private void ShowCustomCloseDialog()
        {

            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
                Opacity = 0
            };


            var dialog = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Width = 400,
                Height = 200,
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 0,
                    BlurRadius = 20,
                    Color = Colors.Black,
                    Opacity = 0.3
                },
                RenderTransform = new ScaleTransform(0.8, 0.8),
                Opacity = 0
            };

            var contentStack = new StackPanel
            {
                Margin = new Thickness(24)
            };


            var headerStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var iconBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(126, 87, 194)),
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(16),
                Margin = new Thickness(0, 0, 12, 0)
            };

            var iconText = new TextBlock
            {
                Text = "?",
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            iconBorder.Child = iconText;

            var textStack = new StackPanel();
            var title = new TextBlock
            {
                Text = "Подтверждение закрытия",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };

            var message = new TextBlock
            {
                Text = "Вы уверены, что хотите закрыть приложение?",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                Margin = new Thickness(0, 4, 0, 0)
            };

            textStack.Children.Add(title);
            textStack.Children.Add(message);

            headerStack.Children.Add(iconBorder);
            headerStack.Children.Add(textStack);


            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var btnNo = new Button
            {
                Content = "Нет",
                Style = (Style)FindResource("CustomMessageBoxButton"),
                Background = new SolidColorBrush(Color.FromRgb(237, 231, 246)),
                Foreground = new SolidColorBrush(Color.FromRgb(126, 87, 194))
            };

            var btnYes = new Button
            {
                Content = "Да, закрыть",
                Style = (Style)FindResource("CustomMessageBoxButton"),
                Margin = new Thickness(8, 0, 0, 0)
            };

            btnNo.Click += (s, args) => { CloseDialog(overlay, dialog); };
            btnYes.Click += (s, args) => { this.Close(); };

            buttonStack.Children.Add(btnNo);
            buttonStack.Children.Add(btnYes);

            contentStack.Children.Add(headerStack);
            contentStack.Children.Add(buttonStack);
            dialog.Child = contentStack;


            var mainGrid = this.Content as Grid;
            mainGrid.Children.Add(overlay);
            mainGrid.Children.Add(dialog);


            dialog.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            dialog.Arrange(new Rect(
                (this.ActualWidth - dialog.DesiredSize.Width) / 2,
                (this.ActualHeight - dialog.DesiredSize.Height) / 2,
                dialog.DesiredSize.Width,
                dialog.DesiredSize.Height
            ));


            AnimateDialogAppearance(overlay, dialog);
        }

        /// <summary>
        /// Анимирует появление диалога
        /// </summary>
        private void AnimateDialogAppearance(Border overlay, Border dialog)
        {

            var overlayAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };


            var dialogOpacityAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var dialogScaleAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1, Springiness = 4 }
            };

            overlay.BeginAnimation(UIElement.OpacityProperty, overlayAnimation);
            dialog.BeginAnimation(UIElement.OpacityProperty, dialogOpacityAnimation);
            dialog.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, dialogScaleAnimation);
            dialog.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, dialogScaleAnimation);
        }

        /// <summary>
        /// Закрывает диалог с анимацией
        /// </summary>
        private void CloseDialog(Border overlay, Border dialog)
        {
            var overlayAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var dialogAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            overlayAnimation.Completed += (s, e) =>
            {
                var mainGrid = this.Content as Grid;
                mainGrid.Children.Remove(overlay);
                mainGrid.Children.Remove(dialog);
            };

            overlay.BeginAnimation(UIElement.OpacityProperty, overlayAnimation);
            dialog.BeginAnimation(UIElement.OpacityProperty, dialogAnimation);
        }

        /// <summary>
        /// Обрабатывает перетаскивание окна за верхнюю панель
        /// </summary>
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                btnMaximize_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }
    }
}