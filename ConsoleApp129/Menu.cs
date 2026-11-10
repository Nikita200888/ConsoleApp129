using System;

namespace ConsoleApp129
{
    public static class Menu
    {
        // Главное меню
        public static int ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=======================================================================================================================");
            Console.WriteLine("\t\t\t\t        ДОБРО ПОЖАЛОВАТЬ В ГЛАВНОЕ МЕНЮ                                                                        ");
            Console.WriteLine("=======================================================================================================================");
            Console.WriteLine("=======================================================================================================================");
            Console.WriteLine("=======================================================================================================================");
            Console.ResetColor();
            Console.WriteLine("\n\t\t\t\t\t\t  1. Начать игру");
            Console.WriteLine("\t\t\t\t\t\t  2. Выйти");
            Console.Write("\n\t\t\t\t\t\t  Выберите действие: ");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) return 1;
                if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) return 2;
            }
        }

        // Меню паузы
        public static int ShowPauseMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("========================================================================================================================");
            Console.WriteLine("\t\t\t\t\t\t            ПАУЗА                                                                                           ");
            Console.WriteLine("========================================================================================================================");
            Console.ResetColor();
            Console.WriteLine("\n\t\t\t\t\t\t\t1. Продолжить");
            Console.WriteLine("\n\t\t\t\t\t\t\t2. Начать заново");
            Console.WriteLine("\n\t\t\t\t\t\t\t3. Выйти в главное меню");
            Console.Write("\n\t\t\t\t\t\t\tВыберите действие: ");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) return 1;
                if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) return 2;
                if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3) return 3;
            }
        }
    }
}