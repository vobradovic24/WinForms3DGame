using Microsoft.VisualBasic.Devices;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;

namespace WinForms3DGame; 
public partial class GameForm : Form {
    public static Camera camera;
    public static bool mouseLocked = false;
    public static HashSet<Keys> pressedKeys = new HashSet<Keys>();
    public static MovementMode movementMode = MovementMode.Walking;
    public static float movementSpeed = 0.1f;
    public static FractalType currentFractal = FractalType.Square;
    public static int fractalDepth = 0;
    public static Polygon[] polygons = [];
    public static float size = 10;
    public static Thread logicThread = new Thread(RunLogic);

    public void RenderFractal() {
        List<Polygon> newPolygons = new List<Polygon>();
        if (currentFractal == FractalType.Triangle) {
            TriangleFractal(0, 0, 0, size, size / 2 * MathF.Sqrt(3), newPolygons);
        } else if (currentFractal == FractalType.Pyramid) {
            List<Vector3[]> polyhedrons = new List<Vector3[]>();
            PyramidFractal(0, 0, 0, 0, size, size / 2 * MathF.Sqrt(3), size * MathF.Sqrt(2f / 3f), polyhedrons);
            foreach (Vector3[] polyhedron in polyhedrons) {
                newPolygons.Add(new Polygon([polyhedron[0], polyhedron[1], polyhedron[2]], Brushes.Orange));
                newPolygons.Add(new Polygon([polyhedron[0], polyhedron[1], polyhedron[3]], Brushes.Red));
                newPolygons.Add(new Polygon([polyhedron[0], polyhedron[2], polyhedron[3]], Brushes.Green));
                newPolygons.Add(new Polygon([polyhedron[1], polyhedron[2], polyhedron[3]], Brushes.Blue));
            }
        } else if (currentFractal == FractalType.Snowflake3D) {
            Snowflake3DFractal(0, 0, 0, 0, size, size / 2 * MathF.Sqrt(3), size * MathF.Sqrt(2f / 3f), newPolygons);
        }
        polygons = newPolygons.ToArray();
        //Invalidate(true);
    }

    public void TriangleFractal(int depth, float xOffset, float yOffset, float side, float height, List<Polygon> polygons) {
        if (depth == fractalDepth) {
            polygons.Add(new Polygon([
                new Vector3(xOffset, yOffset, 10),
                new Vector3(xOffset + side / 2, yOffset + height, 10),
                new Vector3(xOffset + side, yOffset, 10)
            ]));
        } else {
            TriangleFractal(depth + 1, xOffset, yOffset, side / 2, height / 2, polygons);
            TriangleFractal(depth + 1, xOffset + size / MathF.Pow(2, depth + 2), yOffset + size / 2 * MathF.Sqrt(3) / MathF.Pow(2, depth + 1), side / 2, height / 2, polygons);
            TriangleFractal(depth + 1, xOffset + size / MathF.Pow(2, depth + 1), yOffset, side / 2, height / 2, polygons);
        }
    }

    public void PyramidFractal(int depth, float xOffset, float yOffset, float zOffset, float side, float sideHeight, float height, List<Vector3[]> polyhedrons) {
        if (depth == fractalDepth) {
            polyhedrons.Add([
                new Vector3(xOffset, yOffset, zOffset),
                new Vector3(xOffset + side, yOffset, zOffset),
                new Vector3(xOffset + side / 2, yOffset, zOffset + sideHeight),
                new Vector3(xOffset + side / 2, yOffset + height, zOffset + sideHeight / 3)
            ]);
        } else {
            PyramidFractal(depth + 1, xOffset, yOffset, zOffset, side / 2, sideHeight / 2, height / 2, polyhedrons);
            PyramidFractal(depth + 1, xOffset + side / 2, yOffset, zOffset, side / 2, sideHeight / 2, height / 2, polyhedrons);
            PyramidFractal(depth + 1, xOffset + side / 4, yOffset, zOffset + sideHeight / 2, side / 2, sideHeight / 2, height / 2, polyhedrons);
            PyramidFractal(depth + 1, xOffset + side / 4, yOffset + height / 2, zOffset + sideHeight / 6, side / 2, sideHeight / 2, height / 2, polyhedrons);
        }
    }

