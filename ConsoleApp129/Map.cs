using System;
using System.Collections.Generic;

namespace ConsoleApp129
{
    /// <summary>
    /// Класс, управляющий игровой картой.
    /// </summary>
    internal class Map
    {
        Random rand = new Random();
        MapObject[,] map = new MapObject[25, 25];

        /// <summary>Текущий уровень.</summary>
        public int CurrentLevel = 1;
        /// <summary>Координата X точки спавна героя.</summary>
        public int HeroSpawnX = 12;
        /// <summary>Координата Y точки спавна героя.</summary>
        public int HeroSpawnY = 12;
        /// <summary>Координата X портала (-1 если нет).</summary>
        public int PortalX = -1;
        /// <summary>Координата Y портала (-1 если нет).</summary>
        public int PortalY = -1;

        /// <summary>
        /// Генерирует новую карту под указанный уровень.
        /// </summary>
        /// <param name="level">Номер уровня.</param>
        public void Map_generation(int level = 1)
        {
            CurrentLevel = level;
            PortalX = -1; PortalY = -1;
            HeroSpawnX = map.GetLength(0) / 2;
            HeroSpawnY = map.GetLength(1) / 2;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    // Создание объекта клетки через фабрику (паттерн Фабричный метод)
                    map[i, j] = MapObjectFactory.CreateForGeneration(rand.Next(100), i, j, HeroSpawnX, HeroSpawnY);
                }
            }

