﻿using System;

namespace WorkshopHandsOn5.Log_Strategies
{
    class DebugLogStrategy : LogStrategy
    {
        public override void Log(string message)
        {
            Console.WriteLine(string.Format("Debug logged: {0}",message));
        }
    }
}
