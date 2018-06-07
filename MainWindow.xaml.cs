using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
            generate(getRandomLines(3));
        }

        private static List<Advanced.Algorithms.Geometry.Line> lastLines = null;

        private void redo()
        {
            if(lastLines == null)
            {
                generate(getRandomLines(3));
            }
            else
            {
                generate(lastLines);
            }
           
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            generate(getRandomLines(3));
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            redo();
        }

        private void generate(List<Advanced.Algorithms.Geometry.Line> lines)
        {
            List<Advanced.Algorithms.Geometry.Point> expectedIntersections = new List<Advanced.Algorithms.Geometry.Point>();
            List<Advanced.Algorithms.Geometry.Point> actualIntersections = new List<Advanced.Algorithms.Geometry.Point>();

            while (expectedIntersections.Count == actualIntersections.Count)
            {
                lines = getRandomLines(3);
                expectedIntersections = getExpectedIntersections(lines);

                actualIntersections = Advanced.Algorithms.Geometry
                .SweepLineIntersection.FindIntersections(lines);
            }

            lastLines = lines;
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
                var myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                myLine.X1 = line.Start.X;
                myLine.X2 = line.End.X;
                myLine.Y1 = line.Start.Y;
                myLine.Y2 = line.End.Y;

                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 2;
                canvas.Children.Add(myLine);
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

        private void setPoint(Canvas canvas, Advanced.Algorithms.Geometry.Point point, bool actual)
        {
            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();


            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color =
                actual ? (Color)ColorConverter.ConvertFromString("Green")
                : (Color)ColorConverter.ConvertFromString("Red");

            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Transparent;

            myEllipse.Width = actual ? 10 : 15;
            myEllipse.Height = actual ? 10 : 15;

            double left = point.X - (myEllipse.Width / 2);
            double top = point.Y - (myEllipse.Height / 2);

            canvas.Children.Add(myEllipse);

            Canvas.SetLeft(myEllipse, left);
            Canvas.SetTop(myEllipse, top);
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

            return result;
        }

        private static List<Advanced.Algorithms.Geometry.Line> getRandomLines(int lineCount)
        {
            var lines = new List<Advanced.Algorithms.Geometry.Line>();

            while (lineCount > 0)
            {
                lines.Add(getRandomLine());
                lineCount--;
            }

            return lines;
        }
        private static Advanced.Algorithms.Geometry.Line getRandomLine()
        {
            return new Advanced.Algorithms.Geometry.Line(new Advanced.Algorithms.Geometry.Point(random.Next(0, 1000) * random.NextDouble(), random.Next(0, 1000) * random.NextDouble()),
                new Advanced.Algorithms.Geometry.Point(random.Next(0, 1000) * random.NextDouble(), random.Next(0, 1000) * random.NextDouble()));
        }

       
    }
}
