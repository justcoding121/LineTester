using Advanced.Algorithms.DataStructures;
using Advanced.Algorithms.Tests.DataStructures;
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
            if(obj == this)
            {
                return 0;
            }

            var tgt = obj as EventPoint;
            var result = X.CompareTo(tgt.X);

            if (result != 0)
            {
                return result;
            }

            else
            {
                return Y.CompareTo(tgt.Y);
            }
        }
    }

    internal class EventPointNode : IComparable
    {
        internal double Y;
        private readonly int precision;
        internal EventPointNode(EventPoint eventEndPoint, double y, int precision = 3)
        {
            this.precision = precision;
            EventEndPoint = eventEndPoint;
            Y = y;
        }

        internal EventPoint EventEndPoint { get; set; }

        public int CompareTo(object obj)
        {
            if (this == obj)
            {
                return 0;
            }

            var tgt = obj as EventPointNode;

            var result = Y.CompareTo(tgt.Y);

            if (result != 0)
            {
                return result;
            }

            double x1, y1, x2, y2;
            var line1 = EventEndPoint.LineSegment;
            if (line1.Start.X < line1.End.X)
            {
                x1 = line1.Start.X;
                x2 = line1.End.X;
                y1 = line1.Start.Y;
                y2 = line1.End.Y;
            }
            else
            {
                x1 = line1.End.X;
                x2 = line1.Start.X;
                y1 = line1.End.Y;
                y2 = line1.Start.Y;
            }

            double x3, y3, x4, y4;
            var line2 = tgt.EventEndPoint.LineSegment;
            if (line2.Start.X < line2.End.X)
            {
                x3 = line2.Start.X;
                x4 = line2.End.X;
                y3 = line2.Start.Y;
                y4 = line2.End.Y;
            }
            else
            {
                x3 = line2.End.X;
                x4 = line2.Start.X;
                y3 = line2.End.Y;
                y4 = line2.Start.Y;
            }

            var tolerance = Math.Round(Math.Pow(0.1, precision), precision);

            //equations of the form x=c (two vertical lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
            {
                throw new Exception("Both lines overlap vertically, ambiguous intersection points.");
            }

            //equations of the form y=c (two horizontal lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
            {
                throw new Exception("Both lines overlap horizontally, ambiguous intersection points.");
            }

            //equations of the form x=c (two vertical lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
            {
                return 0;
            }

            //equations of the form y=c (two horizontal lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
            {
                return 0;
            }

            //lineA is vertical x1 = x2
            //slope will be infinity
            //so lets derive another solution
            if (Math.Abs(x1 - x2) < tolerance)
            {
                return 1;
            }
            //lineB is vertical x3 = x4
            //slope will be infinity
            //so lets derive another solution
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                return -1;
            }
            //lineA & lineB are not vertical 
            //(could be horizontal we can handle it with slope = 0)
            else
            {
                //compute slope of line 1 (m1) and c2
                double m1 = (y2 - y1) / (x2 - x1);

                //compute slope of line 2 (m2) and c2
                double m2 = (y4 - y3) / (x4 - x3);

                return m1 > m2 ? 1 : m1 == m2 ? 0 : -1;
            }
        }
    }

    /// <summary>
    ///     Bentley-Ottmann Algorithm
    /// </summary>
    public class SweepLineIntersection
    {
        public static List<Point> FindIntersections(IEnumerable<Line> lineSegments)
        {
            var result = new HashSet<Point>();

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

                                    }).ToDictionary(x => x.Key, x => x.Value);

            var lineRightLeftMap = lineLeftRightMap.ToDictionary(x => x.Value, x => x.Key);

            var eventQueue = new BMinHeap<EventPoint>(lineLeftRightMap.SelectMany(x => new[] { x.Key, x.Value }));

            var currentlyTracked = new BST<EventPointNode>();

            var nodeMapping = new Dictionary<EventPoint, BSTNode<EventPointNode>>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.PeekMin();

                if (currentEvent.EventType == EventType.LeftEndPoint)
                {
                    var segE = currentEvent.LineSegment;

                    var node = insert(currentlyTracked, currentEvent);

                    nodeMapping.Add(currentEvent, node);
                    nodeMapping.Add(lineLeftRightMap[currentEvent], node);

                    var segA = getClosestUpperEndPoint(node);
                    var segB = getClosestLowerEndPoint(node);

                    if (segA != null)
                    {
                        var upperIntersection = LineIntersection.FindIntersection(segA.LineSegment, segE);
                        if (upperIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(upperIntersection)
                            {
                                LeftUpLineSegment = getTop(segA, currentEvent),
                                LeftDownLineSegment = getBottom(segA, currentEvent)
                            });
                        }
                    }

                    if (segB != null)
                    {
                        var lowerIntersection = LineIntersection.FindIntersection(segB.LineSegment, segE);
                        if (lowerIntersection != null)
                        {
                            eventQueue.Insert(new EventPoint(lowerIntersection)
                            {
                                LeftUpLineSegment = getTop(segB, currentEvent),
                                LeftDownLineSegment = getBottom(segB, currentEvent)
                            });
                        }
                    }
                }
                else if (currentEvent.EventType == EventType.RightEndPoint)
                {
                    var node = nodeMapping[currentEvent];

                    var segA = getClosestUpperEndPoint(node);
                    var segB = getClosestLowerEndPoint(node);

                    verifyBST(currentlyTracked);

                    delete(currentlyTracked, node.Value);

                    verifyBST(currentlyTracked);

                    nodeMapping.Remove(lineRightLeftMap[currentEvent]);
                    nodeMapping.Remove(currentEvent);

                    if (segA != null && segB != null
                        && segA !=segB)
                    {
                        var lowerUpperIntersection = LineIntersection.FindIntersection(segA.LineSegment, segB.LineSegment);
                        if (lowerUpperIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(lowerUpperIntersection))
                                && !result.Contains(lowerUpperIntersection))
                            {
                                eventQueue.Insert(new EventPoint(lowerUpperIntersection)
                                {
                                    LeftUpLineSegment = getTop(segA, segB),
                                    LeftDownLineSegment = getBottom(segA, segB)
                                });
                            }
                        }
                    }

                }
                else
                {
                    result.Add(currentEvent as Point);

                    var up = currentEvent.LeftUpLineSegment;
                    var down = currentEvent.LeftDownLineSegment;

                    var segUp = nodeMapping[up];
                    currentlyTracked.Delete(segUp.Value);

                    var segDown = nodeMapping[down];
                    currentlyTracked.Delete(segDown.Value);

                    verifyBST(currentlyTracked);

                    segUp = insert(currentlyTracked, new EventPointNode(down, currentEvent.Y));
                    nodeMapping[down] = segUp;
                    nodeMapping[lineLeftRightMap[down]] = segUp;

                    segDown = insert(currentlyTracked, new EventPointNode(up, currentEvent.Y));
                    nodeMapping[up] = segDown;
                    nodeMapping[lineLeftRightMap[up]] = segDown;

                    verifyBST(currentlyTracked);

                    var segUpUp = getClosestUpperEndPoint(segUp);
                    var segDownDown = getClosestLowerEndPoint(segDown);

                    if (segUpUp != null && segUp.Value.EventEndPoint != segUpUp)
                    {
                        var segUpIntersection = LineIntersection.FindIntersection(segUp.Value.EventEndPoint.LineSegment, segUpUp.LineSegment);
                        if (segUpIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(segUpIntersection))
                                && !result.Contains(segUpIntersection))
                            {
                                eventQueue.Insert(new EventPoint(segUpIntersection)
                                {
                                    LeftUpLineSegment = getTop(segUp.Value.EventEndPoint, segUpUp),
                                    LeftDownLineSegment = getBottom(segUp.Value.EventEndPoint, segUpUp)
                                });
                            }
                        }
                    }

                    if (segDownDown != null && (segDown.Value.EventEndPoint != segDownDown))
                    {
                        var segDownIntersection = LineIntersection.FindIntersection(segDown.Value.EventEndPoint.LineSegment, segDownDown.LineSegment);
                        if (segDownIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(segDownIntersection))
                                && !result.Contains(segDownIntersection))
                            {
                                eventQueue.Insert(new EventPoint(segDownIntersection)
                                {
                                    LeftUpLineSegment = getTop(segDown.Value.EventEndPoint, segDownDown),
                                    LeftDownLineSegment = getBottom(segDown.Value.EventEndPoint, segDownDown)
                                });
                            }
                        }
                    }
                }

                eventQueue.ExtractMin();
            }

            return result.ToList();
        }

        private static void verifyBST(BST<EventPointNode> currentlyTracked)
        {
            if (!BinarySearchTreeTester<EventPointNode>
                        .VerifyIsBinarySearchTree(currentlyTracked.Root,
                        new EventPointNode(new EventPoint(new Point(double.MinValue, double.MinValue)), double.MinValue),
                        new EventPointNode(new EventPoint(new Point(double.MaxValue, double.MaxValue)), double.MaxValue)))
            {
                throw new Exception();
            }
        }

        private static EventPoint getBottom(EventPoint lineSegment, EventPoint currentLineSegment)
        {
            var segments = new[] { lineSegment, currentLineSegment };

            if (segments.Any(x => x.EventType != EventType.LeftEndPoint))
            {
                throw new Exception();
            }

            var result = segments.OrderBy(x => x.Y).First();
            return result;
        }

        private static EventPoint getTop(EventPoint lineSegment, EventPoint currentLineSegment)
        {
            var segments = new[] { lineSegment, currentLineSegment };

            if (segments.Any(x => x.EventType != EventType.LeftEndPoint))
            {
                throw new Exception();
            }

            var result = segments.OrderByDescending(x => x.Y).First();
            return result;
        }

        private static void delete(BST<EventPointNode> currentlyTracked, EventPointNode node)
        {
            currentlyTracked.Delete(node);
        }

        private static BSTNode<EventPointNode> insert(BST<EventPointNode> currentlyTracked, EventPoint lineEndPoint)
        {
            var newNode = new EventPointNode(lineEndPoint, lineEndPoint.Y);
            return currentlyTracked.InsertAndReturnNewNode(newNode);
        }

        private static BSTNode<EventPointNode> insert(BST<EventPointNode> currentlyTracked, EventPointNode lineEndPointNode)
        {
            return currentlyTracked.InsertAndReturnNewNode(lineEndPointNode);
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
