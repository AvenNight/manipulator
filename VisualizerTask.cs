using System;
using System.Drawing;
using System.Windows.Forms;

namespace Manipulation
{
	public static class VisualizerTask
	{
		public static double X = 220;
		public static double Y = -100;
		public static double Alpha = 0.05;
		public static double Wrist = 2 * Math.PI / 3;
		public static double Elbow = 3 * Math.PI / 4;
		public static double Shoulder = Math.PI / 2;
		public static double Step = Math.PI / 18;

		public static Brush UnreachableAreaBrush = new SolidBrush(Color.FromArgb(255, 255, 230, 230));
		public static Brush ReachableAreaBrush = new SolidBrush(Color.FromArgb(255, 230, 255, 230));
		public static Pen ManipulatorPen = new Pen(Color.Black, 3);
		public static Brush JointBrush = Brushes.Gray;

		/// Реакцию на QAWS и пересчёт Wrist
		public static void KeyDown(Form form, KeyEventArgs key)
		{
			switch (key.KeyCode)
			{
				case Keys.Q:
					Shoulder += Step;
					break;
				case Keys.A:
					Shoulder -= Step;
					break;
				case Keys.W:
					Elbow += Step;
					break;
				case Keys.S:
					Elbow -= Step;
					break;
			}

			Wrist = -Alpha - Shoulder - Elbow;

			form.Invalidate();
		}

		/// TODO: Измените X и Y пересчитав координаты (e.X, e.Y) в логические.
		public static void MouseMove(Form form, MouseEventArgs e)
		{
			PointF coords = ConvertWindowToMath(e.Location, GetShoulderPos(form));
			X = coords.X;
			Y = coords.Y;

			UpdateManipulator();
			form.Invalidate();
		}

		/// TODO: Измените Alpha, используя e.Delta — размер прокрутки колеса мыши
		public static void MouseWheel(Form form, MouseEventArgs e)
		{
			Alpha += e.Delta > 0 ? Step : -Step;

			UpdateManipulator();
			form.Invalidate();
		}

		/// Вызовите ManipulatorTask.MoveManipulatorTo и обновите значения полей Shoulder, Elbow и Wrist, 
		/// если они не NaN. Это понадобится для последней задачи.
		public static void UpdateManipulator()
		{
			double[] angles = ManipulatorTask.MoveManipulatorTo(X, Y, Alpha);
			bool needUpdate = false;
			foreach (var angle in angles)
				needUpdate = !angle.Equals(double.NaN);
			if (needUpdate)
			{
				Shoulder = angles[0];
				Elbow = angles[1];
				Wrist = angles[2];
			}
		}

		/// Нарисуйте сегменты манипулятора методом graphics.DrawLine используя ManipulatorPen.
		/// Нарисуйте суставы манипулятора окружностями методом graphics.FillEllipse используя JointBrush.
		/// Не забудьте сконвертировать координаты из логических в оконные
		public static void DrawManipulator(Graphics graphics, PointF shoulderPos)
		{
			var joints = AnglesToCoordinatesTask.GetJointPositions(Shoulder, Elbow, Wrist);

			DrawReachableZone(graphics, ReachableAreaBrush, UnreachableAreaBrush, shoulderPos, joints);

			for (int i = 0; i < joints.Length; i++)
				joints[i] = ConvertMathToWindow(joints[i], shoulderPos);

			graphics.DrawLine(ManipulatorPen, shoulderPos, joints[0]);
			graphics.DrawLines(ManipulatorPen, joints);
			float r = 5;
			graphics.FillEllipse(JointBrush, -r, -r, 2 * r, 2 * r);
			foreach (var e in joints)
				graphics.FillEllipse(JointBrush, e.X - r, e.Y - r, 2 * r, 2 * r);

			graphics.DrawString(
				$"X={X:0}, Y={Y:0}, Alpha={Alpha:0.00}",
				new Font(SystemFonts.DefaultFont.FontFamily, 12),
				Brushes.DarkRed,
				10,
				10);
		}

		private static void DrawReachableZone(
			Graphics graphics,
			Brush reachableBrush,
			Brush unreachableBrush,
			PointF shoulderPos,
			PointF[] joints)
		{
			var rmin = Math.Abs(Manipulator.UpperArm - Manipulator.Forearm);
			var rmax = Manipulator.UpperArm + Manipulator.Forearm;
			var mathCenter = new PointF(joints[2].X - joints[1].X, joints[2].Y - joints[1].Y);
			var windowCenter = ConvertMathToWindow(mathCenter, shoulderPos);
			graphics.FillEllipse(reachableBrush, windowCenter.X - rmax, windowCenter.Y - rmax, 2 * rmax, 2 * rmax);
			graphics.FillEllipse(unreachableBrush, windowCenter.X - rmin, windowCenter.Y - rmin, 2 * rmin, 2 * rmin);
		}

		public static PointF GetShoulderPos(Form form)
		{
			return new PointF(form.ClientSize.Width / 2f, form.ClientSize.Height / 2f);
		}

		public static PointF ConvertMathToWindow(PointF mathPoint, PointF shoulderPos)
		{
			return new PointF(mathPoint.X + shoulderPos.X, shoulderPos.Y - mathPoint.Y);
		}

		public static PointF ConvertWindowToMath(PointF windowPoint, PointF shoulderPos)
		{
			return new PointF(windowPoint.X - shoulderPos.X, shoulderPos.Y - windowPoint.Y);
		}
	}
}