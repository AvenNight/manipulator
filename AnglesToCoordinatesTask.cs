using NUnit.Framework;
using System;
using System.Drawing;

namespace Manipulation
{
    public static class AnglesToCoordinatesTask
    {
        static Func<double, float> sin = angle => (float)Math.Sin(angle);
        static Func<double, float> cos = angle => (float)Math.Cos(angle);

        /// <summary>
        /// По значению углов суставов возвращает массив координат суставов
        /// в порядке new []{elbow, wrist, palmEnd}
        /// </summary>
        public static PointF[] GetJointPositions(double shoulder, double elbow, double wrist)
        {
            double newAngle1 = shoulder + elbow - Math.PI;
            double newAngle2 = newAngle1 + wrist - Math.PI;

            float x = Manipulator.UpperArm * cos(shoulder);
            float y = Manipulator.UpperArm * sin(shoulder);
            var elbowPos = new PointF(x, y);

            x += Manipulator.Forearm * cos(newAngle1);
            y += Manipulator.Forearm * sin(newAngle1);
            var wristPos = new PointF(x, y);

            x += Manipulator.Palm * cos(newAngle2);
            y += Manipulator.Palm * sin(newAngle2);
            var palmEndPos = new PointF(x, y);

            return new PointF[]
            {
                elbowPos,
                wristPos,
                palmEndPos
            };
        }
    }

    [TestFixture]
    public class AnglesToCoordinatesTask_Tests
    {
        static Func<PointF, PointF, double> hypotenuse = (p1, p2) => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

        [TestCase(Math.PI / 2, Math.PI / 2, Math.PI, Manipulator.UpperArm, Manipulator.Forearm, Manipulator.Palm)]
        public void TestGetJointPositions(double shoulder, double elbow, double wrist, double upperArm, double forearm, double palm)
        {
            var joints = AnglesToCoordinatesTask.GetJointPositions(shoulder, elbow, wrist);
            
            Assert.AreEqual(forearm + palm, joints[2].X, 1e-9, "palm endX");
            Assert.AreEqual(upperArm, joints[2].Y, 1e-9, "palm endY");

            double upperArmActual = hypotenuse(joints[0], new Point(0, 0));
            double forearmActual = hypotenuse(joints[1], joints[0]);
            double palmActual = hypotenuse(joints[2], joints[1]);

            Assert.AreEqual(upperArm, upperArmActual, 1e-9, "UpperArm length");
            Assert.AreEqual(forearm, forearmActual, 1e-9, "Forearm length");
            Assert.AreEqual(palm, palmActual, 1e-9, "Palm length");
        }
    }
}