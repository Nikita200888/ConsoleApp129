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
                return '+'; // Сделал блок плотнее, чтобы отличать от ресурса
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
        public bool IsWeapon = false;

        public override char Rendering_on_the_map()
        {
            if (IsWeapon)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                return 'T';
            }
            Console.ForegroundColor = ConsoleColor.Green;
            return 'T';
        }
    }
}