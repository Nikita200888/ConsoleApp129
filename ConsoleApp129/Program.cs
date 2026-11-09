using System;

namespace ConsoleApp129
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            Map map = new Map();

            // Генерация карты (герой в центре, враги, стены, деревья)
            map.Map_generation();

            // Основной игровой цикл
            while (true)
            {
                Console.Clear();

                // Рисуем текущее состояние карты
                map.Drawing_the_map();

                Console.WriteLine();
                Console.WriteLine("Управление: стрелки. Выход: Esc");

                // Ждём нажатия клавиши
                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Escape)
                    break;

                // Ход героя (только по стрелкам)
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                    key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
                {
                    map.MovePersons(key);   // двигаем героя
                    map.MovePersons();      // двигаем врагов
                }
            }
        }
    }
}
