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
            if (obj == this)
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

            var nodeMapping = new Dictionary<EventPoint, EventPointNode>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.ExtractMin();

                if (currentEvent.EventType == EventType.LeftEndPoint)
                {
                    var segE = currentEvent.LineSegment;

                    var leftNode = insert(currentlyTracked, currentEvent);

                    nodeMapping.Add(currentEvent, leftNode);

                    var segments = new List<EventPoint>();

                    segments.Add(getClosestUpperEndPoint(currentlyTracked, leftNode, true, segments));
                    segments.Add(getClosestUpperEndPoint(currentlyTracked, leftNode, false, segments));
                    segments.Add(getClosestLowerEndPoint(currentlyTracked, leftNode, true, segments));
                    segments.Add(getClosestLowerEndPoint(currentlyTracked, leftNode, false, segments));

                    var rightNode = insert(currentlyTracked, lineLeftRightMap[currentEvent]);
                    nodeMapping.Add(lineLeftRightMap[currentEvent], rightNode);

                    segments.Add(getClosestUpperEndPoint(currentlyTracked, rightNode, true, segments));
                    segments.Add(getClosestUpperEndPoint(currentlyTracked, rightNode, false, segments));
                    segments.Add(getClosestLowerEndPoint(currentlyTracked, rightNode, true, segments));
                    segments.Add(getClosestLowerEndPoint(currentlyTracked, rightNode, false, segments));

                    foreach (var segA in segments.Where(x => x != null && x.LineSegment != segE))
                    {
                        var upperIntersection = LineIntersection.FindIntersection(segA.LineSegment, segE);
                        if (upperIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(upperIntersection))
                                && !result.Contains(upperIntersection))
                            {
                                eventQueue.Insert(new EventPoint(upperIntersection)
                                {
                                    LeftUpLineSegment = getTop(segA, currentEvent, lineRightLeftMap),
                                    LeftDownLineSegment = getBottom(segA, currentEvent, lineRightLeftMap)
                                });
                            }
                        }
                    }


                }
                else if (currentEvent.EventType == EventType.RightEndPoint)
                {
                    var segE = currentEvent.LineSegment;

                    verifyBST(currentlyTracked);

                    var left = lineRightLeftMap[currentEvent];
                    var leftNode = nodeMapping[left];
                    delete(currentlyTracked, leftNode);
                    nodeMapping.Remove(left);

                    var rightNode = nodeMapping[currentEvent];

                    var segmentsA = new List<EventPoint>();
                    var segmentsB = new List<EventPoint>();

                    segmentsA.Add(getClosestUpperEndPoint(currentlyTracked, rightNode, true, segmentsA));
                    segmentsA.Add(getClosestUpperEndPoint(currentlyTracked, rightNode, false, segmentsA));
                    segmentsA.Add(getClosestLowerEndPoint(currentlyTracked, rightNode, true, segmentsA));
                    segmentsA.Add(getClosestLowerEndPoint(currentlyTracked, rightNode, false, segmentsA));


                    segmentsB.Add(getClosestLowerEndPoint(currentlyTracked, rightNode, true, segmentsA));
                    segmentsB.Add(getClosestLowerEndPoint(currentlyTracked, rightNode, false, segmentsA.Concat(segmentsB).ToList()));
                    segmentsB.Add(getClosestUpperEndPoint(currentlyTracked, rightNode, true, segmentsA.Concat(segmentsB).ToList()));
                    segmentsB.Add(getClosestUpperEndPoint(currentlyTracked, rightNode, false, segmentsA.Concat(segmentsB).ToList()));

                    delete(currentlyTracked, rightNode);
                    nodeMapping.Remove(currentEvent);

                    verifyBST(currentlyTracked);
                    foreach (var segA in segmentsA)
                        foreach (var segB in segmentsB)
                        {
                            if (segA != null && segB != null
                                && segA.LineSegment != segB.LineSegment)
                            {
                                var lowerUpperIntersection = LineIntersection.FindIntersection(segA.LineSegment, segB.LineSegment);
                                if (lowerUpperIntersection != null)
                                {
                                    if (!eventQueue.Exists(new EventPoint(lowerUpperIntersection))
                                        && !result.Contains(lowerUpperIntersection))
                                    {
                                        eventQueue.Insert(new EventPoint(lowerUpperIntersection)
                                        {
                                            LeftUpLineSegment = getTop(segA, segB, lineRightLeftMap),
                                            LeftDownLineSegment = getBottom(segA, segB, lineRightLeftMap)
                                        });
                                    }
                                }
                            }
                        }

                }
                else
                {
                    result.Add(currentEvent as Point);

                    var up = currentEvent.LeftUpLineSegment.EventType == EventType.LeftEndPoint ?
                        currentEvent.LeftUpLineSegment : lineRightLeftMap[currentEvent.LeftUpLineSegment];

                    var down = currentEvent.LeftDownLineSegment.EventType == EventType.LeftEndPoint ?
                        currentEvent.LeftDownLineSegment : lineRightLeftMap[currentEvent.LeftDownLineSegment];

                    var segUp = nodeMapping[up];
                    currentlyTracked.Delete(segUp);

                    var segDown = nodeMapping[down];
                    currentlyTracked.Delete(segDown);

                    verifyBST(currentlyTracked);

                    segUp = insert(currentlyTracked, new EventPointNode(up, currentEvent.Y));
                    nodeMapping[up] = segUp;

                    segDown = insert(currentlyTracked, new EventPointNode(down, currentEvent.Y));
                    nodeMapping[down] = segDown;

                    verifyBST(currentlyTracked);

                    var segUpUps = new List<EventPoint>(new[] { segUp.EventEndPoint, segDown.EventEndPoint });

                    segUpUps.Add(getClosestUpperEndPoint(currentlyTracked, segDown, true, segUpUps));
                    segUpUps.Add(getClosestUpperEndPoint(currentlyTracked, segDown, false, segUpUps));
                    segUpUps.Add(getClosestLowerEndPoint(currentlyTracked, segDown, true, segUpUps));
                    segUpUps.Add(getClosestLowerEndPoint(currentlyTracked, segDown, false, segUpUps));


                    var segDownDowns = new List<EventPoint>(segUpUps);

                    segDownDowns.Add(getClosestLowerEndPoint(currentlyTracked, segUp, true, segUpUps));
                    segDownDowns.Add(getClosestLowerEndPoint(currentlyTracked, segUp, false, segUpUps.Concat(segDownDowns).ToList()));
                    segDownDowns.Add(getClosestUpperEndPoint(currentlyTracked, segUp, true, segUpUps.Concat(segDownDowns).ToList()));
                    segDownDowns.Add(getClosestUpperEndPoint(currentlyTracked, segUp, false, segUpUps.Concat(segDownDowns).ToList()));


                    foreach (var segDownDown in segDownDowns)
                    {
                        if (segDownDown != null && (segUp.EventEndPoint != segDownDown))
                        {
                            var segDownIntersection = LineIntersection.FindIntersection(segUp.EventEndPoint.LineSegment, segDownDown.LineSegment);
                            if (segDownIntersection != null)
                            {
                                if (!eventQueue.Exists(new EventPoint(segDownIntersection))
                                    && !result.Contains(segDownIntersection))
                                {
                                    eventQueue.Insert(new EventPoint(segDownIntersection)
                                    {
                                        LeftUpLineSegment = getTop(segUp.EventEndPoint, segDownDown, lineRightLeftMap),
                                        LeftDownLineSegment = getBottom(segUp.EventEndPoint, segDownDown, lineRightLeftMap)
                                    });
                                }
                            }
                        }

                        var segUpUp = segDownDown;
                        if (segUpUp != null && segDown.EventEndPoint != segUpUp)
                        {
                            var segUpIntersection = LineIntersection.FindIntersection(segDown.EventEndPoint.LineSegment, segUpUp.LineSegment);
                            if (segUpIntersection != null)
                            {
                                if (!eventQueue.Exists(new EventPoint(segUpIntersection))
                                    && !result.Contains(segUpIntersection))
                                {
                                    eventQueue.Insert(new EventPoint(segUpIntersection)
                                    {
                                        LeftUpLineSegment = getTop(segDown.EventEndPoint, segUpUp, lineRightLeftMap),
                                        LeftDownLineSegment = getBottom(segDown.EventEndPoint, segUpUp, lineRightLeftMap)
                                    });
                                }
                            }
                        }
                    }


                    currentlyTracked.Delete(segUp);
                    currentlyTracked.Delete(segDown);

                    verifyBST(currentlyTracked);

                    segUp = insert(currentlyTracked, new EventPointNode(up, up.Y));
                    nodeMapping[up] = segUp;

                    segDown = insert(currentlyTracked, new EventPointNode(down, down.Y));
                    nodeMapping[down] = segDown;

                }


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

        private static EventPoint getBottom(EventPoint lineSegment, EventPoint currentLineSegment,
            Dictionary<EventPoint, EventPoint> rightLeft)
        {
            var segments = new[] { lineSegment, currentLineSegment };

            //if (segments.Any(x => x.EventType != EventType.LeftEndPoint))
            //{
            //    throw new Exception();
            //}

            var result = segments.OrderBy(x => x.EventType == EventType.LeftEndPoint ? x : rightLeft[x]).First();
            return result;
        }

        private static EventPoint getTop(EventPoint lineSegment, EventPoint currentLineSegment,
            Dictionary<EventPoint, EventPoint> rightLeft)
        {
            var segments = new[] { lineSegment, currentLineSegment };

            //if (segments.Any(x => x.EventType != EventType.LeftEndPoint))
            //{
            //    throw new Exception();
            //}

            var result = segments.OrderByDescending(x => x.EventType == EventType.LeftEndPoint ? x : rightLeft[x]).First();
            return result;
        }

        private static void delete(BST<EventPointNode> currentlyTracked, EventPointNode node)
        {
            currentlyTracked.Delete(node);
        }

        private static EventPointNode insert(BST<EventPointNode> currentlyTracked, EventPoint lineEndPoint)
        {
            var newNode = new EventPointNode(lineEndPoint, lineEndPoint.Y);
            currentlyTracked.Insert(newNode);
            return newNode;
        }

        private static EventPointNode insert(BST<EventPointNode> currentlyTracked, EventPointNode lineEndPointNode)
        {
            currentlyTracked.Insert(lineEndPointNode);
            return lineEndPointNode;
        }

        private static EventPoint getClosestLowerEndPoint(BST<EventPointNode> currentlyTracked, EventPointNode node, bool left,
            List<EventPoint> existing)
        {
            var bstNode = currentlyTracked.FindNode(node);
            bstNode = getNextLower(bstNode);

            while (bstNode != null
                && (bstNode.Value.EventEndPoint.LineSegment == node.EventEndPoint.LineSegment
                || (left ? bstNode.Value.EventEndPoint.X >= node.EventEndPoint.X :
                    bstNode.Value.EventEndPoint.X < node.EventEndPoint.X)
                || existing.Any(x => x?.LineSegment == bstNode.Value.EventEndPoint.LineSegment)))
            {
                bstNode = getNextLower(bstNode);
            }

            return bstNode?.Value.EventEndPoint;
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

        private static EventPoint getClosestUpperEndPoint(BST<EventPointNode> currentlyTracked,
            EventPointNode node, bool left, List<EventPoint> existing)
        {
            var bstNode = currentlyTracked.FindNode(node);
            bstNode = getNextUpper(bstNode);

            while (bstNode != null
                && (bstNode.Value.EventEndPoint.LineSegment == node.EventEndPoint.LineSegment
                || (left ? bstNode.Value.EventEndPoint.X >= node.EventEndPoint.X :
                    bstNode.Value.EventEndPoint.X < node.EventEndPoint.X)
                || existing.Any(x => x?.LineSegment == bstNode.Value.EventEndPoint.LineSegment)))
            {
                bstNode = getNextUpper(bstNode);
            }

            return bstNode?.Value.EventEndPoint;
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
