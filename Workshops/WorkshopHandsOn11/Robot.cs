﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using WorkshopHandsOn11.Exceptions;
using WorkshopHandsOn11.Validation;
using WorkshopHandsOn11.Log_Strategies;
using WorkshopHandsOn11.Rules;
using WorkshopHandsOn11.ValueObjects;
using WorkshopHandsOn11.Messages;
using WorkshopHandsOn11.Extensions;
using WorkshopHandsOn11.Navigation;

namespace WorkshopHandsOn11
{
    public class Robot : IRobot
    {
        private INavigationService _navigationService;
        private RobotFlags _flags = RobotFlags.None;
        private IServiceBus _servicebus;
        private IValidationService<IRobot> _validationService;
        private ILogService _logger;
        private Position _position = Position.Create();

        public Guid ID { get; private set; }
        public string Name { get; private set; }

        public static IRobot Create(IValidationService<IRobot> validationService, ILogService logger, IServiceBus servicebus, string name, INavigationService navigationService)
        {
            return new Robot(validationService, logger, servicebus, name, navigationService);
        }
        private Robot(IValidationService<IRobot> validationService, ILogService logger, IServiceBus servicebus, string name, INavigationService navigationService)
        {
            this._navigationService = navigationService;
            this.Name = name;
            this.ID = Guid.NewGuid();
            this._validationService = validationService;
            this._logger = logger;
            this._servicebus = servicebus;
            this._servicebus.Register<CheckPositionCommand>(e => this.CheckPositionCommandHandler(e));
            this._servicebus.Register<PositionIsFreeEvent>(e => this.PositionIsFreeEventHandler(e));
        }
        private void CheckPositionCommandHandler(CheckPositionCommand e)
        {
            //this._logger.Log(string.Format("{0}, Receiving message CheckPositionCommand", this.Name));
            //this._logger.Log(string.Format("{0}, Sending message MyPositionEvent: My Position {1}", this.Name, this.CurrentPosition().ToString()));

            this._servicebus.Publish<RobotPositionEvent>(new RobotPositionEvent(this.CurrentPosition(), this.ID));
        }
        private void PositionIsFreeEventHandler(PositionIsFreeEvent e)
        {
            if (e.IsSender(this.ID))
            {
                // this._logger.Log(string.Format("{0}, Receiving message PositionIsFreeEvent: Moving To {1}", this.Name, e.Data.ToString()));
                this.MoveTo(e.Data);
            }
        }
        private bool CanMoveTo(Position position)
        {
            return this._navigationService.IsPositionFree(this.ID, position);
        }
        public void MoveToAnyFreePosition()
        {
            this._navigationService.RequestNewPosition(this.ID, this._navigationService.GetRandomPosition());
        }
        public void ForceMoveTo(Position position)
        {
            this.MoveTo(position, true);
        }
        public void Move(IList<Position> route)
        {
            if (route.Count == 0)
                this._logger.Log("No route! Robot can't jump!");
            else
            {
                this._servicebus.Publish<SendSMSCommand>(SendSMSCommand.Create(SMS.Create(string.Format("Robot{0} is moving!", this.Name)), this.ID));
                foreach (Position p in route)
                {
                    this.MoveTo(p);
                }
                this._servicebus.Publish<SendSMSCommand>(SendSMSCommand.Create(SMS.Create(string.Format("Robot{0} has reached his goal!", this.Name)), this.ID));
            }
        }
        public IRobot MoveTo(Position position)
        {
            this.MoveTo(position, false);
            return this;
        }
        public void Release(Func<IRobot, object> func)
        {
            this.CanTakeAction();
            this._logger.Log(func.Invoke(this).ToString());
            // throw new RobotOutOfMemoryException();
        }
        public void Grab(Func<IRobot, object> func)
        {
            this.CanTakeAction();
            this._logger.Log(func.Invoke(this).ToString());
        }
        public Position CurrentPosition()
        {
            return this._position;
        }
        public void SetCurrentPosition(Position position)
        {
            this._position = Position.Create(position);
        }
        public Tuple<Boolean, IEnumerable<string>> Validate(IValidator<IRobot> validator)
        {
            IEnumerable<string> broken = validator.BrokenRules(this);
            return Tuple.Create(broken.Count() == 0, broken);
        }
        public bool CanWalk()
        {
            return this._flags.HasFlag(RobotFlags.CanWalk);
        }
        public IRobotState SetCanWalkOn()
        {
            this._flags = this._flags.Set(RobotFlags.CanWalk);
            return this;
        }
        public IRobotState SetCanWalkOff()
        {
            this._flags = this._flags.Remove(RobotFlags.CanWalk);
            return this;
        }
        public IRobotState SetPowerOff()
        {
            this._flags = this._flags.Remove(RobotFlags.PowerOn);
            return this;
        }
        public bool IsPowerOn()
        {
            return this._flags.HasFlag(RobotFlags.PowerOn);
        }
        public IRobotState SetPowerOn()
        {
            this._flags = this._flags.Set(RobotFlags.PowerOn);
            return this;
        }
        public bool InStelthMode()
        {
            return this._flags.HasFlag(RobotFlags.StelthMode);
        }
        public IRobotState SetStelthModeOff()
        {
            this._flags = this._flags.Remove(RobotFlags.StelthMode);
            return this;
        }     
        public IRobotState SetStelthModeOn()
        {
            this._flags = this._flags.Set(RobotFlags.StelthMode);
            return this;
        }
        public IRobotState Reset()
        {
            this._flags = RobotFlags.None;
            return this;
        }
        private void MoveTo(Position position, bool force)
        {
            if (this.CurrentPosition().Equals(position))
                return;

            string message = string.Format("{0} is going to move to Position {1}.", this.Name, position.ToString());
            this._logger.Log(message);

            if (this.CanMoveTo(position))
                this.SetCanWalkOn();
            else if (force)
            {
                this._navigationService.RequestAccessToPosition(this.ID, position);
                return;
            }

            this.CanTakeAction();
            this._position = Position.Create(position);

            message = string.Format("{0} moved to Position {1}.", this.Name, position.ToString());
            this._logger.Log(message);
            //throw new RobotNotWalkingException();
        }
        private void CanTakeAction()
        {
            Tuple<Boolean, IEnumerable<string>> result = this._validationService.Validate((IRobot)this);
            if (!result.Item1)
            {
                string msg = string.Format("{0}, Validation failed: {1}", this.Name, result.Item2.Aggregate((s, b) => s + ", " + b));
                this._logger.Log(msg);
                throw new ApplicationException(msg);
            }
        }
    }
}
