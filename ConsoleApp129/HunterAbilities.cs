using System;
using System.Collections.Generic;

namespace ConsoleApp129
{
    /// <summary>
    /// Абстрактная стратегия способности врага-охотника (паттерн Стратегия).
    /// Каждая способность оформлена отдельным классом-стратегией, а клиентский код
    /// работает только с базовым типом и реестром стратегий.
    /// </summary>
    internal abstract class HunterAbility
    {
        /// <summary>Номер способности (1-5).</summary>
        public abstract int Id { get; }

        /// <summary>Название способности для вывода в интерфейсе.</summary>
        public abstract string Name { get; }

        /// <summary>
        /// Признак того, что способность действует на всех врагов карты,
        /// а не только на выбранную при старте охоты группу.
        /// </summary>
        public virtual bool TargetsAllEnemies
        {
            get { return false; }
        }

        /// <summary>
        /// Возвращает количество шагов охотника за один игровой тик.
        /// </summary>
        /// <param name="speedMultiplier">Множитель скорости, заданный уровнем.</param>
        /// <returns>Число шагов за тик.</returns>
        public virtual int GetSteps(float speedMultiplier)
        {
            return 1;
        }

        /// <summary>
        /// Применяет способность к группе охотников при старте охоты.
        /// Базовая реализация помечает врагов как охотников.
        /// </summary>
        /// <param name="positions">Список позиций врагов-охотников.</param>
        /// <param name="map">Двумерный массив карты.</param>
        /// <param name="heroX">Координата X героя (-1, если герой не найден).</param>
        /// <param name="heroY">Координата Y героя (-1, если герой не найден).</param>
        /// <param name="rnd">Генератор случайных чисел.</param>
        public virtual void ApplyToGroup(List<(int x, int y)> positions, MapObject[,] map,
            int heroX, int heroY, Random rnd)
        {
            foreach (var pos in positions)
            {
                if (map[pos.x, pos.y] is Enemy enemy)
                {
                    enemy.IsChasing = true;
                    enemy.Ability = Id;
                    enemy.HasTeleported = false;
                }
            }
        }
    }

    /// <summary>
    /// Способность «Скорость»: охотник бегает за героем в два раза быстрее.
    /// </summary>
    internal class SpeedHunterAbility : HunterAbility
    {
        /// <summary>Номер способности.</summary>
        public override int Id { get { return 1; } }

        /// <summary>Название способности.</summary>
        public override string Name { get { return "Скорость (Красный)"; } }

        /// <summary>
        /// Скорый охотник делает два шага за тик (с учетом множителя уровня).
        /// </summary>
        public override int GetSteps(float speedMultiplier)
        {
            int baseSpeed = 2;
            return Math.Max(1, (int)Math.Round(baseSpeed * speedMultiplier));
        }
    }

    /// <summary>
    /// Способность «Телепорт»: при старте охоты охотник один раз переносится
    /// на расстояние 8-12 клеток от героя.
    /// </summary>
    internal class TeleportHunterAbility : HunterAbility
    {
        /// <summary>Номер способности.</summary>
        public override int Id { get { return 2; } }

        /// <summary>Название способности.</summary>
        public override string Name { get { return "Телепорт (Фиолетовый)"; } }

