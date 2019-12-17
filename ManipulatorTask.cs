using NUnit.Framework;
using System;
using System.Drawing;

namespace Manipulation
{
    public static class ManipulatorTask
    {
        static Func<double, double, double, double, double> distance =
            (x1, y1, x2, y2) => Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        /// <summary>
        /// Возвращает массив углов (shoulder, elbow, wrist),
        /// необходимых для приведения эффектора манипулятора в точку x и y 
        /// с углом между последним суставом и горизонталью, равному alpha (в радианах)
        /// См. чертеж manipulator.png!
        /// Используйте поля Forearm, UpperArm, Palm класса Manipulator
        /// </summary>
        public static double[] MoveManipulatorTo(double x, double y, double alpha)
        {
            double wristX = x >= 0 ?
                x - Manipulator.Palm * Math.Cos(alpha) :
                Math.Abs(x) + Manipulator.Palm * Math.Cos(alpha);
            double wristY = y + Manipulator.Palm * Math.Sin(alpha);

            double distanceToWrist = distance(0, 0, wristX, wristY);
            double distanceWristToX = distance(x, 0, wristX, wristY);

            double elbow = TriangleTask.GetABAngle(Manipulator.UpperArm, Manipulator.Forearm, distanceToWrist);

            double shoulder1 = TriangleTask.GetABAngle(Manipulator.UpperArm, distanceToWrist, Manipulator.Forearm);
            double shoulder2 = TriangleTask.GetABAngle(distanceToWrist, Math.Abs(x), distanceWristToX);
            shoulder2 *= wristY < 0 ? -1 : 1;

            double shoulder = shoulder1 + shoulder2;

            double wrist = -alpha - shoulder - elbow;

            if (double.IsNaN(shoulder) || double.IsNaN(elbow) || double.IsNaN(wrist))
                return new[] { double.NaN, double.NaN, double.NaN };
            return new[] { shoulder, elbow, wrist };
        }
    }

    [TestFixture]
    public class ManipulatorTask_Tests
    {
        static (double x, double y, double angle) Generate()
        {
            Random rnd = new Random();

            double x = (rnd.NextDouble() - 0.5) * 100;
            double y = (rnd.NextDouble() - 0.5) * 100;
            double angle = (rnd.NextDouble() - 0.5) * 2 * Math.PI;

            return (x, y, angle);
        }

        [Test]
        [Repeat(100)]
        public void TestMoveManipulatorTo()
        {
            var p = Generate();
            var angles = ManipulatorTask.MoveManipulatorTo(p.x, p.y, p.angle);
            var points = AnglesToCoordinatesTask.GetJointPositions(angles[0], angles[1], angles[2]);
            var generatePoint = new PointF((float)p.x, (float)p.y);

            Assert.AreEqual(generatePoint.X, points[2].X, 1e-4);
            Assert.AreEqual(generatePoint.Y, points[2].Y, 1e-4);
        }
    }
}