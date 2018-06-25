using System;

namespace Advanced.Algorithms.Geometry
{
    public class Line
    {
        private readonly double tolerance;

        public Point Left { get; private set; }
        public Point Right { get; private set; }

        public bool IsVertical => Left.X.IsEqual(Right.X, tolerance);
        public bool IsHorizontal => Left.Y.IsEqual(Right.Y, tolerance);

        public double Slope => slope.Value;

        private Line(double tolerance)
        {
            this.tolerance = tolerance;
            slope = new Lazy<double>(() => calcSlope());
        }

        internal Line(Point start, Point end, double tolerance)
            : this(tolerance)
        {
            if (start.X < end.X)
            {
                Left = start;
                Right = end;
            }
            else if (start.X > end.X)
            {
                Left = end;
                Right = start;
            }
            else
            {
                //use Y
                if (start.Y < end.Y)
                {
                    Left = start;
                    Right = end;
                }
                else
                {
                    Left = end;
                    Right = start;
                }
            }
        }

        public Line(Point start, Point end, int precision = 5)
        : this(start, end, Math.Round(Math.Pow(0.1, precision), precision)) { }

        private readonly Lazy<double> slope;
        private double calcSlope()
        {
            Point left = Left, right = Right;

            //vertical line has infinite slope
            if (left.Y.IsEqual(right.Y, tolerance))
            {
                return double.MaxValue;
            }

            return ((right.Y - left.Y) / (right.X - left.X));
        }

        public Line Clone()
        {
            return new Line(Left.Clone(), Right.Clone());
        }
    }
}
