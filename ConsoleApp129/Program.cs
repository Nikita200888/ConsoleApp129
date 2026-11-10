using System;

namespace ConsoleApp129
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            bool exitGame = false;

            while (!exitGame)
            {
                int mainChoice = Menu.ShowMainMenu();
                if (mainChoice == 2) break; // Выход

                bool restartGame = true;
                while (restartGame)
                {
                    restartGame = false;
                    Map map = new Map();
                    map.Map_generation();
                    bool isPlaying = true;

                    while (isPlaying)
                    {
                        Console.Clear();
                        map.Drawing_the_map();

                        Console.WriteLine("\nУправление: стрелки. Esc - меню.");
                        ConsoleKey key = Console.ReadKey(true).Key;

                        // Пауза
                        if (key == ConsoleKey.Escape)
                        {
                            int pauseChoice = Menu.ShowPauseMenu();
                            if (pauseChoice == 1) continue;
                            if (pauseChoice == 2) { restartGame = true; isPlaying = false; continue; }
                            if (pauseChoice == 3) { isPlaying = false; restartGame = false; continue; }
                        }

                        // Ход героя
                        if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                            key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                        {
                            map.MovePersons(key);
                        }

                        // Ход врагов
                        map.MovePersons();

                        // --- ПРОВЕРКА НА СМЕРТЬ ---
                        if (!map.IsHeroAlive())
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n\n       ВЫ ПОГИБЛИ! ИГРА ОКОНЧЕНА.");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для выхода в меню...");
                            Console.ReadKey(true);

                            isPlaying = false;
                            restartGame = false;
                        }
                    }
                }
            }
        }
    }
}