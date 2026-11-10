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
                if (mainChoice == 3) break; // выход

                bool restartGame = true;
                while (restartGame)
                {
                    restartGame = false;
                    Map map = new Map();
                    SaveData loadedData = null;

                    // Если нажали "Вернуться в последнюю сессию"
                    if (mainChoice == 2)
                    {
                        loadedData = SaveManager.LoadGame();
                        if (loadedData != null)
                        {
                            // Загружаем карту из JSON
                            map.LoadMapFromSave(loadedData);
                        }
                        else
                        {
                            map.Map_generation(); // Если файл битый - генерируем новую
                        }
                    }
                    else
                    {
                        map.Map_generation(); // Новая игра
                    }

                    bool isPlaying = true;

                    while (isPlaying)
                    {
                        Console.Clear();
                        map.Drawing_the_map();
                        Console.WriteLine("\nУправление: стрелки. Esc - меню.");

                        ConsoleKey key = Console.ReadKey(true).Key;

                        // ПАУЗА
                        if (key == ConsoleKey.Escape)
                        {
                            int pauseChoice = Menu.ShowPauseMenu();
                            if (pauseChoice == 1) continue;//продолжить

                            if (pauseChoice == 2) // СОХРАНИТЬ ВСЁ
                            {
                                MapObject[,] array = map.GetMapArray();
                                int hX = 0, hY = 0;
                                // Ищем, где сейчас герой, чтобы сохранить его координаты
                                for (int i = 0; i < array.GetLength(0); i++)
                                    for (int j = 0; j < array.GetLength(1); j++)
                                        if (array[i, j] is Hero) { hX = i; hY = j; }

                                SaveManager.SaveGame(1, array, hX, hY); // 1 - уровень

                                Console.WriteLine("\nИгра сохранена! Нажмите любую клавишу...");
                                Console.ReadKey(true);
                                continue;
                            }
                            if (pauseChoice == 3) { isPlaying = false; restartGame = false; continue; }
                        }

                        // ДВИЖЕНИЕ
                        if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                            key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                        {
                            map.MovePersons(key);
                        }

                        map.MovePersons();

                        // СМЕРТЬ
                        if (!map.IsHeroAlive())
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n\n       ВЫ ПОГИБЛИ! ИГРА ОКОНЧЕНА.");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для выхода в меню...");
                            Console.ReadKey(true);

                            SaveManager.DeleteSave();

                            isPlaying = false;
                            restartGame = false;
                        }
                    }
                }
            }
        }
    }
}