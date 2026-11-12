using System;

namespace ConsoleApp129
{
    internal abstract class MapObject
    {
        public abstract char Rendering_on_the_map();
    }

    internal class Wall : MapObject
    {
        public bool IsPlacedByPlayer = false;

        public override char Rendering_on_the_map()
        {
            if (IsPlacedByPlayer)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                return '+';
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                return '+';
            }
        }
    }

    internal class Field : MapObject
    {
        public override char Rendering_on_the_map()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            return '.';
        }
    }

    internal class Tree : MapObject
    {
        public override char Rendering_on_the_map()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            return 'T';
        }
    }
}