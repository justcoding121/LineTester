using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced.Algorithms.Geometry
{
    internal static class DoubleExtensions
    {
        internal static bool IsEqual(this double a, double b, double tolerance)
        {
            return Math.Abs(a - b) < tolerance;
        }

        internal static bool IsLessThan(this double a, double b, double tolerance)
        {
            return (a - b) < -tolerance;
        }

        internal static bool IsLessThanOrEqual(this double a, double b, double tolerance)
        {
            var result = a - b;

            return result < -tolerance || Math.Abs(result) < tolerance;
        }

        internal static bool IsGreaterThan(this double a, double b, double tolerance)
        {
            return (a - b) > tolerance;
        }

        internal static bool IsGreaterThanOrEqual(this double a, double b, double tolerance)
        {
            var result = a - b;
            return result > tolerance || Math.Abs(result) < tolerance;
        }

        internal static int Compare(this double a, double b, double tolerance)
        {
            var result = a - b;

            if (Math.Abs(result) < tolerance)
            {
                return 0;
            }

            return (a - b) < -tolerance ? -1 : 1;
        }
    }
}
