using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public static class Functions
{
    /// <summary>
    /// Letter to its value.
    /// </summary>
    public static readonly Dictionary<char, int> pieceValues = new Dictionary<char, int>(){
            {'p', 1}, {'P', -1},
            {'n', 3}, {'N', -3},
            {'b', 3}, {'B', -3},
            {'r', 5}, {'R', -5},
            {'q', 9}, {'Q', -9},
            {'k', 0}, {'K', 0},
            {' ', 0}
        };
    /// <summary>
    /// column or row to value.
    /// </summary>
    public static readonly Dictionary<char, int> tileValues = new Dictionary<char, int>(){
            {'a', 0},
            {'b', 1},
            {'c', 2},
            {'d', 3},
            {'e', 4},
            {'f', 5},
            {'g', 6},
            {'h', 7},
            {'1', 56},
            {'2', 48},
            {'3', 40},
            {'4', 32},
            {'5', 24},
            {'6', 16},
            {'7', 8},
            {'8', 0}
        };
    /// <summary>
    /// value to row
    /// </summary>
    public static readonly Dictionary<int, char> numValues = new Dictionary<int, char>(){
            {0, 'a'},
            {1, 'b'},
            {2, 'c'},
            {3, 'd'},
            {4, 'e'},
            {5, 'f'},
            {6, 'g'},
            {7, 'h'}
        };
    /// <summary>
    /// value to column
    /// </summary>
    public static readonly Dictionary<int, char> rowValues = new Dictionary<int, char>(){
            {56, '1'},
            {48, '2'},
            {40, '3'},
            {32, '4'},
            {24, '5'},
            {16, '6'},
            {8, '7'},
            {0, '8'}
        };
    public static readonly Dictionary<int, char> pieceCodes = new Dictionary<int, char>(){
            {0, 'P'},
            {1, 'N'},
            {2, 'B'},
            {3, 'R'},
            {4, 'Q'},
            {5, 'K'},
            {7, 'p'},
            {8, 'n'},
            {9, 'b'},
            {10, 'r'},
            {11, 'q'},
            {12, 'k'}
        };
    /// <summary>
    /// checks if piece belongs to player off turn.
    /// </summary>
    /// <param name="onTurn"> player on turn.</param>
    /// <param name="piece"> piece to check.</param>
    /// <returns> true if is enemy </returns>
    public static bool IsEnemy(string onTurn, char piece)
    {
        return (onTurn == "w" && char.IsLower(piece))
            || (onTurn == "b" && char.IsUpper(piece));
    }
    /// <summary>
    /// checks if piece belongs to player on turn.
    /// </summary>
    /// <param name="onTurn"> player on turn.</param>
    /// <param name="piece"> piece to check.</param>
    /// <returns> true if is friend </returns>
    public static bool IsFriend(string onTurn, char piece)
    {
        return (onTurn == "b" && char.IsLower(piece))
            || (onTurn == "w" && char.IsUpper(piece));
    }
    /// <summary>
    /// basic evaluation of board.
    /// </summary>
    /// <param name="board"> board to evaluate. </param>
    /// <returns> Evaluation </returns>
    public static int Eval(List<char> board)
    {
        int sum = 0;
        foreach (char c in board)
            sum += pieceValues[c];
        return sum;
    }
    /// <summary>
    /// Generates FEN string from board
    /// </summary>
    /// <param name="board"> board from which to create FEN</param>
    /// <param name="formatFEN"> used to add rules at the end. </param>
    /// <returns>FEN string</returns>
    public static string GenerateFEN(List<char> board, string[] formatFEN)
    {
        string FEN = "";
        int tileCount = 0;
        for (int i = 0; i < 64; i++)
        {
            if (board[i] == ' ')
                tileCount++;
            else
            {
                FEN += (tileCount == 0 ? "" : tileCount.ToString()) + board[i];
                tileCount = 0;
            }
            if (i % 8 == 7)
            {
                FEN += (tileCount == 0 ? "" : tileCount) + (i != 63 ? "/" : "");
                tileCount = 0;
            }
        }
        return FEN += " " + formatFEN[1] + " " + formatFEN[2] + " " + formatFEN[3] + " " + formatFEN[4] + " " + formatFEN[5];
    }
    /// <summary>
    /// translates row and column to num.
    /// </summary>
    /// <param name="s"> row and column</param>
    /// <returns>int representation of a tile</returns>
    public static int TileToNum(string s)
    {
        if (s == "-")
            return -1;
        return tileValues[s[0]] + tileValues[s[1]];
    }
    public static Color ConvertToColor(string color)
    {
        string[] rgba = color.Substring(5, color.Length - 6).Split(", ");
        return new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
    }
    public static string NumToTile(int i)
    {
        if (i == -1)
            return "-";
        else
            return numValues[i % 8].ToString() + rowValues[i - (i % 8)];
    }
}