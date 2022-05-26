using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationGridForm;

namespace CalculationGrid
{
    class Cell                
      {
        Tuple<double, double> location;
        double width;
        double height;
        int num_children = 2;
        Cell[,] Children;
        List<List<MyPoint>> points;

        public Cell()
        {
            location = Tuple.Create<double, double> (0, 0);
            width = 0;
            height = 0;
            Children = null;
            points = null; 
        }
        public Cell(Tuple<double, double> location, double width, double height)
        {
            this.location = location;
            this.width = width;
            this.height = height;
            Children = null;
            points = null; 
        }
        public Tuple<double, double> Location
        {
            get { return location; }
        }
        public double Width
        {
            get { return width; }
        }
        public double Height
        {
            get { return height; }
        }
        public int Num_children
        {
            get { return num_children; }
        }
        public void addPointAtCur(MyPoint point)
        {
            if (points == null)
            {
                points = new List<List<MyPoint>>();
                points.Add(new List<MyPoint>());
            }
            points.Last().Add(point);
        }
        public void addPointAtNew(MyPoint point)
        {
            if (points == null)
            {
                points = new List<List<MyPoint>>();
            }
            points.Add(new List<MyPoint>());
            points.Last().Add(point);
        }
        public bool generateChildren()
        {
            if (points == null) return false;
            Children = new Cell[num_children, num_children];
            for (int i = 0; i < num_children; i++)
            {
                for (int j = 0; j < num_children; j++)
                {
                    Children[i, j] = new Cell(new Tuple<double, double>(location.Item1 + j * width / num_children, location.Item2 + i * height / num_children), 
                        width/num_children, height/num_children);
                }
            }
            while (points.Count != 0)
            {
                MyPoint prev = new MyPoint(-1, -1);
                while (points[0].Count != 0)
                {
                    MyPoint cur = points[0][0];
                    cur.CreateBelonging(this);
                    if (prev.X == -1)
                        for (int i = 0; i < 4; i++)
                        {
                            if (cur.Belonging[i]) Children[i / num_children, i % num_children].addPointAtNew(cur);
                        }
                    else
                    {
                        bool at_same = false;
                        for (int i = 0; i < 4; i++)
                        {
                            if (cur.Belonging[i] && prev.Belonging[i])
                            {
                                Children[i / num_children, i % num_children].addPointAtCur(cur);
                                at_same = true;
                            }
                        }
                        if (at_same)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (cur.Belonging[i] && !prev.Belonging[i])
                                {
                                    Children[i / num_children, i % num_children].addPointAtNew(cur);
                                }
                            }
                        }
                        else
                        {
                            MyPoint intersection = null;
                            if (cur.X == prev.X) intersection = new MyPoint(cur.X, location.Item2 + height / 2);
                            else if (cur.Y == prev.Y) intersection = new MyPoint(location.Item1 + width / 2, cur.Y);
                            if (intersection != null)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (cur.Belonging[i] && prev.Belonging[j])
                                        {
                                            if (i != j && prev.Belonging[i]) continue;
                                            if (i == j) Children[i / num_children, i % num_children].addPointAtCur(cur);
                                            else
                                            {
                                                Children[j / num_children, j % num_children].addPointAtCur(intersection);
                                                Children[i / num_children, i % num_children].addPointAtNew(intersection);
                                                Children[i / num_children, i % num_children].addPointAtCur(cur);
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                double eps = 0.01;
                                double x1 = prev.X;
                                double y1 = prev.Y;
                                double x2 = cur.X;
                                double y2 = cur.Y;
                                double y_inter = location.Item2 + height / 2;
                                double x_inter = location.Item1 + width / 2;
                                MyPoint intersectionHorizontal = new MyPoint((y_inter - y1) / (y2 - y1) * (x2 - x1) + x1, y_inter);
                                MyPoint intersectionVertical = new MyPoint(x_inter, (x_inter - x1) / (x2 - x1) * (y2 - y1) + y1);
                                intersectionHorizontal.CreateBelonging(this);
                                intersectionVertical.CreateBelonging(this);

                                //пересекает две границы
                                if (Math.Abs(Math.Abs(intersectionHorizontal.X - cur.X) + Math.Abs(intersectionHorizontal.X - prev.X)
                                    - Math.Abs(cur.X - prev.X)) < eps
                                    &&
                                    Math.Abs(Math.Abs(intersectionVertical.Y - cur.Y) + Math.Abs(intersectionVertical.Y - prev.Y)
                                    - Math.Abs(cur.Y - prev.Y)) < eps)
                                {
                                    //пересечение с горизонталью ближе
                                    if (Math.Abs(intersectionHorizontal.X - prev.X) < Math.Abs(intersectionVertical.X - prev.X))
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (prev.Belonging[i] && intersectionHorizontal.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtCur(intersectionHorizontal);
                                            if (!prev.Belonging[i] && intersectionHorizontal.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtNew(intersectionHorizontal);
                                        }
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (intersectionHorizontal.Belonging[i] && intersectionVertical.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtCur(intersectionVertical);
                                            if (!intersectionHorizontal.Belonging[i] && intersectionVertical.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtNew(intersectionVertical);
                                        }
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (intersectionVertical.Belonging[i] && cur.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtCur(cur);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (prev.Belonging[i] && intersectionVertical.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtCur(intersectionVertical);
                                            if (!prev.Belonging[i] && intersectionVertical.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtNew(intersectionVertical);
                                        }
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (intersectionVertical.Belonging[i] && intersectionHorizontal.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtCur(intersectionHorizontal);
                                            if (!intersectionVertical.Belonging[i] && intersectionHorizontal.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtNew(intersectionHorizontal);
                                        }
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (intersectionHorizontal.Belonging[i] && cur.Belonging[i])
                                                Children[i / num_children, i % num_children].addPointAtCur(cur);
                                        }
                                    }
                                }
                                //пересекает только горизонтальную границу
                                else if (Math.Abs(Math.Abs(intersectionHorizontal.X - cur.X) + Math.Abs(intersectionHorizontal.X - prev.X)
                                    - Math.Abs(cur.X - prev.X)) < eps)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (prev.Belonging[i] && intersectionHorizontal.Belonging[i])
                                        {
                                            Children[i / num_children, i % num_children].addPointAtCur(intersectionHorizontal);
                                        }
                                    }
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (cur.Belonging[i] && intersectionHorizontal.Belonging[i])
                                        {
                                            Children[i / num_children, i % num_children].addPointAtNew(intersectionHorizontal);
                                            Children[i / num_children, i % num_children].addPointAtCur(cur);
                                        }
                                    }
                                }
                                //пересекает только вертикальную границу
                                else if (Math.Abs(Math.Abs(intersectionVertical.Y - cur.Y) + Math.Abs(intersectionVertical.Y - prev.Y)
                                    - Math.Abs(cur.Y - prev.Y)) < eps)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (prev.Belonging[i] && intersectionVertical.Belonging[i])
                                        {
                                            Children[i / num_children, i % num_children].addPointAtCur(intersectionVertical);
                                        }
                                    }
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (cur.Belonging[i] && intersectionVertical.Belonging[i])
                                        {
                                            Children[i / num_children, i % num_children].addPointAtNew(intersectionVertical);
                                            Children[i / num_children, i % num_children].addPointAtCur(cur);
                                        }
                                    }
                                }
                            }
                        }
                       
                    }
                    prev = cur;
                    Points[0].RemoveAt(0);
                }
                Points.RemoveAt(0);
            }
            return true;
        }
        public Cell[,] getChildren()
        {
            return Children;
        }
        public List<List<MyPoint>> Points
        {
            get { return points; }
        }        
    }
}
