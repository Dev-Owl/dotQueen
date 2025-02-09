namespace DotQueens.Core;

public class Board
{
    private readonly Cell[,] grid;

    public Cell[,] Grid
    {
        get
        {
            return grid;
        }
    }
    private readonly int size;

    public int Size
    {
        get
        {
            return size;
        }
    }

    public bool DisplayEachStep { get; set; } = false;

    public string[] availableColors = ["red", "blue", "green", "yellow", "purple", "orange", "darkRed", "darkCyan", "gray", "white"];
    public Dictionary<string, ConsoleColor> colorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "red", ConsoleColor.Red},
        { "blue", ConsoleColor.Blue },
        { "green", ConsoleColor.Green },
        { "yellow", ConsoleColor.Yellow },
        { "purple", ConsoleColor.Magenta },
        { "orange", ConsoleColor.DarkYellow },
        { "darkRed", ConsoleColor.DarkRed },
        { "darkCyan", ConsoleColor.DarkCyan },
        { "gray", ConsoleColor.Gray },
        { "cyan", ConsoleColor.Cyan },
        { "white", ConsoleColor.White }
    };

    // Constructor that creates a square board with unique row colors.
    // Each row is filled with a uniform color drawn from a predefined list.
    public Board(int size)
    {
        // In Board constructor:
        if (size <= 2)
            throw new ArgumentException("Board size must be positive and at least 2", nameof(size));
        if (size > availableColors.Length)
            throw new ArgumentException("Not enough unique colors for the board size.");

        this.size = size;
        grid = new Cell[size, size];
        if (size > availableColors.Length)
            throw new ArgumentException("Not enough unique colors for the board size.");

        // Shuffle available colors and pick exactly 'size' colors.
        Random rnd = new Random();
        List<string> colors = availableColors.OrderBy(x => rnd.Next()).ToList();
        // Take the required number of colors based on the grid size
        List<string> selectedColors = colors.GetRange(0, size);

        // Partition the board into 'size' contiguous groups.
        // Each cell will eventually be assigned a group id (0 to size-1).
        int[,] groups = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                groups[i, j] = -1;
            }
        }

        // Choose 'size' random unique seed cells.
        List<(int row, int col)> freeCells = new List<(int, int)>();
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                freeCells.Add((i, j));


        for (int groupId = 0; groupId < size; groupId++)
        {
            int index = rnd.Next(freeCells.Count);
            var (r, c) = freeCells[index];
            freeCells.RemoveAt(index);
            groups[r, c] = groupId;
        }
        BuildBoardFromConfigAndPrint(selectedColors.ToArray(), groups, "After group seeding...");

        // Multi-seed flood fill to assign remaining cells.
        // Use 4-directional connectivity.
        Queue<(int row, int col, int groupId)> queue = new Queue<(int, int, int)>();
        // Enqueue all seed positions.
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (groups[i, j] != -1)
                    queue.Enqueue((i, j, groups[i, j]));
            }
        }

        int[,] directions = new int[,] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
        // All group spawn cells are enqueued. Now flood fill.
        while (queue.Count > 0)
        {
            var (r, c, groupId) = queue.Dequeue();
            // Randomize neighbor order.
            List<(int dr, int dc)> dirList = new List<(int, int)>();
            for (int d = 0; d < directions.GetLength(0); d++)
                dirList.Add((directions[d, 0], directions[d, 1]));

            dirList.OrderBy(x => rnd.Next());

            foreach (var (dr, dc) in dirList)
            {
                int nr = r + dr, nc = c + dc;
                // Ensure the neighbor is within bounds and unassigned.
                if (nr >= 0 && nr < size && nc >= 0 && nc < size && groups[nr, nc] == -1)
                {
                    groups[nr, nc] = groupId;
                    // Continue flood fill from this neighbor.
                    queue.Enqueue((nr, nc, groupId));
                }

            }
            BuildBoardFromConfigAndPrint(selectedColors.ToArray(), groups, "Flood fill in progress...");
        }

        // Now assign each Cell its color based on its group assignment.
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                grid[i, j] = new Cell(selectedColors[groups[i, j]]);
            }
        }
    }

    private void BuildBoardFromConfigAndPrint(string[] selectedColors, int[,] groups, string? preMessage = null)
    {
        if (DisplayEachStep == false)
        {
            return;
        }

        if (preMessage != null)
        {
            Console.WriteLine(preMessage);
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var currentGroup = groups[i, j];
                var selectedColorForCell = "white";
                if (currentGroup != -1)
                {
                    selectedColorForCell = selectedColors[currentGroup];
                }

                grid[i, j] = new Cell(selectedColorForCell);
            }
        }
        PrintBoard();
        Console.ReadLine();
    }

    // Places a queen at given cell.
    public void PlaceQueen(int i, int j)
    {
        grid[i, j].HasQueen = true;
    }

    public enum ValidationType
    {
        RowsAndColumns,
        Adjacency,
        Colors,
        None
    }

    public bool Validate()
    {
        return ValidateDetail() == ValidationType.None;
    }

    // Validate ensures:
    // - One queen per row and column.
    // - No two queens are directly adjacent (including diagonals).
    // - Each queen’s color (unique per row) appears exactly once.
    public ValidationType ValidateDetail()
    {
        // Check rows and columns for one queen each.
        for (int i = 0; i < size; i++)
        {
            int queenCountRow = 0;
            int queenCountCol = 0;
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j].HasQueen)
                    queenCountRow++;
                if (grid[j, i].HasQueen)
                    queenCountCol++;
            }
            if (queenCountRow > 1 || queenCountCol > 1)
                return ValidationType.RowsAndColumns;
        }

        // Check that queens are not adjacent.
        int[,] directions = new int[,] {
            { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 },
            { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 }
        };

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j].HasQueen)
                {
                    for (int d = 0; d < directions.GetLength(0); d++)
                    {
                        int ni = i + directions[d, 0], nj = j + directions[d, 1];
                        if (ni >= 0 && ni < size && nj >= 0 && nj < size && grid[ni, nj].HasQueen)
                            return ValidationType.Adjacency;
                    }
                }
            }
        }

        // Check that each color is covered exactly once.
        HashSet<string> colorsUsed = new HashSet<string>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j].HasQueen)
                    colorsUsed.Add(grid[i, j].Color);
            }
        }
        // Since board rows have unique colors, we expect size queen colors.
        return colorsUsed.Count == size ? ValidationType.None : ValidationType.Colors;
    }

    // IsLevelSolvable attempts to place queens on the board.
    // It uses backtracking and places one queen per row.
    // Goal is to check if the level is solvable.
    public bool IsLevelSolvable()
    {
        int[] solution = new int[size]; // solution[row] holds the chosen column index for the queen in that row.
        bool[] usedColumns = new bool[size];
        HashSet<string> usedColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool solved = SolveLevel(0, solution, usedColumns, usedColors);
        if (solved)
        {
            // Clear any previous queen placements.
            ResetQueens();
            // Place queens according to the solution.
            for (int i = 0; i < size; i++)
            {
                PlaceQueen(i, solution[i]);
            }
        }
        return solved;
    }

    /// <summary>
    /// Recursive backtracking method to solve the level.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="solution">Array indicating what column has queen by a given row</param>
    /// <param name="usedColumns">Array of already used columns from a previous row</param>
    /// <param name="usedColors">Array to track what color has been used for queens</param>
    /// <returns></returns>
    private bool SolveLevel(int row, int[] solution, bool[] usedColumns, HashSet<string> usedColors)
    {
        if (row == size)
            return true;

        for (int col = 0; col < size; col++)
        {
            // Ensure this column isn't already chosen. If so skip.
            if (usedColumns[col])
                continue;

            // Check the adjacent rule with the previous row (only immediate neighbors can violate adjacency).
            if (row > 0)
            {
                int prevCol = solution[row - 1];
                if (Math.Abs(prevCol - col) <= 1)
                    continue;
            }

            // Ensure that this cell's color group hasn't been used yet.
            string cellColor = grid[row, col].Color;
            if (usedColors.Contains(cellColor))
                continue;

            // All conditions met; choose this cell.
            solution[row] = col;
            usedColumns[col] = true;
            usedColors.Add(cellColor);

            if (SolveLevel(row + 1, solution, usedColumns, usedColors))
                return true;

            // Backtrack.
            usedColumns[col] = false;
            usedColors.Remove(cellColor);
        }
        return false;
    }

    // Method to reset any queen placements.
    public void ResetQueens()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                grid[i, j].HasQueen = false;
            }
        }
    }

    // Utility method to print the board (for debugging/visualization)
    public void PrintBoard()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var cell = grid[i, j];
                // Set text color based on cell's color.
                if (colorMap.TryGetValue(cell.Color, out ConsoleColor consoleColor))
                    Console.ForegroundColor = consoleColor;
                else
                    Console.ForegroundColor = ConsoleColor.White;

                // Print "Q" for a queen, "." for empty cells.
                Console.Write(cell.HasQueen ? "Q " : ". ");
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.ResetColor();
    }
}