using ZedGraph;

namespace BotCoin.BitmexScalper
{
    internal class ChartZedGraphControl : ZedGraphControl
    {
        public AxisType AxisX
        {
            set
            {
                this.GraphPane.XAxis.Type =
                this.GraphPane.X2Axis.Type = value;
            }
            get
            {
                return this.GraphPane.X2Axis.Type;
            }
        }

        public double MinScaleX
        {
            set
            {
                this.GraphPane.XAxis.Scale.Min =
                this.GraphPane.X2Axis.Scale.Min = value;
            }
            get
            {
                return this.GraphPane.XAxis.Scale.Min;
            }
        }

        public double MaxScaleX
        {
            set
            {
                this.GraphPane.XAxis.Scale.Max =
                this.GraphPane.X2Axis.Scale.Max = value;
            }
            get
            {
                return this.GraphPane.XAxis.Scale.Max;
            }
        }

        public DateUnit MajorUnitX
        {
            set
            {
                this.GraphPane.XAxis.Scale.MajorUnit =
                this.GraphPane.X2Axis.Scale.MajorUnit = value;
            }
            get
            {
                return this.GraphPane.XAxis.Scale.MajorUnit;
            }
        }

        public DateUnit MinorUnitX
        {
            set
            {
                this.GraphPane.XAxis.Scale.MinorUnit =
                this.GraphPane.X2Axis.Scale.MinorUnit = value;
            }
            get
            {
                return this.GraphPane.XAxis.Scale.MinorUnit;
            }
        }
    }
}
