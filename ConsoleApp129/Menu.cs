using System;
using System.IO;

namespace ConsoleApp129
{
    public static class Menu
    {
        public static int ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("=================================");
            Console.WriteLine("       ДОБРО ПОЖАЛОВАТЬ         ");
            Console.WriteLine("=================================");
            Console.ResetColor();
            Console.WriteLine("\n1. Начать новую игру");

            // Показываем пункт "Продолжить", только если есть файл сохранения
            bool hasSave = File.Exists("savegame.json");
            Console.WriteLine("2. Продолжить (Загрузить последнее сохранение)");
            Console.WriteLine("3. Выйти");
            Console.WriteLine("4. Изменить скин");
            Console.Write("\nВыберите действие: ");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) return 1;

                if (hasSave && (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)) return 2;

                if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3) return 3;
                if (key == ConsoleKey.D4 || key == ConsoleKey.NumPad4) return 4;
                if (key == ConsoleKey.D5 || key == ConsoleKey.NumPad5) return 5;

            }
        }

        public static int ShowPauseMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=================================");
            Console.WriteLine("            ПАУЗА                ");
            Console.WriteLine("=================================");
            Console.ResetColor();
            Console.WriteLine("\n1. Продолжить игру");
            Console.WriteLine("2. Сохранить игру");
            Console.WriteLine("3. Выйти в главное меню");
            Console.WriteLine("4. Скин");
            Console.Write("\nВыберите действие: ");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) return 1;
                if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) return 2; // НОВЫЙ ПУНКТ
                if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3) return 3;
                if (key == ConsoleKey.D4 || key == ConsoleKey.NumPad4) return 4;


            }
        }
    }
}