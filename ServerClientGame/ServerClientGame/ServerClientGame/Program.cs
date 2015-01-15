using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace ServerClientGame
{
#if WINDOWS || XBOX
    static class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

        static Game1 Game;


        static void Main(string[] args)
        {
            Game = Game1.MakeGame(args);
            
            using (Game)
                Game.Run();

#if DEBUG
            if (OtherProcess != null && Debugger.IsAttached && !OtherProcess.HasExited)
                OtherProcess.Kill();
#endif
            Game.Exit();
        }

#if DEBUG
        static Process OtherProcess;
        internal static void StartSecondProcess()
        {
            if (Debugger.IsAttached)
            {
                Console.Title = "Debugging Window";
                Console.WriteLine("Debugging");

                OtherProcess = Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"ServerClientGame.exe", "client");
            }
            else
                Console.Title = "Main Window";
        }
#endif

    }
#endif
}

