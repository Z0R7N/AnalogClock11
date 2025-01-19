using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AnalogClock11
{
    public partial class MainWindow : Window
    {
        private const int CLOCK_SIZE = 150; // Диаметр часов
        private const int CENTER_X = CLOCK_SIZE / 2; // Центр часов по X
        private const int CENTER_Y = CLOCK_SIZE / 2; // Центр часов по Y
        private const int RADIUS = CLOCK_SIZE / 2 - 10; // Радиус циферблата

        private bool isDragging = false; // Флаг, указывающий, что окно перемещается
        private Point dragStartPoint; // Точка, с которой началось перемещение

        public MainWindow()
        {
            InitializeComponent();

            // Устанавливаем размеры окна
            Width = CLOCK_SIZE;
            Height = CLOCK_SIZE;

            // Загружаем положение окна
            LoadWindowPosition();

            // Устанавливаем таймер для обновления каждые 5 секунд
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Перерисовываем часы
            DrawClock();
        }

        private void DrawClock()
        {
            // Очищаем Canvas
            canvas.Children.Clear();

            // Рисуем чёрный толстый круг
            Ellipse outerCircle = new Ellipse
            {
                Width = CLOCK_SIZE,
                Height = CLOCK_SIZE,
                Stroke = Brushes.Black,
                StrokeThickness = 8,
            };
            Canvas.SetLeft(outerCircle, 0);
            Canvas.SetTop(outerCircle, 0);
            canvas.Children.Add(outerCircle);

            // Рисуем белый тонкий круг
            Ellipse outerCircle2 = new Ellipse
            {
                Width = CLOCK_SIZE - 2,
                Height = CLOCK_SIZE - 2,
                Stroke = Brushes.White,
                StrokeThickness = 6,
                Fill = Brushes.Transparent // Прозрачная заливка
            };
            Canvas.SetLeft(outerCircle2, 1);
            Canvas.SetTop(outerCircle2, 1);
            canvas.Children.Add(outerCircle2);

            // Добавляем день недели вверху циферблата
            TextBlock dayOfWeekText = new TextBlock
            {
                Text = DateTime.Now.ToString("dddd"), // Полное название дня недели
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White, // Белый цвет текста
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Измеряем размер текста, чтобы правильно центрировать
            dayOfWeekText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            dayOfWeekText.Arrange(new Rect(dayOfWeekText.DesiredSize));

            // Рассчитываем позицию по X для центрирования
            double textWidth = dayOfWeekText.DesiredSize.Width;
            double textX = CENTER_X - textWidth / 2; // Центрируем по горизонтали

            // Устанавливаем позицию текста
            Canvas.SetLeft(dayOfWeekText, textX); // Центрируем по X
            Canvas.SetTop(dayOfWeekText, 25);     // Смещаем по Y на 25 пикселей

            // Создаём тень с помощью нескольких смещённых текстовых блоков
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue; // Пропускаем центральный блок (основной текст)

                    TextBlock shadowText = new TextBlock
                    {
                        Text = DateTime.Now.ToString("dddd"),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black, // Тень чёрная
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Opacity = 0.5 // Полупрозрачная тень
                    };

                    // Измеряем размер текста тени
                    shadowText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    shadowText.Arrange(new Rect(shadowText.DesiredSize));

                    // Устанавливаем позицию тени с небольшим смещением
                    Canvas.SetLeft(shadowText, textX + i * 2); // Смещение по X
                    Canvas.SetTop(shadowText, 25 + j * 2);    // Смещение по Y

                    // Добавляем тень на Canvas
                    canvas.Children.Add(shadowText);
                }
            }

            // Добавляем основной текст поверх теней
            canvas.Children.Add(dayOfWeekText);


            // Рисуем деления на циферблате
            for (int i = 0; i < 12; i++)
            {
                double angle = i * 30 * (Math.PI / 180);
                int x1 = CENTER_X + (int)((RADIUS - 3) * Math.Sin(angle));
                int y1 = CENTER_Y - (int)((RADIUS - 3) * Math.Cos(angle));
                int x2 = CENTER_X + (int)((RADIUS - 7) * Math.Sin(angle));
                int y2 = CENTER_Y - (int)((RADIUS - 7) * Math.Cos(angle));

                // Рисуем толстую чёрную линию
                Line tick = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = Brushes.Black,
                    StrokeThickness = 6,
                    StrokeStartLineCap = PenLineCap.Round, // Закруглённые концы (начало линии)
                    StrokeEndLineCap = PenLineCap.Round    // Закруглённые концы (конец линии)
                };
                canvas.Children.Add(tick); // Добавляем линию в Canvas

                // рисуем тонкую белую линию
                Line tick2 = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = Brushes.White,
                    StrokeThickness = 4,
                    StrokeStartLineCap = PenLineCap.Round, // Закруглённые концы (начало линии)
                    StrokeEndLineCap = PenLineCap.Round    // Закруглённые концы (конец линии)
                };
                canvas.Children.Add(tick2); // Добавляем линию в Canvas

            }

            // Получаем текущее время
            DateTime now = DateTime.Now;

            // Рисуем часовую стрелку чёрную толстую
            double hourAngle = (now.Hour % 12 + now.Minute / 60.0) * 30 * (Math.PI / 180);
            int hourX = CENTER_X + (int)((RADIUS - 35) * Math.Sin(hourAngle));
            int hourY = CENTER_Y - (int)((RADIUS - 35) * Math.Cos(hourAngle));
            Line hourHand = new Line
            {
                X1 = CENTER_X,
                Y1 = CENTER_Y,
                X2 = hourX,
                Y2 = hourY,
                Stroke = Brushes.Black,
                StrokeThickness = 5,
                StrokeStartLineCap = PenLineCap.Round, // Закруглённый конец в начале
                StrokeEndLineCap = PenLineCap.Round   // Закруглённый конец в конце
            };
            canvas.Children.Add(hourHand);

            // Рисуем часовую стрелку белую тонкую
            double hourAngle2 = (now.Hour % 12 + now.Minute / 60.0) * 30 * (Math.PI / 180);
            int hourX2 = CENTER_X + (int)((RADIUS - 36) * Math.Sin(hourAngle2));
            int hourY2 = CENTER_Y - (int)((RADIUS - 36) * Math.Cos(hourAngle2));
            Line hourHand2 = new Line
            {
                X1 = CENTER_X,
                Y1 = CENTER_Y,
                X2 = hourX2,
                Y2 = hourY2,
                Stroke = Brushes.White,
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round, // Закруглённый конец в начале
                StrokeEndLineCap = PenLineCap.Round   // Закруглённый конец в конце
            };
            canvas.Children.Add(hourHand2);

            // Рисуем минутную стрелку чёрную толстую
            double minuteAngle = now.Minute * 6 * (Math.PI / 180);
            int minuteX = CENTER_X + (int)((RADIUS - 16) * Math.Sin(minuteAngle));
            int minuteY = CENTER_Y - (int)((RADIUS - 16) * Math.Cos(minuteAngle));
            Line minuteHand = new Line
            {
                X1 = CENTER_X,
                Y1 = CENTER_Y,
                X2 = minuteX,
                Y2 = minuteY,
                Stroke = Brushes.Black,
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round, // Закруглённый конец в начале
                StrokeEndLineCap = PenLineCap.Round   // Закруглённый конец в конце
            };
            canvas.Children.Add(minuteHand);

            // Рисуем минутную стрелку белую тонкую
            double minuteAngle2 = now.Minute * 6 * (Math.PI / 180);
            int minuteX2 = CENTER_X + (int)((RADIUS - 17) * Math.Sin(minuteAngle2));
            int minuteY2 = CENTER_Y - (int)((RADIUS - 17) * Math.Cos(minuteAngle2));
            Line minuteHand2 = new Line
            {
                X1 = CENTER_X,
                Y1 = CENTER_Y,
                X2 = minuteX2,
                Y2 = minuteY2,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                StrokeStartLineCap = PenLineCap.Round, // Закруглённый конец в начале
                StrokeEndLineCap = PenLineCap.Round   // Закруглённый конец в конце
            };
            canvas.Children.Add(minuteHand2);
        }

        // Метод для рисования конусообразной стрелки
        private void DrawConicalHand(double angle, int length, int baseWidth, Brush color)
        {
            // Координаты конца стрелки
            int endX = CENTER_X + (int)(length * Math.Sin(angle));
            int endY = CENTER_Y - (int)(length * Math.Cos(angle));

            // Координаты основания стрелки (широкая часть)
            int baseX1 = CENTER_X + (int)(baseWidth * Math.Cos(angle));
            int baseY1 = CENTER_Y + (int)(baseWidth * Math.Sin(angle));
            int baseX2 = CENTER_X - (int)(baseWidth * Math.Cos(angle));
            int baseY2 = CENTER_Y - (int)(baseWidth * Math.Sin(angle));

            // Создаём Polygon для стрелки
            Polygon hand = new Polygon
            {
                Points = new PointCollection
        {
            new Point(endX, endY), // Острый конец стрелки
            new Point(baseX1, baseY1), // Основание стрелки (широкая часть)
            new Point(baseX2, baseY2)  // Основание стрелки (широкая часть)
        },
                Fill = color, // Заливка стрелки
                Stroke = Brushes.Black, // Обводка стрелки
                StrokeThickness = 1
            };

            // Добавляем стрелку на Canvas
            canvas.Children.Add(hand);
        }

        private void SaveWindowPosition()
        {
            // Сохраняем положение окна в файл
            File.WriteAllText("window_position.txt", $"{Left} {Top}");
        }

        private void LoadWindowPosition()
        {
            // Загружаем положение окна из файла
            if (File.Exists("window_position.txt"))
            {
                string[] position = File.ReadAllText("window_position.txt").Split(' ');
                if (position.Length == 2 && double.TryParse(position[0], out double left) && double.TryParse(position[1], out double top))
                {
                    Left = left;
                    Top = top;
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Начинаем перемещение окна
            isDragging = true;
            dragStartPoint = e.GetPosition(this);
            CaptureMouse();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            // Перемещаем окно
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Left += currentPosition.X - dragStartPoint.X;
                Top += currentPosition.Y - dragStartPoint.Y;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Заканчиваем перемещение окна
            isDragging = false;
            ReleaseMouseCapture();
            SaveWindowPosition();
        }
    }
}