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
            Game = GameFromArgs(args);


            using (Game)
                Game.Run();

#if DEBUG
            if (OtherProcess != null && System.Diagnostics.Debugger.IsAttached && !OtherProcess.HasExited)
                OtherProcess.Kill();
#endif

            Game.Exit();
        }

        private static Game1 GameFromArgs(string[] args)
        {
            string input = "";
            if (args.Contains("server"))
            {
                bool startsecondprocess = false;
                int port = 3000;
                input = "y";
                Console.WriteLine("Running as Server");

                if (args.Length >= 2)
                    port = Convert.ToInt32(args[1]);

#if DEBUG
                if (args.Length >= 3)
                    startsecondprocess = Convert.ToBoolean(args[2]);
                if (startsecondprocess)
                    Program.StartSecondProcess();
#endif

                return new Game1(true, port, startsecondprocess);
            }
            else if (args.Count() == 1 && args[0] == "client")
            {
                input = "n";
                Console.WriteLine("Running as Client");
                return new Game1();
            }
            else
            {
                Console.Write("Server? ");

                input = Console.ReadLine();
                if (input.ToLower().Contains("y"))
                    return new Game1(true, 3000, false);
                else
                    return new Game1();
            }


        }

        static void RunConsole()
        {
            

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

