# gyroscope
try to fast relocate code
using System;
using System.IO.Ports;
using System.Runtime.InteropServices;

class Program
{
    const int DMDO_DEFAULT = 0; // 0 degrees
    const int DMDO_90 = 1;      // 90 degrees
    const int DMDO_180 = 2;     // 180 degrees
    const int DMDO_270 = 3;     // 270 degrees

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwFlags, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
    }

    static void Main(string[] args)
    {
        // Подключение к последовательному порту
        using (SerialPort serialPort = new SerialPort("COM3", 9600))
        {
            try
            {
                serialPort.Open();
                Console.WriteLine("Порт открыт. Нажмите 'Esc' для выхода.");

                while (true)
                {
                    // Проверка нажатия клавиши Escape для выхода
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("Выход из программы.");
                        break; // Выход из цикла
                    }

                    try
                    {
                        string data = serialPort.ReadLine();
                        Console.WriteLine("Получено: " + data);

                        // Обработка полученных данных для изменения ориентации экрана
                        if (data.Contains("Landscape"))
                        {
                            ChangeScreenOrientation(DMDO_DEFAULT); // Портретная ориентация
                        }
                        else if (data.Contains("Portrait"))
                        {
                            ChangeScreenOrientation(DMDO_90); // Альбомная ориентация
                        }
                    }
                    catch (TimeoutException)
                    {
                        // Обработка таймаута при чтении данных
                        Console.WriteLine("Таймаут при чтении данных.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка при чтении из порта: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при открытии порта: " + ex.Message);
            }
            finally
            {
                // Порт будет закрыт автоматически при выходе из блока using
                Console.WriteLine("Порт закрыт.");
            }
        }
    }

    public static void ChangeScreenOrientation(int orientation)
    {
        DEVMODE dm = new DEVMODE();
        dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
        dm.dmFields = 0x00000001 | 0x00000002; // DM_DISPLAYORIENTATION | DM_PELSWIDTH | DM_PELSHEIGHT
        dm.dmDisplayOrientation = orientation;
        dm.dmPelsWidth = 2880; // Укажите ваше текущее разрешение экрана
        dm.dmPelsHeight = 1800; // Укажите ваше текущее разрешение экрана
        dm.dmDisplayFrequency = 90; 

        int result = ChangeDisplaySettingsEx(null, ref dm, IntPtr.Zero, 0, IntPtr.Zero);
        if (result == 0)
        {
            Console.WriteLine("Ориентация экрана изменена");
        }
        else
        {   
            Console.WriteLine("Ошибка при изменении ориентации: " + result);
        }
    }
}
