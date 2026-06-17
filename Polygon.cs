using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DGame; 
public class Polygon {
    Vector3[] points;
    Brush brush;
    Pen pen;

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
}