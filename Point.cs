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
        public double X { get; private set; }
        public double Y { get; private set; }

        public override string ToString()
        {
            return X.ToString("F") + " " + Y.ToString("F");
        }

        public Point Clone()
        {
            return new Point(X, Y);
        }
    }
}
