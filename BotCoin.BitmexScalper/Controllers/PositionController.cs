using BotCoin.BitmexScalper.Domain;
using BotCoin.BitmexScalper.Models;
using BotCoin.DataType.Exchange;
using System;
using System.Collections.Generic;

namespace BotCoin.BitmexScalper.Controllers
{
    internal class PositionController
    {
        readonly Dictionary<string, PositionModel> _posModel;
        public event EventHandler<PositionControllerEventArgs> Changed;

        public PositionController()
        {
            this.Changed += (s, e) => { };
            _posModel = new Dictionary<string, PositionModel>();
        }

        public void Add(BitmexPositionData pos, string instrument, MainWindowController wndCtrl)
        {
            var model = PositionModel.ToModel(pos, wndCtrl);
            _posModel[instrument] = model;

            Changed(this, new PositionControllerEventArgs(model));
        }

        public void Remove(string instrument)
        {
            var model = GetPosition(instrument);

            _posModel.Remove(instrument);
            Changed(this, new PositionControllerEventArgs(model));
        }

        public void SelectInstrument(string symbol)
        {
            var model = GetPosition(symbol);
            Changed(this, new PositionControllerEventArgs(model));
        }

        public bool PositionOpened(string instrument)
        {
            return _posModel.ContainsKey(instrument);
        }

        public PositionModel GetPosition(string instrument)
        {
            return _posModel.ContainsKey(instrument) ? _posModel[instrument] : null;
        }

        public void Refresh(PositionModel model)
        {
            Changed(this, new PositionControllerEventArgs(model));
        }

        public int CalculatePosition(double priceOpen, double stopValue, double risk, double balance, MainWindowController wndCtrl, string symbol)
        {
            if (stopValue == 0) return 0;

            int position = Math.Abs(wndCtrl.GetPosition(priceOpen, stopValue, risk, balance, symbol));
            if (position == 0) return 0;

            for ( ; ; )
            {
                var realRisk = wndCtrl.GetRealRisk(position, priceOpen, stopValue, risk, balance, symbol);
                if (realRisk < 0)
                    throw new InvalidOperationException(String.Format("Invalid stop value '{0}' because real risk is negative.", stopValue));
                
                if (realRisk - risk >= 0.01)
                {
                    if (realRisk > risk) position -= 1;
                    continue;
                }
                return position;
            }
        }
    }
}
