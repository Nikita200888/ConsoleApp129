using System;

namespace ConsoleApp129
{
    /// <summary>
    /// Статический класс отрисовки правой панели игрового интерфейса.
    /// Исправляет баг наложения надписей: каждая строка панели дополняется пробелами
    /// до фиксированной ширины, поэтому остатки старого текста полностью затираются.
    /// Класс также подписан на игровые события (паттерн Наблюдатель) и выводит их
    /// в отведенной строке «СОБЫТИЕ».
    /// </summary>
    internal static class GameUI
    {
        /// <summary>Колонка, с которой начинается панель интерфейса.</summary>
        public const int PanelX = 52;

        /// <summary>Ширина строки панели (для затирания старого текста).</summary>
        public const int PanelWidth = 28;

        /// <summary>Строка панели, в которой выводятся события игры.</summary>
        private const int EventLine = 18;

        /// <summary>Строка панели для временных сообщений, ожидающих нажатия клавиши.</summary>
        private const int MessageLine = 20;

        /// <summary>Текст последнего события игры.</summary>
        private static string lastEvent = "";

        /// <summary>
        /// Выводит одну строку панели, дополняя ее пробелами до полной ширины.
        /// </summary>
        /// <param name="line">Номер строки панели.</param>
        /// <param name="text">Текст надписи.</param>
        /// <param name="color">Цвет текста.</param>
        public static void Write(int line, string text, ConsoleColor color)
        {
            if (text.Length > PanelWidth) text = text.Substring(0, PanelWidth);
            Console.SetCursorPosition(PanelX, line);
            Console.ForegroundColor = color;
            Console.Write(text.PadRight(PanelWidth));
            Console.ResetColor();
        }

        /// <summary>
        /// Запоминает событие игры для вывода в строке «СОБЫТИЕ».
        /// </summary>
        /// <param name="text">Текст события.</param>
        public static void SetEvent(string text)
        {
            lastEvent = text;
        }

        /// <summary>
        /// Показывает временное сообщение в строке событий и ждет нажатия клавиши.
        /// Используется вместо Console.WriteLine, чтобы сообщение не накладывалось на карту.
        /// </summary>
        /// <param name="text">Текст сообщения.</param>
        public static void ShowMessage(string text)
        {
            Write(MessageLine, text, ConsoleColor.Red);
            Console.ReadKey(true);
            Write(MessageLine, "", ConsoleColor.Gray);
        }

        /// <summary>
        /// Подписывает интерфейс на события игры (паттерн Наблюдатель).
        /// Вызывается один раз при старте программы.
        /// </summary>
        public static void SubscribeToGameEvents()
        {
            GameEvents.HuntStarted += delegate (int abilityId)
            {
                HunterAbility ability = HunterAbilityRegistry.Get(abilityId);
                SetEvent("ОХОТА! " + (ability != null ? ability.Name : ""));
            };
            GameEvents.HuntEnded += delegate () { SetEvent("Охота закончилась"); };
            GameEvents.PortalSpawned += delegate (int x, int y) { SetEvent("Портал открыт! Иди к O"); };
            GameEvents.RobotDestroyed += delegate () { SetEvent("Робот уничтожен!"); };
        }

