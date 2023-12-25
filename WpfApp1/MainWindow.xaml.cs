using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    class MapGenerator
    {
        public int[,] matrix;

        public MapGenerator(int x = 30, int y = 40) 
        {
            matrix = Generate(x, y);
        }

        /// <summary>
        /// Генерирование карты
        /// </summary>
        /// <param name="x">Размер по горизонтале</param>
        /// <param name="y">Размер по вертикале</param>
        /// <returns>Возращает матрицу int с размерами x+2 y+2, рабочей частью является область от [1,1] до [x+1,y+1]</returns>
        private int[,] Generate(int x, int y)
        {
            int tile;
            Random random = new Random();

            //Вычисление итоговой матрицы
            int[,] RMap = new int[x + 2, y + 2];
            for (int i = 1; i < x + 1; i++)
                for (int j = 1; j < y + 1; j++)
                {
                    if (RMap[i, j] == 0)
                    {
                        tile = random.Next(100);
                        RMap[i, j] = tile <= 5 && j < y ? 9 : tile <= 40 && tile >= 30 ? 6 : tile <= 25 && tile >= 10 ? 3 : 0;

                        //Установка коряг
                        if (RMap[i, j] == 9)
                        {
                            RMap[i, j + 1] = -1;

                            RMap[i - 1, j - 1] = random.Next(20) <= 2 ? -1 : RMap[i - 1, j - 1];
                            RMap[i - 1, j]     = random.Next(20) <= 2 ? -1 : RMap[i - 1, j];
                            RMap[i - 1, j + 1] = random.Next(20) <= 2 ? -1 : RMap[i - 1, j + 1];
                            RMap[i, j - 1]     = random.Next(20) <= 1 ? -1 : RMap[i, j - 1];
                            RMap[i, j + 1]     = random.Next(20) <= 2 ? -1 : RMap[i, j + 1];
                            RMap[i + 1, j - 1] = random.Next(20) <= 2 ? -1 : RMap[i + 1, j - 1];
                            RMap[i + 1, j + 1] = random.Next(20) <= 2 ? -1 : RMap[i + 1, j + 1];
                        }
                    }
                }
            //Сглаживание
            for (int i = 1; i < x + 1; i++)
                for (int j = 1; j < y + 1; j++)
                {
                    if (RMap[i, j] == 0)
                    {
                        RMap[i, j] = Math.Abs((RMap[i + 1, j] + RMap[i, j + 1] + RMap[i - 1, j] + RMap[i, j - 1]) / 3);
                    }
                }
            for (int i = 1; i < x + 1; i++)
            {
                RMap[i, 0] = Math.Abs(RMap[i, 1] + RMap[i + 1, 1] + RMap[i - 1, 1]) / 3;
                RMap[i, y+1] = Math.Abs(RMap[i, y] + RMap[i + 1, y] + RMap[i - 1, y]) / 3;
            }
            for (int i = 1; i < y + 1; i++)
            {
                RMap[0, i] = Math.Abs(RMap[1, i] + RMap[1,i + 1] + RMap[1, i - 1]) / 3;
                RMap[x+1, i] = Math.Abs(RMap[x, i] + RMap[x, i + 1] + RMap[x, i - 1]) / 3;
            }
            return RMap;
        }

        /// <summary>
        /// Создание текстуры водоёма
        /// </summary>
        /// <param name="PixelBlockSize">масштабирование, по умолчанию один квадрат - 50 пикселей</param>
        /// <returns>Возвращает текстуру водоёма</returns>
        public WriteableBitmap CreateTexture(int PixelBlockSize = 20)
        {

            // Размеры текстуры в пикселях, умноженные на размер блока
            int textureWidth = matrix.GetLength(1) * PixelBlockSize;
            int textureHeight = matrix.GetLength(0) * PixelBlockSize;

            // Создаем WriteableBitmap для текстуры
            WriteableBitmap texture = new WriteableBitmap(textureWidth, textureHeight, 96, 96, PixelFormats.Bgr32, null);

            // Заполняем текстуру на основе значений матрицы
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    int value = matrix[i, j];
                    Color color = GetColor(value);

                    // Заполняем область nxn пикселей цветом
                    for (int y = 0; y < PixelBlockSize; y++)
                    {
                        for (int x = 0; x < PixelBlockSize; x++)
                        {
                            int pixelX = j * PixelBlockSize + x;
                            int pixelY = i * PixelBlockSize + y;

                            // Задаем цвет пикселя в текстуре
                            texture.WritePixels(new Int32Rect(pixelX, pixelY, 1, 1), new byte[] { color.B, color.G, color.R, 255 }, 4, 0);
                        }
                    }
                }
            }

            return texture;
        }

        //Генерация цвета
        private Color GetColor(int value)
        {
            Color color;
            if (value == -1) color = Colors.Brown;
            else
            {
                color.A = 255;
                color.B = (byte)(-4.4 * value + 171);
                color.G = (byte)(-3.3 * value + 130);
                color.R = (byte)(-0.074 * Math.Pow(value, 3) + 0.944 * Math.Pow(value, 2) - 3.833 * value + 57);
            }
            return color;
        }
    }
    
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
 
        public MainWindow()
        {
            InitializeComponent();

            GlobalMap globalMap = new GlobalMap();

            MainFrame.Content = globalMap;
        }

    }
}