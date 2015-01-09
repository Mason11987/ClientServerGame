using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace ServerClientGame
{
#if WINDOWS || XBOX
    static class Program
    {
        static Game1 Game;


        static void Main(string[] args)
        {
            Game = Game1.MakeGame(args);
            
            using (Game)
                Game.Run();

#if DEBUG
            if (OtherProcess != null && System.Diagnostics.Debugger.IsAttached && !OtherProcess.HasExited)
                OtherProcess.Kill();
#endif
            Game.Exit();
        }

#if DEBUG
        static Process OtherProcess;
        internal static void StartSecondProcess()
        {
            if (System.Diagnostics.Debugger.IsAttached)
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

