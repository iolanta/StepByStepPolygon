using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlinePolygon
{
    public partial class Form1 : Form
    {
        private List<PointF> polygon_points;
        public Form1()
        {
            InitializeComponent();
            polygon_points = new List<PointF>();
        }

        // Считает, пересекаются ли отрезки (p1,p2) и (p3,p4). Функция взята из лабораторной работы №4
        private bool FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection)
        {
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            if (float.IsInfinity(t1))
            {
                intersection = new PointF(float.NaN, float.NaN);
                return false;
            }
                
            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            return ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));
        }

        // Функция взята из лабораторной работы №4
        private bool point_in_polygon(PointF p)
        {
            for (int i = 0; i < polygon_points.Count; ++i)
                if (polygon_points[i] == p)
                    return true;


            int cnt_intersec = 0;
            PointF intersec;
            PointF finish = new PointF(0, 0);
            for (var i = 0; i < polygon_points.Count - 1; ++i)
            {
                if (FindIntersection(p, finish, polygon_points[i], polygon_points[i + 1], out intersec) && intersec != polygon_points[i])
                    ++cnt_intersec;
            }

            if (FindIntersection(p, finish, polygon_points.First(), polygon_points.Last(), out intersec) && intersec != polygon_points.First())
                ++cnt_intersec;

            if (cnt_intersec % 2 != 0)     // принадлежит многоугольнику
                return true;
            else                           // не принадлежит
                return false;   
        }

        // Расстояние между двумя точками
        private float distance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        private int orientation(PointF a, PointF b, PointF c)
        {
            float res = (b.Y - a.Y) * (c.X - b.X) - (c.Y - b.Y) * (b.X - a.X);

            if (res == 0)
                return 0;
            if (res > 0)
                return 1;
            return -1;
        }

        private void add_new_point(PointF p)
        {
            if (point_in_polygon(p))
                return;

            // поиск точки, имеющей минимальное расстояние до p
            int ind = 0;
            int n = polygon_points.Count;
            for (int i = 0; i < n; ++i)
            {
                if (distance(p, polygon_points[i]) < distance(p, polygon_points[ind]))
                    ind = i;
            }

            // поиск касательных
            int t1 = ind;
            while(orientation(p, polygon_points[t1], polygon_points[(t1 + 1) % n]) >= 0)
                t1 = (t1 + 1) % n;

            int t2 = ind;
            while (orientation(p, polygon_points[t2], polygon_points[(n + t2 - 1) % n]) <= 0)
                t2 = (n + t2 - 1) % n;

            // создание нового полигона
            List<PointF> new_polygon = new List<PointF>();
            int cur = t1;
            new_polygon.Add(polygon_points[cur]);
            while(cur != t2)
            {
                cur = (cur + 1) % n;
                new_polygon.Add(polygon_points[cur]);
            }

            new_polygon.Add(p);
            polygon_points = new_polygon;

            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (polygon_points.Count <= 1)
                polygon_points.Add(e.Location);
            else
                add_new_point(e.Location);
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Pen pnt = new Pen(Color.Fuchsia, 2);
            Pen ln = new Pen(Color.Blue);
            if (polygon_points.Count == 1)
                e.Graphics.DrawEllipse(pnt, polygon_points[0].X, polygon_points[0].Y, 2, 2);
            else if(polygon_points.Count >= 2)
            {
                for(int i = 0; i < polygon_points.Count - 1; ++i)
                {
                    e.Graphics.DrawEllipse(pnt, polygon_points[i].X, polygon_points[i].Y, 2, 2);
                    e.Graphics.DrawLine(ln, polygon_points[i], polygon_points[i + 1]);
                }
                e.Graphics.DrawLine(ln, polygon_points.Last(), polygon_points.First());
                e.Graphics.DrawEllipse(pnt, polygon_points.Last().X, polygon_points.Last().Y, 2, 2);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            polygon_points.Clear();
            pictureBox1.Invalidate();
        }
    }
}
