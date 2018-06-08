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

        internal EventPointNode(EventPoint eventEndPoint, double y)
        {
            EventEndPoint = eventEndPoint;
            Y = y;
        }

        internal EventPoint EventEndPoint { get; set; }

        public int CompareTo(object obj)
        {
            return Y.CompareTo((obj as EventPointNode).Y);
        }
    }

    public class SweepLineIntersection
    {
        public static List<Point> FindIntersections(IEnumerable<Line> lineSegments)
        {
            var result = new List<Point>();

            var lineRightLeftMap = lineSegments
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

                                    }).ToDictionary(x => x.Value, x => x.Key);


            var eventQueue = new BMinHeap<EventPoint>(lineRightLeftMap.SelectMany(x => new[] { x.Key, x.Value }));

            var currentlyTracked = new BST<EventPointNode>();

            while (eventQueue.Count > 0)
            {
                var currentEvent = eventQueue.PeekMin();

                if (currentEvent.EventType == EventType.LeftEndPoint)
                {
                    var segE = currentEvent.LineSegment;

                    var node = insert(currentlyTracked, currentEvent);

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
                    var segE = lineRightLeftMap[currentEvent];
                    var node = currentlyTracked.FindNode(new EventPointNode(segE, segE.Y));

                    var segA = getClosestUpperEndPoint(node);
                    var segB = getClosestLowerEndPoint(node);

                    delete(currentlyTracked, segE);

                    if (segA != null && segB != null)
                    {
                        var lowerUpperIntersection = LineIntersection.FindIntersection(segA.LineSegment, segB.LineSegment);
                        if (lowerUpperIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(lowerUpperIntersection))
                                && !result.Contains(lowerUpperIntersection))
                            {
                                eventQueue.Insert(new EventPoint(lowerUpperIntersection)
                                {
                                    LeftUpLineSegment = getTop(segA, segE),
                                    LeftDownLineSegment = getBottom(segA, segE)
                                });
                            }
                        }
                    }

                }
                else
                {
                    result.Add(currentEvent as Point);

                    var segE1 = currentlyTracked.FindNode(new EventPointNode(currentEvent.LeftUpLineSegment, currentEvent.LeftUpLineSegment.Y));
                    currentlyTracked.Delete(segE1.Value);

                    var segE2 = currentlyTracked.FindNode(new EventPointNode(currentEvent.LeftDownLineSegment, currentEvent.LeftDownLineSegment.Y));
                    currentlyTracked.Delete(segE2.Value);

                    verifyBST(currentlyTracked);

                    var up = currentEvent.LeftUpLineSegment;
                    var down = currentEvent.LeftDownLineSegment;

                    segE1 = currentlyTracked.InsertAndReturnNewNode(new EventPointNode(down, up.Y));
                    segE2 = currentlyTracked.InsertAndReturnNewNode(new EventPointNode(up, down.Y));

                    verifyBST(currentlyTracked);

                    var segA = getClosestLowerEndPoint(segE1);
                    var segB = getClosestUpperEndPoint(segE2);

                    if (segA != null && segE2.Value.EventEndPoint != segA)
                    {
                        var segE2AIntersection = LineIntersection.FindIntersection(segE2.Value.EventEndPoint.LineSegment, segA.LineSegment);
                        if (segE2AIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(segE2AIntersection))
                                && !result.Contains(segE2AIntersection))
                            {
                                eventQueue.Insert(new EventPoint(segE2AIntersection)
                                {
                                    LeftUpLineSegment = getTop(segE2.Value.EventEndPoint, segA),
                                    LeftDownLineSegment = getBottom(segE2.Value.EventEndPoint, segA)
                                });
                            }
                        }
                    }

                    if (segB != null && (segE1.Value.EventEndPoint != segB))
                    {
                        var segE1BIntersection = LineIntersection.FindIntersection(segE1.Value.EventEndPoint.LineSegment, segB.LineSegment);
                        if (segE1BIntersection != null)
                        {
                            if (!eventQueue.Exists(new EventPoint(segE1BIntersection))
                                &&  !result.Contains(segE1BIntersection))
                            {
                                eventQueue.Insert(new EventPoint(segE1BIntersection)
                                {
                                    LeftUpLineSegment = getTop(segE1.Value.EventEndPoint, segB),
                                    LeftDownLineSegment = getBottom(segE1.Value.EventEndPoint, segB)
                                });
                            }
                        }
                    }
                }

                eventQueue.ExtractMin();
            }

            return result;
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

        private static void delete(BST<EventPointNode> currentlyTracked, EventPoint lineEndPoint)
        {
            var existing = currentlyTracked.FindNode(new EventPointNode(lineEndPoint, lineEndPoint.Y));
            currentlyTracked.Delete(existing.Value);
        }

        private static BSTNode<EventPointNode> insert(BST<EventPointNode> currentlyTracked, EventPoint lineEndPoint)
        {
            var newNode = new EventPointNode(lineEndPoint, lineEndPoint.Y);
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
