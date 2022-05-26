using CalculationGrid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace CalculationGridForm
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Cell mainCell;
        private List<Cell> processed;
        private List<Cell> toSave; 
        private List<Tuple <double, double, double, double>> ToDraw;
        private List<PointCollection> points;
        private Button buttonUpload;
        private Button buttonStep;
        private Button buttonSaveImage;
        private Button buttonSaveGrid;
        private Button buttonClean;
        private PictureBox picture; 
        bool Stop = true;
        bool First = true;
        bool Step = false;
        Point prev; 

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.Size = new System.Drawing.Size(1000, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Name = "Form1";
            this.Text = "CalculationGrid";
            this.ResumeLayout(false);
            this.Shown += Form1_Shown;
            points = new List<PointCollection>();

            ToDraw = new List<Tuple<double, double, double, double>>();
            mainCell = new Cell(new Tuple<double, double>(10, 10), 800, 800);
            ToDraw.Add(new Tuple<double, double, double, double>(mainCell.Location.Item1, mainCell.Location.Item2,
                mainCell.Width, mainCell.Height));
            processed = new List<Cell>();
            toSave = new List<Cell>(); 

            buttonUpload = new Button();
            buttonUpload.Text = "Загрузить из файла";
            buttonUpload.Click += UploadClick;
            buttonUpload.Location = new Point(840, 20);
            buttonUpload.Size = new System.Drawing.Size(120, 60);
            Controls.Add(buttonUpload);

            buttonStep = new Button();
            buttonStep.Text = "Следующий шаг";
            buttonStep.Click += StepClick;
            buttonStep.Location = new Point(840, 100);
            buttonStep.Size = new System.Drawing.Size(120, 60);
            Controls.Add(buttonStep);

            buttonSaveImage = new Button();
            buttonSaveImage.Text = "Сохранить изображение";
            buttonSaveImage.Click += buttonSaveImage_Click;
            buttonSaveImage.Location = new Point(840, 180);
            buttonSaveImage.Size = new System.Drawing.Size(120, 60);
            Controls.Add(buttonSaveImage);

            buttonSaveGrid = new Button();
            buttonSaveGrid.Text = "Сохранить данные сетки";
            buttonSaveGrid.Click += buttonSaveGrid_Click;
            buttonSaveGrid.Location = new Point(840, 260);
            buttonSaveGrid.Size = new System.Drawing.Size(120, 60);
            Controls.Add(buttonSaveGrid);

            buttonClean = new Button();
            buttonClean.Text = "Очистить";
            buttonClean.Click += buttonClean_Click;
            buttonClean.Location = new Point(840, 340);
            buttonClean.Size = new System.Drawing.Size(120, 60);
            Controls.Add(buttonClean);

            picture = new PictureBox();
            picture.Location = new Point(10, 10);
            picture.Size = new System.Drawing.Size(820, 820);
            picture.Image = new Bitmap(picture.Width, picture.Height);
            picture.MouseUp += MyMouseUp;
            picture.MouseDown += MyMouseDown;
            picture.MouseMove += MyMouseMove;
            Controls.Add(picture);

            prev = new Point();
        }
        private void UploadClick(Object sender, EventArgs e)
        {
            Step = true;
            using (OpenFileDialog fin = new OpenFileDialog() { Filter = "Файлы txt|*.txt" })
            {
                if (fin.ShowDialog() == DialogResult.OK)
                {
                    StreamReader sr = new StreamReader(fin.FileName);
                    string identity = null;
                    while ((identity = sr.ReadLine()) != null)
                    {
                        if (identity == "lineto")
                        {
                            string numbers = sr.ReadLine();
                            string[] arr_numbers = numbers.Split();
                            double x = Convert.ToDouble(arr_numbers[0]);
                            double y = Convert.ToDouble(arr_numbers[1]);
                            points[points.Count - 1].Add(new System.Windows.Point(x, y));
                            mainCell.addPointAtCur(new MyPoint(x, y));
                        }
                        else if (identity == "linefrom")
                        {
                            points.Add(new PointCollection());
                            string numbers = sr.ReadLine();
                            string[] arr_numbers = numbers.Split();
                            double x = Convert.ToDouble(arr_numbers[0]);
                            double y = Convert.ToDouble(arr_numbers[1]);
                            points[points.Count - 1].Add(new System.Windows.Point(x, y));
                            mainCell.addPointAtNew(new MyPoint(x, y));
                        }
                        else if (identity == "ellipse")
                        {
                            points.Add(new PointCollection());
                            string numbers = sr.ReadLine();
                            string[] arr_numbers = numbers.Split();
                            double x = Convert.ToDouble(arr_numbers[0]);
                            double y = Convert.ToDouble(arr_numbers[1]);
                            double a = Convert.ToDouble(arr_numbers[2]);
                            double b = Convert.ToDouble(arr_numbers[3]);
                            double angle_start = Convert.ToDouble(arr_numbers[4]);
                            double angle_finish = Convert.ToDouble(arr_numbers[5]);

                            mainCell.addPointAtNew(new MyPoint(a * Math.Cos(angle_start * Math.PI) + x, b * Math.Sin(angle_start * Math.PI) + y));
                            points[points.Count - 1].Add(new System.Windows.Point(a * Math.Cos(angle_start * Math.PI) + x, b * Math.Sin(angle_start * Math.PI) + y));
                            double angle_step = 0.01;
                            for (double angle_cur = angle_start + angle_step; angle_cur <= angle_finish + angle_step; angle_cur += angle_step)
                            {
                                mainCell.addPointAtCur(new MyPoint(a * Math.Cos(angle_cur * Math.PI) + x, b * Math.Sin(angle_cur * Math.PI) + y));
                                points[points.Count - 1].Add(new System.Windows.Point(a * Math.Cos(angle_cur * Math.PI) + x, b * Math.Sin(angle_cur * Math.PI) + y));
                            }

                        }
                    }
                    processed.Add(mainCell);
                    System.Drawing.Pen redPen = new System.Drawing.Pen(System.Drawing.Color.Red, 3);
                    Graphics gr = Graphics.FromImage(picture.Image);
                    foreach (PointCollection points_cur in points)
                    {
                        PointF[] convert_points = new PointF[points_cur.Count];
                        int index = 0;
                        foreach (System.Windows.Point point in points_cur)
                        {
                            convert_points[index] = new System.Drawing.PointF((float)point.X, (float)point.Y);
                            index++;
                        }
                        gr.DrawLines(redPen, convert_points);
                        picture.Invalidate();
                    }
                }
            }            
        }
        private void StepClick(Object sender, EventArgs e)
        {
            Step = true;
            if (processed.Count == 0) processed.Add(mainCell);
            Add();
            Draw();
        }
        private void buttonSaveImage_Click(Object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = @"PNG|*.png" })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    picture.Image.Save(saveFileDialog.FileName);
                }
            }
        }
        private void buttonSaveGrid_Click(Object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = @"TXT|*.txt" })
            {
                toSave.Add(mainCell);
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                    while (toSave.Count != 0)
                    {
                        Cell cur = toSave[0];
                        writer.WriteLine(String.Format("{0} {1} {2} {3}", cur.Location.Item1, cur.Location.Item2, cur.Width, cur.Height));
                        if (cur.Points != null)
                        {
                            writer.WriteLine(cur.Points.Count.ToString());
                            foreach (List<MyPoint> points in cur.Points)
                            {
                                writer.WriteLine(points.Count);
                                foreach (MyPoint point in points)
                                {
                                    writer.WriteLine(String.Format("{0} {1}", point.X, point.Y));
                                }
                            }
                        }
                        else writer.WriteLine(0);
                        if (cur.getChildren() != null)
                        {
                            writer.WriteLine("children");
                            toSave.Add(cur.getChildren()[0, 0]);
                            toSave.Add(cur.getChildren()[0, 1]);
                            toSave.Add(cur.getChildren()[1, 0]);
                            toSave.Add(cur.getChildren()[1, 1]);
                        }
                        else writer.WriteLine("nochildren");

                        toSave.RemoveAt(0);
                    }
                    writer.Close();
                }
            }
            toSave.Clear();
        }
        private void buttonClean_Click(Object sender, EventArgs e)
        {
            points.Clear();
            mainCell = null;
            mainCell = new Cell(new Tuple<double, double>(10, 10), 800, 800);
            ToDraw.Clear();
            processed.Clear();
            ToDraw.Add(new Tuple<double, double, double, double>(mainCell.Location.Item1, mainCell.Location.Item2,
                mainCell.Width, mainCell.Height));
            buttonUpload.Enabled = true;
            Step = false;
            picture.Image = new Bitmap(picture.Width, picture.Height);
            Draw();
            
        }
        private void MyMouseDown(Object sender, EventArgs e)
        {
            buttonUpload.Enabled = false;
            if (!Step)
            {
                prev.X = -1;
                prev.Y = -1;
                First = true;
                Stop = false;
            }
             
        }
        private void MyMouseUp(Object sender, EventArgs e)
        {
            Stop = true;
        }
        private void MyMouseMove(Object sender, EventArgs e)
        {
            MouseEventArgs mouseEvent = (MouseEventArgs) e;
            if (!Stop)
            {
                if (First)
                {
                    mainCell.addPointAtNew(new MyPoint(mouseEvent.X, mouseEvent.Y));
                    First = false;
                }
                else
                {
                    mainCell.addPointAtCur(new MyPoint(mouseEvent.X, mouseEvent.Y));
                }
                
                if (prev.X != -1)
                {
                    System.Drawing.Pen redPen = new System.Drawing.Pen(System.Drawing.Color.Red, 5);
                    Graphics gr = Graphics.FromImage(picture.Image);
                    gr.DrawLine(redPen, prev, new Point(mouseEvent.X, mouseEvent.Y));
                    picture.Invalidate();
                }

                prev.X = mouseEvent.X;
                prev.Y = mouseEvent.Y;
            }       
        }
        private void Form1_Shown(Object sender, EventArgs e)
        {
          Draw();
        }
        private void Draw()
        {
            while (ToDraw.Count != 0)
            {
                double x = ToDraw[0].Item1;
                double y = ToDraw[0].Item2;
                double width = ToDraw[0].Item3;
                double height = ToDraw[0].Item4;
                PointF[] points = new PointF[4];
                points[0] = new PointF((float)x, (float)y);
                points[1] = new PointF((float)(x + width), (float)y);
                points[2] = new PointF((float)(x + width), (float)(y + height));
                points[3] = new PointF((float)x, (float)(y + height));
                System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
                Graphics gr = Graphics.FromImage(picture.Image);
                gr.DrawPolygon(blackPen, points);
                picture.Invalidate();
                ToDraw.RemoveAt(0);
            }
        }
        private void Add()
        {
            List<Cell> new_processed = new List<Cell>();
            foreach (Cell cell in processed)
            {
                if (cell.generateChildren())
                {
                    for (int i = 0; i < cell.Num_children; i++)
                        for (int j = 0; j < cell.Num_children; j++)
                        {
                            Cell child = cell.getChildren()[i, j];
                            new_processed.Add(child);
                            ToDraw.Add(new Tuple<double, double, double, double>(child.Location.Item1, child.Location.Item2,
                                child.Width, child.Height));
                        }
                }
            }
            processed = new_processed;
        }
    }
}

