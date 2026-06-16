using System.Numerics;
using System.Drawing.Drawing2D;
using Microsoft.VisualBasic.Devices;
using System.Drawing.Design;
using System.Drawing;
using System.Runtime.InteropServices.Marshalling;
using System.Reflection;

namespace WinForms3DGame {
    public partial class Form1 : Form {
        Camera camera;
        public bool mouseLocked = false;
        HashSet<Keys> pressedKeys = new HashSet<Keys>();
        MovementMode movementMode = MovementMode.Normal;
        float movementSpeed = 0.1f;

        public Form1() {
            InitializeComponent();
            DoubleBuffered = true;
            Utils.screenSize = new Vector2(ClientSize.Width, ClientSize.Height);
            Utils.windowCenter = PointToScreen(Utils.Vec2ToPoint(Utils.screenSize / 2));
            camera = new Camera(new Vector3(2f, 2f, 0f), new Vector3(0f, 0f, 0f), 100 * MathF.PI / 180);
            gameTimer.Interval = 1000 / 60;
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            Vector3[] poly1 = { new Vector3(0f, 4f, 8f), new Vector3(4f, 4f, 8f), new Vector3(4f, 0f, 8f), new Vector3(0f, 0f, 8f) };
            PointF[] projPoly1 = Utils.Vec2ToPointF_Flipped(camera.ProjectPolygon(poly1));
            Vector3[] poly2 = { new Vector3(4f, 4f, 8f), new Vector3(4f, 4f, 4f), new Vector3(4f, 0f, 4f), new Vector3(4f, 0f, 8f) };
            PointF[] projPoly2 = Utils.Vec2ToPointF_Flipped(camera.ProjectPolygon(poly2));
            Vector3[] poly3 = { new Vector3(0f, 0f, 8f), new Vector3(4f, 4f, 4f), new Vector3(4f, 0f, 4f) };
            PointF[] projPoly3 = Utils.Vec2ToPointF_Flipped(camera.ProjectPolygon(poly3));

            float[] distances = { camera.PolygonDist(poly1), camera.PolygonDist(poly2), camera.PolygonDist(poly3) };
            Vector3[][] polygons = { poly1, poly2, poly3 };
            Array.Sort(distances, polygons);

            Pen pen = new Pen(Color.Black);
            Brush brushBlue = new SolidBrush(Color.Blue);
            Brush brushGreen = new SolidBrush(Color.Green);
            Brush brushRed = new SolidBrush(Color.Red);
            Brush brushBlack = new SolidBrush(Color.Black);

            for (int i = 2; i >= 0; i--) {
                Brush brush;
                Vector3[] currentPoly = polygons[i];

                if (currentPoly == poly1) brush = brushBlue;
                else if (currentPoly == poly2) brush = brushGreen;
                else if (currentPoly == poly3) brush = brushRed;
                else brush = brushBlack;

                camera.RenderPolygon(e.Graphics, currentPoly, pen, brush);
            }
        }

        private void Form1_Resize(object sender, EventArgs e) {
            Utils.screenSize = new Vector2(ClientSize.Width, ClientSize.Height);
            Utils.windowCenter = PointToScreen(Utils.Vec2ToPoint(Utils.screenSize / 2));
            camera.updateFOV(camera.hFOV);
            Invalidate();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            if (!mouseLocked || !Focused) return;
            Vector2 deltaPos = Utils.PointToVector2(Cursor.Position) - Utils.PointToVector2(Utils.windowCenter);
            Cursor.Position = Utils.windowCenter;
            camera.rotation.Y += deltaPos.X * MathF.PI / 360;
            camera.rotation.X -= deltaPos.Y * MathF.PI / 360;
            camera.rotation.X = Math.Clamp(camera.rotation.X, -MathF.PI / 2, MathF.PI / 2);
            Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.ControlKey) {
                mouseLocked = !mouseLocked;
                if (mouseLocked) {
                    Cursor.Hide();
                    Cursor.Position = Utils.windowCenter;
                } else Cursor.Show();
            } else if (e.KeyCode == Keys.NumPad6) movementSpeed += 0.05f;
            else if (e.KeyCode == Keys.NumPad4) movementSpeed -= 0.05f;
            else if (e.KeyCode == Keys.NumPad8) movementMode = (MovementMode)((int)(movementMode + 1) % 3);
            else if (e.KeyCode == Keys.NumPad2) movementMode = (MovementMode)((int)(movementMode + 2) % 3);
            else pressedKeys.Add(e.KeyCode);
            movementSpeed = Math.Clamp(movementSpeed, 0.05f, 0.5f);
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e) {
            pressedKeys.Remove(e.KeyCode);
        }

