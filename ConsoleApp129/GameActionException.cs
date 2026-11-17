using System;

namespace ConsoleApp129
{
    /// <summary>
    /// Пользовательское исключение для обработки ошибок игрового процесса.
    /// </summary>
    public class GameActionException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="GameActionException"/> с указанным сообщением.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public GameActionException(string message) : base(message) { }
    }
}