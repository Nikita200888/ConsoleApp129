using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp129
{
    internal class Person : MapObject
    {
        int pointX;
        int pointY;

        public Person(int X,int Y)
        {
            pointX = X; pointY = Y; 
        }
        public override char Rendering_on_the_map()
        {
            return '☺';
        }
    }

    internal class Hero : Person
    {
        public Hero(int X, int Y) : base(X, Y)
        {

        }
        public override char Rendering_on_the_map()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            return '☻';
        }
    }

    internal class Enemy : Person
    {
        public Enemy(int X, int Y) : base(X, Y)
        {
        }
        public override char Rendering_on_the_map()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            return base.Rendering_on_the_map();
        }
    }
}
