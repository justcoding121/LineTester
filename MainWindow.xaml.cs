using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Advanced.Algorithms.Geometry;
using System.Diagnostics;

namespace LineTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            generate(false);
        }

        private static List<Advanced.Algorithms.Geometry.Line> lines = null;

        private void redo()
        {
            generate(true);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            generate(false);
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            redo();
        }

        List<Advanced.Algorithms.Geometry.Point> expectedIntersections
            = new List<Advanced.Algorithms.Geometry.Point>();

        List<Advanced.Algorithms.Geometry.Point> actualIntersections
            = new List<Advanced.Algorithms.Geometry.Point>();

        private static int nodeCount = 100;

        private void generate(bool redo)
        {
            var sweepLine = new BentleyOttmann();

            if (!redo)
            {
                lines = new List<Advanced.Algorithms.Geometry.Line>();
                expectedIntersections.Clear();
                actualIntersections.Clear();

                while (expectedIntersections.Count == actualIntersections.Count)
                {
                    lines = getRandomLines(nodeCount);

                    //while (lines.Any(x => x.Left.X == x.Right.X || x.Left.Y == x.Right.Y))
                    //{
                    //    lines = getRandomLines(nodeCount);
                    //}
                    var watch = new Stopwatch();
                    watch.Start();
                    expectedIntersections = getExpectedIntersections(lines);
                    watch.Stop();

                    var orgElapsed = watch.ElapsedMilliseconds;
                    watch.Reset();
                    long actualElapsed;

                    var orgCalls = LineIntersection.calls;
                    try
                    {
                        watch.Start();
                        var actual = sweepLine.FindIntersections(lines);
                        watch.Start();

                        actualElapsed = watch.ElapsedMilliseconds;
                        actualIntersections = actual.Select(x => x.Key).ToList();
                    }
                    catch
                    {
                       break;
                    }
                    var calls = LineIntersection.calls - orgCalls;
                    LineIntersection.calls = 0;
                }
            }
            else
            {
                try
                {
                    expectedIntersections = getExpectedIntersections(lines);
                  
                   var ss = new HashSet<Advanced.Algorithms.Geometry.Line>(lines);
                    actualIntersections = sweepLine.FindIntersections(ss)
                        .Select(x => x.Key).ToList();
                }
                catch { }

            }


            display(lines, expectedIntersections, actualIntersections);

        }

        private void display(List<Advanced.Algorithms.Geometry.Line> lines,
            List<Advanced.Algorithms.Geometry.Point> expectedIntersections,
            List<Advanced.Algorithms.Geometry.Point> actualIntersections)
        {
            myGrid.Children.Clear();

            var canvas = new Canvas();

            foreach (var line in lines)
            {
                // Add a Line Element
                var myLine = new System.Windows.Shapes.Line();
                myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                myLine.X1 = line.Left.X;
                myLine.X2 = line.Right.X;
                myLine.Y1 = line.Left.Y;
                myLine.Y2 = line.Right.Y;

                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 2;
                canvas.Children.Add(myLine);

                setPoint(canvas, line.Left, true);
                setPoint(canvas, line.Right, true);
            }

            expectedIntersections
              .RemoveAll(x => actualIntersections.Any(y => y.Equals(x)));

            foreach (var point in expectedIntersections)
            {
                setPoint(canvas, point, false);
            }

            foreach (var point in actualIntersections)
            {
                setPoint(canvas, point, true);
            }

            myGrid.Children.Add(canvas);
        }

        private void setPoint(Canvas canvas, Advanced.Algorithms.Geometry.Point point, bool green)
        {
            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();


            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color =
                green ? (Color)ColorConverter.ConvertFromString("Green")
                : (Color)ColorConverter.ConvertFromString("Red");

            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Transparent;

            myEllipse.Width = green ? 10 : 15;
            myEllipse.Height = green ? 10 : 15;

            double left = point.X - (myEllipse.Width / 2);
            double top = point.Y - (myEllipse.Height / 2);

            canvas.Children.Add(myEllipse);

            Canvas.SetLeft(myEllipse, left);
            Canvas.SetTop(myEllipse, top);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = point.ToString(); ;
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
            Canvas.SetLeft(textBlock, left + 10);
            Canvas.SetTop(textBlock, top + 10);
            canvas.Children.Add(textBlock);
        }

        private static Random random = new Random();

        private static List<Advanced.Algorithms.Geometry.Point> getExpectedIntersections(List<Advanced.Algorithms.Geometry.Line> lines)
        {
            var result = new List<Advanced.Algorithms.Geometry.Point>();

            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    var intersection = Advanced.Algorithms.Geometry.LineIntersection.FindIntersection(lines[i], lines[j]);

                    if (intersection != null)
                    {
                        result.Add(intersection);
                    }
                }
            }

            return result.Distinct().ToList();
        }

        private static List<Advanced.Algorithms.Geometry.Line> getRandomLines(int lineCount)
        {
            var lines = new List<Advanced.Algorithms.Geometry.Line>();

            var s1 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(0, 200), new Advanced.Algorithms.Geometry.Point(100, 300));
            var s2 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(20, 350), new Advanced.Algorithms.Geometry.Point(110, 150));
            var s3 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(30, 250), new Advanced.Algorithms.Geometry.Point(80, 120));
            var s4 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(50, 100), new Advanced.Algorithms.Geometry.Point(120, 300));

            lines.AddRange(new[] { s1, s2, s3, s4 });

            lines.AddRange(verticalLines());
            lines.AddRange(horizontalLines());

            while (lineCount > 0)
            {
                lines.Add(getRandomLine());
                lineCount--;
            }

            return lines;
        }

        private static List<Advanced.Algorithms.Geometry.Line> verticalLines()
        {
            var lines = new List<Advanced.Algorithms.Geometry.Line>();

            var s1 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(100, 200), new Advanced.Algorithms.Geometry.Point(100, 600));
            var s2 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(100, 225), new Advanced.Algorithms.Geometry.Point(100, 625));
            var s3 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(100, 250), new Advanced.Algorithms.Geometry.Point(100, 475));
            var s4 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(100, 290), new Advanced.Algorithms.Geometry.Point(100, 675));

            lines.AddRange(new[] { s1, s2, s3, s4 });

            return lines;
        }

        private static List<Advanced.Algorithms.Geometry.Line> horizontalLines()
        {
            var lines = new List<Advanced.Algorithms.Geometry.Line>();

            var s1 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(200, 100), new Advanced.Algorithms.Geometry.Point(600, 100));
            var s2 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(225, 100), new Advanced.Algorithms.Geometry.Point(625, 100));
            var s3 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(250, 100), new Advanced.Algorithms.Geometry.Point(475, 100));
            var s4 = new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(290, 100), new Advanced.Algorithms.Geometry.Point(675, 100));

            lines.AddRange(new[] { s1, s2, s3, s4 });

            return lines;
        }

        private static Advanced.Algorithms.Geometry.Line getRandomLine()
        {
            return new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(random.Next(0, 1000) * random.NextDouble(), random.Next(0, 1000) * random.NextDouble()),
                new Advanced.Algorithms.Geometry.Point(random.Next(0, 1000) * random.NextDouble(), random.Next(0, 1000) * random.NextDouble()));
        }


    }
}
