using BotCoin.BitmexScalper.Models;
using System;

namespace BotCoin.BitmexScalper.Domain
{
    internal class PositionControllerEventArgs : EventArgs
    {
        public readonly PositionModel Model;
        public PositionControllerEventArgs(PositionModel model)
        {
            Model = model;
        }
    }
}
