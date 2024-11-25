using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("user32.dll")]
    private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

    private const int DMDO_DEFAULT = 0;
    private const int DMDO_90 = 1;
    private const int DMDO_180 = 2;
    private const int DMDO_270 = 3;
    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int CDS_UPDATEREGISTRY = 0x01;

    [StructLayout(LayoutKind.Sequential)]
    private struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
    }

    static bool isRunning = true;  // Флаг для управления завершением работы потока

    static void Main()
    {
        // Запуск фонового потока для работы с Arduino
        Thread backgroundThread = new Thread(BackgroundTask);
        backgroundThread.IsBackground = true;
        backgroundThread.Start();

        // Программа теперь будет продолжать работать, пока не получит команду на завершение
        while (isRunning)
        {
            // Ожидание завершения работы потока
            Thread.Sleep(1000);  // Задержка, чтобы не перегружать процессор
        }
    }

    // Фоновая задача для получения данных с Arduino и поворота экрана
    private static void BackgroundTask()
    {
        // Настройка порта
        SerialPort serialPort = new SerialPort("COM3", 9600); // Укажите ваш COM порт
        serialPort.DataReceived += SerialPort_DataReceived;
        serialPort.Open();

        // Ожидание получения данных с Arduino
        while (isRunning)
        {
            // Данные будут обрабатываться в обработчике события SerialPort_DataReceived
            Thread.Sleep(100); // Задержка для предотвращения чрезмерной загрузки процессора
        }

        // Закрытие порта после завершения работы
        serialPort.Close();
    }

    // Обработчик события получения данных с Arduino
    private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var serialPort = sender as SerialPort;
        string data = serialPort.ReadLine().Trim();

        if (int.TryParse(data, out int angle))
        {
            RotateScreen(angle);
        }
    }

    // Логика для поворота экрана
    private static void RotateScreen(int angle)
    {
        int orientation = angle switch
        {
            0 => DMDO_DEFAULT,
            90 => DMDO_90,
            180 => DMDO_180,
            270 => DMDO_270,
            _ => DMDO_DEFAULT
        };

        DEVMODE dm = new DEVMODE();
        dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

        // Получаем текущие настройки дисплея
        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
        {
            dm.dmDisplayOrientation = orientation;

            if (orientation == DMDO_90 || orientation == DMDO_270)
            {
                dm.dmPelsWidth = 2160;
                dm.dmPelsHeight = 3840;
            }
            else
            {
                dm.dmPelsWidth = 3840;
                dm.dmPelsHeight = 2160;
            }

            dm.dmFields |= 0x80000 | 0x100000;

            // Применяем изменения
            ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
        }
    }
}
