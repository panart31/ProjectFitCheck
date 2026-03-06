using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ClassLibraryFitCheck;

namespace NutritionApp
{
    public partial class MainWindow : Window
    {
        // Трекеры и калькуляторы
        private DietTracker _dietTracker;
        private UserProfile _currentUser;
        private List<FoodEntry> _foodHistory = new List<FoodEntry>();

        // Константы валидации
        private const int MIN_AGE = 14, MAX_AGE = 100;
        private const double MIN_WEIGHT = 45, MAX_WEIGHT = 110;
        private const double MIN_HEIGHT = 130, MAX_HEIGHT = 200;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Инициализация интерфейса
            CreateMainContent();
        }

        private void CreateMainContent()
        {
            var mainStack = new StackPanel { Margin = new Thickness(20) };

            // Карточка профиля
            var profileCard = CreateProfileCard();
            mainStack.Children.Add(profileCard);

            // Карточка питания
            var dietCard = CreateDietCard();
            mainStack.Children.Add(dietCard);

            // Карточка целей
            var goalCard = CreateGoalCard();
            mainStack.Children.Add(goalCard);

            var scrollViewer = new ScrollViewer
            {
                Content = mainStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            MainContentGrid.Children.Add(scrollViewer);
        }

        private Border CreateProfileCard()
        {
            var card = new Border { Style = (Style)FindResource("InteractiveCard") };
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = "Мой профиль",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            // Поля ввода
            var leftStack = new StackPanel();
            leftStack.Children.Add(new TextBlock { Text = "Возраст (лет)", Margin = new Thickness(0, 10, 0, 5) });
            var txtAge = new TextBox { Style = (Style)FindResource("ModernTextBox") };
            txtAge.TextChanged += (s, e) => ValidateInputs();
            leftStack.Children.Add(txtAge);

            var rightStack = new StackPanel();
            rightStack.Children.Add(new TextBlock { Text = "Вес (кг)", Margin = new Thickness(0, 10, 0, 5) });
            var txtWeight = new TextBox { Style = (Style)FindResource("ModernTextBox") };
            txtWeight.TextChanged += (s, e) => ValidateInputs();
            rightStack.Children.Add(txtWeight);

            Grid.SetColumn(leftStack, 0);
            Grid.SetColumn(rightStack, 2);
            grid.Children.Add(leftStack);
            grid.Children.Add(rightStack);

            stack.Children.Add(grid);

            // Кнопка расчета
            var btnCalc = new Button
            {
                Content = "Рассчитать норму",
                Style = (Style)FindResource("PrimaryButton"),
                Margin = new Thickness(0, 20, 0, 0)
            };
            btnCalc.Click += (s, e) => CalculateCalories(txtAge, txtWeight);
            stack.Children.Add(btnCalc);

            card.Child = stack;
            return card;
        }

        private Border CreateDietCard()
        {
            var card = new Border { Style = (Style)FindResource("InteractiveCard") };
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = "Дневник питания",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Функционал в разработке",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 10, 0, 0)
            });

            card.Child = stack;
            return card;
        }

        private Border CreateGoalCard()
        {
            var card = new Border { Style = (Style)FindResource("InteractiveCard") };
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = "Планирование целей",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Функционал в разработке",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 10, 0, 0)
            });

            card.Child = stack;
            return card;
        }

        private void ValidateInputs()
        {
            // Базовая валидация
        }

        private void CalculateCalories(TextBox txtAge, TextBox txtWeight)
        {
            try
            {
                if (!int.TryParse(txtAge.Text, out int age) || age < MIN_AGE || age > MAX_AGE)
                {
                    MessageBox.Show($"Возраст должен быть от {MIN_AGE} до {MAX_AGE} лет");
                    return;
                }

                if (!double.TryParse(txtWeight.Text, out double weight) || weight < MIN_WEIGHT || weight > MAX_WEIGHT)
                {
                    MessageBox.Show($"Вес должен быть от {MIN_WEIGHT} до {MAX_WEIGHT} кг");
                    return;
                }

                // Создаем пользователя и рассчитываем
                _currentUser = new UserProfile(Gender.Male, age, 175, weight);
                var calculator = new CalorieCalculator(_currentUser);
                var result = calculator.Calculate();

                MessageBox.Show($"Суточная норма: {result.Calories} ккал\n" +
                              $"Белки: {result.Protein}г\n" +
                              $"Жиры: {result.Fat}г\n" +
                              $"Углеводы: {result.Carbs}г");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Анимации для UI
        private void AnimateElement(FrameworkElement element)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}