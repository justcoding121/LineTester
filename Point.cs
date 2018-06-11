namespace Advanced.Algorithms.Geometry
{
    public class Point
    {
        private readonly int precision;
        public Point(double x, double y, int precision = 5)
        {
            X = x;
            Y = y;
            this.precision = precision;
        }
        public double X { get; set; }
        public double Y { get; set; }

        public override bool Equals(object that)
        {
            // Check for null values 
            if (that == null)
            {
                return false;
            }

            if (that == this)
            {
                return true;
            }

            var thatPoint = that as Point;
            return (thatPoint.X.Truncate(precision) ==  X.Truncate(precision)) 
                        && (thatPoint.Y.Truncate(precision) == Y.Truncate(precision));
        }

        public override int GetHashCode()
        {
            var hashCode = 33;
            hashCode = hashCode * -21 + X.Truncate(precision).GetHashCode();
            hashCode = hashCode * -21 + Y.Truncate(precision).GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return X.ToString("F") + " " + Y.ToString("F");
        }
    }
}
