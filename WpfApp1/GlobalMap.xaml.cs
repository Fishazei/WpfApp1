using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    class HookMap
    {
        double x0, y0;
        double xcur, ycur;
        double l, m;
        double force;

        public Image MyImage { get; set; }
        public Rectangle MyRectangle { get; set; }
        Canvas myCanvas;

        public HookMap(double mouseTop, double mouseLeft, double currentTop, double currentLeft, Canvas canvas, TimeSpan heldTime)
        {
            myCanvas = canvas;
            y0 = currentLeft + 50;
            x0 = currentTop + 50;

            l = mouseTop - 10 - x0;
            m = mouseLeft - 10 - y0;

            //Задание крючка
            MyRectangle = new Rectangle();

            MyRectangle.Height = 20;
            MyRectangle.Width = 20;
            MyRectangle.Fill = new SolidColorBrush(Colors.AliceBlue);
            Canvas.SetTop(MyRectangle, y0 - 10);
            Canvas.SetLeft(MyRectangle, x0 - 10);

            myCanvas.Children.Add(MyRectangle);

            // Вычисление силы вылета в зависимости от времени зажатия
            force = Math.Min(100, heldTime.TotalMilliseconds / 800);

            //Эксперимент
            Line line = new Line();
            line.X1 = x0;
            line.Y1 = y0;
            line.X2 = l * force + x0;
            line.Y2 = m * force + y0;
            line.StrokeThickness = 4;
            line.Stroke = new SolidColorBrush(Colors.AliceBlue);

            myCanvas.Children.Add(line);
            MessageBox.Show($"{line.X2}, {line.Y2}");

            // Создание анимации перемещения
            TranslateTransform translateTransform = new TranslateTransform();
            MyRectangle.RenderTransform = translateTransform;

            DoubleAnimation animationX = new DoubleAnimation
            {
                From = 0,
                To = l * force,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            DoubleAnimation animationY = new DoubleAnimation
            {
                From= 0,
                To = m * force,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            animationX.IsAdditive = true;
            animationY.IsAdditive = true;

            translateTransform.BeginAnimation(TranslateTransform.XProperty, animationX);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, animationY);

            xcur = l * force - 10 + x0;
            ycur = m * force - 10 + y0;


            
        }

        public void GoBack()
        {
            
            xcur -= l * force * 0.01;
            ycur -= m * force * 0.01;

            MessageBox.Show($"{Canvas.GetTop(MyRectangle)}, {Canvas.GetLeft(MyRectangle)}");

            Canvas.SetTop(MyRectangle, ycur);
            Canvas.SetLeft(MyRectangle, xcur);

            MessageBox.Show($"{Canvas.GetTop(MyRectangle)}, {Canvas.GetLeft(MyRectangle)}");
        }
    }
    /// <summary>
    /// Interaction logic for GlobalMap.xaml
    /// </summary>
    public partial class GlobalMap : Page
    {
        private const int TimerInterval = 16; // Примерно 60 FPS
        private const int MoveSpeed = 5;

        private DateTime keyDownTime;
        private bool isSpaceKeyDown;
        private bool isThrHook;

        private HookMap myHook;

        private readonly DispatcherTimer timer;

        public GlobalMap()
        {
            InitializeComponent();

            CreateTexture();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            timer.Tick += MovePlayer;
            timer.Tick += HookThr;
            isSpaceKeyDown = false;
            isThrHook = false;
            Loaded += YourPage_Loaded;
            timer.Start();
        }

        private async void YourPage_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }
        public void MovePlayer(object sender, EventArgs e)
        {
            double currentLeft = Canvas.GetLeft(movingRectangle);
            double currentTop = Canvas.GetTop(movingRectangle);

            if (!isThrHook && Keyboard.IsKeyDown(Key.W) && currentTop - MoveSpeed > 100)
            {
                Canvas.SetTop(movingRectangle, currentTop - MoveSpeed);
            }
            if (!isThrHook && Keyboard.IsKeyDown(Key.S) && currentTop + movingRectangle.Width + MoveSpeed < 655)
            {
                Canvas.SetTop(movingRectangle, currentTop + MoveSpeed);
            }
            if (!isThrHook && Keyboard.IsKeyDown(Key.D) && currentLeft + movingRectangle.Width + MoveSpeed < 185)
            {
                Canvas.SetLeft(movingRectangle, currentLeft + MoveSpeed);
            }
            if (!isThrHook && Keyboard.IsKeyDown(Key.A) && currentLeft - MoveSpeed > 10)
            {
                Canvas.SetLeft(movingRectangle, currentLeft - MoveSpeed);
            }
            //Начали замахиваться
            if (Keyboard.IsKeyDown(Key.Space) && !isSpaceKeyDown)
            {
                isSpaceKeyDown = true;
                keyDownTime = DateTime.Now;
            }
            //Тянем назад
            if (isThrHook && Keyboard.IsKeyDown(Key.E))
            {
                myHook.GoBack();
            }
        }
        //Кинули крюк
        public void HookThr(object sender, EventArgs e)
        {
            if(isSpaceKeyDown && !isThrHook && Keyboard.IsKeyUp(Key.Space))
            {
                isThrHook = true;
                isSpaceKeyDown = false;

                double currentLeft = Canvas.GetLeft(movingRectangle);
                double currentTop = Canvas.GetTop(movingRectangle);

                double mouseLeft = Mouse.GetPosition(GlobalMap1).X;
                double mouseTop = Mouse.GetPosition(GlobalMap1).Y;

                TimeSpan heldTime = DateTime.Now - keyDownTime;

                myHook = new HookMap(
                    mouseLeft,
                    mouseTop,
                    currentLeft,
                    currentTop,
                    MyCanvas,
                    heldTime
                    );
            }
        }

        private async Task GameLoop()
        {
            while (true)
            {
                await Task.Delay(10);
            }
        }

        private void CreateTexture()
        {
            MapGenerator mapGenerator = new MapGenerator(15,22);
            // Устанавливаем текстуру как источник изображения для Image
            matrixImage.Source = mapGenerator.CreateTexture();
        }
    }
}
