using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    static extern int SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    static extern int ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    static void Main()
    {
        // Ajustar tamaño y posición de la ventana de la consola
        IntPtr consoleWindow = GetConsoleWindow();
        MoveWindow(consoleWindow, 100, 100, 1100, 900, true);

        // Mostrar introducción
        ShowIntroduction();

        bool continuar = true;

        while (continuar)
        {
            Console.WriteLine("Ingrese el PID del programa que desea pausar:");
            int pid = int.Parse(Console.ReadLine());

            try
            {
                Process process = Process.GetProcessById(pid);

                if (process != null)
                {
                    Console.WriteLine($"Pausando el programa con PID: {pid}");
                    IntPtr processHandle = OpenProcess(0x1F0FFF, false, pid); // 0x1F0FFF = Todos los derechos necesarios para suspender/reanudar
                    foreach (ProcessThread thread in process.Threads)
                    {
                        IntPtr threadHandle = OpenThread(0x2, false, (uint)thread.Id); // 0x2 = Derecho de suspensión/resume
                        SuspendThread(threadHandle);
                        CloseHandle(threadHandle);
                    }
                    CloseHandle(processHandle);
                    Console.WriteLine("El programa ha sido pausado exitosamente.");

                    Console.WriteLine("¿Desea restaurar el programa? [Y/N]");
                    char respuesta = Console.ReadKey().KeyChar;

                    if (respuesta == 'Y' || respuesta == 'y')
                    {
                        process.Refresh();
                        foreach (ProcessThread thread in process.Threads)
                        {
                            IntPtr threadHandle = OpenThread(0x2, false, (uint)thread.Id);
                            ResumeThread(threadHandle);
                            CloseHandle(threadHandle);
                        }
                        Console.WriteLine("El programa ha sido restaurado exitosamente.");
                    }
                }
                else
                {
                    Console.WriteLine("No se encontró un programa con el PID especificado.");
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("El PID especificado no es válido.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error al pausar/restaurar el programa: {ex.Message}");
            }

            Console.WriteLine("¿Desea continuar con el programa? [Y/N]");
            char continuarRespuesta = Console.ReadKey().KeyChar;

            if (continuarRespuesta != 'Y' && continuarRespuesta != 'y')
            {
                continuar = false;
            }

            Console.Clear();
        }

        Console.WriteLine("Presione cualquier tecla para salir.");
        Console.ReadKey();
    }

    static void ShowIntroduction()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        string logo = @"
      :::    :::     :::       :::   :::    :::::::: ::::::::::: :::::::::: :::::::::  ::::::::::: ::::::::  :::    :::  :::::::: 
     :+:    :+:   :+: :+:    :+:+: :+:+:  :+:    :+:    :+:     :+:        :+:    :+:     :+:    :+:    :+: :+:    :+: :+:    :+: 
    +:+    +:+  +:+   +:+  +:+ +:+:+ +:+ +:+           +:+     +:+        +:+    +:+     +:+    +:+    +:+ +:+    +:+ +:+         
   +#++:++#++ +#++:++#++: +#+  +:+  +#+ +#++:++#++    +#+     +#++:++#   +#++:++#:      +#+    +#+    +:+ +#+    +:+ +#++:++#++   
  +#+    +#+ +#+     +#+ +#+       +#+        +#+    +#+     +#+        +#+    +#+     +#+    +#+    +#+ +#+    +#+        +#+    
 #+#    #+# #+#     #+# #+#       #+# #+#    #+#    #+#     #+#        #+#    #+#     #+#    #+#    #+# #+#    #+# #+#    #+#     
###    ### ###     ### ###       ###  ########     ###     ########## ###    ### ########### ########   ########   ########       ";

        string[] colors = { "Red", "Green", "Blue" };
        int colorIndex = 0;
        for (int i = 0; i < logo.Length; i++)
        {
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colors[colorIndex]);
            Console.Write(logo[i]);

            if (logo[i] != ' ' && logo[i] != '\n' && logo[i] != '\r')
            {
                Thread.Sleep(10);
                colorIndex = (colorIndex + 1) % colors.Length;
            }
        }
        Console.ResetColor();
        Console.WriteLine("\n\n¡Bienvenido al programa de pausa y restauración de procesos!\n");
        Console.WriteLine("Este programa te permite pausar y restaurar la ejecución de un proceso mediante su PID.\n");
        Console.WriteLine("A continuación, podrás ingresar el PID del programa que deseas pausar.\n");
        Console.WriteLine("Presiona cualquier tecla para continuar...");
        Console.ReadKey();
        Console.Clear();
    }
}
