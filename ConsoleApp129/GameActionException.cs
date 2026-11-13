using System;

namespace ConsoleApp129
{
    public class GameActionException : Exception
    {
        public GameActionException(string message) : base(message) { }
    }
}