    public void Snowflake3DFractal(int depth, float xOffset, float yOffset, float zOffset, float side, float sideHeight, float height, List<Polygon> polygons) {
        if (depth == fractalDepth) {
            Vector3[] polyhedron = [
                new Vector3(xOffset, yOffset, zOffset),
                new Vector3(xOffset + side, yOffset, zOffset),
                new Vector3(xOffset + side / 2, yOffset, zOffset + sideHeight),
                new Vector3(xOffset + side / 2, yOffset + height, zOffset + sideHeight / 3)
            ];
            polygons.Add(new Polygon([polyhedron[2], polyhedron[1], polyhedron[0]], Brushes.Red, Pens.Black));
            polygons.Add(new Polygon([polyhedron[0], polyhedron[1], polyhedron[3]], Brushes.DarkGreen, Pens.Black));
            polygons.Add(new Polygon([polyhedron[3], polyhedron[2], polyhedron[0]], Brushes.Orange, Pens.Black));
            polygons.Add(new Polygon([polyhedron[1], polyhedron[2], polyhedron[3]], Brushes.DarkBlue, Pens.Black));
        } else {
            Snowflake3DFractal(depth + 1, xOffset, yOffset, zOffset, side, sideHeight, height, polygons);
            List<Polygon> newPolygons = [];
            foreach (Polygon polygon in polygons) {
                Vector3 point0 = polygon.points[0];
                Vector3 point1 = polygon.points[1];
                Vector3 point2 = polygon.points[2];
                Vector3 midpoint0 = (point0 + point1) / 2;
                Vector3 midpoint1 = (point0 + point2) / 2;
                Vector3 midpoint2 = (point1 + point2) / 2;
                Vector3 middle = (point0 + point1 + point2) / 3;
                Vector3 v1 = midpoint1 - midpoint0;
                Vector3 v2 = midpoint2 - midpoint0;
                Vector3 normal = Vector3.Cross(v1, v2);
                normal /= normal.Length();
                Vector3 tip = middle + normal * height / MathF.Pow(2, fractalDepth - depth);
                newPolygons.Add(new Polygon([point0, midpoint0, midpoint1], polygon.brush, polygon.pen));
                newPolygons.Add(new Polygon([midpoint0, point1, midpoint2], polygon.brush, polygon.pen));
                newPolygons.Add(new Polygon([midpoint1, midpoint2, point2], polygon.brush, polygon.pen));
                newPolygons.Add(new Polygon([midpoint0, midpoint2, tip]));
                newPolygons.Add(new Polygon([midpoint2, midpoint1, tip]));
                newPolygons.Add(new Polygon([midpoint1, midpoint0, tip]));
            }
            polygons.Clear();
            polygons.AddRange(newPolygons);
        }
    }

    public GameForm() {
        InitializeComponent();
        DoubleBuffered = true;
        Utils.screenSize = new Vector2(ClientSize.Width, ClientSize.Height);
        Utils.windowCenter = PointToScreen(Utils.Vec2ToPoint(Utils.screenSize / 2));
        camera = new Camera(new Vector3(2f, 2f, 0f), new Vector3(0f, 0f, 0f), 100 * MathF.PI / 180);
        logicThread.Start();
    }

    private void Form1_Paint(object sender, PaintEventArgs e) {
        float[] distances = new float[polygons.Length];
        for (int i = 0; i < polygons.Length; i++)
            distances[i] = camera.PolygonDist(polygons[i]);
        Array.Sort(distances, polygons);
        Array.Reverse(polygons);

        for (int i = 0; i < polygons.Length; i++) {
            Polygon currentPoly = polygons[i];

            camera.RenderPolygon(e.Graphics, currentPoly);
        }

        e.Graphics.DrawString(
            $"FOV: {MathF.Round(camera.hFOV * 180 / MathF.PI, 4)}\n" +
            $"Pitch: {MathF.Round(camera.rotation.X * 180 / MathF.PI, 4)}\n" +
            $"Yaw: {MathF.Round(camera.rotation.Y * 180 / MathF.PI, 4)}\n" +
            $"Roll: {MathF.Round(camera.rotation.Z * 180 / MathF.PI, 4)}\n" +
            $"Position: {camera.position.X}, {camera.position.Y}, {camera.position.Z}\n" +
            $"Movement Mode: {Enum.GetName(typeof(MovementMode), movementMode)}\n" +
            $"Movement Speed: {movementSpeed}\n" +
            $"Fractal Depth: {fractalDepth}\n" +
            $"Fractal Type: {Enum.GetName(typeof(FractalType), currentFractal)}",
            SystemFonts.DefaultFont,
            Brushes.Black,
            5, 5);
    }

