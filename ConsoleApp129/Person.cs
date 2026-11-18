using System;

namespace ConsoleApp129
{
    /// <summary>
    /// Базовый класс для всех персонажей на карте.
    /// </summary>
    internal class Person : MapObject
    {
        public int X;
        public int Y;

        public Person(int X, int Y) { this.X = X; this.Y = Y; }

        public override char Rendering_on_the_map() { return '☺'; }
    }

    /// <summary>
    /// Игровой герой.
    /// </summary>
    internal class Hero : Person
    {
        public static ConsoleColor HeroColor = ConsoleColor.Yellow;

        public Hero(int X, int Y) : base(X, Y) { }

        public override char Rendering_on_the_map() { Console.ForegroundColor = HeroColor; return '☻'; }
    }

    /// <summary>
    /// Враг на карте.
    /// </summary>
    internal class Enemy : Person
    {
        public DateTime StunUntil = DateTime.MinValue;
        public DateTime LastShot = DateTime.MinValue;
        public bool IsChasing = false;
        public int Ability = 0;
        public bool HasTeleported = false;

        /// <summary>Множитель скорости (на уровнях >= 2 = 1.3).</summary>
        public float SpeedMultiplier = 1.0f;

        public Enemy(int X, int Y) : base(X, Y) { }

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