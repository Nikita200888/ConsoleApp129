using System;
using System.Collections.Generic;

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

                if (mainChoice == 4)
                {
                    Console.Clear();
                    Console.WriteLine("Выберите скин:\n1. Синий\n2. Зеленый\n3. Голубой\n4. Красный\n5. Желтый");
                    var colorchoice = Console.ReadKey(true).Key;
                    if (colorchoice == ConsoleKey.D1) Hero.HeroColor = ConsoleColor.DarkBlue;
                    if (colorchoice == ConsoleKey.D2) Hero.HeroColor = ConsoleColor.Green;
                    if (colorchoice == ConsoleKey.D3) Hero.HeroColor = ConsoleColor.Blue;
                    if (colorchoice == ConsoleKey.D4) Hero.HeroColor = ConsoleColor.Red;
                    if (colorchoice == ConsoleKey.D5) Hero.HeroColor = ConsoleColor.Yellow;
                    continue;
                }

                bool restartGame = true;
                while (restartGame)
                {
                    restartGame = false;
                    Map map = new Map();

                    // --- БЛОК ЗАГРУЗКИ С ОБРАБОТКОЙ ИСКЛЮЧЕНИЯ ---
                    if (mainChoice == 2)
                    {
                        try
                        {
                            SaveData loadedData = SaveManager.LoadGame();
                            if (loadedData != null)
                                map.LoadMapFromSave(loadedData);
                            else
                                map.Map_generation();
                        }
                        catch (GameActionException ex)
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n=========================================");
                            Console.WriteLine($"   {ex.Message}");
                            Console.WriteLine("=========================================");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                            Console.ReadKey(true);

                            restartGame = false;
                            continue;
                        }
                    }
                    else
                    {
                        map.Map_generation();
                    }
                    // ----------------------------------------------

                    bool isPlaying = true;
                    int playerWalls = 0;
                    bool hasWeapon = false;
                    DateTime lastEnemyMove = DateTime.Now;

                    DateTime nextRageTime = DateTime.Now.AddSeconds(20);
                    DateTime rageEndTime = DateTime.MinValue;

                    List<(int, int)> hiddenWalls = new List<(int, int)>();

                    while (isPlaying)
                    {
                        Console.SetCursorPosition(0, 0);
                        map.Drawing_the_map();

                        MapObject[,] currentMap = map.GetMapArray();

                        int heroX = -1, heroY = -1;
                        for (int i = 0; i < currentMap.GetLength(0); i++)
                            for (int j = 0; j < currentMap.GetLength(1); j++)
                                if (currentMap[i, j] is Hero) { heroX = i; heroY = j; }

                        // ВОЗВРАЩАЕМ СТЕНЫ
                        for (int i = hiddenWalls.Count - 1; i >= 0; i--)
                        {
                            var wallPos = hiddenWalls[i];
                            if (wallPos.Item1 != heroX || wallPos.Item2 != heroY)
                            {
                                if (currentMap[wallPos.Item1, wallPos.Item2] is Field)
                                {
                                    Wall restoredWall = new Wall();
                                    restoredWall.IsPlacedByPlayer = true;
                                    currentMap[wallPos.Item1, wallPos.Item2] = restoredWall;
                                }
                                hiddenWalls.RemoveAt(i);
                            }
                        }

                        string weaponStatus = hasWeapon ? "Есть (T)" : "Нет";
                        Console.WriteLine($"\nСтен: {playerWalls} | Оружие: {weaponStatus} | Пробел - стена, F - удар. Esc - меню.   ");

                        if (DateTime.Now < rageEndTime)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("ОХОТА! Один из врагов идет по вашему следу!  ");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("                                             ");
                        }

                        // СПАВН ПОГОНИ
                        if (DateTime.Now >= nextRageTime && DateTime.Now >= rageEndTime)
                        {
                            rageEndTime = DateTime.Now.AddSeconds(10);
                            nextRageTime = DateTime.Now.AddSeconds(20);

                            bool found = false;
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                            {
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                {
                                    if (currentMap[i, j] is Enemy e && !found)
                                    {
                                        e.IsChasing = true;
                                        found = true;
                                    }
                                }
                            }
                        }

                        if (DateTime.Now >= rageEndTime)
                        {
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                    if (currentMap[i, j] is Enemy e) e.IsChasing = false;
                        }

                        if ((DateTime.Now - lastEnemyMove).TotalMilliseconds > 500)
                        {
                            map.MovePersons();
                            lastEnemyMove = DateTime.Now;
                        }

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
                                else if (pauseChoice == 4)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Выберите скин:\n1. Синий\n2. Зеленый\n3. Голубой\n4. Красный\n5. Желтый");
                                    var colorchoice = Console.ReadKey(true).Key;
                                    if (colorchoice == ConsoleKey.D1) Hero.HeroColor = ConsoleColor.DarkBlue;
                                    if (colorchoice == ConsoleKey.D2) Hero.HeroColor = ConsoleColor.Green;
                                    if (colorchoice == ConsoleKey.D3) Hero.HeroColor = ConsoleColor.Blue;
                                    if (colorchoice == ConsoleKey.D4) Hero.HeroColor = ConsoleColor.Red;
                                    if (colorchoice == ConsoleKey.D5) Hero.HeroColor = ConsoleColor.Yellow;
                                    continue;
                                }
                            }

                            // ДВИЖЕНИЕ
                            if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                                key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                            {
                                int targetX = heroX, targetY = heroY;
                                switch (key)
                                {
                                    case ConsoleKey.UpArrow: targetX--; break;
                                    case ConsoleKey.DownArrow: targetX++; break;
                                    case ConsoleKey.LeftArrow: targetY--; break;
                                    case ConsoleKey.RightArrow: targetY++; break;
                                }

                                if (targetX >= 0 && targetX < currentMap.GetLength(0) && targetY >= 0 && targetY < currentMap.GetLength(1))
                                {
                                    // ПРОХОД СКВОЗЬ СТЕНУ
                                    if (currentMap[targetX, targetY] is Wall w && w.IsPlacedByPlayer)
                                    {
                                        hiddenWalls.Add((targetX, targetY));

                                        currentMap[targetX, targetY] = currentMap[heroX, heroY];
                                        currentMap[heroX, heroY] = new Field();
                                    }
                                    // СБОР ГОЛУБЫХ СТЕН
                                    else if (currentMap[targetX, targetY] is Wall w2 && !w2.IsPlacedByPlayer)
                                    {
                                        playerWalls++;
                                        currentMap[targetX, targetY] = new Field();
                                        map.MovePersons(key);
                                    }
                                    // СБОР ДЕРЕВЬЕВ
                                    else if (currentMap[targetX, targetY] is Tree t && !hasWeapon)
                                    {
                                        hasWeapon = true;
                                        currentMap[targetX, targetY] = new Field();
                                        map.MovePersons(key);
                                    }
                                    else
                                    {
                                        map.MovePersons(key);
                                    }
                                }
                            }

                            // АТАКА
                            if (key == ConsoleKey.F && hasWeapon)
                            {
                                int[] dx = { -1, 1, 0, 0 };
                                int[] dy = { 0, 0, -1, 1 };

                                for (int i = 0; i < 4; i++)
                                {
                                    int checkX = heroX + dx[i];
                                    int checkY = heroY + dy[i];

                                    if (checkX >= 0 && checkX < currentMap.GetLength(0) && checkY >= 0 && checkY < currentMap.GetLength(1))
                                    {
                                        if (currentMap[checkX, checkY] is Enemy enemy)
                                        {
                                            if (DateTime.Now < enemy.StunUntil)
                                            {
                                                currentMap[checkX, checkY] = new Field();
                                                hasWeapon = false;
                                                Console.WriteLine("\nВраг уничтожен деревом! Нажмите клавишу...");
                                                Console.ReadKey(true);
                                            }
                                            else
                                            {
                                                Console.WriteLine("\nВраг не в стане! Атака провалилась. Нажмите клавишу...");
                                                Console.ReadKey(true);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }

                            // СТАВИМ СТЕНУ
                            if (key == ConsoleKey.Spacebar && playerWalls > 0)
                            {
                                if (heroX != -1 && heroY != -1)
                                {
                                    Wall newWall = new Wall();
                                    newWall.IsPlacedByPlayer = true;
                                    currentMap[heroX, heroY] = newWall;

                                    if (heroX + 1 < currentMap.GetLength(0) && currentMap[heroX + 1, heroY] is Field)
                                        currentMap[heroX + 1, heroY] = new Hero(heroX + 1, heroY);
                                    else if (heroX - 1 >= 0 && currentMap[heroX - 1, heroY] is Field)
                                        currentMap[heroX - 1, heroY] = new Hero(heroX - 1, heroY);
                                    else if (heroY - 1 >= 0 && currentMap[heroX, heroY - 1] is Field)
                                        currentMap[heroX, heroY - 1] = new Hero(heroX, heroY - 1);
                                    else if (heroY + 1 < currentMap.GetLength(1) && currentMap[heroX, heroY + 1] is Field)
                                        currentMap[heroX, heroY + 1] = new Hero(heroX, heroY + 1);

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