using Advanced.Algorithms.DataStructures;
using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Line, double> slopeCache;

        internal EventType Type;

        //The full line only if not an intersection event
        internal Line Segment;

        internal Line SweepLine;

        internal Line LastSweepLine;
        internal Point LastIntersection;

        internal Event(Point eventPoint, EventType eventType,
            Line lineSegment, Line sweepLine, Dictionary<Line, double> slopeCache)
            : base(eventPoint.X, eventPoint.Y)
        {
            Type = eventType;
            Segment = lineSegment;
            SweepLine = sweepLine;
            this.slopeCache = slopeCache;
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
                    && LastSweepLine.Equals(SweepLine))
                {
                    intersectionA = LastIntersection;
                }
                else
                {
                    intersectionA = LineIntersection.FindIntersection(line1, SweepLine);
                    LastSweepLine = SweepLine.Clone();
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
                    && thatEvent.LastSweepLine.Equals(thatEvent.SweepLine))
                {
                    intersectionB = thatEvent.LastIntersection;
                }
                else
                {
                    intersectionB = LineIntersection.FindIntersection(line2, thatEvent.SweepLine);
                    thatEvent.LastSweepLine = thatEvent.SweepLine.Clone();
                    thatEvent.LastIntersection = intersectionB;
                }
            }

            var result = intersectionA.Y.Truncate().CompareTo(intersectionB.Y.Truncate());
            if (result != 0)
            {
                return result;
            }

            //if Y is same use slope as comparison
            double slope1;
            if (slopeCache.ContainsKey(line1))
            {
                slope1 = slopeCache[line1];
            }
            else
            {
                slope1 = getSlope(line1).Truncate();
                slopeCache[line1] = slope1;
            }

            //if Y is same use slope as comparison
            double slope2;
            if (slopeCache.ContainsKey(line2))
            {
                slope2 = slopeCache[line2];
            }
            else
            {
                slope2 = getSlope(line2).Truncate();
                slopeCache[line2] = slope2;
            }

            result = slope1.CompareTo(slope2);
            if (result != 0)
            {
                return result;
            }

            //if slope is the same use diff of X co-ordinate
            result = line1.Left.X.Truncate().CompareTo(line2.Left.X.Truncate());
            if (result != 0)
            {
                return result;
            }

            //if diff of X co-ordinate is same use diff of Y co-ordinate
            result = line1.Left.Y.Truncate().CompareTo(line2.Left.Y.Truncate());

            //at this point this is guaranteed to be not same.
            //since we don't let duplicate lines with input HashSet of lines.
            //see line equals override in Line class.
            return result;
        }

        private double getSlope(Line line)
        {
            Point left = line.Left, right = line.Right;

            //vertical line has infinite slope
            if (left.Y.Truncate() == right.Y.Truncate())
            {
                return double.MaxValue;
            }

            return (right.Y - left.Y) / (right.X - left.X);
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
                return base.Equals(thatEvent as Point);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    //Used to override event comparison when using BMinHeap for Event queue.
    internal class EventQueueComparer : Comparer<Event>
    {
        public override int Compare(Event a, Event b)
        {
            //same object
            if (a == b)
            {
                return 0;
            }

            //compare X
            var result = a.X.Truncate().CompareTo(b.X.Truncate());

            if (result != 0)
            {
                return result;
            }

            //compare Y
            result = a.Y.Truncate().CompareTo(b.Y.Truncate());
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
    public class SweepLineIntersection
    {
        internal static int intersectionCount;
        public static Dictionary<Point, List<Line>> FindIntersections(HashSet<Line> lineSegments)
        {
            var sweepLine = new Line(new Point(0, 0), new Point(0, int.MaxValue));
            var slopeCache = new Dictionary<Line, double>();

            var lineRightLeftMap = lineSegments
                                   .Select(x =>
                                   {
                                       return new KeyValuePair<Event, Event>(
                                          new Event(x.Left, EventType.Start, x, sweepLine, slopeCache),
                                          new Event(x.Right, EventType.End, x, sweepLine, slopeCache)
                                       );

                                   }).ToDictionary(x => x.Value, x => x.Key);

            var currentEvents = new HashSet<Event>(lineRightLeftMap.SelectMany(x => new[] {
                                    x.Key,
                                    x.Value
                                }));

            var eventQueue = new BMinHeap<Event>(currentEvents, new EventQueueComparer());

            var currentlyTracked = new RedBlackTree<Event>(true);

            var intersectionEvents = new Dictionary<Point, HashSet<Tuple<Event, Event>>>();

            var specialLines = new HashSet<Event>();
            var normalLines = new HashSet<Event>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.ExtractMin();
              
                intersectionCount = Math.Max(intersectionCount, currentlyTracked.Count);
                switch (currentEvent.Type)
                {
                    case EventType.Start:

                        sweepLine.Left.X = currentEvent.X;
                        sweepLine.Right.X = currentEvent.X;

                        if (specialLines.Count > 0)
                        {
                            foreach (var verticalLine in specialLines)
                            {
                                var intersection = findIntersection(currentEvent, verticalLine);
                                recordIntersection(intersectionEvents, currentEvent, verticalLine, intersection);
                            }
                        }

                        if (currentEvent.Segment.IsVertical || currentEvent.Segment.IsHorizontal)
                        {
                            specialLines.Add(currentEvent);

                            foreach (var verticalLine in normalLines)
                            {
                                var intersection = findIntersection(currentEvent, verticalLine);
                                recordIntersection(intersectionEvents, currentEvent, verticalLine, intersection);
                            }

                            break;
                        }

                        normalLines.Add(currentEvent);

                        currentlyTracked.Insert(currentEvent);

                        var lower = currentlyTracked.Previous(currentEvent);
                        var upper = currentlyTracked.Next(currentEvent);

                        var lowerIntersection = findIntersection(currentEvent, lower);
                        recordIntersection(intersectionEvents, currentEvent, lower, lowerIntersection);
                        enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, lowerIntersection);

                        var upperIntersection = findIntersection(currentEvent, upper);
                        recordIntersection(intersectionEvents, currentEvent, upper, upperIntersection);
                        enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, upperIntersection);

                        break;

                    case EventType.End:

                        sweepLine.Left.X = currentEvent.X;
                        sweepLine.Right.X = currentEvent.X;

                        currentEvent = lineRightLeftMap[currentEvent];

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
                        recordIntersection(intersectionEvents, lower, upper, upperLowerIntersection);
                        enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, upperLowerIntersection);

                        break;

                    case EventType.Intersection:

                        sweepLine.Left.X = currentEvent.X;
                        sweepLine.Right.X = currentEvent.X;

                        var intersectionLines = intersectionEvents[currentEvent as Point].ToList();

                        foreach (var item in intersectionLines)
                        {
                            currentlyTracked.Swap(item.Item1, item.Item2);

                            item.Item1.Segment.Left.X = currentEvent.X;
                            item.Item1.Segment.Left.Y = currentEvent.Y;

                            item.Item2.Segment.Left.Y = currentEvent.Y;
                            item.Item2.Segment.Left.X = currentEvent.X;
                        }

                        foreach (var item in intersectionLines)
                        {
                            var upperLine = item.Item1;

                            var upperUpper = currentlyTracked.Next(upperLine);

                            var newUpperIntersection = findIntersection(upperLine, upperUpper);
                            recordIntersection(intersectionEvents, upperLine, upperUpper, newUpperIntersection);
                            enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, newUpperIntersection);

                            var lowerLine = item.Item2;

                            var lowerLower = currentlyTracked.Previous(lowerLine);

                            var newLowerIntersection = findIntersection(lowerLine, lowerLower);
                            recordIntersection(intersectionEvents, lowerLine, lowerLower, newLowerIntersection);
                            enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, newLowerIntersection);
                        }

                        break;
                }
                currentEvents.Remove(currentEvent);
            }

            return intersectionEvents.ToDictionary(x => x.Key,
                                                   x => x.Value.SelectMany(y => new[] { y.Item1.Segment, y.Item2.Segment })
                                                        .Distinct()
                                                        .ToList());
        }

        private static void enqueueIntersectionEvent(BMinHeap<Event> eventQueue,
         HashSet<Event> currentEvents,
          Event currentEvent, Line sweepLine, Point intersection)
        {
            if (intersection == null)
            {
                return;
            }

            var intersectionEvent = new Event(intersection, EventType.Intersection, null, sweepLine, null);

            if (currentEvent.X.Truncate() < intersectionEvent.X.Truncate()
                || (currentEvent.X.Truncate() == intersectionEvent.X.Truncate()
                   && currentEvent.Y.Truncate() < intersectionEvent.Y.Truncate()))
            {
                if (!currentEvents.Contains(intersectionEvent))
                {
                    eventQueue.Insert(intersectionEvent);
                    currentEvents.Add(intersectionEvent);
                }
            }

        }

        private static Point findIntersection(Event a, Event b)
        {
            if (a == null || b == null
                || a.Type == EventType.Intersection
                || b.Type == EventType.Intersection)
            {
                return null;
            }

            return a.Segment.Intersection(b.Segment);
        }

        private static void recordIntersection(Dictionary<Point, HashSet<Tuple<Event, Event>>> intersectionEvents,
            Event line1, Event line2, Point intersection)
        {
            if (intersection == null)
            {
                return;
            }

            var existing = intersectionEvents.ContainsKey(intersection) ?
                    intersectionEvents[intersection] : new HashSet<Tuple<Event, Event>>();

            if (line1.CompareTo(line2) < 0)
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
