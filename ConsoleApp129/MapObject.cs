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
        public bool IsTurret = false;
        public bool IsTrap = false;
        public DateTime LastShot = DateTime.Now;
        public DateTime ReadyTime = DateTime.MinValue;

        // Новые флаги для "строительства"
        public bool IsBlueprint = false; // Это часть строящегося объекта
        public int BuildProgress = 0;    // Сколько клеток заполнено
        public int BuildTotal = 0;       // Сколько всего нужно

        public bool IsReady => DateTime.Now >= ReadyTime;

        public override char Rendering_on_the_map()
        {
            if (IsBlueprint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray; // Непостроенная часть
                return '+';
            }
            if (IsPlacedByPlayer) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.Cyan;
            return '+';
        }
    }

    internal class Field : MapObject
    {
        public override char Rendering_on_the_map() { Console.ForegroundColor = ConsoleColor.DarkGreen; return '.'; }
    }

    internal class Tree : MapObject
    {
        public override char Rendering_on_the_map() { Console.ForegroundColor = ConsoleColor.Green; return 'T'; }
    }

    internal class Bullet : MapObject
    {
        public int DirX, DirY;
        public bool IsEnemyBullet;
        public Bullet(int dx, int dy, bool isEnemy = false)
        {
            DirX = dx; DirY = dy; IsEnemyBullet = isEnemy;
        }
        public override char Rendering_on_the_map()
        {
            if (IsEnemyBullet) { Console.ForegroundColor = ConsoleColor.Red; return '*'; }
            else { Console.ForegroundColor = ConsoleColor.White; return '*'; }
        }
    }
}