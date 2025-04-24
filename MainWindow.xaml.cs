using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GO
{
    public partial class MainWindow : Window
    {
        private const int BoardSize = 19; 
        private const int CellSize = 25; 
        private const int StoneRadius = 10; 
        private const int BoardMargin = 10; 

        private bool isBlackTurn = true; 
        private Ellipse[,] stones = new Ellipse[BoardSize, BoardSize]; 

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
        
            GoBoard.Children.Clear();

            double boardWidth = (BoardSize - 1) * CellSize + 2 * BoardMargin;
            double boardHeight = (BoardSize - 1) * CellSize + 2 * BoardMargin;

            GoBoard.Width = boardWidth;
            GoBoard.Height = boardHeight;

          
            for (int i = 0; i < BoardSize; i++)
            {
              
                Line horizontalLine = new Line
                {
                    X1 = BoardMargin,
                    Y1 = BoardMargin + i * CellSize,
                    X2 = BoardMargin + (BoardSize - 1) * CellSize,
                    Y2 = BoardMargin + i * CellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                GoBoard.Children.Add(horizontalLine);

                Line verticalLine = new Line
                {
                    X1 = BoardMargin + i * CellSize,
                    Y1 = BoardMargin,
                    X2 = BoardMargin + i * CellSize,
                    Y2 = BoardMargin + (BoardSize - 1) * CellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                GoBoard.Children.Add(verticalLine);
            }

        
            int[] starPoints = { 3, 9, 15 };
            foreach (int x in starPoints)
            {
                foreach (int y in starPoints)
                {
                    Ellipse starPoint = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = Brushes.Black,
                        Margin = new Thickness(
                            BoardMargin + x * CellSize - 2.5,
                            BoardMargin + y * CellSize - 2.5, 0, 0)
                    };
                    GoBoard.Children.Add(starPoint);
                }
            }
        }

        private void GoBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(GoBoard);

           
            int x = (int)System.Math.Round((clickPosition.X - BoardMargin) / CellSize);
            int y = (int)System.Math.Round((clickPosition.Y - BoardMargin) / CellSize);

            
            if (x >= 0 && x < BoardSize && y >= 0 && y < BoardSize)
            {
                
                if (stones[x, y] == null)
                {
                    PlaceStone(x, y);
                }
            }
        }

        private void PlaceStone(int x, int y)
        {
            
            Ellipse stone = new Ellipse
            {
                Width = StoneRadius * 2,
                Height = StoneRadius * 2,
                Fill = isBlackTurn ? Brushes.Black : Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

          
            double left = BoardMargin + x * CellSize - StoneRadius;
            double top = BoardMargin + y * CellSize - StoneRadius;

       
            Canvas.SetLeft(stone, left);
            Canvas.SetTop(stone, top);

            GoBoard.Children.Add(stone);
            stones[x, y] = stone;

            
            isBlackTurn = !isBlackTurn;
            UpdateStatus();
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            
            isBlackTurn = !isBlackTurn;
            UpdateStatus();
            MessageBox.Show(isBlackTurn ? "Белые pass. Ход черных." : "Черные pass. Ход белых.", "Пас");
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    stones[x, y] = null;

            isBlackTurn = true;
            InitializeBoard();
            UpdateStatus();
        }

        private void ResignButton_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBox.Show(isBlackTurn ? "Черные сдались. Победа белых!" : "Белые сдались. Победа черных!", "Сдача");
            NewGameButton_Click(sender, e);
        }

        private void UpdateStatus()
        {
            StatusText.Text = isBlackTurn ? "Черные ходят" : "Белые ходят";
            CurrentPlayerIndicator.Fill = isBlackTurn ? Brushes.Black : Brushes.White;
        }
    }
}