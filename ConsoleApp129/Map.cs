using System;

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

        // ХОД ВРАГОВ (остается как было)
        public void MovePersons()
        {
            MapObject[,] newMap = new MapObject[map.GetLength(0), map.GetLength(1)];
            Array.Copy(map, newMap, map.Length);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Enemy enemy)
                    {
                        if (DateTime.Now < enemy.StunUntil) continue;

                        int direction = rand.Next(4);
                        int newX = i, newY = j;

                        switch (direction)
                        {
                            case 0: newX = (i - 1 + map.GetLength(0)) % map.GetLength(0); break;
                            case 1: newX = (i + 1) % map.GetLength(0); break;
                            case 2: newY = (j - 1 + map.GetLength(1)) % map.GetLength(1); break;
                            case 3: newY = (j + 1) % map.GetLength(1); break;
                        }

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

        // ХОД ГЕРОЯ (теперь он реально меняет свои координаты)
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

                        // Проверка на выход за края
                        if (newX >= 0 && newX < map.GetLength(0) && newY >= 0 && newY < map.GetLength(1))
                        {
                            if (map[newX, newY] is Field || map[newX, newY] is Wall)
                            {
                                // СНАЧАЛА ставим героя на новое место
                                map[newX, newY] = map[i, j];
                                // ПОТОМ стираем его со старого (ставим траву)
                                map[i, j] = new Field();
                                return; // Выходим, чтобы не двигать других героев (если они вдруг есть)
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