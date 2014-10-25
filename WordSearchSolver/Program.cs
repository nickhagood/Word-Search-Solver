using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSearchSolver
{
    enum WordType : byte
    {
        FullWord,
        PartialWord,
        FullWordAndPartialWord
    }

    class Program
    {
        static Dictionary<string, WordType> WordDictionary = new Dictionary<string, WordType>(StringComparer.Ordinal);
        static int GridWidth;
        static int GridHeight;
        static int RowStart;
        static int ColStart;
        static string DirectionName;
        static string OutputString;

        /*
         * Requires the following files:
         * 1) "..\..\..\WordFiles\WordSearch.txt" --> Format: Rectangular grid of letters composing a word search puzzle.
         * 2) "..\..\..\WordFiles\WordList.txt"   --> Format: List of words to search for within the puzzle, one word per line.
         * 
         * Creates/Overwrites:
         * 1) "..\..\..\WordFiles\Output.txt"
         */
        static void Main(string[] args)
        {
            // Read file into string and split based on newlines & carriage returns (remove blank lines to prevent errors).
            string puzzleFile = System.IO.File.ReadAllText(@"..\..\..\WordFiles\WordSearch.txt").ToUpper();
            string[] lines = puzzleFile.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Assumes that every row has the same number of columns, like a standard word search puzzle.
            // Note: This does not mean the grid must be a square, just rectangular.
            int rows = lines.Length;
            int cols = lines[0].Length;

            // Read each character into the grid array.
            char[,] grid = new char[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = lines[r][c];
                }
            }

            // Read file into string and split based on newlines & carriage returns (remove spaces & blank lines to prevent errors).
            string dictionaryFile = System.IO.File.ReadAllText(@"..\..\..\WordFiles\WordList.txt").ToUpper();
            dictionaryFile = dictionaryFile.Replace(" ", String.Empty);
            string[] words = dictionaryFile.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // For each word, create dictionary entries that the grid will be searched against.
            foreach (string word in words)
            {
                /* 
                 * Dictionary entries are stored as partial substrings of that word.
                 * 
                 * For example, processing the word "CODE" would produce the following dictionary entries:
                 * Key: C,    Value: PartialWord
                 * Key: CO,   Value: PartialWord
                 * Key: COD,  Value: PartialWord
                 * Key: CODE, Value: FullWord
                 */
                for (int i = 0; i <= word.Length; i++)
                {
                    string substring = word.Substring(0, i);
                    WordType value;
                    if (WordDictionary.TryGetValue(substring, out value))
                    {
                        // If Fullword.
                        if (i == word.Length)
                        {
                            // If PartialWord is stored.
                            if (value == WordType.PartialWord)
                            {
                                // Change type to FullWordAndPartialWord.
                                WordDictionary[substring] = WordType.FullWordAndPartialWord;
                            }
                        }
                        else
                        {
                            // If !FullWord and PartialWord is stored.
                            if (value == WordType.FullWord)
                            {
                                WordDictionary[substring] = WordType.FullWordAndPartialWord;
                            }
                        }
                    }
                    else
                    {
                        // If FullWord then store FullWord, else store PartialWord.
                        if (i == word.Length)
                        {
                            WordDictionary.Add(substring, WordType.FullWord);
                        }
                        else
                        {
                            WordDictionary.Add(substring, WordType.PartialWord);
                        }
                    }
                }
            }

            // Set boundaries for search function.
            GridWidth = cols;
            GridHeight = rows;

            // Search the grid at each character, in every direction.
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    for (int d = 0; d < 8; d++)
                    {
                        RowStart = r + 1;
                        ColStart = c + 1;
                        SearchAt(grid, r, c, String.Empty, d);
                    }
                }
            }

            // Creates/Overwrites output file.
            System.IO.File.WriteAllText(@"..\..\..\WordFiles\Output.txt", OutputString);
        }

        /*
         * Recursively searches the grid in a given direction for each dictionary entry.
         * 
         * Parameters:
         * grid      --> Grid to search.
         * r         --> Current row. 
         * c         --> Current column.
         * builder   --> Current version of the string built for the dictionary comparisons.
         * direction --> Current direction the search is following.
         * 
         */
        static void SearchAt(char[,] grid, int r, int c, string builder, int direction)
        {
            // Stay within grid boundaries.
            if (r >= GridHeight || r < 0 || c >= GridWidth || c < 0) { return; }

            // Get current letter and append to current word builder.
            char letter = grid[r, c];
            string word = builder + letter;

            // If FullWord.
            WordType value;
            if (WordDictionary.TryGetValue(word, out value))
            {
                // If FullWord is found, append to OutputString.
                if (value == WordType.FullWord || value == WordType.FullWordAndPartialWord)
                {
                    OutputString = OutputString +
                        String.Format("{0}: Row {1}, Col {2}, {3}", word, RowStart, ColStart, DirectionName) +
                        Environment.NewLine;
                }

                // If PartialWord is found, recursively navigate in the current direction.
                if (value == WordType.PartialWord || value == WordType.FullWordAndPartialWord)
                {
                    switch (direction)
                    {
                        case 0:
                            DirectionName = "LR - Left to Right";
                            SearchAt(grid, r, c + 1, word, direction);
                            break;
                        case 1:
                            DirectionName = "RL - Right to Left";
                            SearchAt(grid, r, c - 1, word, direction);
                            break;
                        case 2:
                            DirectionName = "U - Up";
                            SearchAt(grid, r - 1, c, word, direction);
                            break;
                        case 3:
                            DirectionName = "D - Down";
                            SearchAt(grid, r + 1, c, word, direction);
                            break;
                        case 4:
                            DirectionName = "DUL - Diagonal Up Left";
                            SearchAt(grid, r - 1, c - 1, word, direction);
                            break;
                        case 5:
                            DirectionName = "DUR - Diagonal Up Right";
                            SearchAt(grid, r - 1, c + 1, word, direction);
                            break;
                        case 6:
                            DirectionName = "DDL - Diagonal Down Left";
                            SearchAt(grid, r + 1, c - 1, word, direction);
                            break;
                        case 7:
                            DirectionName = "DDR - Diagonal Down Right";
                            SearchAt(grid, r + 1, c + 1, word, direction);
                            break;
                    }
                }
            }
        }
    }
}