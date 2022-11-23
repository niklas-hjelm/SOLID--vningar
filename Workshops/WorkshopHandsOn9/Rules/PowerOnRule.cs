﻿using System;
using System.Linq;
using WorkshopHandsOn9;

namespace WorkshopHandsOn9.Rules
{
    public class PowerOnRule : Rule<IRobotState>
    {
        public PowerOnRule()
            : base(r => r.IsPowerOn())
        {
            this.SetMessage("Power is not On!");
        }
    }
}
