using System;
using System.Collections.Generic;
using System.Text;

namespace BrixTest.Services
{
    public interface ILogServices
    {
        public void WriteLine (string Msg , ConsoleColor Color , ConsoleColor ReturnToColor = ConsoleColor.White );
    }

    public class ConsoleLogger:ILogServices
    {
        public void WriteLine ( string Msg , ConsoleColor Color , ConsoleColor ReturnToColor = ConsoleColor.White )
        {
            Console.ForegroundColor = Color;
            Console.WriteLine( Msg );
            Console.ForegroundColor = ReturnToColor;
        }
    }
}
