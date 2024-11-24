using System;  // База .NET
using System.IO.Ports; // Работа с COM-портом для общения с ардуино
using System.Runtime.InteropServices; // Для вызова функций Windows API (sys call и внешн библиотека user32.dll)
using System.Threading; // Работа с потоками, для искл. терминала

class Program
{
    // Импорт функций Windows API для управления экраном
    [DllImport("user32.dll")]  // Атрибут для вызова внешн. библиотек
    private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);  // Получение текущих натсроек экрана

    [DllImport("user32.dll")]
    private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);  // Изменение параметров экрана.

    // Константы для ориентации экрана
    private const int DMDO_DEFAULT = 0;
    private const int DMDO_90 = 1;
    private const int DMDO_180 = 2;
    private const int DMDO_270 = 3;
    private const int ENUM_CURRENT_SETTINGS = -1; // Текущие настройки экрана
    private const int CDS_UPDATEREGISTRY = 0x01;  // Обновление в реестре (Change Display Setting)
    // Структура для работы с настройками экрана
    [StructLayout(LayoutKind.Sequential)]  // Указывает, как структура должна быть упорядочена в памяти
    private struct DEVMODE  // Хранит все настройки экран
    { 
        private const int CCHDEVICENAME = 32; // Максимальная длина имени устройства
        private const int CCHFORMNAME = 32;  // Максимальная длина формы

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]  // Как поля структуры должны быть преобразованы для вызовов Windows API.
        public string dmDeviceName;  // Имя устройства
        public short dmSpecVersion;         // Версия спецификации
        public short dmDriverVersion;       // Версия драйвера
        public short dmSize;                // Размер структуры
        public short dmDriverExtra;         // Дополнительные данные драйвера
        public int dmFields;                // Поля для изменения настроек

        public int dmPositionX;             // Позиция по оси X
        public int dmPositionY;             // Позиция по оси Y
        public int dmDisplayOrientation;    // Ориентация экрана
        public int dmDisplayFixedOutput;    // Фиксированная ориентация

        public short dmColor;               // Цветовые настройки
        public short dmDuplex;              // Дуплексный режим
        public short dmYResolution;         // Разрешение по оси Y
        public short dmTTOption;            // Настройки TrueType
        public short dmCollate;             // Настройки компоновки
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]  // Хуй пойми зачем 2 раза, я устад все блять писать, я блять эту рутину тупую через ГПТ закину, пусть он закоменит, я подредачу
        public string dmFormName;           // Имя формы
        public short dmLogPixels;           // Логические пиксели
        public int dmBitsPerPel;            // Бит на пиксель
        public int dmPelsWidth;             // Ширина экрана
        public int dmPelsHeight;            // Высота экрана
        public int dmDisplayFlags;          // Флаги дисплея
        public int dmDisplayFrequency;      // Частота обновления
    }

    // Флаг для управления завершением программы
    static bool isRunning = true;

    static void Main()
    {
        // Запуск фонового потока для работы с Arduino
        Thread backgroundThread = new Thread(BackgroundTask)    // Создаётся новый поток для выполнения задачи BackgroundTask, чтобы программа могла продолжать работать, не блокируя основной поток.
        {
            IsBackground = true // Поток завершится при завершении основного приложения
        };
        backgroundThread.Start();    // Запускает поток, где будет выполняться чтение данных с Arduino.

        // Сообщение пользователю о работе программы
        Console.WriteLine("Программа работает. Нажмите 'Ctrl+C' для завершения.");
        
        // Основной цикл программы, работает до изменения флага `isRunning`
        while (isRunning)
        {
            Thread.Sleep(1000); // Задержка для снижения нагрузки на процессор
        }
    }

    // Фоновая задача для работы с Arduino
    private static void BackgroundTask()
    {
        // Настройка COM-порта
        SerialPort serialPort = new SerialPort("COM3", 9600);
        serialPort.DataReceived += SerialPort_DataReceived; // Привязываем обработчик события
        serialPort.Open(); // Открываем порт

        // Цикл ожидания завершения работы
        while (isRunning)
        {
            Thread.Sleep(100); // Задержка, чтобы избежать перегрузки процессора
        }

        // Закрываем порт при завершении работы
        serialPort.Close();
        Console.WriteLine("Порт закрыт.");
    }

    // Обработчик события получения данных
    private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var serialPort = sender as SerialPort;  // Получаем ссылку на объект порта, который вызвал событие
        string data = serialPort.ReadLine().Trim(); // Чтение строки из порта и удаление лишних пробелов

        if (int.TryParse(data, out int angle)) // Преобразование строки в число (угол)
        {
            RotateScreen(angle); // Поворот экрана на заданный угол
        }
    }

    // Функция для поворота экрана
    private static void RotateScreen(int angle)
    {
        // Преобразование угла в ориентацию
        int orientation = angle switch
        {
            0 => DMDO_DEFAULT,
            90 => DMDO_90,
            180 => DMDO_180,
            270 => DMDO_270,
            _ => DMDO_DEFAULT // Если угол некорректен, оставляем ориентацию по умолчанию
        };

        DEVMODE dm = new DEVMODE(); // Создаём структуру для настроек экрана
        dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)); // Устанавливаем размер структуры

        // Получаем текущие настройки экрана
        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
        {
            dm.dmDisplayOrientation = orientation; // Устанавливаем ориентацию

            // Изменяем ширину и высоту экрана для портретной ориентации
            if (orientation == DMDO_90 || orientation == DMDO_270)
            {
                dm.dmPelsWidth = 1800;  // Книжная ширина
                dm.dmPelsHeight = 2880; // Книжная высота
            }
            else
            {
                dm.dmPelsWidth = 2880; // Альбомная ширина
                dm.dmPelsHeight = 1800; // Альбомная высота
            }

            dm.dmFields |= 0x80000 | 0x100000; // Указываем, что меняем размеры экрана

            // Применяем изменения к ориентации экрана
            ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
        }
    }
}
