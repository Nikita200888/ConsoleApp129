using System;
using System.Collections.Generic;

namespace ConsoleApp129
{
    internal class Map
    {
        Random rand = new Random();
        MapObject[,] map = new MapObject[25, 25];

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

        private int[] GetNextStep(int startX, int startY, int targetX, int targetY)
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
                    while (curr != (-1, -1) && curr != (startX, startY))
                    {
                        path.Add(curr);
                        curr = cameFrom[curr];
                    }

                    if (path.Count > 0)
                    {
                        path.Reverse();
                        return new int[] { path[0].Item1, path[0].Item2 };
                    }
                    return null;
                }

                for (int i = 0; i < 4; i++)
                {
                    int nx = current.x + dx[i];
                    int ny = current.y + dy[i];

                    if (nx >= 0 && nx < map.GetLength(0) && ny >= 0 && ny < map.GetLength(1))
                    {
                        if (map[nx, ny] is Field || map[nx, ny] is Hero)
                        {
                            if (!cameFrom.ContainsKey((nx, ny)))
                            {
                                queue.Enqueue((nx, ny));
                                cameFrom[(nx, ny)] = current;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void MovePersons()
        {
            MapObject[,] newMap = new MapObject[map.GetLength(0), map.GetLength(1)];
            Array.Copy(map, newMap, map.Length);

            int heroX = -1, heroY = -1;
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Hero) { heroX = i; heroY = j; }

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Enemy enemy)
                    {
                        if (DateTime.Now < enemy.StunUntil) continue;

                        int newX = i, newY = j;

                        if (enemy.IsChasing && heroX != -1 && heroY != -1)
                        {
                            int[] nextStep = GetNextStep(i, j, heroX, heroY);
                            if (nextStep != null)
                            {
                                newX = nextStep[0];
                                newY = nextStep[1];
                            }
                        }
                        else
                        {
                            int direction = rand.Next(4);
                            switch (direction)
                            {
                                case 0: newX = (i - 1 + map.GetLength(0)) % map.GetLength(0); break;
                                case 1: newX = (i + 1) % map.GetLength(0); break;
                                case 2: newY = (j - 1 + map.GetLength(1)) % map.GetLength(1); break;
                                case 3: newY = (j + 1) % map.GetLength(1); break;
                            }
                        }

                        if (newMap[newX, newY] is Enemy) continue; // Не наступаем на других врагов

                        if (newMap[newX, newY] is Wall w)
                        {
                            if (w.IsPlacedByPlayer)
                                enemy.StunUntil = DateTime.Now.AddSeconds(5);

                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                        else if (newMap[newX, newY] is Field)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                        else if (newMap[newX, newY] is Hero)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                    }
                }
            }
            Array.Copy(newMap, map, map.Length);
        }

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

        public bool IsHeroAlive()
        {
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Hero) return true;
            return false;
        }

        public MapObject[,] GetMapArray() => map;

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