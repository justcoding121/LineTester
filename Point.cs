namespace Advanced.Algorithms.Geometry
{
    public class Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double X { get; set; }
        public double Y { get; set; }

        public override bool Equals(object that)
        {
            // Check for null values and compare run-time types.
            if (that == null || GetType() != that.GetType())
            {
                return false;
            }

            if (that == this)
            {
                return true;
            }

            var thatPoint = that as Point;
            return (thatPoint.X == X) && (thatPoint.Y == Y);
        }

        public override int GetHashCode()
        {
            var hashCode = 33;
            hashCode = hashCode * -21 + X.GetHashCode();
            hashCode = hashCode * -21 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return X.ToString("F") + " " + Y.ToString("F");
        }
    }
}
