using System;
using System.Collections.Generic;
public static class Values
{
    public static readonly sbyte[] pawnPST = new sbyte[]
    {
            0,  0,  0,  0,  0,  0,  0,  0,
            75, 75, 75, 75, 75, 75, 75, 75,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            -5, -5,  5, 20, 20,  5, -5, -5,
            5, 10,-15,  0,  0,-15, 10,  5,
            10, 15, 15,-20,-20, 15, 15, 10,
            0,  0,  0,  0,  0,  0,  0,  0
    };
    public static readonly sbyte[] knightPST = new sbyte[]
    {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-25,-30,-30,-30,-30,-25,-50,
    };
    public static readonly sbyte[] bishopPST = new sbyte[]
    {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
              0, 10, 10, 10, 10, 10, 10,  0,
            -10, 10,  0,  0,  0,  0, 10,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
    };
    public static readonly sbyte[] rookPST = new sbyte[]
    {

             0,  0,  0,  0,  0,  0,  0,  0,
             0,  5,  5,  5,  5,  5,  5,  0,
             0,  0,  0,  0,  0,  0,  0,  0,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5, -5,  5, 10, 10,  5, -5, -5
    };
    public static readonly sbyte[] queenPST = new sbyte[]
    {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  5, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
    };
    public static readonly sbyte[] mg_kingPST = new sbyte[]
    {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 40, 20,  0,  0, 10, 40, 30
    };
    public static readonly sbyte[] eg_kingPST = new sbyte[]
    {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,  0,  5, 10, 10,  5,  0,-40,
            -30,  5, 10, 20, 20, 10,  5,-30,
            -30, 10, 20, 30, 30, 20, 10,-30,
            -30, 10, 20, 30, 30, 20, 10,-30,
            -30,  5, 10, 20, 20, 10,  5,-30,
            -40,  0,  5, 10, 10,  5,  0,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
    };

    /// <summary>5
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
    public static readonly Dictionary<char, byte> pieceToNum = new() {
        {'P', 0},
        {'N', 1},
        {'B', 2},
        {'R', 3},
        {'Q', 4},
        {'K', 5},
        {' ', 6},
        {'p', 7},
        {'n', 8},
        {'b', 9},
        {'r', 10},
        {'q', 11},
        {'k', 12}
    };

    public static readonly int[] pieceValues = new int[]
    {
                100,
                320,
                330,
                500,
                900,
                0
    };
}

public struct PseudoMove
{
    public bool enP;
    public bool promoted;
    public byte from;
    public sbyte to;
    public byte piece;

    public PseudoMove(bool enP, bool promoted, byte from, sbyte to, byte piece)
    {
        this.enP = enP;
        this.promoted = promoted;
        this.from = from;
        this.to = to;
        this.piece = piece;


    }

}
public struct Move
{
    public bool enP;
    public bool promoted;
    public byte enPsq;
    public byte halfMoveClock;
    public byte castlingRights;
    public byte from;
    public sbyte to;
    public byte piece;
    public byte capturedPiece;

    public Move(bool enP, bool promoted, byte enPsq, byte halfMoveClock, byte castlingRights, byte from, sbyte to, byte piece, byte capturedPiece)
    {
        this.enP = enP;
        this.promoted = promoted;
        this.enPsq = enPsq;
        this.halfMoveClock = halfMoveClock;
        this.castlingRights = castlingRights;
        this.from = from;
        this.to = to;
        this.piece = piece;
        this.capturedPiece = capturedPiece;
    }
}

