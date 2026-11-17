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
            Random rnd = new Random();

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

                    if (mainChoice == 2)
                    {
                        try
                        {
                            SaveData loadedData = SaveManager.LoadGame();
                            if (loadedData != null) map.LoadMapFromSave(loadedData);
                            else map.Map_generation();
                        }
                        catch (GameActionException ex)
                        {
                            Console.Clear(); Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n[ОШИБКА] {ex.Message}\nНажмите клавишу...");
                            Console.ResetColor(); Console.ReadKey(true);
                            restartGame = false; continue;
                        }
                    }
                    else map.Map_generation();

                    bool isPlaying = true;
                    int playerWalls = 0;
                    bool squareUsed = false, crossUsed = false, plusUsed = false;
                    bool hasWeapon = false;

                    int buildingMode = 0; // 0 - нет, 2 - квадрат, 3 - турель, 4 - приманка
                    int buildCenterX = -1;
                    int buildCenterY = -1;
                    int buildFilled = 0;

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

                        for (int i = hiddenWalls.Count - 1; i >= 0; i--)
                        {
                            var w = hiddenWalls[i];
                            if (w.Item1 != heroX || w.Item2 != heroY)
                            {
                                if (currentMap[w.Item1, w.Item2] is Field)
                                {
                                    Wall rw = new Wall { IsPlacedByPlayer = true };
                                    currentMap[w.Item1, w.Item2] = rw;
                                }
                                hiddenWalls.RemoveAt(i);
                            }
                        }

                        // ===== СТАТИЧНЫЙ СИЛУЭТ =====
                        if (buildingMode > 0 && buildCenterX != -1)
                        {
                            int[] dx = null;
                            int[] dy = null;
                            ConsoleColor color = ConsoleColor.DarkYellow;

                            if (buildingMode == 2)
                            {
                                dx = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 };
                                dy = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 };
                                color = ConsoleColor.Yellow;
                            }
                            else if (buildingMode == 3)
                            {
                                dx = new int[] { 0, -1, 1, 0, 0 };
                                dy = new int[] { 0, 0, 0, -1, 1 };
                                color = ConsoleColor.Cyan;
                            }
                            else if (buildingMode == 4)
                            {
                                dx = new int[] { -1, 1, 0, 0, -2, 2 };
                                dy = new int[] { 0, 0, -1, 1, 0, 0 };
                                color = ConsoleColor.Magenta;
                            }

                            if (dx != null)
                            {
                                buildFilled = 0;

                                for (int k = 0; k < dx.Length; k++)
                                {
                                    int nx = buildCenterX + dx[k];
                                    int ny = buildCenterY + dy[k];

                                    if (nx >= 0 && nx < currentMap.GetLength(0) && ny >= 0 && ny < currentMap.GetLength(1))
                                    {
                                        if (currentMap[nx, ny] is Wall w && w.IsPlacedByPlayer)
                                        {
                                            buildFilled++;
                                        }
                                        else
                                        {
                                            Console.SetCursorPosition(ny * 2, nx);
                                            Console.ForegroundColor = color;
                                            Console.Write("+");
                                            Console.ResetColor();
                                        }
                                    }
                                }
                            }
                        }

                        Console.SetCursorPosition(0, 26);
                        if (buildingMode > 0)
                        {
                            string name = buildingMode == 2 ? "КВАДРАТ" : buildingMode == 3 ? "ТУРЕЛЬ" : "ПРИМАНКА";
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"РЕЖИМ: {name} | Заполнено: {buildFilled} / {GetBuildTotal(buildingMode)} | 1/Пробел - установить | Esc - отмена");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write("1-Стена  2-Квадрат  3-Турель  4-Приманка  |  F-удар  Esc-меню                    ");
                        }

                        Console.SetCursorPosition(0, 27);
                        Console.WriteLine($"\nПлюсов: {playerWalls} | Кв:{squareUsed} Кр:{crossUsed} Кст:{plusUsed}");

                        if (DateTime.Now < rageEndTime)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"ОХОТА! {GetAbilityName(currentMap)}  ");
                            Console.ResetColor();
                        }
                        else Console.WriteLine("                                             ");

                        if (DateTime.Now >= nextRageTime && DateTime.Now >= rageEndTime)
                        {
                            rageEndTime = DateTime.Now.AddSeconds(10);
                            nextRageTime = DateTime.Now.AddSeconds(20);
                            int abilityType = rnd.Next(1, 6);

                            List<(int, int)> enemyPositions = new List<(int, int)>();
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                    if (currentMap[i, j] is Enemy) enemyPositions.Add((i, j));

                            if (enemyPositions.Count > 0)
                            {
                                if (abilityType == 5)
                                {
                                    int[] cx = { 1, 1, 23, 23 };
                                    int[] cy = { 1, 23, 1, 23 };
                                    int count = 0;

                                    foreach (var pos in enemyPositions)
                                    {
                                        if (count >= 4) break;
                                        Enemy e = (Enemy)currentMap[pos.Item1, pos.Item2];
                                        currentMap[pos.Item1, pos.Item2] = new Field();

                                        while (count < 4)
                                        {
                                            if (currentMap[cx[count], cy[count]] is Field)
                                            {
                                                e.IsChasing = true;
                                                e.Ability = 5;
                                                e.HasTeleported = false;
                                                currentMap[cx[count], cy[count]] = e;
                                                count++;
                                                break;
                                            }
                                            count++;
                                        }
                                    }
                                }
                                else
                                {
                                    var randomEnemyPos = enemyPositions[rnd.Next(enemyPositions.Count)];
                                    if (currentMap[randomEnemyPos.Item1, randomEnemyPos.Item2] is Enemy e)
                                    {
                                        e.IsChasing = true;
                                        e.Ability = abilityType;
                                        e.HasTeleported = false;

                                        if (abilityType == 2 && heroX != -1)
                                        {
                                            for (int attempt = 0; attempt < 30; attempt++)
                                            {
                                                double angle = rnd.NextDouble() * Math.PI * 2;
                                                int dist = rnd.Next(8, 13);
                                                int tx = heroX + (int)(Math.Cos(angle) * dist);
                                                int ty = heroY + (int)(Math.Sin(angle) * dist);

                                                if (tx >= 1 && tx < currentMap.GetLength(0) - 1 &&
                                                    ty >= 1 && ty < currentMap.GetLength(1) - 1 &&
                                                    currentMap[tx, ty] is Field)
                                                {
                                                    currentMap[randomEnemyPos.Item1, randomEnemyPos.Item2] = new Field();
                                                    currentMap[tx, ty] = e;
                                                    e.HasTeleported = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (DateTime.Now >= rageEndTime)
                        {
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                    if (currentMap[i, j] is Enemy e)
                                    {
                                        e.IsChasing = false;
                                        e.Ability = 0;
                                    }
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
                                if (buildingMode > 0)
                                {
                                    buildingMode = 0;
                                    buildCenterX = -1;
                                    buildCenterY = -1;
                                    buildFilled = 0;
                                    continue;
                                }
                                int pChoice = Menu.ShowPauseMenu();
                                if (pChoice == 1) continue;
                                else if (pChoice == 2) { SaveManager.SaveGame(1, currentMap, heroX, heroY); continue; }
                                else if (pChoice == 3) { isPlaying = false; restartGame = false; continue; }
                            }

                            // ДВИЖЕНИЕ
                            if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                                key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                            {
                                int tx = heroX, ty = heroY;
                                if (key == ConsoleKey.UpArrow) tx--;
                                if (key == ConsoleKey.DownArrow) tx++;
                                if (key == ConsoleKey.LeftArrow) ty--;
                                if (key == ConsoleKey.RightArrow) ty++;

                                if (tx >= 0 && tx < currentMap.GetLength(0) && ty >= 0 && ty < currentMap.GetLength(1))
                                {
                                    if (currentMap[tx, ty] is Wall w && w.IsPlacedByPlayer)
                                    {
                                        hiddenWalls.Add((tx, ty));
                                        currentMap[tx, ty] = currentMap[heroX, heroY];
                                        currentMap[heroX, heroY] = new Field();
                                    }
                                    else if (currentMap[tx, ty] is Wall w2 && !w2.IsPlacedByPlayer) { playerWalls++; currentMap[tx, ty] = new Field(); map.MovePersons(key); }
                                    else if (currentMap[tx, ty] is Tree t) { hasWeapon = true; currentMap[tx, ty] = new Field(); map.MovePersons(key); }
                                    else map.MovePersons(key);
                                }
                            }

                            // СТРОИТЕЛЬСТВО (Клавиша 1 ИЛИ Пробел)
                            if ((key == ConsoleKey.D1 || key == ConsoleKey.Spacebar) && playerWalls >= 1)
                            {
                                if (buildingMode > 0 && buildCenterX != -1)
                                {
                                    int[] dx = null;
                                    int[] dy = null;

                                    if (buildingMode == 2) { dx = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 }; dy = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 }; }
                                    else if (buildingMode == 3) { dx = new int[] { 0, -1, 1, 0, 0 }; dy = new int[] { 0, 0, 0, -1, 1 }; }
                                    else if (buildingMode == 4) { dx = new int[] { -1, 1, 0, 0, -2, 2 }; dy = new int[] { 0, 0, -1, 1, 0, 0 }; }

                                    for (int k = 0; k < dx.Length; k++)
                                    {
                                        int nx = buildCenterX + dx[k];
                                        int ny = buildCenterY + dy[k];
                                        if (nx == heroX && ny == heroY)
                                        {
                                            Wall nw = new Wall { IsPlacedByPlayer = true };
                                            if (buildingMode == 3) { nw.IsTurret = true; nw.ReadyTime = DateTime.Now.AddSeconds(3); nw.LastShot = DateTime.Now.AddSeconds(-10); }
                                            if (buildingMode == 4) { nw.IsTrap = true; nw.ReadyTime = DateTime.Now.AddSeconds(3); }

                                            // Ставим стену (как раньше, с отодвиганием героя)
                                            currentMap[nx, ny] = nw;
                                            playerWalls--;

                                            // Герой отходит, чтобы не застрять
                                            if (heroX + 1 < currentMap.GetLength(0) && currentMap[heroX + 1, heroY] is Field) currentMap[heroX + 1, heroY] = new Hero(heroX + 1, heroY);
                                            else if (heroX - 1 >= 0 && currentMap[heroX - 1, heroY] is Field) currentMap[heroX - 1, heroY] = new Hero(heroX - 1, heroY);
                                            break;
                                        }
                                    }
                                }
                                else if (buildingMode == 0)
                                {
                                    Wall nw = new Wall { IsPlacedByPlayer = true };
                                    currentMap[heroX, heroY] = nw;
                                    if (heroX + 1 < currentMap.GetLength(0) && currentMap[heroX + 1, heroY] is Field)
                                        currentMap[heroX + 1, heroY] = new Hero(heroX + 1, heroY);
                                    else if (heroX - 1 >= 0 && currentMap[heroX - 1, heroY] is Field)
                                        currentMap[heroX - 1, heroY] = new Hero(heroX - 1, heroY);
                                    else if (heroY - 1 < currentMap.GetLength(0) && currentMap[heroY - 1, heroX] is Field)
                                        currentMap[heroX + 1, heroY] = new Hero(heroX + 1, heroY);



                                    playerWalls -= 1;
                                }
                            }

                            // ВХОД В РЕЖИМ СТРОИТЕЛЬСТВА
                            if (key == ConsoleKey.D2 && playerWalls >= 8 && !squareUsed && buildingMode == 0)
                            { buildingMode = 2; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }
                            else if (key == ConsoleKey.D3 && playerWalls >= 5 && !crossUsed && buildingMode == 0)
                            { buildingMode = 3; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }
                            else if (key == ConsoleKey.D4 && playerWalls >= 6 && !plusUsed && buildingMode == 0)
                            { buildingMode = 4; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }

                            // ПРОВЕРКА ЗАВЕРШЕНИЯ
                            if (buildingMode > 0 && buildFilled == GetBuildTotal(buildingMode))
                            {
                                if (buildingMode == 2) squareUsed = true;
                                else if (buildingMode == 3) crossUsed = true;
                                else if (buildingMode == 4) plusUsed = true;
                                buildingMode = 0; buildCenterX = -1; buildCenterY = -1; buildFilled = 0;
                            }

                            if (key == ConsoleKey.F && hasWeapon)
                            {
                                int[] dx = { -1, 1, 0, 0 }; int[] dy = { 0, 0, -1, 1 };
                                for (int i = 0; i < 4; i++)
                                {
                                    int cx = heroX + dx[i], cy = heroY + dy[i];
                                    if (cx >= 0 && cx < currentMap.GetLength(0) && cy >= 0 && cy < currentMap.GetLength(1))
                                    {
                                        if (currentMap[cx, cy] is Enemy enemy)
                                        {
                                            if (DateTime.Now < enemy.StunUntil) { currentMap[cx, cy] = new Field(); hasWeapon = false; Console.WriteLine("\nВраг уничтожен! Нажмите клавишу..."); Console.ReadKey(true); }
                                            else { Console.WriteLine("\nВраг не в стане! Нажмите клавишу..."); Console.ReadKey(true); }
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!map.IsHeroAlive())
                        {
                            Console.Clear(); Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n\n       ВЫ ПОГИБЛИ! ИГРА ОКОНЧЕНА.");
                            Console.ResetColor(); Console.WriteLine("\nНажмите любую клавишу...");
                            Console.ReadKey(true); SaveManager.DeleteSave();
                            isPlaying = false; restartGame = false;
                        }
                    }
                }
            }
        }

        static int GetBuildTotal(int mode)
        {
            if (mode == 2) return 8;
            if (mode == 3) return 5;
            if (mode == 4) return 6;
            return 0;
        }

        static string GetAbilityName(MapObject[,] m)
        {
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    if (m[i, j] is Enemy e && e.IsChasing)
                    {
                        switch (e.Ability)
                        {
                            case 1: return "Скорость (Красный)";
                            case 2: return "Телепорт (Фиолетовый)";
                            case 3: return "Стрельба (Желтый)";
                            case 4: return "Фазовый проход (Белый)";
                            case 5: return "Волна (Зеленый)";
                        }
                    }
            return "";
        }
    }
}