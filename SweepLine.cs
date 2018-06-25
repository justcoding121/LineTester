using Advanced.Algorithms.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Advanced.Algorithms.Geometry
{
    //point type
    internal enum EventType
    {
        Start = 0,
        Intersection = 1,
        End = 2
    }

    /// <summary>
    ///     A custom object representing start/end/intersection point.
    /// </summary>
    internal class Event : Point, IComparable
    {
        private readonly double tolerance;

        internal EventType Type;

        //The full line only if not an intersection event
        internal Line Segment;

        internal BentleyOttmann Algorithm;

        internal Line LastSweepLine;
        internal Point LastIntersection;

        internal Event(Point eventPoint, EventType eventType,
            Line lineSegment, BentleyOttmann algorithm)
            : base(eventPoint.X, eventPoint.Y)
        {
            tolerance = algorithm.Tolerance;
            Type = eventType;
            Segment = lineSegment;
            Algorithm = algorithm;
        }

        public int CompareTo(object that)
        {
            if (Equals(that))
            {
                return 0;
            }

            var thatEvent = that as Event;

            var line1 = Segment;
            var line2 = thatEvent.Segment;

            Point intersectionA;
            if (Type == EventType.Intersection)
            {
                intersectionA = this as Point;
            }
            else
            {
                if (LastSweepLine != null
                    && LastSweepLine == Algorithm.SweepLine)
                {
                    intersectionA = LastIntersection;
                }
                else
                {
                    intersectionA = LineIntersection.FindIntersection(line1, Algorithm.SweepLine, tolerance);
                    LastSweepLine = Algorithm.SweepLine;
                    LastIntersection = intersectionA;
                }
            }


            Point intersectionB;
            if (Type == EventType.Intersection)
            {
                intersectionB = thatEvent as Point;
            }
            else
            {
                if (thatEvent.LastSweepLine != null
                    && thatEvent.LastSweepLine == thatEvent.Algorithm.SweepLine)
                {
                    intersectionB = thatEvent.LastIntersection;
                }
                else
                {
                    intersectionB = LineIntersection.FindIntersection(line2, thatEvent.Algorithm.SweepLine, tolerance);
                    thatEvent.LastSweepLine = thatEvent.Algorithm.SweepLine;
                    thatEvent.LastIntersection = intersectionB;
                }
            }

            var result = intersectionA.Y.Compare(intersectionB.Y, tolerance);
            if (result != 0)
            {
                return result;
            }

            //if Y is same use slope as comparison
            double slope1 = line1.Slope;

            //if Y is same use slope as comparison
            double slope2 = line2.Slope;

            result = slope1.Compare(slope2, tolerance);
            if (result != 0)
            {
                return result;
            }

            //if slope is the same use diff of X co-ordinate
            result = line1.Left.X.Compare(line2.Left.X, tolerance);
            if (result != 0)
            {
                return result;
            }

            //if diff of X co-ordinate is same use diff of Y co-ordinate
            result = line1.Left.Y.Compare(line2.Left.Y, tolerance);

            //at this point this is guaranteed to be not same.
            //since we don't let duplicate lines with input HashSet of lines.
            //see line equals override in Line class.
            return result;
        }

        public override bool Equals(object that)
        {
            if (that == this)
            {
                return true;
            }

            var thatEvent = that as Event;

            if ((Type != EventType.Intersection && thatEvent.Type == EventType.Intersection)
                || (Type == EventType.Intersection && thatEvent.Type != EventType.Intersection))
            {
                return false;
            }

            if (Type == EventType.Intersection && thatEvent.Type == EventType.Intersection)
            {
                return new PointComparer(tolerance).Equals(this as Point, thatEvent);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    internal class PointComparer : IEqualityComparer<Point>
    {
        private readonly double tolerance;

        internal PointComparer(double tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool Equals(Point x, Point y)
        {
            // Check for null values 
            if (x == null || y == null)
            {
                return false;
            }

            if (x == y)
            {
                return true;
            }

            return (x.X.IsEqual(y.X, tolerance))
                        && (x.Y.IsEqual(y.Y, tolerance));
        }


        public int GetHashCode(Point point)
        {
            var hashCode = 33;
            hashCode = hashCode * -21 + Math.Truncate(point.X).GetHashCode();
            hashCode = hashCode * -21 + Math.Truncate(point.Y).GetHashCode();
            return hashCode;
        }
    }

    //Used to override event comparison when using BMinHeap for Event queue.
    internal class EventQueueComparer : Comparer<Event>
    {
        private readonly double tolerance;

        internal EventQueueComparer(double tolerance)
        {
            this.tolerance = tolerance;
        }

        public override int Compare(Event a, Event b)
        {
            //same object
            if (a == b)
            {
                return 0;
            }

            //compare X
            var result = a.X.CompareTo(b.X);

            if (result != 0)
            {
                return result;
            }

            //Left event first, then intersection and finally right.
            return a.Type.CompareTo(b.Type);
        }
    }

    /// <summary>
    ///     Bentley-Ottmann Algorithm
    /// </summary>
    public class BentleyOttmann
    {
        private readonly int precision;
        internal readonly double Tolerance;

        internal Line SweepLine;

        private RedBlackTree<Event> currentlyTracked;
        private Dictionary<Point, HashSet<Tuple<Event, Event>>> intersectionEvents;

        private HashSet<Event> specialLines;
        private HashSet<Event> normalLines;

        private Dictionary<Event, Event> rightLeftEventLookUp;

        private HashSet<Event> eventQueueLookUp;
        private BMinHeap<Event> eventQueue;

        public BentleyOttmann(int precision = 5)
        {
            this.precision = precision;
            Tolerance = Math.Round(Math.Pow(0.1, precision), precision);
        }

        private void initialize(IEnumerable<Line> lineSegments)
        {
            SweepLine = new Line(new Point(0, 0), new Point(0, int.MaxValue), Tolerance);

            var pointComparer = new PointComparer(Tolerance);

            currentlyTracked = new RedBlackTree<Event>(true, pointComparer);
            intersectionEvents = new Dictionary<Point, HashSet<Tuple<Event, Event>>>(pointComparer);

            specialLines = new HashSet<Event>();
            normalLines = new HashSet<Event>();

            rightLeftEventLookUp = lineSegments
                                   .Select(x =>
                                   {
                                       return new KeyValuePair<Event, Event>(
                                          new Event(x.Left, EventType.Start, x, this),
                                          new Event(x.Right, EventType.End, x, this)
                                       );

                                   }).ToDictionary(x => x.Value, x => x.Key);

            eventQueueLookUp = new HashSet<Event>(rightLeftEventLookUp.SelectMany(x => new[] {
                                    x.Key,
                                    x.Value
                                }), new PointComparer(Tolerance));

            eventQueue = new BMinHeap<Event>(eventQueueLookUp, new EventQueueComparer(Tolerance));

        }

        public Dictionary<Point, List<Line>> FindIntersections(IEnumerable<Line> lineSegments)
        {
            initialize(lineSegments);

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.ExtractMin();
                eventQueueLookUp.Remove(currentEvent);
                sweepTo(currentEvent);

                switch (currentEvent.Type)
                {
                    case EventType.Start:

                        if (specialLines.Count > 0)
                        {
                            foreach (var verticalLine in specialLines)
                            {
                                var intersection = findIntersection(currentEvent, verticalLine);
                                recordIntersection(currentEvent, verticalLine, intersection);
                            }
                        }

                        if (currentEvent.Segment.IsVertical || currentEvent.Segment.IsHorizontal)
                        {
                            specialLines.Add(currentEvent);

                            foreach (var verticalLine in normalLines)
                            {
                                var intersection = findIntersection(currentEvent, verticalLine);
                                recordIntersection(currentEvent, verticalLine, intersection);
                            }

                            break;
                        }

                        normalLines.Add(currentEvent);

                        currentlyTracked.Insert(currentEvent);

                        var lower = currentlyTracked.Previous(currentEvent);
                        var upper = currentlyTracked.Next(currentEvent);

                        var lowerIntersection = findIntersection(currentEvent, lower);
                        recordIntersection(currentEvent, lower, lowerIntersection);
                        enqueueIntersectionEvent(currentEvent, lowerIntersection);

                        var upperIntersection = findIntersection(currentEvent, upper);
                        recordIntersection(currentEvent, upper, upperIntersection);
                        enqueueIntersectionEvent(currentEvent, upperIntersection);

                        break;

                    case EventType.End:

                        currentEvent = rightLeftEventLookUp[currentEvent];

                        if (currentEvent.Segment.IsVertical || currentEvent.Segment.IsHorizontal)
                        {
                            specialLines.Remove(currentEvent);
                            break;
                        }

                        normalLines.Remove(currentEvent);

                        lower = currentlyTracked.Previous(currentEvent);
                        upper = currentlyTracked.Next(currentEvent);

                        currentlyTracked.Delete(currentEvent);

                        var upperLowerIntersection = findIntersection(lower, upper);
                        recordIntersection(lower, upper, upperLowerIntersection);
                        enqueueIntersectionEvent(currentEvent, upperLowerIntersection);

                        break;

                    case EventType.Intersection:

                        var intersectionLines = intersectionEvents[currentEvent as Point].ToList();

                        foreach (var item in intersectionLines)
                        {
                            currentlyTracked.Swap(item.Item1, item.Item2);

                            var upperLine = item.Item1;
                            var upperUpper = currentlyTracked.Next(upperLine);

                            var newUpperIntersection = findIntersection(upperLine, upperUpper);
                            recordIntersection(upperLine, upperUpper, newUpperIntersection);
                            enqueueIntersectionEvent(currentEvent, newUpperIntersection);

                            var lowerLine = item.Item2;
                            var lowerLower = currentlyTracked.Previous(lowerLine);

                            var newLowerIntersection = findIntersection(lowerLine, lowerLower);
                            recordIntersection(lowerLine, lowerLower, newLowerIntersection);
                            enqueueIntersectionEvent(currentEvent, newLowerIntersection);
                        }

                        break;
                }

            }

            return intersectionEvents.ToDictionary(x => x.Key,
                                                   x => x.Value.SelectMany(y => new[] { y.Item1.Segment, y.Item2.Segment })
                                                       .Distinct().ToList());
        }

        private void sweepTo(Event currentEvent)
        {
            SweepLine = new Line(new Point(currentEvent.X, 0), new Point(currentEvent.X, int.MaxValue), Tolerance);
        }

        private void enqueueIntersectionEvent(Event currentEvent, Point intersection)
        {
            if (intersection == null)
            {
                return;
            }

            var intersectionEvent = new Event(intersection, EventType.Intersection, null, this);

            if (intersectionEvent.X > SweepLine.Left.X
                || (intersectionEvent.X == SweepLine.Left.X
                   && intersectionEvent.Y > currentEvent.Y))
            {
                if (!eventQueueLookUp.Contains(intersectionEvent))
                {
                    eventQueue.Insert(intersectionEvent);
                    eventQueueLookUp.Add(intersectionEvent);
                }
            }

        }

        private Point findIntersection(Event a, Event b)
        {
            if (a == null || b == null
                || a.Type == EventType.Intersection
                || b.Type == EventType.Intersection)
            {
                return null;
            }

            return a.Segment.Intersection(b.Segment, Tolerance);
        }

        private void recordIntersection(Event line1, Event line2, Point intersection)
        {
            if (intersection == null)
            {
                return;
            }

            var existing = intersectionEvents.ContainsKey(intersection) ?
                intersectionEvents[intersection] : new HashSet<Tuple<Event, Event>>();

            if (line1.Segment.Slope.Compare(line2.Segment.Slope, Tolerance) > 0)
            {
                existing.Add(new Tuple<Event, Event>(line1, line2));
            }
            else
            {
                existing.Add(new Tuple<Event, Event>(line2, line1));
            }

            intersectionEvents[intersection] = existing;
        }

    }
}
