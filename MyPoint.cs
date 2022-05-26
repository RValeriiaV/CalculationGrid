using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationGrid;


namespace CalculationGridForm
{
    class MyPoint
    {
        double x;
        double y;
        bool[] belonging;
        public MyPoint()
        {
            x = 0;
            y = 0;
            belonging = new bool[4] { false, false, false, false};
        }
        public MyPoint(double X, double Y):this()
        {
            x = X;
            y = Y;
        }
        public double X
        {
            get { return x; }
        }
        public double Y
        {
            get { return y; }
        }
        public bool[] Belonging
        {
            get { return belonging; }
        }
        public void CreateBelonging(Cell cell)
        {
            belonging =new bool [4]{ false, false, false, false};
            if (x <= cell.Location.Item1 + cell.Width / 2)
            {
                if (y < cell.Location.Item2 + cell.Height / 2) belonging[0] = true;
                else if (y > cell.Location.Item2 + cell.Height / 2) belonging[2] = true;
                else
                {
                    belonging[0] = true;
                    belonging[2] = true;
                }
            }
            if (x >= cell.Location.Item1 + cell.Width / 2)
            {
                if (y < cell.Location.Item2 + cell.Height / 2) belonging[1] = true;
                else if (y > cell.Location.Item2 + cell.Height / 2) belonging[3] = true;
                else
                {
                    belonging[1] = true;
                    belonging[3] = true;
                }
            }
        }

    }
}
