﻿using System;
using System.Collections.Generic;
using WorkshopHandsOn11.Commands;
using WorkshopHandsOn11.Log_Strategies;
using WorkshopHandsOn11.ExceptionChain;
using WorkshopHandsOn11.Validation;
using WorkshopHandsOn11.Algorithms;
using WorkshopHandsOn11.Messages;
using WorkshopHandsOn11.Navigation;
using WorkshopHandsOn11.ValueObjects;

namespace WorkshopHandsOn11
{
    class ProgramRandom
    {
        public static void Execute()
        {
            IPlayingBoard board = PlayingBoard.Create(8, 8);
            IServiceBus servicebus = ServiceBus.Create();
            ILogService logger = LogService.Create();
            IExceptionService service = ExceptionService.Create(logger);
            IValidationService<IRobot> validationService = ValidationService<IRobot>.Create().AddValidator(RobotValidator.Create());
            INavigationService navigationService = NavigationService.Create(servicebus, ParkingLot.Create(), board);
            try
            {
                IList<IRobot> robots = new List<IRobot>();
                for (int i = 1; i <= board.Area; i++)
                {
                    robots.Add(Robot.Create(validationService, logger, servicebus, "Robot" + i, navigationService));
                }
                foreach (IRobot r in robots)
                {
                    r.SetPowerOn();
                    r.ForceMoveTo(navigationService.GetRandomPosition());
                }
                PrintSorted(logger, robots);
                logger.Log("Program Done!");
            }
            catch (Exception ex)
            {
                service.Process(ex);
            }
            Console.ReadKey();
        }
        private static void PrintSorted(ILogService logger, IList<IRobot> robots)
        {
            IDictionary<string, IRobot> sortedRobots = new SortedList<string, IRobot>();
            foreach (IRobot r in robots)
            {
                sortedRobots.Add(r.CurrentPosition().ToString(), r);
            }
            foreach (KeyValuePair<string, IRobot> kv in sortedRobots)
            {
                logger.Log(string.Format("Position {0} has {1}.", kv.Key, kv.Value.Name));
            }
        }
    }
}
