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
        internal EventType Type;

        //The full line only if not an intersection event
        internal Line Segment;

        internal Line SweepLine;

        internal Event(Point eventPoint, EventType eventType, Line lineSegment, Line sweepLine)
            : base(eventPoint.X, eventPoint.Y)
        {
            Type = eventType;
            Segment = lineSegment;
            SweepLine = sweepLine;
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

            var intersectionA = Type == EventType.Intersection ? this as Point
                : LineIntersection.FindIntersection(SweepLine, line1);

            var intersectionB = thatEvent.Type == EventType.Intersection ? thatEvent as Point
                : LineIntersection.FindIntersection(SweepLine, line2);

            var result = intersectionA.Y.Truncate().CompareTo(intersectionB.Y.Truncate());
            if (result != 0)
            {
                return result;
            }

            //if Y is same use slope as comparison
            var slope1 = getSlope(line1).Truncate();
            var slope2 = getSlope(line2).Truncate();

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

            return Segment.Equals(thatEvent.Segment);
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

            //Left event first, then right and finally intersection.
            return a.Type.CompareTo(b.Type);

        }
    }

    /// <summary>
    ///     Bentley-Ottmann Algorithm
    /// </summary>
    public class SweepLineIntersection
    {
        public static Dictionary<Point, List<Line>> FindIntersections(HashSet<Line> lineSegments)
        {
            var sweepLine = new Line(new Point(0, 0), new Point(0, int.MaxValue));

            var currentEvents = new HashSet<Event>(lineSegments.SelectMany(x => new[] {
                                    new Event(x.Left, EventType.Start, x, sweepLine),
                                    new Event(x.Right, EventType.End, x, sweepLine)
                                }));

            var eventQueue = new BMinHeap<Event>(currentEvents, new EventQueueComparer());

            var currentlyTracked = new BST<Event>();

            var intersectionEvents = new Dictionary<Point, HashSet<Tuple<Event, Event>>>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.ExtractMin();
                currentEvents.Remove(currentEvent);

                switch (currentEvent.Type)
                {
                    case EventType.Start:

                        sweepLine.Left.X = currentEvent.X;
                        sweepLine.Right.X = currentEvent.X;

                        currentlyTracked.Insert(currentEvent);

                        var lower = currentlyTracked.Previous(currentEvent);
                        var upper = currentlyTracked.Next(currentEvent);

                        var lowerIntersection = findIntersection(currentEvent, lower);
                        recordIntersection(intersectionEvents, sweepLine, currentEvent, lower, lowerIntersection);
                        enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, lowerIntersection);

                        var upperIntersection = findIntersection(currentEvent, upper);
                        recordIntersection(intersectionEvents, sweepLine, currentEvent, upper, upperIntersection);
                        enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, upperIntersection);

                        break;

                    case EventType.End:

                        sweepLine.Left.X = currentEvent.X;
                        sweepLine.Right.X = currentEvent.X;

                        lower = currentlyTracked.Previous(currentEvent);
                        upper = currentlyTracked.Next(currentEvent);

                        var upperLowerIntersection = findIntersection(lower, upper);
                        recordIntersection(intersectionEvents, sweepLine, lower, upper, upperLowerIntersection);
                        enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, upperLowerIntersection);

                        if (!currentlyTracked.Delete(currentEvent))
                        {
                            throw new Exception();
                        }

                        break;

                    case EventType.Intersection:

                        var intersectionLines = intersectionEvents[currentEvent as Point];
                        var count = intersectionLines.Count;

                        intersectionLines.RemoveWhere(x =>
                        {
                            return !(currentlyTracked.Delete(x.Item1)
                                     && currentlyTracked.Delete(x.Item2));
                        });

                        if (count != intersectionLines.Count)
                        {
                            throw new Exception();
                        }

                        sweepLine.Left.X = currentEvent.X;
                        sweepLine.Right.X = currentEvent.X;

                        foreach (var intersectionLine in intersectionLines.ToList())
                        {
                            lower = intersectionLine.Item1;
                            upper = intersectionLine.Item2;

                            currentlyTracked.Insert(lower);
                            currentlyTracked.Insert(upper);

                            var tmp = upper;
                            upper = lower;
                            lower = tmp;

                            var lowerLower = currentlyTracked.Previous(lower);
                            var upperUpper = currentlyTracked.Next(upper);

                            var newLowerIntersection = findIntersection(lower, lowerLower);
                            recordIntersection(intersectionEvents, sweepLine, lower, lowerLower, newLowerIntersection);
                            enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, newLowerIntersection);

                            var newUpperIntersection = findIntersection(upper, upperUpper);
                            recordIntersection(intersectionEvents, sweepLine, upper, upperUpper, newUpperIntersection);
                            enqueueIntersectionEvent(eventQueue, currentEvents, currentEvent, sweepLine, newUpperIntersection);
                        }

                        break;

                }

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

            var intersectionEvent = new Event(intersection, EventType.Intersection, null, sweepLine);

            if (sweepLine.Left.X.Truncate() < intersectionEvent.X.Truncate()
                || (sweepLine.Left.X.Truncate() == intersectionEvent.X.Truncate()
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
            Line sweepLine, Event line1, Event line2, Point intersection)
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
