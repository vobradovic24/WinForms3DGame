using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DGame;

public class Camera {
    public Vector3 position;
    public Vector3 rotation;
    public float hFOV;
    public float vFOV;
    public float viewportDist;
    public float nearClipPlane = 0.01f;

    public Camera(Vector3 position, Vector3 rotation, float FOV) {
        this.position = position;
        this.rotation = rotation;
        hFOV = FOV;
        viewportDist = Utils.screenSize.X * 0.5f / MathF.Tan(FOV * 0.5f);
        vFOV = 2 * MathF.Atan(2 * viewportDist / Utils.screenSize.Y);
    }

    public void updateFOV(float FOV) {
        hFOV = FOV;
        viewportDist = Utils.screenSize.X * 0.5f / MathF.Tan(FOV * 0.5f);
        vFOV = 2 * MathF.Atan(2 * viewportDist / Utils.screenSize.Y);
    }

    public Vector3 GetDeltaPoint(Vector3 point) {
        Vector3 absDeltaPoint = point - position;
        Vector3 relDeltaPoint = new Vector3();


        float sinPitch = MathF.Sin(-rotation.X);
        float cosPitch = MathF.Cos(-rotation.X);

        float sinYaw = MathF.Sin(rotation.Y);
        float cosYaw = MathF.Cos(rotation.Y);

        float sinRoll = MathF.Sin(-rotation.Z);
        float cosRoll = MathF.Cos(-rotation.Z);

        Matrix4x4 pitchMatrix = new Matrix4x4(
             1, 0, 0, 0,
             0, cosPitch, -sinPitch, 0,
             0, sinPitch, cosPitch, 0,
             0, 0, 0, 1);

        Matrix4x4 yawMatrix = new Matrix4x4(
             cosYaw, 0, sinYaw, 0,
             0, 1, 0, 0,
            -sinYaw, 0, cosYaw, 0,
             0, 0, 0, 1);

        Matrix4x4 rollMatrix = new Matrix4x4(
             cosRoll, -sinRoll, 0, 0,
             sinRoll, cosRoll, 0, 0,
             0, 0, 1, 0,
             0, 0, 0, 1);

        relDeltaPoint = Vector3.Transform(Vector3.Transform(Vector3.Transform(absDeltaPoint, yawMatrix), pitchMatrix), rollMatrix);

        if (MathF.Abs(relDeltaPoint.Z) < nearClipPlane) relDeltaPoint.Z = relDeltaPoint.Z < 0 ? -nearClipPlane : nearClipPlane;
        return relDeltaPoint;
    }

    public Vector2 ProjectDeltaPoint(Vector3 deltaPoint) {
        Vector2 projPoint = new Vector2();

        projPoint.X = viewportDist * deltaPoint.X / deltaPoint.Z + Utils.screenSize.X / 2;
        projPoint.Y = viewportDist * deltaPoint.Y / deltaPoint.Z + Utils.screenSize.Y / 2;

        return projPoint;
    }

    public Vector2 ProjectPoint(Vector3 point) {
        return ProjectDeltaPoint(GetDeltaPoint(point));
    }

