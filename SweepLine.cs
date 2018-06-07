using Advanced.Algorithms.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advanced.Algorithms.Geometry
{
    //point type
    internal enum EventType
    {
        LeftEndPoint = 0,
        RightEndPoint = 1,
        IntersectionPoint = 2
    }

    /// <summary>
    ///     A custom object representing start/end/intersection point.
    /// </summary>
    internal class EventPoint : Point, IComparable
    {
        internal EventType EventType;

        //The full line if not an intersection
        internal Line LineSegment;

        //The two separate lines if an intersection
        internal EventPoint LeftUpLineSegment;
        internal EventPoint LeftDownLineSegment;

        /// <param name="p">One end point of the line.</param>
        /// <param name="lineSegment">The full line.</param>
        /// <param name="isLeftEndPoint">Is this point the left end point of line.</param>
        internal EventPoint(Point p, Line lineSegment = null, EventType eventType = EventType.IntersectionPoint)
            : base(p.X, p.Y)
        {
            EventType = eventType;
            LineSegment = lineSegment;
        }

        public int CompareTo(object obj)
        {
            var tgt = obj as EventPoint;
            var result = X.CompareTo(tgt.X);

            if (result != 0)
            {
                return result;
            }

            else
            {
                return X.CompareTo(tgt.Y);
            }
        }
    }

    internal class EventPointNode : IComparable
    {
        internal EventPointNode(EventPoint eventEndPoint)
        {
            EventEndPoint = eventEndPoint;
        }

        internal EventPoint EventEndPoint { get; set; }

        public int CompareTo(object obj)
        {
            return EventEndPoint.Y.CompareTo((obj as EventPointNode).EventEndPoint.Y);
        }
    }

    public class SweepLineIntersection
    {
        public static List<Point> FindIntersections(IEnumerable<Line> lineSegments)
        {
            var result = new List<Point>();

            var lineLeftRightMap = lineSegments
                                    .Select(x =>
                                    {
                                        if (x.Start.X < x.End.X)
                                        {
                                            return new KeyValuePair<EventPoint, EventPoint>(
                                                new EventPoint(x.Start, x, EventType.LeftEndPoint),
                                                new EventPoint(x.End, x, EventType.RightEndPoint)
                                            );
                                        }
                                        else
                                        {
                                            return new KeyValuePair<EventPoint, EventPoint>(
                                                 new EventPoint(x.End, x, EventType.LeftEndPoint),
                                                 new EventPoint(x.Start, x, EventType.RightEndPoint)
                                             );
                                        }

                                    })
                                    .ToDictionary(x => x.Key, x => x.Value);

            var lineRightLeftMap = lineLeftRightMap.ToDictionary(x => x.Value, x => x.Key);

            var eventQueue = new BMinHeap<EventPoint>(lineLeftRightMap.SelectMany(x => new[] { x.Key, x.Value }));

            var currentlyTracked = new BST<EventPointNode>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.ExtractMin();

                if (currentEvent.EventType == EventType.LeftEndPoint)
                {
                    var currentLineSegment = currentEvent.LineSegment;

                    var node = insert(currentlyTracked, currentEvent);

                    var above = getClosestUpperEndPoint(node);
                    var below = getClosestLowerEndPoint(node);

                    if (above != null && below != null)
                    {
                        var lowerUpperIntersection = LineIntersection.FindIntersection(above.LineSegment, below.LineSegment);
                        if (lowerUpperIntersection != null)
                        {
                            eventQueue.Delete(new EventPoint(lowerUpperIntersection));
                        }
                    }

                    if (above != null)
                    {
                        var upperIntersection = LineIntersection.FindIntersection(above.LineSegment, currentLineSegment);
                        if (upperIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(upperIntersection)
                            {
                                LeftUpLineSegment = getLeftTop(above, currentEvent),
                                LeftDownLineSegment = getLeftBottom(above, currentEvent)
                            });
                        }
                    }

                    if (below != null)
                    {
                        var lowerIntersection = LineIntersection.FindIntersection(below.LineSegment, currentLineSegment);
                        if (lowerIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(lowerIntersection)
                            {
                                LeftUpLineSegment = getLeftTop(below, currentEvent),
                                LeftDownLineSegment = getLeftBottom(below, currentEvent)
                            });
                        }
                    }
                }
                else if (currentEvent.EventType == EventType.RightEndPoint)
                {
                    var currentLine = lineRightLeftMap[currentEvent];
                    var node = currentlyTracked.FindNode(new EventPointNode(currentLine));

                    var above = getClosestUpperEndPoint(node);
                    var below = getClosestLowerEndPoint(node);

                    delete(currentlyTracked, currentLine);

                    if (above != null && below != null)
                    {
                        var lowerUpperIntersection = LineIntersection.FindIntersection(above.LineSegment, below.LineSegment);
                        if (lowerUpperIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(lowerUpperIntersection)
                            {
                                LeftUpLineSegment = getLeftTop(above, currentLine),
                                LeftDownLineSegment = getLeftBottom(above, currentLine)
                            });
                        }
                    }

                }
                else
                {
                    result.Add(currentEvent as Point);

                    var segE1 = currentlyTracked.FindNode(new EventPointNode(currentEvent.LeftUpLineSegment));
                    var segE2 = currentlyTracked.FindNode(new EventPointNode(currentEvent.LeftDownLineSegment));

                    //swap
                    var tmp = segE1.Value.EventEndPoint;
                    segE1.Value.EventEndPoint = segE2.Value.EventEndPoint;
                    segE2.Value.EventEndPoint = tmp;

                    var segA = getClosestUpperEndPoint(segE2);
                    var segB = getClosestLowerEndPoint(segE1);

                    if (segA != null)
                    {
                        var segE1AItersection = LineIntersection.FindIntersection(segE1.Value.EventEndPoint.LineSegment, segA.LineSegment);
                        if (segE1AItersection != null)
                        {
                            eventQueue.Delete(new EventPoint(segE1AItersection));
                        }
                    }

                    if (segB != null)
                    {
                        var segE2BIntersection = LineIntersection.FindIntersection(segE2.Value.EventEndPoint.LineSegment, segB.LineSegment);
                        if (segE2BIntersection != null)
                        {
                            eventQueue.Delete(new EventPoint(segE2BIntersection));
                        }
                    }

                    if (segA != null)
                    {
                        var segE2AIntersection = LineIntersection.FindIntersection(segE2.Value.EventEndPoint.LineSegment, segA.LineSegment);
                        if (segE2AIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(segE2AIntersection)
                            {
                                LeftUpLineSegment = getLeftTop(segE2.Value.EventEndPoint, segA),
                                LeftDownLineSegment = getLeftBottom(segE2.Value.EventEndPoint, segA)
                            });
                        }
                    }

                    if (segB != null)
                    {
                        var segE1BIntersection = LineIntersection.FindIntersection(segE1.Value.EventEndPoint.LineSegment, segB.LineSegment);
                        if (segE1BIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(segE1BIntersection)
                            {
                                LeftUpLineSegment = getLeftTop(segE1.Value.EventEndPoint, segB),
                                LeftDownLineSegment = getLeftBottom(segE1.Value.EventEndPoint, segB)
                            });
                        }
                    }
                }
            }

            return result;
        }

        private static EventPoint getLeftBottom(EventPoint lineSegment, EventPoint currentLineSegment)
        {
            var segments = new[] { lineSegment, currentLineSegment };

            var result = segments.OrderBy(x => Math.Min(x.LineSegment.Start.X, x.LineSegment.End.X))
                                           .ThenBy(x => Math.Min(x.LineSegment.Start.Y, x.LineSegment.End.Y))
                                           .First();
            return result;
        }

        private static EventPoint getLeftTop(EventPoint lineSegment, EventPoint currentLineSegment)
        {
            var segments = new[] { lineSegment, currentLineSegment };

            var result = segments.OrderBy(x => Math.Min(x.LineSegment.Start.X, x.LineSegment.End.X))
                                           .ThenByDescending(x => Math.Max(x.LineSegment.Start.Y, x.LineSegment.End.Y))
                                           .First();
            return result;
        }

        private static void delete(BST<EventPointNode> currentlyTracked, EventPoint lineEndPoint)
        {
            var existing = currentlyTracked.FindNode(new EventPointNode(lineEndPoint));
            currentlyTracked.Delete(existing.Value);
        }

        private static BSTNode<EventPointNode> insert(BST<EventPointNode> currentlyTracked, EventPoint lineEndPoint)
        {
            var newNode = new EventPointNode(lineEndPoint);
            var existing = currentlyTracked.FindNode(newNode);
            currentlyTracked.Insert(newNode);

            return currentlyTracked.FindNode(newNode);
        }

        private static EventPoint getClosestLowerEndPoint(BSTNode<EventPointNode> node)
        {
            return getNextLower(node)?.Value.EventEndPoint;
        }

        private static BSTNode<EventPointNode> getNextLower(BSTNode<EventPointNode> node)
        {
            //root or left child
            if (node.Parent == null || node.IsLeftChild)
            {
                if (node.Left != null)
                {
                    node = node.Left;

                    while (node.Right != null)
                    {
                        node = node.Right;
                    }

                    return node;
                }
                else
                {
                    while (node.Parent != null && node.IsLeftChild)
                    {
                        node = node.Parent;
                    }

                    return node?.Parent;
                }
            }
            //right child
            else
            {
                if (node.Left != null)
                {
                    node = node.Left;

                    while (node.Right != null)
                    {
                        node = node.Right;
                    }

                    return node;
                }
                else
                {
                    return node.Parent;
                }
            }

        }

        private static EventPoint getClosestUpperEndPoint(BSTNode<EventPointNode> node)
        {
            return getNextUpper(node)?.Value.EventEndPoint;
        }

        private static BSTNode<EventPointNode> getNextUpper(BSTNode<EventPointNode> node)
        {
            //root or left child
            if (node.Parent == null || node.IsLeftChild)
            {
                if (node.Right != null)
                {
                    node = node.Right;

                    while (node.Left != null)
                    {
                        node = node.Left;
                    }

                    return node;
                }
                else
                {
                    return node?.Parent;
                }
            }
            //right child
            else
            {
                if (node.Right != null)
                {
                    node = node.Right;

                    while (node.Left != null)
                    {
                        node = node.Left;
                    }

                    return node;
                }
                else
                {
                    while (node.Parent != null && node.IsRightChild)
                    {
                        node = node.Parent;
                    }

                    return node?.Parent;
                }
            }
        }
    }
}
