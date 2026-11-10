using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    if (A > 1 && A<6)
                    {
                        map[i, j] = new Wall();
                    }
                    if (A < 1)
                    {
                        map[i, j] = new Enemy(i, j);
                    }
                    if (A > 5 && A < 10)
                    {
                        map[i, j] = new Tree();
                    }
                    if (i == map.GetLength(0) / 2 && j == map.GetLength(1) / 2)
                    {
                        map[i, j] = new Hero(i, j);
                    }

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

        public void MovePersons()
        {
            MapObject[,] newMap = new MapObject[map.GetLength(0), map.GetLength(1)];
            Array.Copy(map, newMap, map.Length);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Enemy)
                    {
                        int direction = rand.Next(4);
                        int newX = i, newY = j;

                        switch (direction)
                        {
                            case 0: newX = (i - 1 + map.GetLength(0)) % map.GetLength(0); break;
                            case 1: newX = (i + 1) % map.GetLength(0); break;
                            case 2: newY = (j - 1 + map.GetLength(1)) % map.GetLength(1); break;
                            case 3: newY = (j + 1) % map.GetLength(1); break;
                        }

                        // Враг ходит на Field, ИЛИ НА ГЕРОЯ (чтобы убить его)
                        if (newMap[newX, newY] is Field)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                        else if (newMap[newX, newY] is Hero)
                        {
                            // ХАРДКОР: Враг встает на место героя и УБИВАЕТ его
                            // В массиве герой просто затирается врагом. 
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
            
            MapObject[,] newMap = new MapObject[map.GetLength(0), map.GetLength(1)];

            
            Array.Copy(map, newMap, map.Length);

            
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Hero)
                    {
                        
                        int newX = i, newY = j;
                        switch (key)
                        {
                            case ConsoleKey.UpArrow: 
                                newX = (i - 1 + map.GetLength(0)) % map.GetLength(0);
                                break;
                            case ConsoleKey.DownArrow: 
                                newX = (i + 1) % map.GetLength(0);
                                break;
                            case ConsoleKey.LeftArrow: 
                                newY = (j - 1 + map.GetLength(1)) % map.GetLength(1);
                                break;
                            case ConsoleKey.RightArrow:
                                newY = (j + 1) % map.GetLength(1);
                                break;
                        }

                        
                        if (newMap[newX, newY] is Field)
                        {
                            newMap[newX, newY] = map[i, j];
                            newMap[i, j] = new Field();
                        }
                    }
                }
            }

            
            Array.Copy(newMap, map, map.Length);

        }
        static Random random = new Random();

      
        public bool IsHeroAlive()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] is Hero) return true;
                }
            }
            return false;
        }        // Метод для получения текущего уровня
        public int GetLevel()
        {
            return 1; // Если найдешь переменную уровня, напиши return ТвоеНазваниеПеременной;
        }

        // Метод для установки уровня (для загрузки)
        public void SetLevel(int lvl)
        {
            // Если найдешь переменную уровня, напиши ТвоеНазваниеПеременной = lvl;
            // Пока оставляем пустым, чтобы компилировалось
        }
        // Метод для получения массива (чтобы сохранить)
        public MapObject[,] GetMapArray()
        {
            return map;
        }

        // Метод для загрузки карты из сохранения
        public void LoadMapFromSave(SaveData data)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    char symbol = data.MapRows[i][j];
                    // Восстанавливаем объекты по буквам
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
