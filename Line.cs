using System;

namespace Advanced.Algorithms.Geometry
{
    public class Line
    {
        public Point Left { get; private set; }
        public Point Right { get; private set; }

        public bool IsVertical => Left.X.Truncate().Equals(Right.X.Truncate());
        public bool IsHorizontal => Left.Y.Truncate().Equals(Right.Y.Truncate());

        public double Slope => slope.Value;

        internal Line()
        {
            slope = new Lazy<double>(() => calcSlope());
        }

        public Line(Point start, Point end)
            : this()
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

        private readonly Lazy<double> slope;
        private double calcSlope()
        {
            Point left = Left, right = Right;

            //vertical line has infinite slope
            if (left.Y.Truncate() == right.Y.Truncate())
            {
                return double.MaxValue;
            }

            return ((right.Y - left.Y) / (right.X - left.X)).Truncate();
        }

        public Line Clone()
        {
            return new Line(Left.Clone(), Right.Clone());
        }

        public override bool Equals(object that)
        {
            // Check for null values.
            if (that == null)
            {
                return false;
            }

            if (that == this)
            {
                return true;
            }

            var thatLine = that as Line;

            if (Left.Equals(thatLine.Left)
                && Right.Equals(thatLine.Right))
            {
                return true;
            }

            return false;
        }


        public override int GetHashCode()
        {
            var hashCode = 18;
            hashCode = hashCode * -23 + Left.GetHashCode();
            hashCode = hashCode * -23 + Right.GetHashCode();
            return hashCode;
        }

    }


}
