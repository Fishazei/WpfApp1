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
        public double xcur, ycur;
        double l, m;
        double force;

        public Line FLine { get; set; }
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
            MyRectangle.Fill = new SolidColorBrush(Colors.IndianRed);
            Canvas.SetTop(MyRectangle, y0 - 10);
            Canvas.SetLeft(MyRectangle, x0 - 10);

            //Задание лески
            FLine = new Line();
            FLine.X1 = x0;
            FLine.Y1 = y0;
            FLine.X2 = x0;
            FLine.Y2 = y0;
            FLine.StrokeThickness = 4;
            FLine.Stroke = new SolidColorBrush(Colors.AliceBlue);

            myCanvas.Children.Add(MyRectangle);
            myCanvas.Children.Add(FLine);
            // Вычисление силы вылета в зависимости от времени зажатия
            force = Math.Min(100, heldTime.TotalMilliseconds / 800);

            MakeUP();
        }

        public bool MakeUP()
        {
            double f = 0;
            while (f < force)
            {
                f += 0.001;
                xcur = l * f - 10 + x0;
                ycur = m * f - 10 + y0;

                Canvas.SetTop(MyRectangle, ycur);
                Canvas.SetLeft(MyRectangle, xcur);

                FLine.X2 = l * f + x0;
                FLine.Y2 = m * f + y0;
            }

            return true;
        }

        public void Move0(double currentTop, double currentLeft)
        {
            y0 = currentLeft + 50;
            x0 = currentTop + 50;

            FLine.X1 = x0;
            FLine.Y1 = y0;

            l = xcur - 10 - x0;
            m = ycur - 10 - y0;
        }

        public void GoBack()
        {
            xcur -= l * force * 0.01;
            ycur -= m * force * 0.01;

            FLine.X2 -= l * force * 0.01;
            FLine.Y2 -= m * force * 0.01;

            Canvas.SetTop(MyRectangle, ycur);
            Canvas.SetLeft(MyRectangle, xcur);

            CheckMePLS(xcur, ycur);
        }

        public void DeleteMePLS()
        {
            myCanvas.Children.Remove(MyRectangle);
            myCanvas.Children.Remove(FLine);
            myCanvas = null;

        }
        //Событие 
        public delegate void Handler(double xcur, double ycur);
        public event Handler CheckMePLS;
    }
    /// <summary>
    /// Interaction logic for GlobalMap.xaml
    /// </summary>
    public partial class GlobalMap : Page
    {
        private const int TimerInterval = 16; // Примерно 60 FPS
        private const int MoveSpeed = 5;      // Скорость персонажа

        private DateTime keyDownTime;   // Длительность удерживания клавишы
        private DateTime cooldown;      // Время начала отсчёта кулдауна
        private bool isSpaceKeyDown;    // Зажат ли пробел
        private bool isThrHook;         // Закинут ли крючок

        MapGenerator mapGenerator;  // Генератор карты и сама карта
        private HookMap myHook;     // Крючок и леска

        private readonly DispatcherTimer timer; // Общеигровой таймер

        int points; // Очки
        int tryAvaible;    // Общее количество оставшихся попыток, изначально пять
        int games;  // Сколько осталось игр, изначально три

        public GlobalMap()
        {
            InitializeComponent();

            CreateTexture();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };

            points = 0;
            tryAvaible = 5;
            games = 3;

            cooldown = DateTime.Now;
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

            if (Keyboard.IsKeyDown(Key.W) && currentTop - MoveSpeed > 76)
            {
                Canvas.SetTop(movingRectangle, currentTop - MoveSpeed);
                if (myHook != null) myHook.Move0(Canvas.GetLeft(movingRectangle), Canvas.GetTop(movingRectangle));
            }
            if (Keyboard.IsKeyDown(Key.S) && currentTop + movingRectangle.Width + MoveSpeed < 655)
            {
                Canvas.SetTop(movingRectangle, currentTop + MoveSpeed);
                if (myHook != null) myHook.Move0(Canvas.GetLeft(movingRectangle), Canvas.GetTop(movingRectangle));
            }
            if (Keyboard.IsKeyDown(Key.D) && currentLeft + movingRectangle.Width + MoveSpeed < 170)
            {
                Canvas.SetLeft(movingRectangle, currentLeft + MoveSpeed);
                if (myHook != null) myHook.Move0(Canvas.GetLeft(movingRectangle), Canvas.GetTop(movingRectangle));
            }
            if (Keyboard.IsKeyDown(Key.A) && currentLeft - MoveSpeed > 10)
            {
                Canvas.SetLeft(movingRectangle, currentLeft - MoveSpeed);
                if (myHook != null) myHook.Move0(Canvas.GetLeft(movingRectangle), Canvas.GetTop(movingRectangle));
            }
            //Начали замахиваться
            if (Keyboard.IsKeyDown(Key.Space) && !isSpaceKeyDown && !isThrHook && ((DateTime.Now - cooldown).TotalSeconds > 2))
            {
                isSpaceKeyDown = true;
                keyDownTime = DateTime.Now;
            }
            //Тянем назад
            if (isThrHook && Keyboard.IsKeyDown(Key.Space))
            {
                myHook.GoBack();
            }
            //Опустили крючок и начали рыбачить
            if (isThrHook && Keyboard.IsKeyDown(Key.E))
            {
                int i, j;
                for (i = 0; myHook.ycur - 45 * i > 61; i++) ;
                for (j = 0; myHook.xcur - 45 * j > 165; j++) ;

                if (i != 0 && j != 0 && i < 15 && j < 23)
                {
                    games--;
                    tryAvaible--;
                    //Запуск игры с настройками в зависимости от mapGenerator.matrix[i-1,j-1]
                    MessageBox.Show($"m[{i-1},{j-1}] = {mapGenerator.matrix[i-1,j-1]}");


                    //Завершение
                    if (games == 0 || tryAvaible == 0) ExitF();
                }
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

                myHook.CheckMePLS += Cheker;
            }
        }

        //Проверка крюка на все венерические 
        void Cheker(double x, double y)
        {
            double currentLeft = Canvas.GetLeft(movingRectangle);
            double currentTop = Canvas.GetTop(movingRectangle);
           
            //Проверка на полное сматывание удочки
            if (x + 20 > currentLeft && x < currentLeft + 100)
                if (y + 20 > currentTop && y < currentTop + 100)
                {
                    myHook.CheckMePLS -= Cheker;
                    isThrHook = false;
                    myHook.DeleteMePLS();

                    //Кулдаун на пробел
                    cooldown = DateTime.Now;
                }
            //Проверка на попадание на корягу
            int i, j;
            for (i = 0; myHook.ycur + 10 - 45 * i > 61; i++) ;
            for (j = 0; myHook.xcur + 10 - 45 * j > 165; j++) ;

            if (i != 0 && j != 0 && i < 15 && j < 23)
                if (mapGenerator.matrix[i - 1, j - 1] == -1)
                {
                    myHook.CheckMePLS -= Cheker;
                    isThrHook = false;
                    myHook.DeleteMePLS();

                    //Кулдаун на пробел
                    cooldown = DateTime.Now;

                    tryAvaible--;
                    if (tryAvaible == 0) ExitF();

                    tryAvaibleTB.Text = "Попыток " + tryAvaible.ToString() + '/' + games.ToString();
                }

        }
        private void ExitF()
        {
            GameOver(points);
            //Дописать выход
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
            mapGenerator = new MapGenerator(13,21);
            // Устанавливаем текстуру как источник изображения для Image
            matrixImage.Source = mapGenerator.CreateTexture();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //Переход в главное меню
            ExitF();
        }

        private void Manual_Click(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("WASD - движение пероснажа; SPACE - забрасывание/подтягивание поплавка; E(У) - начало поклёва;\n" +
                            "Чем темнее клета - тем больше рыбы. Коричневые клетки - коряги, об которые тратятся жизни.\n" +
                            "Всего есть 5 жизней, максимум можно прорыбачить 3 раза, остальные - запасные! Удачной игры!\n");
        }

        public delegate void Handler(int alpha);
        public event Handler GameOver;
    }
}