        /// <summary>
        /// Переносит каждого охотника в случайную точку на расстоянии 8-12 клеток от героя.
        /// </summary>
        public override void ApplyToGroup(List<(int x, int y)> positions, MapObject[,] map,
            int heroX, int heroY, Random rnd)
        {
            base.ApplyToGroup(positions, map, heroX, heroY, rnd);
            if (heroX == -1) return;

            foreach (var pos in positions)
            {
                if (!(map[pos.x, pos.y] is Enemy enemy)) continue;

                for (int attempt = 0; attempt < 30; attempt++)
                {
                    double angle = rnd.NextDouble() * Math.PI * 2;
                    int dist = rnd.Next(8, 13);
                    int tx = heroX + (int)(Math.Cos(angle) * dist);
                    int ty = heroY + (int)(Math.Sin(angle) * dist);

                    if (tx >= 1 && tx < map.GetLength(0) - 1 && ty >= 1 && ty < map.GetLength(1) - 1
                        && map[tx, ty] is Field)
                    {
                        map[pos.x, pos.y] = new Field();
                        map[tx, ty] = enemy;
                        enemy.HasTeleported = true;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Способность «Стрельба»: охотник идет за героем и периодически стреляет в направлении хода.
    /// </summary>
    internal class ShooterHunterAbility : HunterAbility
    {
        /// <summary>Номер способности.</summary>
        public override int Id { get { return 3; } }

        /// <summary>Название способности.</summary>
        public override string Name { get { return "Стрельба (Желтый)"; } }
    }

    /// <summary>
    /// Способность «Фазовый проход»: охотник игнорирует препятствия (проходит сквозь стены).
    /// </summary>
    internal class PhaseHunterAbility : HunterAbility
    {
        /// <summary>Номер способности.</summary>
        public override int Id { get { return 4; } }

        /// <summary>Название способности.</summary>
        public override string Name { get { return "Фазовый проход (Белый)"; } }
    }

    /// <summary>
    /// Способность «Волна»: охотники переносятся в углы карты и наступают на героя оттуда.
    /// </summary>
    internal class WaveHunterAbility : HunterAbility
    {
        /// <summary>Номер способности.</summary>
        public override int Id { get { return 5; } }

        /// <summary>Название способности.</summary>
        public override string Name { get { return "Волна (Зеленый)"; } }

        /// <summary>
        /// «Волна» действует на всех врагов карты: каждый переносится в свой угол.
        /// </summary>
        public override bool TargetsAllEnemies
        {
            get { return true; }
        }

        /// <summary>
        /// Расставляет всех врагов по свободным углам карты (каждого в свой угол,
        /// но не более четырех). Если свободных углов меньше, чем врагов,
        /// оставшиеся враги остаются на своих местах.
        /// </summary>
        public override void ApplyToGroup(List<(int x, int y)> positions, MapObject[,] map,
            int heroX, int heroY, Random rnd)
        {
            int[] cornerX = { 1, 1, map.GetLength(0) - 2, map.GetLength(0) - 2 };
            int[] cornerY = { 1, map.GetLength(1) - 2, 1, map.GetLength(1) - 2 };
            int cornerIdx = 0;

            foreach (var pos in positions)
            {
                if (!(map[pos.x, pos.y] is Enemy enemy)) continue;
                if (cornerIdx >= 4) break;

                while (cornerIdx < 4 && !(map[cornerX[cornerIdx], cornerY[cornerIdx]] is Field)) cornerIdx++;
                if (cornerIdx >= 4) break;

                map[pos.x, pos.y] = new Field();
                enemy.IsChasing = true;
                enemy.Ability = Id;
                enemy.HasTeleported = false;
                map[cornerX[cornerIdx], cornerY[cornerIdx]] = enemy;
                cornerIdx++;
            }
        }
    }

    /// <summary>
    /// Реестр стратегий способностей: связывает номер способности с ее стратегией.
    /// Добавление новой способности сводится к созданию класса и одной строки в реестре.
    /// </summary>
    internal static class HunterAbilityRegistry
    {
        /// <summary>Зарегистрированные стратегии способностей.</summary>
        private static readonly Dictionary<int, HunterAbility> abilities = new Dictionary<int, HunterAbility>();

        /// <summary>
        /// Инициализирует реестр пятью игровыми способностями.
        /// </summary>
        static HunterAbilityRegistry()
        {
            abilities.Add(1, new SpeedHunterAbility());
            abilities.Add(2, new TeleportHunterAbility());
            abilities.Add(3, new ShooterHunterAbility());
            abilities.Add(4, new PhaseHunterAbility());
            abilities.Add(5, new WaveHunterAbility());
        }

        /// <summary>
        /// Возвращает стратегию способности по ее номеру.
        /// </summary>
        /// <param name="id">Номер способности (1-5).</param>
        /// <returns>Стратегия или null, если способности с таким номером нет.</returns>
        public static HunterAbility Get(int id)
        {
            HunterAbility ability;
            return abilities.TryGetValue(id, out ability) ? ability : null;
        }
    }
}
