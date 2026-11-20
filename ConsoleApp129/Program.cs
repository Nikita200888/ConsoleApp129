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
            GameUI.SubscribeToGameEvents();
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
                int currentLevel = 1;
                bool isLoading = (mainChoice == 2);

                while (restartGame)
                {
                    restartGame = false;
                    Map map = new Map();

                    if (isLoading)
                    {
                        try
                        {
                            SaveData loadedData = SaveManager.LoadGame();
                            if (loadedData != null)
                            {
                                map.LoadMapFromSave(loadedData);
                                currentLevel = map.CurrentLevel;
                            }
                            else map.Map_generation(currentLevel);
                        }
                        catch (GameActionException ex)
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n[ОШИБКА] {ex.Message}\nНажмите клавишу...");
                            Console.ResetColor();
                            Console.ReadKey(true);
                            continue;
                        }
                        isLoading = false;
                    }
                    else map.Map_generation(currentLevel);

                    bool isPlaying = true;

                    int playerWalls = 0;
                    bool squareUsed = false, crossUsed = false, plusUsed = false, robotUsed = false;
                    bool hasWeapon = false;
                    bool huntActive = false;

                    int buildingMode = 0;
                    int buildCenterX = -1;
                    int buildCenterY = -1;
                    int buildFilled = 0;

                    DateTime lastEnemyMove = DateTime.Now;
                    DateTime nextRageTime = DateTime.Now.AddSeconds(20);
                    DateTime rageEndTime = DateTime.MinValue;
                    List<(int, int, Wall)> hiddenWalls = new List<(int, int, Wall)>();

                    while (isPlaying)
                    {
                        Console.SetCursorPosition(0, 0);
                        map.Drawing_the_map();

                        MapObject[,] currentMap = map.GetMapArray();
                        int heroX = -1, heroY = -1;
                        Wall robotUnderHero = null;
                        for (int i = 0; i < currentMap.GetLength(0); i++)
                            for (int j = 0; j < currentMap.GetLength(1); j++)
                            {
                                if (currentMap[i, j] is Hero) { heroX = i; heroY = j; }
                                if (currentMap[i, j] is Wall ww && ww.IsRobotCenter && ww.IsRobot && ww.HeroInsideRobot)
                                {
                                    robotUnderHero = ww;
                                    heroX = ww.RobotCenterX;
                                    heroY = ww.RobotCenterY;
                                }
                            }

                        // Восстановление скрытых стен
                        for (int i = hiddenWalls.Count - 1; i >= 0; i--)
                        {
                            var (wx, wy, wallObj) = hiddenWalls[i];
                            if (wx != heroX || wy != heroY)
                            {
                                if (currentMap[wx, wy] is Field)
                                    currentMap[wx, wy] = wallObj;
                                hiddenWalls.RemoveAt(i);
                            }
                        }

                        map.CheckRobotDestruction();

                        if (robotUnderHero == null)
                        {
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                    if (currentMap[i, j] is Wall ww && ww.IsRobotCenter)
                                        ww.HeroInsideRobot = false;
                        }

                        // Чертеж
                        if (buildingMode > 0 && buildCenterX != -1)
                        {
                            int[] dx = null, dy = null;
                            ConsoleColor color = ConsoleColor.DarkYellow;

                            if (buildingMode == 2) { dx = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 }; dy = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 }; color = ConsoleColor.Yellow; }
                            else if (buildingMode == 3) { dx = new int[] { 0, -1, 1, 0, 0 }; dy = new int[] { 0, 0, 0, -1, 1 }; color = ConsoleColor.Cyan; }
                            else if (buildingMode == 4) { dx = new int[] { -1, 1, 0, 0, -2, 2 }; dy = new int[] { 0, 0, -1, 1, 0, 0 }; color = ConsoleColor.Magenta; }
                            else if (buildingMode == 5) { dx = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }; dy = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 }; color = ConsoleColor.DarkCyan; }

                            if (dx != null)
                            {
                                buildFilled = 0;
                                for (int k = 0; k < dx.Length; k++)
                                {
                                    int nx = buildCenterX + dx[k];
                                    int ny = buildCenterY + dy[k];
                                    if (nx < 0 || nx >= currentMap.GetLength(0) || ny < 0 || ny >= currentMap.GetLength(1)) continue;

                                    if (currentMap[nx, ny] is Wall w && w.IsPlacedByPlayer) buildFilled++;
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

                        // Интерфейс СПРАВА от карты: вся отрисовка панели инкапсулирована в GameUI,
                        // каждая строка выводится фиксированной ширины и затирает старый текст
                        GameUI.DrawStatusPanel(map.CurrentLevel, playerWalls, hasWeapon,
                            buildingMode, buildFilled, robotUnderHero,
                            currentMap, heroX, heroY, DateTime.Now < rageEndTime);

                        // Охота (паттерн Стратегия: способность охотника берется из реестра стратегий)
                        if (DateTime.Now >= nextRageTime && DateTime.Now >= rageEndTime)
                        {
                            rageEndTime = DateTime.Now.AddSeconds(10);
                            nextRageTime = DateTime.Now.AddSeconds(20);
                            int abilityType = rnd.Next(1, 6);
                            HunterAbility strategy = HunterAbilityRegistry.Get(abilityType);

                            List<(int, int)> enemyPositions = new List<(int, int)>();
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                    if (currentMap[i, j] is Enemy) enemyPositions.Add((i, j));

                            if (enemyPositions.Count > 0 && strategy != null)
                            {
                                int huntersCount = Math.Min(Math.Max(map.CurrentLevel >= 2 ? 2 : 1, map.CurrentLevel), enemyPositions.Count);

                                for (int k = enemyPositions.Count - 1; k > 0; k--)
                                {
                                    int r = rnd.Next(k + 1);
                                    var tmp = enemyPositions[k];
                                    enemyPositions[k] = enemyPositions[r];
                                    enemyPositions[r] = tmp;
                                }

                                // Полиморфный вызов стратегии способности охотника:
                                // «Волна» действует на всех врагов карты (до четырех углов),
                                // остальные способности — только на выбранную группу охотников
                                List<(int, int)> hunters = strategy.TargetsAllEnemies
                                    ? enemyPositions
                                    : enemyPositions.GetRange(0, huntersCount);
                                strategy.ApplyToGroup(hunters, currentMap, heroX, heroY, rnd);

                                huntActive = true;
                                GameEvents.RaiseHuntStarted(abilityType);
                            }
                        }

                        // Сброс охоты
                        if (DateTime.Now >= rageEndTime)
                        {
                            for (int i = 0; i < currentMap.GetLength(0); i++)
                                for (int j = 0; j < currentMap.GetLength(1); j++)
                                    if (currentMap[i, j] is Enemy e)
                                    {
                                        e.IsChasing = false;
                                        e.Ability = 0;
                                    }

                            if (huntActive)
                            {
                                huntActive = false;
                                GameEvents.RaiseHuntEnded();
                            }
                        }

                        // Ход врагов
                        if ((DateTime.Now - lastEnemyMove).TotalMilliseconds > 500)
                        {
                            map.MovePersons();
                            lastEnemyMove = DateTime.Now;
                            map.SpawnPortalIfNeeded();
                            map.CheckRobotDestruction();
                        }

                        // Переход на уровень
                        if (map.IsHeroOnPortal())
                        {
                            currentLevel = map.CurrentLevel + 1;
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"\n\n       ПЕРЕХОД НА УРОВЕНЬ {currentLevel}...");
                            Console.ResetColor();
                            System.Threading.Thread.Sleep(800);

                            map.Map_generation(currentLevel);
                            playerWalls = 0;
                            squareUsed = false; crossUsed = false; plusUsed = false; robotUsed = false;
                            hasWeapon = false;
                            huntActive = false;
                            GameUI.SetEvent("");
                            buildingMode = 0;
                            buildCenterX = -1; buildCenterY = -1; buildFilled = 0;
                            hiddenWalls.Clear();
                            rageEndTime = DateTime.MinValue;
                            nextRageTime = DateTime.Now.AddSeconds(20);
                            lastEnemyMove = DateTime.Now;
                            continue;
                        }

                        // Управление
                        if (Console.KeyAvailable)
                        {
                            ConsoleKey key = Console.ReadKey(true).Key;

                            if (key == ConsoleKey.Escape)
                            {
                                if (buildingMode > 0)
                                {
                                    buildingMode = 0; buildCenterX = -1; buildCenterY = -1; buildFilled = 0;
                                    continue;
                                }
                                int pChoice = Menu.ShowPauseMenu();
                                if (pChoice == 1) continue;
                                else if (pChoice == 2) { SaveManager.SaveGame(map.CurrentLevel, currentMap, heroX, heroY); continue; }
                                else if (pChoice == 3) { isPlaying = false; restartGame = false; continue; }
                            }

                            // Движение
                            if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow || key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                            {
                                if (robotUnderHero != null)
                                {
                                    int dx = 0, dy = 0;
                                    if (key == ConsoleKey.UpArrow) dx = -1;
                                    if (key == ConsoleKey.DownArrow) dx = 1;
                                    if (key == ConsoleKey.LeftArrow) dy = -1;
                                    if (key == ConsoleKey.RightArrow) dy = 1;

                                    map.MoveRobot(robotUnderHero.RobotCenterX, robotUnderHero.RobotCenterY, dx, dy);
                                }
                                else
                                {
                                    int tx = heroX, ty = heroY;
                                    if (key == ConsoleKey.UpArrow) tx--;
                                    if (key == ConsoleKey.DownArrow) tx++;
                                    if (key == ConsoleKey.LeftArrow) ty--;
                                    if (key == ConsoleKey.RightArrow) ty++;

                                    if (tx >= 0 && tx < currentMap.GetLength(0) && ty >= 0 && ty < currentMap.GetLength(1))
                                    {
                                        if (currentMap[tx, ty] is Wall w && w.IsPlacedByPlayer && !w.IsRobot && !w.IsTurret)
                                        {
                                            hiddenWalls.Add((tx, ty, w));
                                            currentMap[tx, ty] = currentMap[heroX, heroY];
                                            currentMap[heroX, heroY] = new Field();
                                        }
                                        else if (currentMap[tx, ty] is Wall w2 && !w2.IsPlacedByPlayer)
                                        {
                                            // Таран любой чужой стены дает стены
                                            playerWalls++;
                                            if (w2.IsEnemyRobot && w2.IsRobotCenter && w2.EnemyInsideRobot)
                                            {
                                                // Таран центра робота врага: пилот выпрыгивает наружу, робот рассыпается
                                                currentMap[tx, ty] = new Enemy(tx, ty);
                                                for (int di = -1; di <= 1; di++)
                                                    for (int dj = -1; dj <= 1; dj++)
                                                    {
                                                        int rx = tx + di, ry = ty + dj;
                                                        if (rx < 0 || rx >= currentMap.GetLength(0) || ry < 0 || ry >= currentMap.GetLength(1)) continue;
                                                        if (currentMap[rx, ry] is Wall rw2 && rw2.IsRobot && rw2.IsEnemyRobot)
                                                            currentMap[rx, ry] = new Field();
                                                    }
                                                GameEvents.RaiseRobotDestroyed();
                                            }
                                            else
                                                currentMap[tx, ty] = new Field();
                                            map.MovePersons(key);
                                        }
                                        else if (currentMap[tx, ty] is Tree t) { hasWeapon = true; currentMap[tx, ty] = new Field(); map.MovePersons(key); }
                                        else map.MovePersons(key);
                                    }
                                }
                            }

                            // Строительство
                            if ((key == ConsoleKey.D1 || key == ConsoleKey.Spacebar) && playerWalls >= 1)
                            {
                                if (buildingMode > 0 && buildCenterX != -1)
                                {
                                    int[] dx = null, dy = null;
                                    if (buildingMode == 2) { dx = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 }; dy = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 }; }
                                    else if (buildingMode == 3) { dx = new int[] { 0, -1, 1, 0, 0 }; dy = new int[] { 0, 0, 0, -1, 1 }; }
                                    else if (buildingMode == 4) { dx = new int[] { -1, 1, 0, 0, -2, 2 }; dy = new int[] { 0, 0, -1, 1, 0, 0 }; }
                                    else if (buildingMode == 5) { dx = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }; dy = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 }; }

                                    for (int k = 0; k < dx.Length; k++)
                                    {
                                        int nx = buildCenterX + dx[k];
                                        int ny = buildCenterY + dy[k];

                                        if (nx == heroX && ny == heroY)
                                        {
                                            Wall nw = new Wall { IsPlacedByPlayer = true };

                                            if (buildingMode == 3) { nw.IsTurret = true; nw.ReadyTime = DateTime.Now.AddSeconds(3); nw.LastShot = DateTime.Now.AddSeconds(-10); }
                                            if (buildingMode == 4) { nw.IsTrap = true; nw.ReadyTime = DateTime.Now.AddSeconds(3); }
                                            if (buildingMode == 5)
                                            {
                                                nw.IsRobot = true;
                                                nw.RobotCenterX = buildCenterX;
                                                nw.RobotCenterY = buildCenterY;

                                                if (dx[k] == 0 && dy[k] == 0)
                                                {
                                                    nw.IsRobotCenter = true;
                                                    nw.IsPassableForHero = true;
                                                    // Первый залп доступен сразу после сборки робота
                                                    nw.LastShot = DateTime.Now.AddSeconds(-10);
                                                }
                                                else if ((dx[k] == 0 && Math.Abs(dy[k]) == 1) || (dy[k] == 0 && Math.Abs(dx[k]) == 1))
                                                {
                                                    nw.IsRobotGun = true;
                                                }
                                            }

                                            bool moved = TryMoveHeroAway(currentMap, heroX, heroY);
                                            currentMap[nx, ny] = nw;
                                            playerWalls--;

                                            if (!moved)
                                            {
                                                currentMap[nx, ny] = new Hero(nx, ny);
                                            }
                                            break;
                                        }
                                    }
                                }
                                else if (buildingMode == 0)
                                {
                                    Wall nw = new Wall { IsPlacedByPlayer = true };
                                    bool moved = TryMoveHeroAway(currentMap, heroX, heroY);
                                    currentMap[heroX, heroY] = nw;
                                    playerWalls--;
                                    if (!moved) currentMap[heroX, heroY] = new Hero(heroX, heroY);
                                }
                            }

                            // Выбор фигур
                            if (key == ConsoleKey.D2 && playerWalls >= 8 && !squareUsed && buildingMode == 0)
                            { buildingMode = 2; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }
                            else if (key == ConsoleKey.D3 && playerWalls >= 5 && !crossUsed && buildingMode == 0)
                            { buildingMode = 3; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }
                            else if (key == ConsoleKey.D4 && playerWalls >= 6 && !plusUsed && buildingMode == 0)
                            { buildingMode = 4; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }
                            else if (key == ConsoleKey.D5 && playerWalls >= 9 && !robotUsed && buildingMode == 0)
                            { buildingMode = 5; buildCenterX = heroX; buildCenterY = heroY; buildFilled = 0; }

                            if (buildingMode > 0 && buildFilled == GameUI.GetBuildTotal(buildingMode))
                            {
                                if (buildingMode == 2) squareUsed = true;
                                else if (buildingMode == 3) crossUsed = true;
                                else if (buildingMode == 4) plusUsed = true;
                                else if (buildingMode == 5) robotUsed = true;
                                buildingMode = 0; buildCenterX = -1; buildCenterY = -1; buildFilled = 0;
                            }

                            // Удар
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
                                        if (DateTime.Now < enemy.StunUntil)
                                        {
                                            currentMap[cx, cy] = new Field();
                                            hasWeapon = false;
                                            GameUI.ShowMessage("Враг уничтожен!");
                                        }
                                        else
                                        {
                                            GameUI.ShowMessage("Враг не в стане!");
                                        }
                                        break;
                                    }
                                    // Оружием можно отломать пушку подошедшего вплотную робота врага
                                    if (currentMap[cx, cy] is Wall rw && rw.IsRobot && rw.IsEnemyRobot && rw.IsRobotGun)
                                    {
                                        currentMap[cx, cy] = new Field();
                                        GameUI.ShowMessage("Пушка вражеского робота разрушена!");
                                        break;
                                    }
                                    }
                                }
                            }

                            // E: Робот
                            if (key == ConsoleKey.E)
                            {
                                if (robotUnderHero != null && robotUnderHero.IsRobotCenter && robotUnderHero.IsRobot)
                                {
                                    if (robotUnderHero.RobotKills < Wall.RobotMaxKills)
                                    {
                                        // Задержка: пушки робота перезаряжаются 1 секунду между залпами
                                        if ((DateTime.Now - robotUnderHero.LastShot).TotalSeconds >= 1.0)
                                        {
                                            robotUnderHero.LastShot = DateTime.Now;
                                            int rcx = robotUnderHero.RobotCenterX;
                                            int rcy = robotUnderHero.RobotCenterY;
                                            for (int di = -1; di <= 1; di++)
                                                for (int dj = -1; dj <= 1; dj++)
                                                {
                                                    if (di == 0 && dj == 0) continue;
                                                    int gx = rcx + di, gy = rcy + dj;
                                                    if (gx < 0 || gx >= currentMap.GetLength(0) || gy < 0 || gy >= currentMap.GetLength(1)) continue;
                                                    if (currentMap[gx, gy] is Wall gun && gun.IsRobot && gun.IsRobotGun)
                                                    {
                                                        int tx = gx + di, ty = gy + dj;
                                                        if (tx < 0 || tx >= currentMap.GetLength(0) || ty < 0 || ty >= currentMap.GetLength(1)) continue;
                                                        if (currentMap[tx, ty] is Enemy)
                                                        {
                                                            // Враг вплотную к пушке: залп уничтожает его сразу и засчитывает убийство
                                                            currentMap[tx, ty] = new Field();
                                                            robotUnderHero.RobotKills++;
                                                        }
                                                        else if (currentMap[tx, ty] is Field || currentMap[tx, ty] is Tree)
                                                        {
                                                            Bullet rb = new Bullet(di, dj, false, true);
                                                            // Владелец — ссылка на центр робота: убийство засчитается,
                                                            // даже если робот успел уехать, пока пуля летела
                                                            rb.OwnerRobot = robotUnderHero;
                                                            currentMap[tx, ty] = rb;
                                                        }
                                                    }
                                                }
                                        }
                                        else
                                        {
                                            GameUI.SetEvent("Пушка перезаряжается...");
                                        }
                                    }
                                }
                                else
                                {
                                    Wall nearby = null;
                                    for (int di = -2; di <= 2 && nearby == null; di++)
                                        for (int dj = -2; dj <= 2 && nearby == null; dj++)
                                        {
                                            if (di == 0 && dj == 0) continue;
                                            int cx = heroX + di, cy = heroY + dj;
                                            if (cx < 0 || cx >= currentMap.GetLength(0) || cy < 0 || cy >= currentMap.GetLength(1)) continue;
                                            // В робота врага сесть нельзя
                                            if (currentMap[cx, cy] is Wall w && w.IsRobot && !w.IsEnemyRobot)
                                            {
                                                if (w.IsRobotCenter) nearby = w;
                                                else if (w.RobotCenterX >= 0 && w.RobotCenterY >= 0 &&
                                                         currentMap[w.RobotCenterX, w.RobotCenterY] is Wall c && c.IsRobotCenter)
                                                    nearby = c;
                                            }
                                        }
                                    if (nearby != null && nearby.RobotKills < Wall.RobotMaxKills)
                                    {
                                        currentMap[heroX, heroY] = new Field();
                                        nearby.HeroInsideRobot = true;
                                    }
                                }
                            }
                        }

                        // Смерть без авто-респавна (событие Наблюдателя: SaveManager сам удалит сохранение)
                        if (!map.IsHeroAlive())
                        {
                            GameEvents.RaiseHeroDied();

                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n\n       ВЫ ПОГИБЛИ! ИГРА ОКОНЧЕНА.");
                            Console.ResetColor();
                            Console.WriteLine("\nНажмите любую клавишу...");
                            Console.ReadKey(true);
                            isPlaying = false;
                            restartGame = false;
                        }
                    }
                }
            }
        }

        static bool TryMoveHeroAway(MapObject[,] map, int heroX, int heroY)
        {
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = heroX + dx[i];
                int ny = heroY + dy[i];

                if (nx < 0 || nx >= map.GetLength(0) || ny < 0 || ny >= map.GetLength(1))
                    continue;

                if (map[nx, ny] is Field || map[nx, ny] is Portal)
                {
                    MapObject hero = map[heroX, heroY];
                    map[nx, ny] = hero;
                    map[heroX, heroY] = new Field();
                    return true;
                }
            }
            return false;
        }
    }
}