        /// <summary>
        /// Отрисовывает всю панель состояния игры справа от карты.
        /// Все строки (0-20) перерисовываются каждый кадр, что исключает наложение надписей.
        /// </summary>
        /// <param name="level">Текущий уровень.</param>
        /// <param name="playerWalls">Количество стен («+») у игрока.</param>
        /// <param name="hasWeapon">Есть ли у героя оружие (дерево).</param>
        /// <param name="buildingMode">Активный режим строительства (0 — нет).</param>
        /// <param name="buildFilled">Сколько клеток чертежа заполнено.</param>
        /// <param name="robotUnderHero">Центр робота, если герой внутри робота.</param>
        /// <param name="currentMap">Массив карты.</param>
        /// <param name="heroX">Координата X героя.</param>
        /// <param name="heroY">Координата Y героя.</param>
        /// <param name="huntActive">Идет ли сейчас охота.</param>
        public static void DrawStatusPanel(int level, int playerWalls, bool hasWeapon,
            int buildingMode, int buildFilled, Wall robotUnderHero,
            MapObject[,] currentMap, int heroX, int heroY, bool huntActive)
        {
            Write(0, "УРОВЕНЬ: " + level, ConsoleColor.Cyan);
            Write(1, "", ConsoleColor.Gray);
            Write(2, "Стены (+): " + playerWalls, ConsoleColor.White);
            Write(3, hasWeapon ? "Оружие: дерево" : "Оружие: нет",
                hasWeapon ? ConsoleColor.Green : ConsoleColor.Gray);
            Write(4, "", ConsoleColor.Gray);

            if (buildingMode > 0)
            {
                string name = buildingMode == 2 ? "КВАДРАТ" : buildingMode == 3 ? "ТУРЕЛЬ"
                    : buildingMode == 4 ? "ПРИМАНКА" : "РОБОТ";
                Write(5, "РЕЖИМ: " + name, ConsoleColor.Yellow);
                Write(6, "Прогресс: " + buildFilled + "/" + GetBuildTotal(buildingMode), ConsoleColor.Yellow);
                Write(7, "Space — строить", ConsoleColor.Gray);
                Write(8, "Esc — отмена", ConsoleColor.Gray);
                Write(9, "", ConsoleColor.Gray);
                Write(10, "", ConsoleColor.Gray);
                Write(11, "", ConsoleColor.Gray);
            }
            else
            {
                Write(5, "1 Стена (+)", ConsoleColor.Gray);
                Write(6, "2 Квадрат (8)", ConsoleColor.Gray);
                Write(7, "3 Турель (5)", ConsoleColor.Gray);
                Write(8, "4 Приманка (6)", ConsoleColor.Gray);
                Write(9, "5 Робот (9)", ConsoleColor.Gray);
                Write(10, "F Удар", ConsoleColor.Gray);
                Write(11, "Esc Меню", ConsoleColor.Gray);
            }

            Write(12, "", ConsoleColor.Gray);
            DrawStatusArea(robotUnderHero, currentMap, heroX, heroY, huntActive);
            Write(17, "", ConsoleColor.Gray);
            Write(EventLine, lastEvent, ConsoleColor.DarkYellow);
            Write(19, "", ConsoleColor.Gray);
            Write(MessageLine, "", ConsoleColor.Gray);
        }

        /// <summary>
        /// Отрисовывает блок статуса (робот / охота / подсказки).
        /// </summary>
        private static void DrawStatusArea(Wall robotUnderHero, MapObject[,] currentMap,
            int heroX, int heroY, bool huntActive)
        {
            if (robotUnderHero != null)
            {
                Write(13, "В РОБОТЕ!", ConsoleColor.Cyan);
                Write(14, "Убийств: " + robotUnderHero.RobotKills + "/" + Wall.RobotMaxKills, ConsoleColor.Cyan);
                Write(15, "E — стрелять", ConsoleColor.Gray);
                Write(16, "Стрелки — ехать", ConsoleColor.Gray);
                return;
            }

            if (huntActive)
            {
                Write(13, "ОХОТА!", ConsoleColor.Magenta);
                Write(14, FindAbilityName(currentMap), ConsoleColor.Magenta);
                Write(15, "", ConsoleColor.Gray);
                Write(16, "", ConsoleColor.Gray);
                return;
            }

            if (IsNearRobot(currentMap, heroX, heroY))
            {
                Write(13, "Рядом робот", ConsoleColor.Green);
                Write(14, "E — сесть", ConsoleColor.Gray);
                Write(15, "", ConsoleColor.Gray);
                Write(16, "", ConsoleColor.Gray);
                return;
            }

            Write(13, "", ConsoleColor.Gray);
            Write(14, "E — сесть в робота", ConsoleColor.Gray);
            Write(15, "", ConsoleColor.Gray);
            Write(16, "", ConsoleColor.Gray);
        }

        /// <summary>
        /// Проверяет, есть ли робот рядом с героем (в радиусе 2 клеток).
        /// </summary>
        private static bool IsNearRobot(MapObject[,] map, int heroX, int heroY)
        {
            if (heroX < 0) return false;
            for (int di = -2; di <= 2; di++)
                for (int dj = -2; dj <= 2; dj++)
                {
                    int cx = heroX + di, cy = heroY + dj;
                    if (cx < 0 || cx >= map.GetLength(0) || cy < 0 || cy >= map.GetLength(1)) continue;
                    if (map[cx, cy] is Wall rw && rw.IsRobot) return true;
                }
            return false;
        }

        /// <summary>
        /// Возвращает название активной способности охотника через реестр стратегий.
        /// </summary>
        private static string FindAbilityName(MapObject[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i, j] is Enemy e && e.IsChasing)
                    {
                        HunterAbility ability = HunterAbilityRegistry.Get(e.Ability);
                        if (ability != null) return ability.Name;
                    }
            return "";
        }

        /// <summary>
        /// Возвращает количество стен, необходимое для постройки выбранной фигуры.
        /// </summary>
        /// <param name="mode">Режим строительства (2 — квадрат, 3 — турель, 4 — приманка, 5 — робот).</param>
        /// <returns>Число стен.</returns>
        public static int GetBuildTotal(int mode)
        {
            if (mode == 2) return 8;
            if (mode == 3) return 5;
            if (mode == 4) return 6;
            if (mode == 5) return 9;
            return 0;
        }
    }
}
