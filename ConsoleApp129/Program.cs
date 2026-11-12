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
                if (mainChoice == 3) break;

                bool restartGame = true;
                while (restartGame)
                {
                    restartGame = false;
                    Map map = new Map();

                    if (mainChoice == 2)
                    {
                        SaveData loadedData = SaveManager.LoadGame();
                        if (loadedData != null) map.LoadMapFromSave(loadedData);
                        else map.Map_generation();
                    }
                    else map.Map_generation();

                    bool isPlaying = true;
                    int playerWalls = 0;
                    DateTime lastEnemyMove = DateTime.Now;

                    while (isPlaying)
                    {
                        Console.SetCursorPosition(0, 0);
                        map.Drawing_the_map();

                        Console.WriteLine($"\nИнвентарь: {playerWalls}. Пробел - поставить. Esc - меню.      ");

                        if ((DateTime.Now - lastEnemyMove).TotalMilliseconds > 500)
                        {
                            map.MovePersons();
                            lastEnemyMove = DateTime.Now;
                        }

                        MapObject[,] currentMap = map.GetMapArray();

                        // Ищем героя, чтобы знать, где он
                        int heroX = -1, heroY = -1;
                        for (int i = 0; i < currentMap.GetLength(0); i++)
                            for (int j = 0; j < currentMap.GetLength(1); j++)
                                if (currentMap[i, j] is Hero) { heroX = i; heroY = j; }

                        if (Console.KeyAvailable)
                        {
                            ConsoleKey key = Console.ReadKey(true).Key;

                            if (key == ConsoleKey.Escape)
                            {
                                int pauseChoice = Menu.ShowPauseMenu();
                                if (pauseChoice == 1) continue;
                                else if (pauseChoice == 2)
                                {
                                    SaveManager.SaveGame(1, currentMap, heroX, heroY);
                                    Console.WriteLine("\nИгра сохранена! Нажмите любую клавишу...");
                                    Console.ReadKey(true);
                                    continue;
                                }
                                else if (pauseChoice == 3) { isPlaying = false; restartGame = false; continue; }
                            }

                            // ДВИЖЕНИЕ
                            if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                                key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                            {
                                // Запоминаем, куда хотим пойти
                                int targetX = heroX, targetY = heroY;
                                switch (key)
                                {
                                    case ConsoleKey.UpArrow: targetX--; break;
                                    case ConsoleKey.DownArrow: targetX++; break;
                                    case ConsoleKey.LeftArrow: targetY--; break;
                                    case ConsoleKey.RightArrow: targetY++; break;
                                }

                                // Если там стена, и она не наша - собираем её
                                if (targetX >= 0 && targetX < currentMap.GetLength(0) && targetY >= 0 && targetY < currentMap.GetLength(1))
                                {
                                    if (currentMap[targetX, targetY] is Wall w && !w.IsPlacedByPlayer)
                                    {
                                        playerWalls++;
                                        currentMap[targetX, targetY] = new Field(); // Убираем стену
                                    }
                                }

                                // Двигаем героя (он пойдет на пустое место)
                                map.MovePersons(key);
                            }

                            // СТАВИМ СТЕНУ
                            if (key == ConsoleKey.Spacebar && playerWalls > 0)
                            {
                                if (heroX != -1 && heroY != -1)
                                {
                                    // 1. Ставим стену на клетку, где сейчас стоит герой
                                    Wall newWall = new Wall();
                                    newWall.IsPlacedByPlayer = true;
                                    currentMap[heroX, heroY] = newWall;

                                    // 2. Сдвигаем героя на шаг НАЗАД (вниз), чтобы он не телепортировался
                                    if (heroX + 1 < currentMap.GetLength(0) && currentMap[heroX + 1, heroY] is Field)
                                    {
                                        Hero hero = new Hero(heroX + 1, heroY);
                                        currentMap[heroX + 1, heroY] = hero;
                                    }

                                    playerWalls--;
                                }
                            }
                        }

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