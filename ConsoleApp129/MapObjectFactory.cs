namespace ConsoleApp129
{
    /// <summary>
    /// Фабрика игровых объектов карты (паттерн Фабричный метод).
    /// Единая точка создания объектов: генерация уровня (Map.Map_generation)
    /// и восстановление карты из сохранения (Map.LoadMapFromSave) используют ее методы,
    /// поэтому правила создания объектов не дублируются в клиентском коде.
    /// </summary>
    internal static class MapObjectFactory
    {
        /// <summary>
        /// Создает объект карты по результату случайного броска при генерации уровня.
        /// </summary>
        /// <param name="roll">Случайное число от 0 до 99.</param>
        /// <param name="x">Координата X клетки.</param>
        /// <param name="y">Координата Y клетки.</param>
        /// <param name="heroX">Координата X точки спавна героя.</param>
        /// <param name="heroY">Координата Y точки спавна героя.</param>
        /// <returns>Новый объект карты.</returns>
        public static MapObject CreateForGeneration(int roll, int x, int y, int heroX, int heroY)
        {
            if (x == heroX && y == heroY) return new Hero(x, y);
            if (roll < 1) return new Enemy(x, y);
            if (roll > 1 && roll < 15) return new Wall();
            if (roll > 15 && roll < 20) return new Tree();
            return new Field();
        }

        /// <summary>
        /// Создает объект карты по символу из файла сохранения.
        /// </summary>
        /// <param name="symbol">Символ объекта ('H' — герой, 'E' — враг, 'W' — стена, 'T' — дерево, '.' — поле).</param>
        /// <param name="x">Координата X клетки.</param>
        /// <param name="y">Координата Y клетки.</param>
        /// <returns>Новый объект карты.</returns>
        public static MapObject CreateFromSymbol(char symbol, int x, int y)
        {
            switch (symbol)
            {
                case 'H': return new Hero(x, y);
                case 'E': return new Enemy(x, y);
                case 'W': return new Wall();
                case 'T': return new Tree();
                default: return new Field();
            }
        }
    }
}
