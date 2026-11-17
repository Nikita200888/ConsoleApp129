using System;

namespace ConsoleApp129
{
    /// <summary>
    /// Базовый класс для всех персонажей на карте.
    /// </summary>
    internal class Person : MapObject
    {
        /// <summary>Координата X персонажа.</summary>
        public int X;
        /// <summary>Координата Y персонажа.</summary>
        public int Y;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Person"/>.
        /// </summary>
        /// <param name="X">Начальная координата X.</param>
        /// <param name="Y">Начальная координата Y.</param>
        public Person(int X, int Y) { this.X = X; this.Y = Y; }

        /// <summary>
        /// Возвращает символ для отрисовки персонажа на карте.
        /// </summary>
        /// <returns>Символ '☺'.</returns>
        public override char Rendering_on_the_map() { return '☺'; }
    }

    /// <summary>
    /// Класс, представляющий игрового героя.
    /// </summary>
    internal class Hero : Person
    {
        /// <summary>Цвет героя на карте.</summary>
        public static ConsoleColor HeroColor = ConsoleColor.Yellow;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Hero"/>.
        /// </summary>
        /// <param name="X">Начальная координата X.</param>
        /// <param name="Y">Начальная координата Y.</param>
        public Hero(int X, int Y) : base(X, Y) { }

        /// <summary>
        /// Возвращает символ для отрисовки героя.
        /// </summary>
        /// <returns>Символ '☻'.</returns>
        public override char Rendering_on_the_map() { Console.ForegroundColor = HeroColor; return '☻'; }
    }

    /// <summary>
    /// Класс, представляющий врага на карте.
    /// </summary>
    internal class Enemy : Person
    {
        /// <summary>Время, до которого враг оглушен.</summary>
        public DateTime StunUntil = DateTime.MinValue;
        /// <summary>Время последнего выстрела врага.</summary>
        public DateTime LastShot = DateTime.MinValue;
        /// <summary>Флаг, указывающий, преследует ли враг героя.</summary>
        public bool IsChasing = false;
        /// <summary>Номер активной способности (1-5).</summary>
        public int Ability = 0;
        /// <summary>Флаг, указывающий, телепортировался ли враг.</summary>
        public bool HasTeleported = false;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Enemy"/>.
        /// </summary>
        /// <param name="X">Начальная координата X.</param>
        /// <param name="Y">Начальная координата Y.</param>
        public Enemy(int X, int Y) : base(X, Y) { }

        /// <summary>
        /// Возвращает символ для отрисовки врага с учетом его состояния и способности.
        /// </summary>
        /// <returns>Символ '☻'.</returns>
        public override char Rendering_on_the_map()
        {
            if (DateTime.Now < StunUntil) { Console.ForegroundColor = ConsoleColor.Blue; return '☻'; }

            if (IsChasing)
            {
                switch (Ability)
                {
                    case 1: Console.ForegroundColor = ConsoleColor.Red; break;
                    case 2: Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                    case 3: Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case 4: Console.ForegroundColor = ConsoleColor.White; break;
                    case 5: Console.ForegroundColor = ConsoleColor.Green; break;
                    default: Console.ForegroundColor = ConsoleColor.Magenta; break;
                }
                return '☻';
            }

            Console.ForegroundColor = ConsoleColor.Red;
            return base.Rendering_on_the_map();
        }
    }
}