        private void gameTimer_Tick(object sender, EventArgs e) {
            Vector3 posChange = Vector3.Zero;
            foreach (Keys key in pressedKeys) {
                switch (key) {
                    case Keys.Right:
                        camera.rotation.Y += MathF.PI / 180;
                        break;
                    case Keys.Left:
                        camera.rotation.Y -= MathF.PI / 180;
                        break;
                    case Keys.Up:
                        camera.rotation.X += MathF.PI / 180;
                        if (camera.rotation.X > MathF.PI / 2) camera.rotation.X = MathF.PI / 2;
                        break;
                    case Keys.Down:
                        camera.rotation.X -= MathF.PI / 180;
                        if (camera.rotation.X < -MathF.PI / 2) camera.rotation.X = -MathF.PI / 2;
                        break;
                    case Keys.E:
                        camera.rotation.Z += MathF.PI / 180;
                        camera.rotation.Z %= 2 * MathF.PI;
                        break;
                    case Keys.Q:
                        camera.rotation.Z -= MathF.PI / 180;
                        camera.rotation.Z %= 2 * MathF.PI;
                        break;
                    case Keys.W:
                        posChange.Z += movementSpeed;
                        break;
                    case Keys.S:
                        posChange.Z -= movementSpeed;
                        break;
                    case Keys.D:
                        posChange.X += movementSpeed;
                        break;
                    case Keys.A:
                        posChange.X -= movementSpeed;
                        break;
                    case Keys.Space:
                        posChange.Y += movementSpeed;
                        break;
                    case Keys.ShiftKey:
                        posChange.Y -= movementSpeed;
                        break;
                    case Keys.Oemplus:
                        float newFOV = camera.hFOV + MathF.PI / 36;
                        if (newFOV <= MathF.PI * 2 / 3) camera.updateFOV(newFOV);
                        break;
                    case Keys.OemMinus:
                        newFOV = camera.hFOV - MathF.PI / 36;
                        if (newFOV >= MathF.PI / 3) camera.updateFOV(newFOV);
                        break;
                    case Keys.Control | Keys.Oemplus:
                        movementSpeed += 0.05f;
                        break;
                }
            }
            camera.rotation.Y = camera.rotation.Y % (2 * MathF.PI);

            if (posChange != Vector3.Zero) {
                switch (movementMode) {
                    case MovementMode.Normal:
                        camera.position.Z += posChange.Z * MathF.Cos(-camera.rotation.Y) + posChange.X * MathF.Sin(-camera.rotation.Y);
                        camera.position.X += -posChange.Z * MathF.Sin(-camera.rotation.Y) + posChange.X * MathF.Cos(-camera.rotation.Y);
                        break;
                    case MovementMode.Flying:
                        camera.position.Z += posChange.Z * MathF.Cos(-camera.rotation.Y) + posChange.X * MathF.Sin(-camera.rotation.Y);
                        camera.position.X += -posChange.Z * MathF.Sin(-camera.rotation.Y) + posChange.X * MathF.Cos(-camera.rotation.Y);
                        camera.position.Y += posChange.Y;
                        break;
                    /*/ Source-like noclip
                    case MovementMode.Noclip:
                        float sinPitch = MathF.Sin(camera.rotation.X);
                        float cosPitch = MathF.Cos(camera.rotation.X);

                        float sinYaw = MathF.Sin(camera.rotation.Y);
                        float cosYaw = MathF.Cos(camera.rotation.Y);

                        float sinRoll = MathF.Sin(-camera.rotation.Z);
                        float cosRoll = MathF.Cos(-camera.rotation.Z);

                        Matrix4x4 rollMatrix = new Matrix4x4(
                            cosRoll, -sinRoll, 0, 0,
                            sinRoll, cosRoll, 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);

                        Matrix4x4 yawMatrix = new Matrix4x4(
                            );
                        break;
                    //*/
                }
            }

            label1.Text =
                $"FOV: {MathF.Round(camera.hFOV * 180 / MathF.PI, 4)}\n" +
                $"Pitch: {MathF.Round(camera.rotation.X * 180 / MathF.PI, 4)}\n" +
                $"Yaw: {MathF.Round(camera.rotation.Y * 180 / MathF.PI, 4)}\n" +
                $"Roll: {MathF.Round(camera.rotation.Z * 180 / MathF.PI, 4)}\n" +
                $"Position: {camera.position.X}, {camera.position.Y}, {camera.position.Z}\n" +
                $"Movement Mode: {(movementMode == MovementMode.Normal ? "Normal" : movementMode == MovementMode.Flying ? "Flying" : "Noclip")}\n" +
                $"Movement Speed: {movementSpeed}";
            Invalidate();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e) {
        }
    }

    public enum MovementMode {
        Normal = 0,
        Flying = 1,
        Noclip = 2
    }

    public class Polygon {
        Vector3[] points;
        Color fillColor;
        Color outlineColor;
        
        public Polygon(Vector3[] points, Color fillColor, Color outlineColor) {
            this.points = points;
            this.fillColor = fillColor;
            this.outlineColor = outlineColor;
        }
    }

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

            relDeltaPoint = Vector3.Transform(Vector3.Transform(Vector3.Transform(absDeltaPoint, rollMatrix), yawMatrix), pitchMatrix);

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
}
