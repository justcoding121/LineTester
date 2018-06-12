namespace Advanced.Algorithms.Geometry
{
    public class Line
    {
        public Line(Point start, Point end)
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

        public Point Left { get; set; }
        public Point Right { get; set; }

        public bool IsVertical => Left.X.Truncate().Equals(Right.X.Truncate());
        public bool IsHorizontal => Left.Y.Truncate().Equals(Right.Y.Truncate());

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

            if(Left.Equals(thatLine.Left)
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
