using System;
using Random = System.Random;
using UnityEngine;
using UnityEngine.UI;

public class SudokuGrid : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    private readonly int[,] puzzleGrid = new int[9, 9];
    private readonly int[,] gameGrid = new int[9, 9];
    private readonly int[,] solutionGrid = new int[9, 9];

    private void Awake()
    {
        MakeNewPuzzle();
    }

    private void MakeNewPuzzle()
    {
        var rng = new Random((int)DateTime.Now.Ticks);
        int[] deckOfNine = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        FillWithRandomSolution(solutionGrid, rng, deckOfNine);
        Array.Copy(solutionGrid, puzzleGrid, solutionGrid.Length);
        RemoveRandomClues(puzzleGrid, rng);
        Array.Copy(puzzleGrid, gameGrid, puzzleGrid.Length);
    }

    public bool IsSolved()
    {
        for (int row = 0; row< 9; row++)
        {
            for (int col = 0; col< 9; col++)
            {
                if (!IsValid(row,col)) return false;
            }
        }
        return true;
    }

    static bool SolvePuzzle(int[,] grid)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = grid[row, col];
                if (value != 0) continue; // given as a clue, or already solved

                if (SolveSlot(grid, row, col) == 0)
                {
                    return false; // cannot solve for this position
                }

                // Attempt to solve the rest of the puzzle
                if (SolvePuzzle(grid)) // RECURSION!
                {
                    return true;
                }

                return false; // Backtrack
            }
        }
        return true; // Solved
    }

    static int SolveSlot(int[,] grid, int row, int col)
    {
        for (int guess = 1; guess <= 9; guess++)
        {
            if (IsPossible(grid, row, col, guess))
            {
                grid[row, col] = guess;
                return guess;
            }
        }
        return 0; // no number will fit this slot
    }

    public int GetGameValue(int row, int col) => gameGrid[row, col];
    public void SetGameValue(int row, int col, int value) => gameGrid[row, col] = value;
    public int GetPuzzleValue(int row, int col) => puzzleGrid[row, col];
    public int GetSolutionValue(int row, int col) => solutionGrid[row, col];

    static bool IsPossible(int[,] grid, int row, int col, int value)
    {
        Debug.Assert(value >= 1 && value <= 9);
        if (RowHasValue(grid, row, value)) return false;
        if (ColumnHasValue(grid, col, value)) return false;
        if (BlockHasValue(grid, row / 3, col / 3, value)) return false;
        return true;
    }
    public bool IsPossible(int row, int col, int value) => IsPossible(gameGrid, row, col, value);
    static bool IsValid(int[,] grid, int row, int col)
    {
        int value = grid[row, col];
        if (value == 0) return false;
        grid[row, col] = 0;
        bool isValid = IsPossible(grid, row, col, value);
        grid[row, col] = value;
        return isValid;
    }
    public bool IsValid(int row, int col) => IsValid(gameGrid, row, col);

    static bool RowHasValue(int[,] grid, int row, int value)
    {
        for (int col = 0; col < 9; col++)
        {
            if (grid[row, col] == value) return true;
        }
        return false;
    }
    public bool RowHasValue(int row, int value) => RowHasValue(gameGrid, row, value);

    static bool ColumnHasValue(int[,] grid, int col, int value)
    {
        for (int row = 0; row < 9; row++)
        {
            if (grid[row, col] == value) return true;
        }
        return false;
    }
    public bool ColumnHasValue(int col, int value) => ColumnHasValue(gameGrid, col, value);

    static bool BlockHasValue(int[,] grid, int blockRow, int blockCol, int value)
    {
        int rowStart = blockRow * 3;
        int colStart = blockCol * 3;
        for (int row = rowStart; row < rowStart + 3; row++)
        {
            for (int col = colStart; col < colStart + 3; col++)
            {
                if (grid[row, col] == value) return true;
            }
        }
        return false;
    }
    public bool BlockHasValue(int blockRow, int blockCol, int value) => BlockHasValue(gameGrid, blockRow, blockCol, value);

    static int[,] MakeSudokuPuzzle()
    {
        int[,] grid = new int[9, 9];
        int[] deckOfNine = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var rng = new Random((int)DateTime.Now.Ticks);
        FillWithRandomSolution(grid, rng, deckOfNine);
        RemoveRandomClues(grid, rng);
        return grid;
    }

    static bool FillWithRandomSolution(int[,] grid, Random rng, int[] deckOfNine)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = grid[row, col];
                Debug.Assert(value >= 0 && value <= 9);
                if (value != 0) continue; // slot is already filled
                Shuffle(deckOfNine, rng);
                for (int n = 0; n < 9; n++)
                {
                    int guess = deckOfNine[n];
                    if (!IsPossible(grid, row, col, guess)) continue;
                    grid[row, col] = guess;
                    if (FillWithRandomSolution(grid, rng, deckOfNine)) // RECURSIVE!
                    {
                        return true; // Success!
                    }
                    grid[row, col] = 0; // remove the guess
                }
                return false; // unsolvable as-is, so backtrack
            }
        }
        return true; // no blank slots
    }

    static void RemoveRandomClues(int[,] grid, Random rng)
    {
        // Create a list of all possible slots (0..80)
        int[] slots = MakeSequence(0, 81);
        int remainingSlots = slots.Length;
        int remainingClues = grid.Length;
        Debug.Assert(remainingClues == remainingSlots);
        int[,] tempGrid = new int[9, 9]; // used for checking solutions
        const int MIN_CLUES = 17; // smallest number of clues in a solvable Sudoku puzzle
        while (remainingClues > MIN_CLUES && remainingSlots > 0)
        {
            // Select a random slot that hasn't been tried yet
            int slot = TakeRandomItem(slots, rng, ref remainingSlots);
            int row = slot / 9;
            int col = slot % 9;
            int value = grid[row, col];
            Debug.Assert(value >= 1 && value <= 9);

            // Remove the clue in this slot and attempt to solve the puzzle
            grid[row, col] = 0;
            Array.Copy(grid, tempGrid, grid.Length);
            if (SolvePuzzle(tempGrid))
            {
                // Still solvable, so keep this slot blank and keep going
                remainingClues--;
            }
            else
            {
                // Removing this clue would make the puzzle unsolvable
                // So, replace the clue and try elsewhere
                grid[row, col] = value;
            }
        }
    }

    static void Shuffle(int[] items, Random rng)
    {
        // Fisher-Yates shuffle algorithm
        for (int i = 0; i < items.Length - 1; i++)
        {
            int r = rng.Next(i, items.Length);
            (items[r], items[i]) = (items[i], items[r]);
        }
    }

    static int[] MakeSequence(int startAt, int endBefore)
    {
        int[] items = new int[endBefore - startAt];
        for (int i = startAt; i < items.Length; i++)
        {
            items[i] = i + startAt;
        }
        return items;
    }

    static int TakeRandomItem(int[] items, Random rng, ref int itemsCount)
    {
        int index = rng.Next(0, itemsCount);
        int item = items[index];
        items[index] = items[--itemsCount];
        return item;
    }
}
