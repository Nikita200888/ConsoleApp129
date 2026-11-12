using System;

namespace ConsoleApp129
{
    internal class Person : MapObject
    {
        public int X;
        public int Y;

        public Person(int X, int Y)
        {
            this.X = X; this.Y = Y;
        }
        public override char Rendering_on_the_map()
        {
            return '☺';
        }
    }

    internal class Hero : Person
    {
        public static ConsoleColor HeroColor = ConsoleColor.Yellow;

        public Hero(int X, int Y) : base(X, Y) { }

        public override char Rendering_on_the_map()
        {
            Console.ForegroundColor = HeroColor;
            return '☻';
        }
    }

    internal class Enemy : Person
    {
        public DateTime StunUntil = DateTime.MinValue;

        public Enemy(int X, int Y) : base(X, Y) { }

        public override char Rendering_on_the_map()
        {
            if (DateTime.Now < StunUntil)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                return '☻';
            }

            Console.ForegroundColor = ConsoleColor.Red;
            return base.Rendering_on_the_map();
        }
    }
}