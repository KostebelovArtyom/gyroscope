using System;
using System.IO.Ports;
using System.Runtime.InteropServices;

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

    static void Main()
    {
        SerialPort serialPort = new SerialPort("COM3", 9600);
        serialPort.DataReceived += SerialPort_DataReceived;
        serialPort.Open();

        Console.WriteLine("ГДЕ БЛЯТЬ ДАННЫЕ ГДЕ НАХУЙ");
        Console.ReadLine();
        serialPort.Close();
    }

    private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var serialPort = sender as SerialPort;

        try
        {
            string data = serialPort.ReadLine().Trim();
            Console.WriteLine($"о, сюда, чиназес: {data}");

            if (int.TryParse(data, out int angle))
            {
                RotateScreen(angle);
            }
            else
            {
                Console.WriteLine("Хуйня, а не данные: " + data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ебал всё: " + ex.Message);
        }
    }

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

        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
        {
            dm.dmDisplayOrientation = orientation;

            if (orientation == DMDO_90 || orientation == DMDO_270)
            {
                dm.dmPelsWidth = 1080;
                dm.dmPelsHeight = 1920;
            }
            else
            {
                dm.dmPelsWidth = 1920;
                dm.dmPelsHeight = 1080;
            }

            dm.dmFields |= 0x80000 | 0x100000;

            int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);

            if (result == 0)
            {
                Console.WriteLine($"УРА НАХЦЙ УРА ЭКРАН БЛЯТЬ повернут на {angle}");
            }
            else
            {
                Console.WriteLine($"Я Сосалов. Код ошибки: {result}");
            }
        }
        else
        {
            Console.WriteLine("Не удалось получить текущие настройки дисплея.");
        }
    }
}
