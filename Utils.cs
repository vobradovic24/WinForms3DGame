using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DGame {
    public static class Utils {
        public static Vector2 screenSize;
        public static Point windowCenter;
        public static Vector3 minVector = new Vector3(float.MinValue);
        public static Vector3 maxVector = new Vector3(float.MaxValue);

        public static Point Vec2ToPoint(Vector2 vector) {
            return new Point((int)vector.X, (int)vector.Y);
        }
        public static Point[] Vec2ToPoint(Vector2[] vectors) {
            Point[] points = new Point[vectors.Length];
            for (int i = 0; i < vectors.Length; i++) points[i] = new Point((int)vectors[i].X, (int)vectors[i].Y);
            return points;
        }
        public static PointF Vec2ToPointF(Vector2 vector) {
            return new PointF(vector);
        }
        public static PointF[] Vec2ToPointF(Vector2[] vectors) {
            PointF[] points = new PointF[vectors.Length];
            for (int i = 0; i < vectors.Length; i++) points[i] = new PointF(vectors[i].X, vectors[i].Y);
            return points;
        }
        public static PointF[] Vec2ToPointF_Flipped(Vector2[] vectors) {
            PointF[] points = new PointF[vectors.Length];
            for (int i = 0; i < vectors.Length; i++) points[i] = new PointF(vectors[i].X, screenSize.Y - vectors[i].Y);
            return points;
        }
        public static Vector2 PointToVector2(Point point) {
            return new Vector2(point.X, point.Y);
        }
        public static Vector2 PointFToVector2(PointF point) {
            return new Vector2(point.X, point.Y);
        }
    }
}
