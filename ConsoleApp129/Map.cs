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

        /// <summary>
        /// Генерирует новую карту со случайным расположением объектов.
        /// </summary>
        public void Map_generation()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    int A = rand.Next(100);
                    map[i, j] = new Field();
                    if (A > 1 && A < 15) map[i, j] = new Wall();
                    if (A < 1) map[i, j] = new Enemy(i, j);
                    if (A > 15 && A < 20) map[i, j] = new Tree();
                    if (i == map.GetLength(0) / 2 && j == map.GetLength(1) / 2) map[i, j] = new Hero(i, j);
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
        /// <param name="startX">Начальная координата X.</param>
        /// <param name="startY">Начальная координата Y.</param>
        /// <param name="targetX">Целевая координата X.</param>
        /// <param name="targetY">Целевая координата Y.</param>
        /// <param name="ignoreWalls">Игнорировать ли стены при поиске пути.</param>
        /// <returns>Массив из двух координат следующего шага или null.</returns>
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
                        if (ignoreWalls || map[nx, ny] is Field || map[nx, ny] is Hero || (map[nx, ny] is Wall w && w.IsTrap))
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
                    if (map[i, j] is Hero) { heroX = i; heroY = j; }

            List<(int x, int y)> traps = new List<(int, int)>();
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Wall w && w.IsTrap && w.IsReady && !w.IsBlueprint)
                        traps.Add((i, j));

            // ===================== ТУРЕЛИ =====================
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Wall turret && turret.IsTurret && turret.IsReady)
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

            // ===================== ВРАГИ =====================
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
                            int speed = (enemy.IsChasing && enemy.Ability == 1) ? 2 : 1;
                            int curX = i, curY = j;

                            for (int step = 0; step < speed; step++)
                            {
                                int[] nextStep = GetNextStep(curX, curY, targetX, targetY, ignoreWalls);
                                if (nextStep == null) break;

                                if (enemy.IsChasing && enemy.Ability == 3 && step == 0 &&
                                    (DateTime.Now - enemy.LastShot).TotalSeconds >= 1.4)
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
                            if (ignoreWalls)
                            {
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
                            if (ignoreWalls) { newMap[newX, newY] = map[i, j]; newMap[i, j] = new Field(); }
                            else { newMap[newX, newY] = map[i, j]; newMap[i, j] = new Field(); }
                        }
                        else if (newMap[newX, newY] is Field || newMap[newX, newY] is Hero)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                    }
                }
            }

            // ===================== ПУЛИ =====================
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Bullet b)
                    {
                        int nx = i + b.DirX;
                        int ny = j + b.DirY;
                        newMap[i, j] = new Field();

                        if (nx >= 0 && nx < map.GetLength(0) && ny >= 0 && ny < map.GetLength(1))
                        {
                            if (b.IsEnemyBullet)
                            {
                                if (newMap[nx, ny] is Hero) newMap[nx, ny] = new Field();
                                else if (newMap[nx, ny] is Wall || newMap[nx, ny] is Tree) newMap[nx, ny] = new Field();
                                else if (newMap[nx, ny] is Field) newMap[nx, ny] = b;
                            }
                            else
                            {
                                if (newMap[nx, ny] is Enemy) newMap[nx, ny] = new Field();
                                else if (newMap[nx, ny] is Field) newMap[nx, ny] = b;
                            }
                        }
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
                            if (map[newX, newY] is Field || map[newX, newY] is Tree || map[newX, newY] is Wall)
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
        /// Проверяет, жив ли герой (есть ли он на карте).
        /// </summary>
        /// <returns>True, если герой найден.</returns>
        public bool IsHeroAlive()
        {
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Hero) return true;
            return false;
        }

        /// <summary>
        /// Возвращает двумерный массив объектов карты.
        /// </summary>
        /// <returns>Массив объектов.</returns>
        public MapObject[,] GetMapArray() => map;

        /// <summary>
        /// Загружает карту из сохранения.
        /// </summary>
        /// <param name="data">Данные сохранения.</param>
        public void LoadMapFromSave(SaveData data)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    char symbol = data.MapRows[i][j];
                    if (symbol == 'H') map[i, j] = new Hero(i, j);
                    else if (symbol == 'E') map[i, j] = new Enemy(i, j);
                    else if (symbol == 'W') map[i, j] = new Wall();
                    else if (symbol == 'T') map[i, j] = new Tree();
                    else map[i, j] = new Field();
                }
            }
        }
    }
}