    private void Form1_Resize(object sender, EventArgs e) {
        Utils.screenSize = new Vector2(ClientSize.Width, ClientSize.Height);
        Utils.windowCenter = PointToScreen(Utils.Vec2ToPoint(Utils.screenSize / 2));
        camera.updateFOV(camera.hFOV);
        Invalidate(true);
    }

    private void Form1_Move(object sender, EventArgs e) {
        Utils.windowCenter = PointToScreen(Utils.Vec2ToPoint(Utils.screenSize / 2));
    }

    private void Form1_MouseMove(object sender, MouseEventArgs e) {
        if (!mouseLocked || !Focused) return;
        Vector2 deltaPos = Utils.PointToVector2(Cursor.Position) - Utils.PointToVector2(Utils.windowCenter);
        Cursor.Position = Utils.windowCenter;
        camera.rotation.Y += deltaPos.X * MathF.PI / 360;
        camera.rotation.X -= deltaPos.Y * MathF.PI / 360;
        camera.rotation.X = Math.Clamp(camera.rotation.X, -MathF.PI / 2, MathF.PI / 2);
        Invalidate(true);
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
        else if (e.KeyCode == Keys.Z) {
            currentFractal = (FractalType)((int)(currentFractal + 5) % 6);
            fractalDepth = 0;
            RenderFractal();
        } else if (e.KeyCode == Keys.X) {
            currentFractal = (FractalType)((int)(currentFractal + 1) % 6);
            fractalDepth = 0;
            RenderFractal();
        } else if (e.KeyCode == Keys.C) {
            fractalDepth = Math.Max(0, fractalDepth - 1);
            RenderFractal();
        } else if (e.KeyCode == Keys.V) {
            fractalDepth++;
            RenderFractal();
        } else lock (pressedKeys) pressedKeys.Add(e.KeyCode);
        movementSpeed = Math.Clamp(movementSpeed, 0.05f, 0.5f);
    }
    private void Form1_KeyUp(object sender, KeyEventArgs e) {
        lock (pressedKeys) pressedKeys.Remove(e.KeyCode);
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
                    if (newFOV <= MathF.PI * 3 / 2) camera.updateFOV(newFOV);
                    break;
                case Keys.OemMinus:
                    newFOV = camera.hFOV - MathF.PI / 36;
                    if (newFOV >= MathF.PI / 6) camera.updateFOV(newFOV);
                    break;
                case Keys.Control | Keys.Oemplus:
                    movementSpeed += 0.05f;
                    break;
            }
        }
        camera.rotation.Y = camera.rotation.Y % (2 * MathF.PI);

        if (posChange != Vector3.Zero) {
            switch (movementMode) {
                case MovementMode.Walking:
                    camera.position.Z += posChange.Z * MathF.Cos(-camera.rotation.Y) + posChange.X * MathF.Sin(-camera.rotation.Y);
                    camera.position.X += -posChange.Z * MathF.Sin(-camera.rotation.Y) + posChange.X * MathF.Cos(-camera.rotation.Y);
                    break;
                case MovementMode.Flying:
                    camera.position.Z += posChange.Z * MathF.Cos(-camera.rotation.Y) + posChange.X * MathF.Sin(-camera.rotation.Y);
                    camera.position.X += -posChange.Z * MathF.Sin(-camera.rotation.Y) + posChange.X * MathF.Cos(-camera.rotation.Y);
                    camera.position.Y += posChange.Y;
                    break;
                case MovementMode.Noclip:
                    float sinYaw = MathF.Sin(-camera.rotation.Y);
                    float cosYaw = MathF.Cos(camera.rotation.Y);

                    float sinPitch = MathF.Sin(camera.rotation.X);
                    float cosPitch = MathF.Cos(camera.rotation.X);

                    float sinRoll = MathF.Sin(camera.rotation.Z);
                    float cosRoll = MathF.Cos(camera.rotation.Z);

                    Matrix4x4 yawMatrix = new Matrix4x4(
                         cosYaw, 0,  sinYaw, 0,
                         0,      1,  0,      0,
                        -sinYaw, 0,  cosYaw, 0,
                         0,      0,  0,      1);

                    Matrix4x4 pitchMatrix = new Matrix4x4(
                         1,  0,         0, 0,
                         0,  cosPitch, -sinPitch, 0,
                         0,  sinPitch,  cosPitch, 0,
                         0,  0,         0,        1);

                    Matrix4x4 rollMatrix = new Matrix4x4(
                         cosRoll, -sinRoll, 0, 0,
                         sinRoll,  cosRoll, 0, 0,
                         0,        0,       1, 0,
                         0,        0,       0, 1);

                    Vector3 relMovement = Vector3.Transform(posChange, pitchMatrix * yawMatrix * rollMatrix);

                    
                    camera.position += relMovement;

                    break;
            }
        }
        Invalidate(true);
    }

    public static void RunLogic() {
        Stopwatch stopwatch = new Stopwatch();
        const int tickTime = 1000 / 60;
        try {
            while (true) {
                stopwatch.Start();

                // Logic Start

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
                            if (newFOV <= MathF.PI * 3 / 2) camera.updateFOV(newFOV);
                            break;
                        case Keys.OemMinus:
                            newFOV = camera.hFOV - MathF.PI / 36;
                            if (newFOV >= MathF.PI / 6) camera.updateFOV(newFOV);
                            break;
                        case Keys.Control | Keys.Oemplus:
                            movementSpeed += 0.05f;
                            break;
                    }
                }
                camera.rotation.Y = camera.rotation.Y % (2 * MathF.PI);

                if (posChange != Vector3.Zero) {
                    switch (movementMode) {
                        case MovementMode.Walking:
                            camera.position.Z += posChange.Z * MathF.Cos(-camera.rotation.Y) + posChange.X * MathF.Sin(-camera.rotation.Y);
                            camera.position.X += -posChange.Z * MathF.Sin(-camera.rotation.Y) + posChange.X * MathF.Cos(-camera.rotation.Y);
                            break;
                        case MovementMode.Flying:
                            camera.position.Z += posChange.Z * MathF.Cos(-camera.rotation.Y) + posChange.X * MathF.Sin(-camera.rotation.Y);
                            camera.position.X += -posChange.Z * MathF.Sin(-camera.rotation.Y) + posChange.X * MathF.Cos(-camera.rotation.Y);
                            camera.position.Y += posChange.Y;
                            break;
                        case MovementMode.Noclip:
                            float sinYaw = MathF.Sin(-camera.rotation.Y);
                            float cosYaw = MathF.Cos(camera.rotation.Y);

                            float sinPitch = MathF.Sin(camera.rotation.X);
                            float cosPitch = MathF.Cos(camera.rotation.X);

                            float sinRoll = MathF.Sin(camera.rotation.Z);
                            float cosRoll = MathF.Cos(camera.rotation.Z);

                            Matrix4x4 yawMatrix = new Matrix4x4(
                                 cosYaw, 0, sinYaw, 0,
                                 0, 1, 0, 0,
                                -sinYaw, 0, cosYaw, 0,
                                 0, 0, 0, 1);

                            Matrix4x4 pitchMatrix = new Matrix4x4(
                                 1, 0, 0, 0,
                                 0, cosPitch, -sinPitch, 0,
                                 0, sinPitch, cosPitch, 0,
                                 0, 0, 0, 1);

                            Matrix4x4 rollMatrix = new Matrix4x4(
                                 cosRoll, -sinRoll, 0, 0,
                                 sinRoll, cosRoll, 0, 0,
                                 0, 0, 1, 0,
                                 0, 0, 0, 1);

                            Vector3 relMovement = Vector3.Transform(posChange, pitchMatrix * yawMatrix * rollMatrix);


                            camera.position += relMovement;

                            break;
                    }
                }

                // Logic End

                stopwatch.Stop();
                int elapsedTime = (int)stopwatch.ElapsedMilliseconds;

                int timeToSleep = tickTime - elapsedTime;
                if (timeToSleep > 0) Thread.Sleep(timeToSleep);
                //else Console.Error.WriteLine($"Falling behind, tick took {elapsedTime}ms, current tick late {-timeToSleep}ms");
                stopwatch.Reset();
            }
        } catch (ThreadInterruptedException) {

        }
    }
}
