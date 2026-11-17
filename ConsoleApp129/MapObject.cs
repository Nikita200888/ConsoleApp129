using System;

namespace ConsoleApp129
{
    /// <summary>
    /// Абстрактный базовый класс для всех объектов на карте.
    /// </summary>
    internal abstract class MapObject
    {
        /// <summary>
        /// Возвращает символ для отрисовки объекта на консольной карте.
        /// </summary>
        /// <returns>Символ для отображения.</returns>
        public abstract char Rendering_on_the_map();
    }

    /// <summary>
    /// Класс, представляющий стену (или турель, или приманку).
    /// </summary>
    internal class Wall : MapObject
    {
        /// <summary>Указывает, поставлена ли стена игроком.</summary>
        public bool IsPlacedByPlayer = false;
        /// <summary>Указывает, является ли стена турелью.</summary>
        public bool IsTurret = false;
        /// <summary>Указывает, является ли стена приманкой.</summary>
        public bool IsTrap = false;
        /// <summary>Время последнего выстрела (для турели).</summary>
        public DateTime LastShot = DateTime.Now;
        /// <summary>Время, с которого объект начинает работать.</summary>
        public DateTime ReadyTime = DateTime.MinValue;

        /// <summary>Указывает, является ли объект частью строящегося чертежа.</summary>
        public bool IsBlueprint = false;
        /// <summary>Количество заполненных клеток чертежа.</summary>
        public int BuildProgress = 0;
        /// <summary>Общее количество клеток чертежа.</summary>
        public int BuildTotal = 0;

        /// <summary>
        /// Проверяет, готов ли объект к работе (прошло ли время ReadyTime).
        /// </summary>
        public bool IsReady => DateTime.Now >= ReadyTime;

        /// <summary>
        /// Возвращает символ для отрисовки стены.
        /// </summary>
        /// <returns>Символ '+'.</returns>
        public override char Rendering_on_the_map()
        {
            if (IsBlueprint)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                return '+';
            }
            if (IsPlacedByPlayer) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.Cyan;
            return '+';
        }
    }

    /// <summary>
    /// Класс, представляющий пустую клетку.
    /// </summary>
    internal class Field : MapObject
    {
        /// <summary>
        /// Возвращает символ для отрисовки пустой клетки.
        /// </summary>
        /// <returns>Символ '.'.</returns>
        public override char Rendering_on_the_map() { Console.ForegroundColor = ConsoleColor.DarkGreen; return '.'; }
    }

    /// <summary>
    /// Класс, представляющий дерево на карте.
    /// </summary>
    internal class Tree : MapObject
    {
        /// <summary>
        /// Возвращает символ для отрисовки дерева.
        /// </summary>
        /// <returns>Символ 'T'.</returns>
        public override char Rendering_on_the_map() { Console.ForegroundColor = ConsoleColor.Green; return 'T'; }
    }

    /// <summary>
    /// Класс, представляющий пулю.
    /// </summary>
    internal class Bullet : MapObject
    {
        /// <summary>Направление движения пули по X.</summary>
        public int DirX;
        /// <summary>Направление движения пули по Y.</summary>
        public int DirY;
        /// <summary>Указывает, выпущена ли пуля врагом.</summary>
        public bool IsEnemyBullet;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Bullet"/>.
        /// </summary>
        /// <param name="dx">Направление по X.</param>
        /// <param name="dy">Направление по Y.</param>
        /// <param name="isEnemy">Признак вражеской пули.</param>
        public Bullet(int dx, int dy, bool isEnemy = false)
        {
            DirX = dx; DirY = dy; IsEnemyBullet = isEnemy;
        }

        /// <summary>
        /// Возвращает символ для отрисовки пули.
        /// </summary>
        /// <returns>Символ '*'.</returns>
        public override char Rendering_on_the_map()
        {
            if (IsEnemyBullet) { Console.ForegroundColor = ConsoleColor.Red; return '*'; }
            else { Console.ForegroundColor = ConsoleColor.White; return '*'; }
        }
    }
}