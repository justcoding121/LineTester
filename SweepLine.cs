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

            var intersectionA =  Type == EventType.Intersection ? this as Point 
                : LineIntersection.FindIntersection(SweepLine, line1);

            var intersectionB = thatEvent.Type == EventType.Intersection ? thatEvent as Point 
                : LineIntersection.FindIntersection(SweepLine, line2);

            var result = intersectionA.Y.CompareTo(intersectionB.Y);
            if (result != 0)
            {
                return result;
            }

            //if Y is same use slope as comparison
            var slope1 = getSlope(line1);
            var slope2 = getSlope(line2);

            result = slope1.CompareTo(slope1);
            if (result != 0)
            {
                return result;
            }

            //if slope is the same use diff of X co-ordinate
            result = line1.Left.X.CompareTo(line2.Left.X);
            if (result != 0)
            {
                return result;
            }

            //if diff of X co-ordinate is same use diff of Y co-ordinate
            result = line1.Left.X.CompareTo(line2.Left.X);

            //at this point this is guaranteed to be not same.
            //since we don't let duplicate lines with input HashSet of lines.
            //see line equals override in Line class.
            return result;
        }

        private double getSlope(Line line)
        {
            Point left = line.Left, right = line.Right;

            //vertical line has infinite slope
            if (left.Y == right.Y)
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
            var result = a.X.CompareTo(b.X);

            if (result != 0)
            {
                return result;
            }

            //compare Y
            result = a.Y.CompareTo(b.Y);
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

            var eventQueue = new BMinHeap<Event>(lineSegments.SelectMany(x => new[] {
                                    new Event(x.Left, EventType.Start, x, sweepLine),
                                    new Event(x.Right, EventType.End, x, sweepLine)
                                }), new EventQueueComparer());

            var currentlyTracked = new BST<Event>();

            var intersectionEvents = new Dictionary<Point, HashSet<Event>>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.ExtractMin();

                sweepLine.Left.X = currentEvent.X;
                sweepLine.Right.X = currentEvent.X;

                switch (currentEvent.Type)
                {
                    case EventType.Start:

                        currentlyTracked.Insert(currentEvent);

                        var upperIntersection = findIntersection(currentEvent,
                                currentlyTracked.Previous(currentEvent));
                        reportIntersectionEvent(intersectionEvents, sweepLine, upperIntersection);

                        var lowerIntersection = findIntersection(currentEvent,
                                currentlyTracked.Next(currentEvent));
                        reportIntersectionEvent(intersectionEvents, sweepLine, lowerIntersection);

                        break;

                    case EventType.End:

                        var upperLowerIntersection = findIntersection(currentlyTracked.Previous(currentEvent),
                            currentlyTracked.Next(currentEvent));
                        reportIntersectionEvent(intersectionEvents, sweepLine, upperLowerIntersection);

                        currentlyTracked.Delete(currentEvent);

                        break;

                    case EventType.Intersection:

                        var intersections = intersectionEvents[currentEvent as Point];

                        intersections.RemoveWhere(x => !currentlyTracked.Delete(x));

                        foreach (var intersection in intersections)
                        {
                            currentlyTracked.Insert(intersection);

                            var newUpperIntersection = findIntersection(currentEvent,
                                currentlyTracked.Previous(intersection));
                            reportIntersectionEvent(intersectionEvents, sweepLine, newUpperIntersection);

                            var newLowerIntersection = findIntersection(currentEvent,
                                currentlyTracked.Next(intersection));
                            reportIntersectionEvent(intersectionEvents, sweepLine, newLowerIntersection);
                        }

                        break;

                }

            }

            throw new NotImplementedException();
        }

        private static void reportIntersectionEvent(Dictionary<Point, HashSet<Event>> intersectionEvents, 
            Line sweepLine, Point intersection)
        {
            var existing = intersectionEvents.ContainsKey(intersection) ?
                    intersectionEvents[intersection] : new HashSet<Event>();

            existing.Add(new Event(intersection, EventType.Intersection, null, sweepLine));
        }

        private static Point findIntersection(Event a, Event b)
        {
            return a.Segment.Intersection(b.Segment);
        }
    }

    
}
