﻿using System;

namespace WorkshopHandsOn9.Extensions
{
    public static class RobotFlagsExtensions
    {
        public static RobotFlags Set(this RobotFlags value, RobotFlags flag)
        {
            value |= flag;
            return value;
        }
        public static RobotFlags Remove(this RobotFlags value, RobotFlags flag)
        {
            value &= ~flag;
            return value;
        }
    }
}