using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DGame; 
public class Polygon {
    public Vector3[] points;
    public Brush brush;
    public Pen pen;

    public Polygon(Vector3[] polygon, Color fillColor, Color lineColor) {
        points = polygon;
        brush = new SolidBrush(fillColor);
        pen = new Pen(lineColor);
    }
    public Polygon(Vector3[] polygon, Color fillColor, Color lineColor, int lineWidth) {
        points = polygon;
        brush = new SolidBrush(fillColor);
        pen = new Pen(lineColor, lineWidth);
    }
    public Polygon(Vector3[] polygon, Brush fillBrush, Pen linePen) {
        points = polygon;
        brush = fillBrush;
        pen = linePen;
    }

    public Polygon(Vector3[] polygon) {
        points = polygon;
        brush = Brushes.Gray;
        pen = Pens.Black;
    }
}