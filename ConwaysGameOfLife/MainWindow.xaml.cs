using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using ConwaysGameOfLife.Model;
using ConwaysGameOfLife.Logic;

namespace ConwaysGameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string _fileNameOfStateGeneration = "StateCurrentGeneration.xml";
        static readonly int _strokeThickness = 5;
        static readonly int _delay = 300;

        long _offsetCanvasX;
        long _offsetCanvasY;
        double _deltaX;
        double _deltaY;
        Point _basePoint;
        Point _currentMousePoint;

        Game _game;
        bool _isStop;
        bool _isSolve;
        HashSet<Cell> _currentGeneration;

        public MainWindow()
        {
            InitializeComponent();

            _offsetCanvasX = 0;
            _offsetCanvasY = 0;
            _deltaX = 0;
            _deltaY = 0;
            _game = new Game();
            _isStop = false;
            _isSolve = false;
            _currentGeneration = new HashSet<Cell>();
        }

        /**
         * Launch Conway's game of life
         * @param generations
         * @param minimumDelay
         */
        async void LaunchGame(IEnumerable<IEnumerable<Cell>> generations, long minimumDelay)
        {
            long step = 0;
            var it = generations.GetEnumerator();

            Func<IEnumerable<Cell>> getItem = () =>
            {
                if (it.MoveNext())
                    return it.Current;
                return null;
            };

            while (!_isStop)
            {
                var numberTask = Task.Factory.StartNew(getItem);

                await Task.WhenAll(numberTask, Task.Delay(TimeSpan.FromMilliseconds(minimumDelay)));

                if (_isStop)
                    break;

                var generation = numberTask.Result;
                if (generation == null)
                {
                    canvasGrid.Children.Clear();
                    _currentGeneration.Clear();
                    StopGame();
                    break;
                }
                DrawPointOnCanvas(generation, step++);
            }
            it.Dispose();
        }

        /**
         * Stop conway's game of life 
         */
        void StopGame()
        {
            if (_game != null)
            {
                _isStop = true;
                _isSolve = false;
                buttonStart.IsEnabled = true;
                buttonClear.IsEnabled = true;
                buttonRead.IsEnabled = true;
            }
        }

        /**
         * Rendering live points
         * @param points
         * @param step
         */
        void DrawPointOnCanvas(IEnumerable<Cell> generation, long step = 0)
        {
            labelStep.Content = step.ToString();
            canvasGrid.Children.Clear();
            if (generation != null)
            {
                var pointsEllips = CreateEllipses(generation);
                foreach (var point in pointsEllips)
                    canvasGrid.Children.Add(point);
            }
        }

        /**
         * Create ellipse based on points
         * @param points
         * @return list ellipses
         */
        IEnumerable<Ellipse> CreateEllipses(IEnumerable<Cell> points)
        {
            return from point in points where CheckOutputBoard(point.X - _offsetCanvasX, point.Y - _offsetCanvasY)
                select CreateEllipse(point.X - _offsetCanvasX, point.Y - _offsetCanvasY);
        }

        /**
         * Create ellipse based on the coordinates
         * @param x
         * @param y
         * @return ellips
         */
        Ellipse CreateEllipse(long x, long y)
        {
            var ellips = new Ellipse();
            ellips.Fill = Brushes.Red;
            ellips.ToolTip = x.ToString() + " " + y.ToString();
            ellips.Stroke = Brushes.Blue;
            ellips.StrokeThickness = _strokeThickness;

            var left = GetScaleCoordinate(x);
            var top = GetScaleCoordinate(y);
            ellips.Margin = new Thickness(left, top, 0, 0);
            return ellips;
        }

        /**
         * Preparation coordinates scaled by multiplying the thickness of the ellipses
         * @param coordinate
         * @return scaled coordinate
         */
        double GetScaleCoordinate(double coordinate)
        {
            return Math.Floor(coordinate * _strokeThickness);
        }

        /**
         * Preparation coordinates unscaled by dividing by the thickness of the ellipse
         * @param coordinate
         * @return unscaled coordinate
         */
        double GetUnscaleCoordinate(double coordinate)
        {
            return Math.Floor(coordinate / _strokeThickness);
        }

        /**
         * Check the output coordinate abroad Canvas
         * @param x
         * @param y
         * @return if included then true
         */
        bool CheckOutputBoard(long x, long y)
        {
            var borderX = GetUnscaleCoordinate(canvasGrid.Width);
            var borderY = GetUnscaleCoordinate(canvasGrid.Height);
            return (x < borderX) && (y < borderY);
        }

        /**
         * creation event point by pressing the cell
         * @param sender
         * @param e
         */
        private void CanvasGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            _currentMousePoint = e.GetPosition(canvasGrid);
            canvasGrid.CaptureMouse();
            if (_currentGeneration != null && !_isSolve)
            {
                var x = GetUnscaleCoordinate(_currentMousePoint.X);
                var y = GetUnscaleCoordinate(_currentMousePoint.Y);
                var convertX = System.Convert.ToInt64(x);
                var convertY = System.Convert.ToInt64(y);
                var ellips = CreateEllipse(convertX, convertY);
                canvasGrid.Children.Add(ellips);
                _currentGeneration.Add(new Cell { X = convertX + _offsetCanvasX, Y = convertY + _offsetCanvasY });
            }
        }

        /**
         * 
         * @param sender
         * @param e
         */
        private void canvasGridMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _isSolve)
            {
                var moveMousePoint = e.GetPosition(canvasGrid);
                if (_currentMousePoint != null)
                {
                    var offsetX = GetUnscaleCoordinate(_currentMousePoint.X - _basePoint.X - moveMousePoint.X);
                    var offsetY = GetUnscaleCoordinate(_currentMousePoint.Y - _basePoint.Y - moveMousePoint.Y);
                    if (offsetX != 0 && offsetY != 0)
                    {
                        _offsetCanvasX += System.Convert.ToInt64(offsetX);
                        _offsetCanvasY += System.Convert.ToInt64(offsetY);
                        _currentMousePoint = moveMousePoint;
                        labelX.Content = _offsetCanvasX.ToString();
                        labelY.Content = _offsetCanvasY.ToString();
                    }
                }
            }
        }

        private void CanvasGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            canvasGrid.ReleaseMouseCapture();
            _basePoint.X += _deltaX;
            _basePoint.Y += _deltaY;
            _deltaX = 0.0;
            _deltaY = 0.0;
        }

        /**
         * Start the game
         * @param sender
         * @param e
         */
        private void ButtonStartClick(object sender, RoutedEventArgs e)
        {
            if (_currentGeneration != null && _game != null)
            {
                buttonStart.IsEnabled = false;
                buttonClear.IsEnabled = false;
                buttonRead.IsEnabled = false;
                _isSolve = true;
                _isStop = false;
                var generations = _game.Start(_currentGeneration);
                LaunchGame(generations, _delay);
            }
        }

        /**
         * Clear generation
         * @param sender
         * @param e
         */
        private void ButtonClearClick(object sender, RoutedEventArgs e)
        {
            canvasGrid.Children.Clear();
            if (_currentGeneration != null)
            {
                _currentGeneration.Clear();
            }
        }

        /**
         * Stop the game
         * @param sender
         * @param e
         */
        private void ButtonStopClick(object sender, RoutedEventArgs e)
        {
            StopGame();
        }

        /**
         * Save state of game to file
         * @param sender
         * @param e
         */
        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            if (_currentGeneration != null)
            {
                // Write to the Isolated Storage
                var xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                using (var isolatedStoragefile = IsolatedStorageFile.GetUserStoreForAssembly())
                {
                    using (var stream = isolatedStoragefile.OpenFile(_fileNameOfStateGeneration, FileMode.Create))
                    {
                        /* FIXME необходимо добавить структуру и ее сериализовать*/
                        var serializer = new XmlSerializer(typeof(HashSet<Cell>));
                        using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                        {
                            serializer.Serialize(xmlWriter, _currentGeneration);
                            MessageBox.Show("Saved state!");
                        }
                    }
                }
            }   
        }

        /**
         * Read state of game from file
         * @param sender
         * @param e
         */
        private void ButtonReadClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var IsolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
                {
                    using (var file = IsolatedStorage.OpenFile(_fileNameOfStateGeneration, FileMode.Open))
                    {
                        var serializer = new XmlSerializer(typeof(HashSet<Cell>));
                        _currentGeneration = (HashSet<Cell>)serializer.Deserialize(file);
                        DrawPointOnCanvas(_currentGeneration);
                    }
                }
            }
            catch
            {
                //add some code here
            }
        }
    }
}
