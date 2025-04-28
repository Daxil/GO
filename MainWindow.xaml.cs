using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GO
{
    public partial class MainWindow : Window
    {
        public const int BoardSize = 19;
        private const int CellSize = 25;
        private const int StoneRadius = 10;
        private const int BoardMargin = 10;

        private bool isBlackTurn = true;
        private Ellipse[,] stones = new Ellipse[BoardSize, BoardSize];
        private bool gameEnded = false;
        private List<BoardState> boardHistory = new List<BoardState>();
        private Point? lastMove = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            UpdateStatus();
        }

        private void InitializeBoard()
        {
            GoBoard.Children.Clear();
            double boardWidth = (BoardSize - 1) * CellSize + 2 * BoardMargin;
            double boardHeight = (BoardSize - 1) * CellSize + 2 * BoardMargin;
            GoBoard.Width = boardWidth;
            GoBoard.Height = boardHeight;

            // Рисуем сетку
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

            // Рисуем звездные точки
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
            if (gameEnded) return;

            Point clickPosition = e.GetPosition(GoBoard);
            int x = (int)Math.Round((clickPosition.X - BoardMargin) / CellSize);
            int y = (int)Math.Round((clickPosition.Y - BoardMargin) / CellSize);

            if (x >= 0 && x < BoardSize && y >= 0 && y < BoardSize && stones[x, y] == null)
            {
                // Сохраняем состояние до хода
                var boardBeforeMove = GetCurrentBoardState();

                if (IsMoveValid(x, y))
                {
                    // Ставим камень временно
                    PlaceStone(x, y);

                    // Проверяем захваты
                    int capturedStones = CheckCapturedStones(x, y);

                    // Получаем состояние после захватов
                    var boardAfterMove = GetCurrentBoardState();

                    // Проверяем правило ко - сравниваем с предыдущим состоянием
                    if (boardHistory.Count > 0 && boardAfterMove.Equals(boardHistory.Last()))
                    {
                        // Нарушение ко - отменяем ход
                        UndoMove(boardBeforeMove);
                        MessageBox.Show("Нарушение правила ко - нельзя повторять позицию", "Недопустимый ход");
                        return;
                    }

                    // Ход допустим - сохраняем в историю
                    SaveBoardState(boardAfterMove);

                    // Меняем ход, если это не самоубийство
                    if (capturedStones > 0 || GroupHasLiberties(x, y))
                    {
                        isBlackTurn = !isBlackTurn;
                        UpdateStatus();
                    }

                    // Проверяем есть ли допустимые ходы у противника
                    if (!HasValidMoves(!isBlackTurn))
                    {
                        EndGame("Нет допустимых ходов");
                    }
                }
                else
                {
                    MessageBox.Show("Недопустимый ход - самоубийство или нарушение ко", "Недопустимый ход");
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
        }

        private int CheckCapturedStones(int x, int y)
        {
            int capturedCount = 0;
            Brush oppositeColor = isBlackTurn ? Brushes.White : Brushes.Black;

            foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize &&
                    stones[nx, ny]?.Fill == oppositeColor &&
                    !GroupHasLiberties(nx, ny))
                {
                    capturedCount += RemoveGroup(nx, ny);
                }
            }
            return capturedCount;
        }

        private bool GroupHasLiberties(int x, int y)
        {
            if (stones[x, y] == null) return false;
            var color = stones[x, y].Fill;
            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                if (visited.Contains((cx, cy))) continue;
                visited.Add((cx, cy));

                foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize)
                    {
                        if (stones[nx, ny] == null) return true;
                        if (stones[nx, ny].Fill == color && !visited.Contains((nx, ny)))
                        {
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }
            return false;
        }

        private int RemoveGroup(int x, int y)
        {
            if (stones[x, y] == null) return 0;

            int removedCount = 0;
            var color = stones[x, y].Fill;
            var queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                if (cx < 0 || cx >= BoardSize || cy < 0 || cy >= BoardSize) continue;
                if (stones[cx, cy] == null || stones[cx, cy].Fill != color) continue;

                GoBoard.Children.Remove(stones[cx, cy]);
                stones[cx, cy] = null;
                removedCount++;

                foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    queue.Enqueue((cx + dx, cy + dy));
                }
            }
            return removedCount;
        }

        private bool IsMoveValid(int x, int y)
        {
            // Временная установка камня
            stones[x, y] = new Ellipse { Fill = isBlackTurn ? Brushes.Black : Brushes.White };

            // Проверка есть ли у камня свободы
            bool hasLiberties = GroupHasLiberties(x, y);

            // Проверка захватывает ли ход камни противника
            bool capturesAny = CheckCapturesAfterMove(x, y);

            // Удаляем временный камень
            stones[x, y] = null;

            return hasLiberties || capturesAny;
        }

        private bool CheckCapturesAfterMove(int x, int y)
        {
            Brush oppositeColor = isBlackTurn ? Brushes.White : Brushes.Black;

            foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize &&
                    stones[nx, ny]?.Fill == oppositeColor &&
                    !GroupHasLiberties(nx, ny))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasValidMoves(bool forBlack)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (stones[x, y] == null)
                    {
                        stones[x, y] = new Ellipse { Fill = forBlack ? Brushes.Black : Brushes.White };

                        bool hasLiberties = GroupHasLiberties(x, y);
                        bool capturesAny = CheckCapturesAfterMove(x, y);

                        stones[x, y] = null;

                        if (hasLiberties || capturesAny)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void SaveBoardState(BoardState state)
        {
            boardHistory.Add(state);
            if (boardHistory.Count > 4)
            {
                boardHistory.RemoveAt(0);
            }
        }

        private BoardState GetCurrentBoardState()
        {
            var state = new BoardState();
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (stones[x, y] != null)
                    {
                        state[x, y] = stones[x, y].Fill == Brushes.Black ? StoneColor.Black : StoneColor.White;
                    }
                }
            }
            return state;
        }

        private void UndoMove(BoardState boardState)
        {
            // Очищаем текущие камни
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (stones[x, y] != null)
                    {
                        GoBoard.Children.Remove(stones[x, y]);
                        stones[x, y] = null;
                    }
                }
            }

            // Восстанавливаем камни из сохраненного состояния
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (boardState[x, y] != StoneColor.None)
                    {
                        PlaceStone(x, y, boardState[x, y] == StoneColor.Black);
                    }
                }
            }
        }

        private void PlaceStone(int x, int y, bool isBlack)
        {
            Ellipse stone = new Ellipse
            {
                Width = StoneRadius * 2,
                Height = StoneRadius * 2,
                Fill = isBlack ? Brushes.Black : Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            double left = BoardMargin + x * CellSize - StoneRadius;
            double top = BoardMargin + y * CellSize - StoneRadius;
            Canvas.SetLeft(stone, left);
            Canvas.SetTop(stone, top);

            GoBoard.Children.Add(stone);
            stones[x, y] = stone;
        }

        private void EndGame(string reason)
        {
            gameEnded = true;
            var (blackScore, whiteScore) = CalculateScores();
            MessageBox.Show($"{reason}\nФинальный счет:\nЧерные: {blackScore}\nБелые: {whiteScore}\n\n" +
                          $"{(blackScore > whiteScore ? "Победа черных!" : whiteScore > blackScore ? "Победа белых!" : "Ничья!")}",
                          "Игра окончена");
            UpdateStatus();
        }

        private (double black, double white) CalculateScores()
        {
            double blackScore = 0, whiteScore = 0;
            var visited = new bool[BoardSize, BoardSize];

            // Считаем камни
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (stones[x, y] != null)
                    {
                        if (stones[x, y].Fill == Brushes.Black) blackScore++;
                        else whiteScore++;
                    }
                }
            }

            // Считаем территорию
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (stones[x, y] == null && !visited[x, y])
                    {
                        var territory = FindTerritory(x, y, visited);
                        if (territory.owner == StoneColor.Black) blackScore += territory.size;
                        else if (territory.owner == StoneColor.White) whiteScore += territory.size;
                    }
                }
            }

            // Коми для белых
            whiteScore += 6.5;

            return (blackScore, whiteScore);
        }

        private (StoneColor owner, int size) FindTerritory(int x, int y, bool[,] visited)
        {
            var queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));
            visited[x, y] = true;
            var borders = new HashSet<StoneColor>();
            int size = 0;

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                size++;

                foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (nx < 0 || nx >= BoardSize || ny < 0 || ny >= BoardSize) continue;

                    if (stones[nx, ny] != null)
                    {
                        borders.Add(stones[nx, ny].Fill == Brushes.Black ? StoneColor.Black : StoneColor.White);
                    }
                    else if (!visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            return borders.Count == 1 ? (borders.First(), size) : (StoneColor.None, 0);
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameEnded) return;

            if (boardHistory.Count > 0 && boardHistory.Last().IsPass)
            {
                EndGame("Оба игрока пасуют подряд");
                return;
            }

            isBlackTurn = !isBlackTurn;
            var currentBoard = GetCurrentBoardState();
            currentBoard.IsPass = true;
            SaveBoardState(currentBoard);
            UpdateStatus();
            MessageBox.Show(isBlackTurn ? "Белые пасуют. Ход черных." : "Черные пасуют. Ход белых.", "Пас");
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            gameEnded = false;
            isBlackTurn = true;
            boardHistory.Clear();
            lastMove = null;

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    stones[x, y] = null;
                }
            }

            InitializeBoard();
            UpdateStatus();
        }

        private void ResignButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите сдаться?", "Сдача", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MessageBox.Show(isBlackTurn ? "Черные сдаются. Победа белых!" : "Белые сдаются. Победа черных!", "Сдача");
                NewGameButton_Click(sender, e);
            }
        }

        private void UpdateStatus()
        {
            StatusText.Text = gameEnded ? "Игра окончена" : isBlackTurn ? "Ход черных" : "Ход белых";
            CurrentPlayerIndicator.Fill = isBlackTurn ? Brushes.Black : Brushes.White;
        }
    }

    public enum StoneColor { None, Black, White }

    public class BoardState
    {
        public StoneColor[,] Stones { get; } = new StoneColor[GO.MainWindow.BoardSize, GO.MainWindow.BoardSize];
        public bool IsPass { get; set; } = false;

        public StoneColor this[int x, int y]
        {
            get => Stones[x, y];
            set => Stones[x, y] = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is not BoardState other) return false;

            for (int x = 0; x < GO.MainWindow.BoardSize; x++)
            {
                for (int y = 0; y < GO.MainWindow.BoardSize; y++)
                {
                    if (Stones[x, y] != other.Stones[x, y])
                        return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int x = 0; x < GO.MainWindow.BoardSize; x++)
            {
                for (int y = 0; y < GO.MainWindow.BoardSize; y++)
                {
                    hash = hash * 31 + (int)Stones[x, y];
                }
            }
            return hash;
        }
    }
}