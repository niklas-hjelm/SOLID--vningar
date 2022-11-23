﻿using System;
using System.Linq;
using WorkshopHandsOn6;

namespace WorkshopHandsOn6.Rules
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
