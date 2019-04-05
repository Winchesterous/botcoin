using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZedGraph;

namespace BotCoin.BitmexScalper
{
    internal class ZedGraphCrossHair
    {
        readonly ZedGraphControl _zed;
        LineObj _xHairOld;
        LineObj _yHairOld;
        double? _crossHairX;
        double? _crossHairY;

        public ZedGraphCrossHair(ZedGraphControl zed)
        {
            _xHairOld = new LineObj();
            _yHairOld = new LineObj();
            _zed = zed;

            zed.MouseMove += OnMouseMove;
            zed.MouseLeave += OnMouseLeave;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            double x, y;

            var pane = _zed.GraphPane;
            //
            // Пересчитываем пиксели в координаты на графике
            // у ZedGraph есть несколько перегруженных методов ReverseTransform.
            pane.ReverseTransform(e.Location, out x, out y);

            if (x < pane.XAxis.Scale.Min ||
                x > pane.XAxis.Scale.Max ||
                y < pane.YAxis.Scale.Min ||
                y > pane.YAxis.Scale.Max
                )//out of the bounds
            {
                OnMouseLeave(null, null);
            }
            else
            {
                if (_crossHairX != null && _crossHairY != null)
                {
                    pane.GraphObjList.Remove(_xHairOld);
                    pane.GraphObjList.Remove(_yHairOld);
                }

                LineObj xHair = new LineObj(pane.XAxis.Scale.Min, y, pane.XAxis.Scale.Max, y);
                xHair.Line.Style = DashStyle.Custom;
                xHair.Line.DashOff = 5.0f;      // длина пропуска между пунктирами
                xHair.Line.DashOn = 10.0f;      // длина пунктира
                LineObj yHair = new LineObj(x, pane.YAxis.Scale.Min, x, pane.YAxis.Scale.Max);
                yHair.Line.Style = DashStyle.Custom;
                yHair.Line.DashOff = 5.0f;
                yHair.Line.DashOn = 10.0f;

                pane.GraphObjList.Add(xHair);
                _xHairOld = xHair;

                pane.GraphObjList.Add(yHair);
                _yHairOld = yHair;

                _crossHairY = y;
                _crossHairX = x;

                _zed.Refresh();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (_crossHairX != null && _crossHairY != null)
            {
                var pane = _zed.GraphPane;
                pane.GraphObjList.Remove(_xHairOld);
                pane.GraphObjList.Remove(_yHairOld);
                _zed.Refresh();
            }
        }
    }
}
