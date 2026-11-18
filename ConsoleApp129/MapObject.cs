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
    /// Класс, представляющий стену (или турель, приманку, робота).
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

        // ===== РОБОТ =====

        /// <summary>Указывает, является ли стена частью робота.</summary>
        public bool IsRobot = false;
        /// <summary>Указывает, является ли клетка центральной (место героя).</summary>
        public bool IsRobotCenter = false;
        /// <summary>Указывает, является ли клетка пушкой робота.</summary>
        public bool IsRobotGun = false;
        /// <summary>Указывает, проходима ли клетка для героя.</summary>
        public bool IsPassableForHero = false;
        /// <summary>Указывает, находится ли герой внутри робота.</summary>
        public bool HeroInsideRobot = false;
        /// <summary>Количество убийств, совершённых роботом.</summary>
        public int RobotKills = 0;
        /// <summary>Максимальное количество убийств до разрушения робота.</summary>
        public const int RobotMaxKills = 2;
        /// <summary>Координата X центра робота.</summary>
        public int RobotCenterX = -1;
        /// <summary>Координата Y центра робота.</summary>
        public int RobotCenterY = -1;

        /// <summary>
        /// Проверяет, готов ли объект к работе (прошло ли время ReadyTime).
        /// </summary>
        public bool IsReady => DateTime.Now >= ReadyTime;

        /// <summary>
        /// Возвращает символ для отрисовки стены (с учетом робота).
        /// </summary>
        /// <returns>Символ для отображения.</returns>
        public override char Rendering_on_the_map()
        {
            if (IsRobot)
            {
                if (IsRobotGun) { Console.ForegroundColor = ConsoleColor.Red; return '¤'; }
                if (IsRobotCenter) { Console.ForegroundColor = ConsoleColor.Cyan; return '¤'; }
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                return '#';
            }
            if (IsBlueprint) { Console.ForegroundColor = ConsoleColor.DarkGray; return '+'; }
            Console.ForegroundColor = IsPlacedByPlayer ? ConsoleColor.Yellow : ConsoleColor.Cyan;
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
    /// Класс, представляющий портал перехода на следующий уровень.
    /// </summary>
    internal class Portal : MapObject
    {
        /// <summary>
        /// Возвращает символ для отрисовки портала.
        /// </summary>
        /// <returns>Символ 'O'.</returns>
        public override char Rendering_on_the_map() { Console.ForegroundColor = ConsoleColor.Magenta; return 'O'; }
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
        /// <summary>Указывает, выпущена ли пуля роботом.</summary>
        public bool IsRobotBullet = false;
        /// <summary>Центр робота-владельца (для счета убийств, даже если робот уехал).</summary>
        public Wall OwnerRobot = null;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Bullet"/>.
        /// </summary>
        /// <param name="dx">Направление по X.</param>
        /// <param name="dy">Направление по Y.</param>
        /// <param name="isEnemy">Признак вражеской пули.</param>
        /// <param name="isRobot">Признак пули робота.</param>
        public Bullet(int dx, int dy, bool isEnemy = false, bool isRobot = false)
        {
            DirX = dx; DirY = dy; IsEnemyBullet = isEnemy; IsRobotBullet = isRobot;
        }

        /// <summary>
        /// Возвращает символ для отрисовки пули.
        /// </summary>
        /// <returns>Символ '*'.</returns>
        public override char Rendering_on_the_map()
        {
            if (IsRobotBullet) { Console.ForegroundColor = ConsoleColor.Cyan; return '¤'; }
            if (IsEnemyBullet) { Console.ForegroundColor = ConsoleColor.Red; return '*'; }
            Console.ForegroundColor = ConsoleColor.White; return '*';
        }
    }
}