            SetupLevel();
        }

        /// <summary>
        /// Активирует охотников на карте в зависимости от уровня.
        /// </summary>
        public void SetupLevel()
        {
            List<(int, int)> enemies = new List<(int, int)>();
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Enemy) enemies.Add((i, j));

            int huntersCount = CurrentLevel >= 2 ? 2 : 0;

            for (int k = 0; k < enemies.Count; k++)
            {
                Enemy e = (Enemy)map[enemies[k].Item1, enemies[k].Item2];
                e.SpeedMultiplier = CurrentLevel >= 2 ? 1.3f : 1.0f;

                if (k < huntersCount)
                {
                    e.IsChasing = true;
                    e.Ability = k == 0 ? 1 : 4;
                }
            }
        }

        /// <summary>
        /// Отрисовывает текущее состояние карты в консоли.
        /// </summary>
        public void Drawing_the_map()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j].Rendering_on_the_map() + " ");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Находит следующий шаг по алгоритму поиска пути (BFS).
        /// </summary>
        private int[] GetNextStep(int startX, int startY, int targetX, int targetY, bool ignoreWalls)
        {
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();
            queue.Enqueue((startX, startY));
            cameFrom[(startX, startY)] = (-1, -1);
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.x == targetX && current.y == targetY)
                {
                    var path = new List<(int, int)>();
                    var curr = current;
                    while (curr != (-1, -1) && curr != (startX, startY)) { path.Add(curr); curr = cameFrom[curr]; }
                    if (path.Count > 0) { path.Reverse(); return new int[] { path[0].Item1, path[0].Item2 }; }
                    return null;
                }
                for (int i = 0; i < 4; i++)
                {
                    int nx = current.x + dx[i], ny = current.y + dy[i];
                    if (nx >= 0 && nx < map.GetLength(0) && ny >= 0 && ny < map.GetLength(1))
                    {
                        if (ignoreWalls || map[nx, ny] is Field || map[nx, ny] is Hero || map[nx, ny] is Portal
                            || (map[nx, ny] is Wall w && w.IsTrap))
                        {
                            if (!cameFrom.ContainsKey((nx, ny))) { queue.Enqueue((nx, ny)); cameFrom[(nx, ny)] = current; }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Выполняет ход врагов: движение, стрельбу, обработку способностей.
        /// </summary>
        public void MovePersons()
        {
            MapObject[,] newMap = new MapObject[map.GetLength(0), map.GetLength(1)];
            Array.Copy(map, newMap, map.Length);

            int heroX = -1, heroY = -1;
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Hero) { heroX = i; heroY = j; }
                    if (map[i, j] is Wall ww && ww.IsRobotCenter && ww.IsRobot && ww.HeroInsideRobot)
                    {
                        heroX = ww.RobotCenterX;
                        heroY = ww.RobotCenterY;
                    }
                }

            List<(int x, int y)> traps = new List<(int, int)>();
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Wall w && w.IsTrap && w.IsReady && !w.IsBlueprint)
                        traps.Add((i, j));

            // ===== ТУРЕЛИ =====
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Wall turret && turret.IsTurret && turret.IsReady && !turret.IsRobot)
                    {
                        bool hasUp = i > 0 && map[i - 1, j] is Wall wu && wu.IsTurret;
                        bool hasDown = i < map.GetLength(0) - 1 && map[i + 1, j] is Wall wd && wd.IsTurret;
                        bool hasLeft = j > 0 && map[i, j - 1] is Wall wl && wl.IsTurret;
                        bool hasRight = j < map.GetLength(1) - 1 && map[i, j + 1] is Wall wr && wr.IsTurret;

                        int neighbors = (hasUp ? 1 : 0) + (hasDown ? 1 : 0) + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);
                        if (neighbors != 1) continue;

                        if ((DateTime.Now - turret.LastShot).TotalSeconds < 2.0) continue;
                        turret.LastShot = DateTime.Now;

                        if (hasDown && !hasUp && i > 0 && map[i - 1, j] is Field)
                            newMap[i - 1, j] = new Bullet(-1, 0, false);
                        else if (hasUp && !hasDown && i < map.GetLength(0) - 1 && map[i + 1, j] is Field)
                            newMap[i + 1, j] = new Bullet(1, 0, false);
                        else if (hasRight && !hasLeft && j > 0 && map[i, j - 1] is Field)
                            newMap[i, j - 1] = new Bullet(0, -1, false);
                        else if (hasLeft && !hasRight && j < map.GetLength(1) - 1 && map[i, j + 1] is Field)
                            newMap[i, j + 1] = new Bullet(0, 1, false);
                    }
                }
            }

            // ===== ВРАГИ =====
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Enemy enemy)
                    {
                        if (DateTime.Now < enemy.StunUntil) continue;

                        int newX = i, newY = j;
                        bool ignoreWalls = enemy.IsChasing && enemy.Ability == 4;

                        int targetX = -1, targetY = -1;
                        if (traps.Count > 0)
                        {
                            int bestDist = int.MaxValue;
                            foreach (var t in traps)
                            {
                                int dist = Math.Abs(t.x - i) + Math.Abs(t.y - j);
                                if (dist < bestDist) { bestDist = dist; targetX = t.x; targetY = t.y; }
                            }
                        }
                        else if (enemy.IsChasing && heroX != -1)
                        {
                            targetX = heroX;
                            targetY = heroY;
                        }

                        if (targetX != -1)
                        {
                            // Количество шагов за тик запрашивается у стратегии способности (паттерн Стратегия)
                            float mul = enemy.IsChasing ? enemy.SpeedMultiplier : 1.0f;
                            HunterAbility ability = HunterAbilityRegistry.Get(enemy.Ability);
                            int speed = ability != null ? ability.GetSteps(mul) : 1;

                            int curX = i, curY = j;

                            for (int step = 0; step < speed; step++)
                            {
                                int[] nextStep = GetNextStep(curX, curY, targetX, targetY, ignoreWalls);
                                if (nextStep == null) break;

                                if (enemy.IsChasing && enemy.Ability == 3 && step == 0 &&
                                    (DateTime.Now - enemy.LastShot).TotalSeconds >= 1.0)
                                {
                                    int sdx = nextStep[0] - curX;
                                    int sdy = nextStep[1] - curY;
                                    int bx = curX + sdx, by = curY + sdy;
                                    if (bx >= 0 && bx < map.GetLength(0) && by >= 0 && by < map.GetLength(1) &&
                                        newMap[bx, by] is Field)
                                    {
                                        newMap[bx, by] = new Bullet(sdx, sdy, true);
                                        enemy.LastShot = DateTime.Now;
                                    }
                                }
                                curX = nextStep[0];
                                curY = nextStep[1];
                            }
                            newX = curX;
                            newY = curY;
                        }
                        else
                        {
                            int dir = rand.Next(4);
                            switch (dir)
                            {
                                case 0: newX = (newX - 1 + map.GetLength(0)) % map.GetLength(0); break;
                                case 1: newX = (newX + 1) % map.GetLength(0); break;
                                case 2: newY = (newY - 1 + map.GetLength(1)) % map.GetLength(1); break;
                                case 3: newY = (newY + 1) % map.GetLength(1); break;
                            }
                        }

                        if (newMap[newX, newY] is Enemy) continue;

                        if (newMap[newX, newY] is Wall w)
                        {
                            if (w.IsRobotCenter && w.IsPassableForHero) continue;

                            if (ignoreWalls)
                            {
                                if (w.IsRobot) continue;
                                newMap[newX, newY] = map[i, j];
                                newMap[i, j] = new Field();
                                continue;
                            }

                            if (w.IsTrap && !w.IsBlueprint) { newMap[newX, newY] = map[i, j]; newMap[i, j] = new Field(); }
                            else if (w.IsPlacedByPlayer) { enemy.StunUntil = DateTime.Now.AddSeconds(5); newMap[newX, newY] = map[i, j]; newMap[i, j] = new Field(); }
                            else { newMap[newX, newY] = map[i, j]; newMap[i, j] = new Field(); }
                        }
                        else if (newMap[newX, newY] is Tree t)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                        else if (newMap[newX, newY] is Field || newMap[newX, newY] is Hero || newMap[newX, newY] is Portal)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                    }
                }
            }

            // ===== ПУЛИ (летают быстрее — до 2 клеток за тик) =====
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Bullet b)
                    {
                        newMap[i, j] = new Field();
                        int cx = i, cy = j;
                        bool alive = true;

                        for (int step = 0; step < 2 && alive; step++)
                        {
                            int nx = cx + b.DirX;
                            int ny = cy + b.DirY;

                            if (nx < 0 || nx >= map.GetLength(0) || ny < 0 || ny >= map.GetLength(1))
                            {
                                alive = false;
                                break;
                            }

                            if (b.IsEnemyBullet)
                            {
                                if (newMap[nx, ny] is Hero) { newMap[nx, ny] = new Field(); alive = false; }
                                else if (newMap[nx, ny] is Wall cw && cw.IsRobot) { alive = false; }
                                else if (newMap[nx, ny] is Wall || newMap[nx, ny] is Tree) { newMap[nx, ny] = new Field(); alive = false; }
                                else if (newMap[nx, ny] is Field) { cx = nx; cy = ny; }
                                else alive = false;
                            }
                            else if (b.IsRobotBullet)
                            {
                                if (newMap[nx, ny] is Enemy)
                                {
                                    // Убийство засчитывается роботу-владельцу по ссылке на его центр:
                                    // так оно не теряется, даже если робот уехал, пока пуля летела
                                    if (b.OwnerRobot != null && b.OwnerRobot.IsRobot && b.OwnerRobot.IsRobotCenter)
                                    {
                                        b.OwnerRobot.RobotKills++;
                                    }
                                    newMap[nx, ny] = new Field();
                                    alive = false;
                                }
                                else if (newMap[nx, ny] is Wall w2 && !w2.IsRobot) { newMap[nx, ny] = new Field(); alive = false; }
                                else if (newMap[nx, ny] is Tree) { newMap[nx, ny] = new Field(); alive = false; }
                                else if (newMap[nx, ny] is Portal) { newMap[nx, ny] = new Portal(); alive = false; }
                                else if (newMap[nx, ny] is Field) { cx = nx; cy = ny; }
                                else alive = false;
                            }
                            else
                            {
                                if (newMap[nx, ny] is Enemy) { newMap[nx, ny] = new Field(); alive = false; }
                                else if (newMap[nx, ny] is Field) { cx = nx; cy = ny; }
                                else alive = false;
                            }
                        }

                        if (alive && (cx != i || cy != j))
                            newMap[cx, cy] = b;
                    }
                }
            }

            Array.Copy(newMap, map, map.Length);
        }

        /// <summary>
        /// Перемещает героя в указанном направлении.
        /// </summary>
        /// <param name="key">Нажатая клавиша направления.</param>
        public void MovePersons(ConsoleKey key)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Hero)
                    {
                        int newX = i, newY = j;
                        switch (key)
                        {
                            case ConsoleKey.UpArrow: newX--; break;
                            case ConsoleKey.DownArrow: newX++; break;
                            case ConsoleKey.LeftArrow: newY--; break;
                            case ConsoleKey.RightArrow: newY++; break;
                        }
                        if (newX >= 0 && newX < map.GetLength(0) && newY >= 0 && newY < map.GetLength(1))
                        {
                            if (map[newX, newY] is Wall rw && rw.IsRobot) { }
                            else if (map[newX, newY] is Field || map[newX, newY] is Tree
                                || map[newX, newY] is Portal
                                || (map[newX, newY] is Wall w && w.IsPlacedByPlayer && !w.IsRobot))
                            {
                                map[newX, newY] = map[i, j];
                                map[i, j] = new Field();
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Двигает робота (весь 3×3) вместе с героем.
        /// </summary>
        /// <param name="centerX">Координата X текущего центра робота.</param>
        /// <param name="centerY">Координата Y текущего центра робота.</param>
        /// <param name="dx">Смещение по X.</param>
        /// <param name="dy">Смещение по Y.</param>
        /// <returns>True, если ход удался.</returns>
        public bool MoveRobot(int centerX, int centerY, int dx, int dy)
        {
            int ncx = centerX + dx;
            int ncy = centerY + dy;

            for (int di = -1; di <= 1; di++)
                for (int dj = -1; dj <= 1; dj++)
                {
                    int nx = ncx + di, ny = ncy + dj;
                    if (nx < 0 || nx >= map.GetLength(0) || ny < 0 || ny >= map.GetLength(1))
                        return false;
                }

            for (int di = -1; di <= 1; di++)
                for (int dj = -1; dj <= 1; dj++)
                {
                    int nx = ncx + di, ny = ncy + dj;

                    bool isCurrentRobotCell = false;
                    for (int ci = -1; ci <= 1; ci++)
                        for (int cj = -1; cj <= 1; cj++)
                            if (centerX + ci == nx && centerY + cj == ny)
                                isCurrentRobotCell = true;

                    if (isCurrentRobotCell) continue;

                    MapObject obj = map[nx, ny];
                    if (obj is Wall || obj is Hero || obj is Enemy)
                        return false;
                }

            MapObject[,] robotCells = new MapObject[3, 3];
            for (int di = -1; di <= 1; di++)
                for (int dj = -1; dj <= 1; dj++)
                    robotCells[di + 1, dj + 1] = map[centerX + di, centerY + dj];

            for (int di = -1; di <= 1; di++)
                for (int dj = -1; dj <= 1; dj++)
                    map[centerX + di, centerY + dj] = new Field();

            for (int di = -1; di <= 1; di++)
                for (int dj = -1; dj <= 1; dj++)
                {
                    MapObject obj = robotCells[di + 1, dj + 1];
                    if (obj is Wall w)
                    {
                        w.RobotCenterX = ncx;
                        w.RobotCenterY = ncy;
                    }
                    map[ncx + di, ncy + dj] = obj;
                }

            return true;
        }

        /// <summary>
        /// Проверяет, жив ли герой (есть ли он на карте).
        /// </summary>
        /// <returns>True, если герой найден.</returns>
        public bool IsHeroAlive()
        {
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Hero) return true;
                    if (map[i, j] is Wall w && w.IsRobotCenter && w.IsRobot && w.HeroInsideRobot) return true;
                }
            return false;
        }

        /// <summary>
        /// Проверяет, все ли враги убиты.
        /// </summary>
        /// <returns>True, если на карте нет врагов.</returns>
        public bool IsLevelCleared()
        {
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Enemy) return false;
            return true;
        }

        /// <summary>
        /// Создает портал в центре, если все враги мертвы и портала еще нет.
        /// </summary>
        public void SpawnPortalIfNeeded()
        {
            if (PortalX != -1) return;
            if (!IsLevelCleared()) return;

            if (map[HeroSpawnX, HeroSpawnY] is Field || map[HeroSpawnX, HeroSpawnY] is Hero)
            {
                map[HeroSpawnX, HeroSpawnY] = new Portal();
                PortalX = HeroSpawnX; PortalY = HeroSpawnY;
                // Оповещение подписчиков об открытии портала (паттерн Наблюдатель)
                GameEvents.RaisePortalSpawned(PortalX, PortalY);
                return;
            }

            for (int r = 1; r < map.GetLength(0); r++)
            {
                for (int di = -r; di <= r; di++)
                {
                    for (int dj = -r; dj <= r; dj++)
                    {
                        int nx = HeroSpawnX + di, ny = HeroSpawnY + dj;
                        if (nx < 0 || nx >= map.GetLength(0) || ny < 0 || ny >= map.GetLength(1)) continue;
                        if (map[nx, ny] is Field)
                        {
                            map[nx, ny] = new Portal();
                            PortalX = nx; PortalY = ny;
                            // Оповещение подписчиков об открытии портала (паттерн Наблюдатель)
                            GameEvents.RaisePortalSpawned(PortalX, PortalY);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет, стоит ли герой на портале.
        /// </summary>
        /// <returns>True, если герой на портале.</returns>
        public bool IsHeroOnPortal()
        {
            if (PortalX == -1) return false;
            return map[PortalX, PortalY] is Hero;
        }

        /// <summary>
        /// Возвращает двумерный массив объектов карты.
        /// </summary>
        /// <returns>Массив объектов.</returns>
        public MapObject[,] GetMapArray() => map;

        /// <summary>
        /// Удаляет робота, если у его центра накопилось достаточно убийств.
        /// </summary>
        public void CheckRobotDestruction()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Wall center && center.IsRobotCenter && center.IsRobot
                        && center.RobotKills >= Wall.RobotMaxKills)
                    {
                        int cx = center.RobotCenterX;
                        int cy = center.RobotCenterY;
                        if (cx < 0 || cy < 0) continue;

                        bool heroInside = center.HeroInsideRobot;

                        for (int di = -1; di <= 1; di++)
                            for (int dj = -1; dj <= 1; dj++)
                            {
                                int nx = cx + di, ny = cy + dj;
                                if (nx < 0 || nx >= map.GetLength(0) || ny < 0 || ny >= map.GetLength(1)) continue;
                                if (map[nx, ny] is Wall w && w.IsRobot)
                                {
                                    if (heroInside && di == 0 && dj == 0)
                                        map[nx, ny] = new Hero(nx, ny);
                                    else
                                        map[nx, ny] = new Field();
                                }
                            }

                        // Оповещение подписчиков о разрушении робота (паттерн Наблюдатель)
                        GameEvents.RaiseRobotDestroyed();
                    }
                }
            }
        }

        /// <summary>
        /// Загружает карту из сохранения.
        /// </summary>
        /// <param name="data">Данные сохранения.</param>
        public void LoadMapFromSave(SaveData data)
        {
            HeroSpawnX = map.GetLength(0) / 2;
            HeroSpawnY = map.GetLength(1) / 2;
            PortalX = -1; PortalY = -1;
            CurrentLevel = data.Level > 0 ? data.Level : 1;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    char symbol = data.MapRows[i][j];
                    // Восстановление объекта через фабрику (паттерн Фабричный метод)
                    map[i, j] = MapObjectFactory.CreateFromSymbol(symbol, i, j);
                }
            }

            SetupLevel();
        }
    }
}