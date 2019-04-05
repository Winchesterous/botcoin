using BotCoin.BitmexScalper.Controllers;
using BotCoin.BitmexScalper.Helpers;
using BotCoin.DataType.Exchange;
using System;
using System.Text;

namespace BotCoin.BitmexScalper.Models
{
    internal class PositionModel
    {
        private long? _size;
        
        public double AvgEntryPrice { set; get; }
        public string Side { set; get; }
        public string Symbol { set; get; }
        public string Size { set; get; }
        public string EntryPrice { set; get; }
        public double? LastPrice { set; get; }
        public string MarkPrice { set; get; }
        public string Value { set; get; }
        public string LiqPrice { set; get; }
        public string PositionMargin { set; get; }
        public string RealisedProfit { set; get; }
        public string ProfitXbt { set; get; }
        public string TradeProfitXbt { set; get; }
        public string MakerProfitUsd { set; get; }
        public string TakerProfitUsd { set; get; }
        public string Time { set; get; }
        public bool? IsOpen { set; get; }
        public double? FeePaid { set; get; }
        public double? Commision { set; get; }
        public bool IsSelected { set; get; }
        public double? OpenPosFee { set; get; }

        public static PositionModel ToModel(BitmexPositionData pos, MainWindowController wndCtrl)
        {
            var model = new PositionModel { Symbol = pos.Symbol };
            model.InitModel(pos, wndCtrl);
            return model;
        }

        public static PositionModel ToModel(double openPosFee, MainWindowController wndCtrl)
        {
            var model = new PositionModel();
            model.InitModel(null, wndCtrl);
            return model;
        }

        public void InitModel(BitmexPositionData pos, MainWindowController wndCtrl)
        {
            SetSide(pos);
            SetSize(pos);
            SetPositionCost(pos);
            SetLiquidationPrice(pos, wndCtrl);
            SetPrices(pos, wndCtrl);
            SetMargin(pos);
            SetProfit(pos);
            SetTimestamp(pos);
            SetCommission(pos);
        }
                
        private string ToPrice(double? price, MainWindowController ctrl)
        {
            return price.HasValue ? ctrl.ToStringPrice(price.Value, this.Symbol) : string.Empty; 
        }

        private void SetCommission(BitmexPositionData pos)
        {
            if (pos.Commission.HasValue)
            {
                this.Commision = pos.Commission.Value;
                if (pos.PositionSize.HasValue)
                    this.FeePaid = Math.Round(pos.Commission.Value * pos.PositionSize.Value, 8);
            }
        }

        private void SetProfit(BitmexPositionData pos)
        {            
            if (pos.RealisedPnl.HasValue)
            {
                var value = BitmexMargin.ToBtc(pos.RealisedPnl.Value);
                this.RealisedProfit = String.Format("{0} XBT", value.ToString("0.00000000"));
            }
        }

        private void SetTimestamp(BitmexPositionData pos)
        {
            if (pos.Timestamp.HasValue) 
                this.Time = pos.Timestamp.Value.ToLocalTime().ToLongTimeString();
        }

        private void SetPositionCost(BitmexPositionData pos)
        {
            if (pos.PosCost.HasValue) 
                this.Value = String.Format("{0} XBT", BitmexMargin.ToBtc(Math.Abs(pos.PosCost.Value), 5).ToString("0.00000"));
        }

        private void SetSide(BitmexPositionData pos)
        {
            if (pos.IsOpen.HasValue && pos.IsOpen.Value)
            {
                this.Side = pos.PositionSide;
                this.IsOpen = pos.IsOpen.Value;
            }
        }

        private void SetSize(BitmexPositionData pos)
        {
            if (pos.PositionSize.HasValue)
            {
                _size = Math.Abs(pos.PositionSize.Value);
                this.Size = _size.Value.ToString();
            }
        }

        private void SetPrices(BitmexPositionData pos, MainWindowController ctrl)
        {
            if (pos.AvgEntryPrice.HasValue)
            {
                this.EntryPrice = ctrl.ToStringPrice(pos.AvgEntryPrice.Value, this.Symbol);
                this.AvgEntryPrice = pos.AvgEntryPrice.Value;
            }
            if (pos.MarkPrice.HasValue)
                this.MarkPrice = ToPrice(pos.MarkPrice.Value, ctrl);
            if (pos.LastPrice.HasValue)  
                this.LastPrice = pos.LastPrice.Value;
        }

        private void SetMargin(BitmexPositionData pos)
        {
            if (pos.PosMargin.HasValue) 
                this.PositionMargin = BitmexMargin.ToBtc(pos.PosMargin.Value, 5).ToString("0.00000") + " XBT";
        }

        private void SetLiquidationPrice(BitmexPositionData pos, MainWindowController ctrl)
        {
            if (pos.MarkPrice.HasValue && pos.LiquidationPrice.HasValue)
            {
                var diff = ToPrice(Math.Abs(pos.MarkPrice.Value - pos.LiquidationPrice.Value), ctrl);
                this.LiqPrice = String.Format("(+{0}) {1}", diff, ToPrice(pos.LiquidationPrice.Value, ctrl));
            }
        }

        public void SetProfit(MainWindowController wndCtrl, double bidPrice, double askPrice)
        {
            if (_size.HasValue)
            {
                if (!OpenPosFee.HasValue)
                    return;

                if (this.Side == "Buy")
                {
                    wndCtrl.SetProfit(bidPrice, _size.Value, OpenPosFee.Value,
                        (profitMaker, profitTaker) => SetProfitUsd(profitMaker * bidPrice, profitTaker * bidPrice),
                        this);
                }
                else
                {
                    wndCtrl.SetProfit(askPrice, _size.Value, OpenPosFee.Value,
                        (profitMaker, profitTaker) => SetProfitUsd(profitMaker * askPrice, profitTaker * askPrice),
                        this);
                }
            }
        }

        private void SetProfitUsd(double profitMaker, double profitTaker)
        {
            var maker = Math.Round(profitMaker, 2).ToString("0.00");
            var taker = Math.Round(profitTaker, 2).ToString("0.00");

            var usd2 = Math.Round(profitMaker - profitTaker, 2).ToString("0.00");
            var diff = (profitMaker - profitTaker) > 0 ? "+" + usd2 : usd2;

            TakerProfitUsd = String.Format("{0} ({1}) USD", taker, diff);
            MakerProfitUsd = String.Format("{0} USD", maker);
        }

        public long? Update(PositionModel model, Action<string> log, Action update)
        {
            var str = new StringBuilder();
            long? newSize = null;

            if (model.Size.NotNull())
            {
                if (model._size.HasValue && this._size.Value != model._size.Value)
                {
                    newSize = model._size;
                    this._size = newSize;
                    this.Size = model.Size; 
                    str.AppendFormat(" Size({0})", model.Size);
                }
            }
            if (model.Value.NotNull())
            {
                this.Value = model.Value;
                str.AppendFormat(" Value({0})", model.Value);
            }
            if (str.Length > 0)
            {
                log(str.ToString());
            }
            if (model.IsOpen.HasValue || model.Size.NotNull())
            {
                update();
            }
            return newSize;
        }
    }
}