    public Vector3[] GetDeltaPolygon(Vector3[] points) {
        Vector3[] deltaPolygon = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++) {
            deltaPolygon[i] = GetDeltaPoint(points[i]);
        }
        return deltaPolygon;
    }

    public Vector2[] ProjectDeltaPolygon(Vector3[] deltaPoints) {
        Vector2[] projPoints = new Vector2[deltaPoints.Length];
        for (int i = 0; i < deltaPoints.Length; i++) {
            projPoints[i] = ProjectDeltaPoint(deltaPoints[i]);
        }
        return projPoints;
    }

    public Vector2[] ProjectPolygon(Vector3[] points) {
        Vector2[] projPoints = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++) {
            projPoints[i] = ProjectPoint(points[i]);
        }
        return projPoints;
    }
    public float PointDist(Vector3 point) {
        return Vector3.Distance(position, point);
    }

    public float PolygonDist(Vector3[] points) {
        return Vector3.Distance(position, points.Aggregate(Vector3.Zero, (sum, vector) => sum + vector) / points.Length);
    }
    public Vector2 GetLimits(Vector3 point) {
        return new Vector2(point.Z * MathF.Tan(hFOV / 2), point.Z * MathF.Tan(vFOV / 2));
    }

    public Vector2[] GetLimits(Vector3[] point) {
        Vector2[] limits = new Vector2[point.Length];
        for (int i = 0; i < point.Length; i++) limits[i] = GetLimits(point[i]);
        return limits;
    }

    public void RenderPolygon(Graphics graphics, Vector3[] points, Pen pen) {
        Vector3[] deltaPolygon = GetDeltaPolygon(points);
        PointF[] projPolygon = Utils.Vec2ToPointF_Flipped(ProjectDeltaPolygon(deltaPolygon));
        graphics.DrawPolygon(pen, projPolygon);
    }
    public void RenderPolygon(Graphics graphics, Vector3[] points, Brush brush) {
        Vector3[] deltaPolygon = GetDeltaPolygon(points);
        PointF[] projPolygon = Utils.Vec2ToPointF_Flipped(ProjectDeltaPolygon(deltaPolygon));
        graphics.FillPolygon(brush, projPolygon);
    }
    public void RenderPolygon(Graphics graphics, Vector3[] points, Pen pen, Brush brush) {
        int n = points.Length;
        Vector3[] deltaPolygon = GetDeltaPolygon(points);
        Vector2[] limits = GetLimits(deltaPolygon);
        List<Vector3> fixedPolygon = new List<Vector3>();
        int unrenderedPoints = 0;
        for (int i = 0; i < n; i++) {
            if (points[i].Z < 0 ||
                MathF.Abs(deltaPolygon[i].X) > limits[i].X ||
                MathF.Abs(deltaPolygon[i].Y) > limits[i].Y) unrenderedPoints++;
        }
        if (unrenderedPoints > 0) {
            for (int i = 0; i < deltaPolygon.Length; i++) {
                if (deltaPolygon[i].Z < 0) {
                    Vector3 currentPoint = deltaPolygon[i];
                    Vector3 prevPoint = deltaPolygon[(n + i - 1) % n];
                    Vector3 nextPoint = deltaPolygon[(i + 1) % n];

                    bool prevValid = !(prevPoint.Z < 0);
                    bool nextValid = !(nextPoint.Z < 0);

                    if (!prevValid && !nextValid) continue;
                    else {
                        if (prevValid) {
                            fixedPolygon.Add(new Vector3(
                                (prevPoint.Z - nearClipPlane) * (currentPoint.X - prevPoint.X) / (prevPoint.Z - currentPoint.Z) + prevPoint.X,
                                (prevPoint.Z - nearClipPlane) * (currentPoint.Y - prevPoint.Y) / (prevPoint.Z - currentPoint.Z) + prevPoint.Y,
                                nearClipPlane));
                        }
                        if (nextValid) {
                            fixedPolygon.Add(new Vector3(
                                (nextPoint.Z - nearClipPlane) * (currentPoint.X - nextPoint.X) / (nextPoint.Z - currentPoint.Z) + nextPoint.X,
                                (nextPoint.Z - nearClipPlane) * (currentPoint.Y - nextPoint.Y) / (nextPoint.Z - currentPoint.Z) + nextPoint.Y,
                                nearClipPlane));
                        }
                    }
                } else {
                    fixedPolygon.Add(deltaPolygon[i]);
                }
            }
            if (fixedPolygon.Count == 0) return;
            PointF[] projPolygon = Utils.Vec2ToPointF_Flipped(ProjectDeltaPolygon(fixedPolygon.ToArray()));
            graphics.FillPolygon(brush, projPolygon);
            graphics.DrawPolygon(pen, projPolygon);
        } else {
            PointF[] projPolygon = Utils.Vec2ToPointF_Flipped(ProjectDeltaPolygon(deltaPolygon));
            graphics.FillPolygon(brush, projPolygon);
            graphics.DrawPolygon(pen, projPolygon);
        }
    }
}
