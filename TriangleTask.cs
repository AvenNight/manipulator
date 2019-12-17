using NUnit.Framework;
using System;

namespace Manipulation
{
    public class TriangleTask
    {
        /// <summary>
        /// Возвращает угол (в радианах) между сторонами a и b в треугольнике со сторонами a, b, c 
        /// c^2 = a^2 + b^2 - 2ab·cos γ
        /// cosAB = (a^2 + b^2 - c^2) / 2ab
        /// </summary>
        public static double GetABAngle(double a, double b, double c)
        {
            if (a < 0 || b < 0 || c < 0)
                return double.NaN;

            double gamma = Math.Acos((a * a + b * b - c * c) / (2 * a * b));

            return gamma;
        }
    }

    [TestFixture]
    public class TriangleTask_Tests
    {
        [TestCase(3, 4, 5, Math.PI / 2)]
        [TestCase(1, 1, 1, Math.PI / 3)]
        [TestCase(3, 4, 5, 1.5707963267949d)]
        [TestCase(1, 1, 1, 1.0471975511966d)]
        [TestCase(0, 0, 0, double.NaN)]
        [TestCase(1, 1, 3, double.NaN)]
        [TestCase(3, 4.2426406871193, 3, Math.PI / 4)]
        [TestCase(-1, 1, 1, double.NaN)]
        [TestCase(4, 3, 5, 1.5707963267949)]
        [TestCase(0, 1, 1, double.NaN)]
        [TestCase(1, 0, 1, double.NaN)]
        [TestCase(1, 1, 0, 0)]
        public void TestGetABAngle(double a, double b, double c, double expectedAngle)
        {
            Assert.AreEqual(expectedAngle, TriangleTask.GetABAngle(a, b, c), 1e-9);
        }
    }
}