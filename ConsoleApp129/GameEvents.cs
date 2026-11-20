using System;

namespace ConsoleApp129
{
    /// <summary>
    /// Шина игровых событий (паттерн Наблюдатель).
    /// Игровая логика (Map, Program) публикует события через методы Raise...,
    /// а подписчики (GameUI, SaveManager) реагируют на них, не зная друг о друге.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Событие: началась охота (передается номер способности охотника).</summary>
        public static event Action<int> HuntStarted;

        /// <summary>Событие: охота закончилась.</summary>
        public static event Action HuntEnded;

        /// <summary>Событие: герой погиб.</summary>
        public static event Action HeroDied;

        /// <summary>Событие: открыт портал на следующий уровень (передаются координаты).</summary>
        public static event Action<int, int> PortalSpawned;

        /// <summary>Событие: робот разрушен.</summary>
        public static event Action RobotDestroyed;

        /// <summary>Событие: враг построил своего робота (уровни 2+).</summary>
        public static event Action EnemyRobotBuilt;

        /// <summary>
        /// Публикует событие начала охоты.
        /// </summary>
        /// <param name="abilityId">Номер способности охотника (1-5).</param>
        public static void RaiseHuntStarted(int abilityId)
        {
            if (HuntStarted != null) HuntStarted(abilityId);
        }

        /// <summary>
        /// Публикует событие окончания охоты.
        /// </summary>
        public static void RaiseHuntEnded()
        {
            if (HuntEnded != null) HuntEnded();
        }

        /// <summary>
        /// Публикует событие смерти героя.
        /// </summary>
        public static void RaiseHeroDied()
        {
            if (HeroDied != null) HeroDied();
        }

        /// <summary>
        /// Публикует событие появления портала.
        /// </summary>
        /// <param name="x">Координата X портала.</param>
        /// <param name="y">Координата Y портала.</param>
        public static void RaisePortalSpawned(int x, int y)
        {
            if (PortalSpawned != null) PortalSpawned(x, y);
        }

        /// <summary>
        /// Публикует событие разрушения робота.
        /// </summary>
        public static void RaiseRobotDestroyed()
        {
            if (RobotDestroyed != null) RobotDestroyed();
        }

        /// <summary>
        /// Публикует событие постройки робота врагом.
        /// </summary>
        public static void RaiseEnemyRobotBuilt()
        {
            if (EnemyRobotBuilt != null) EnemyRobotBuilt();
        }
    }
}
