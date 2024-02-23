using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChessBotFunctions
{
    const ulong one = 1;
    public static byte[][] DistanceToEdge()
    {
        byte[][] board = new byte[64][];
        for (byte i = 0; i < 64; i++)
            board[i] = new byte[]{
                (byte)(i % 8 > (i - (i % 8)) / 8 ? (i - (i % 8)) / 8 : i % 8),                              //up left
                (byte)(7 - i % 8 > (i - (i % 8)) / 8 ? (i - (i % 8)) / 8 : 7 - i % 8),                      //up right
                (byte)(7 - i % 8 > 7 - ((i - (i % 8)) / 8) ?  7 - ((i - (i % 8)) / 8) : 7 - i % 8),         //down right
                (byte)(i % 8 > 7 - ((i - (i % 8)) / 8) ?  7 - ((i - (i % 8)) / 8): i % 8),                  //down left
                (byte)((i - (i % 8)) / 8),                                                                  //up
                (byte)(7 - i % 8),                                                                          //right
                (byte)(7 - ((i - (i % 8)) / 8)),                                                            //down
                (byte)(i % 8)                                                                               //left
            };
        return board;
    }
    public static bool[,] LegalKnightMoves(byte[][] dte)
    {
        bool[,] legalMoves = new bool[64, 8];
        for (byte i = 0; i < 64; i++)
        {
            if (dte[i][5] != 0)
            {
                if (dte[i][5] != 1)
                {
                    if (dte[i][4] != 0)
                        legalMoves[i, 2] = true;
                    if (dte[i][6] != 0)
                        legalMoves[i, 3] = true;
                }
                if (dte[i][4] > 1)
                    legalMoves[i, 1] = true;
                if (dte[i][6] > 1)
                    legalMoves[i, 4] = true;
            }
            if (dte[i][7] != 0)
            {
                if (dte[i][7] != 1)
                {
                    if (dte[i][4] != 0)
                        legalMoves[i, 7] = true;
                    if (dte[i][6] != 0)
                        legalMoves[i, 6] = true;
                }
                if (dte[i][4] > 1)
                    legalMoves[i, 0] = true;
                if (dte[i][6] > 1)
                    legalMoves[i, 5] = true;
            }
        }
        return legalMoves;
    }
    public static sbyte[][] PossibleKnightMoves(byte[][] dte)
    {
        sbyte[] nMoves = new sbyte[] { -17, -15, -6, 10, 17, 15, 6, -10 };
        sbyte[][] board = new sbyte[64][];
        for (byte i = 0; i < 64; i++)
        {
            List<sbyte> moves = new();
            if (dte[i][5] != 0)
            {
                if (dte[i][5] != 1)
                {
                    if (dte[i][4] != 0)
                        moves.Add((sbyte)(i + nMoves[2]));
                    if (dte[i][6] != 0)
                        moves.Add((sbyte)(i + nMoves[3]));
                }
                if (dte[i][4] > 1)
                    moves.Add((sbyte)(i + nMoves[1]));
                if (dte[i][6] > 1)
                    moves.Add((sbyte)(i + nMoves[4]));
            }
            if (dte[i][7] != 0)
            {
                if (dte[i][7] != 1)
                {
                    if (dte[i][4] != 0)
                        moves.Add((sbyte)(i + nMoves[7]));
                    if (dte[i][6] != 0)
                        moves.Add((sbyte)(i + nMoves[6]));
                }
                if (dte[i][4] > 1)
                    moves.Add((sbyte)(i + nMoves[0]));
                if (dte[i][6] > 1)
                    moves.Add((sbyte)(i + nMoves[5]));
            }
            board[i] = moves.ToArray();
        }
        return board;
    }

    public static sbyte[,][] PossibleSlidingMoves(byte[][] dte)
    {
        sbyte[] qkMoves = new sbyte[8] { -9, -7, 9, 7, -8, 1, 8, -1 };
        sbyte[,][] board = new sbyte[64, 8][];
        for (sbyte i = 0; i < 64; i++)
        {
            for (byte y = 0; y < 8; y++)
            {
                sbyte[] arr = new sbyte[dte[i][y]];

                for (byte x = 0; x < dte[i][y]; x++)
                {
                    arr[x] = (sbyte)(i + ((x + 1) * qkMoves[y]));

                }
                board[i, y] = arr;
            }
        }
        return board;
    }
    public static byte[,][] PossiblePawnMoves(byte[][] dte)
    {
        byte[,][] board = new byte[64, 2][];
        for (sbyte i = 8; i < 56; i++)
        {

            List<byte> arrW = new();
            arrW.Add((byte)(i - 8));
            if (i > 47) arrW.Add((byte)(i - 16));

            List<byte> arrB = new();
            arrB.Add((byte)(i + 8));
            if (i < 16) arrB.Add((byte)(i + 16));

            board[i, 0] = arrW.ToArray();
            board[i, 1] = arrB.ToArray();
        }
        return board;
    }
    public static string GenerateFENposition(ulong[] bitBoard, int whiteKingPos, int blackKingPos)
    {
        string positionFEN = "";
        int blankSquareCounter = 0;
        for (int i = 0; i < 64; i++)
        {
            if(i == whiteKingPos)
            {
                positionFEN += (blankSquareCounter == 0 ? "" : blankSquareCounter.ToString()) + "K";
                blankSquareCounter = 0;
            }
            else if(i == blackKingPos)
            {
                positionFEN += (blankSquareCounter == 0 ? "" : blankSquareCounter.ToString()) + "k";
                blankSquareCounter = 0;
            }
            else if ((bitBoard[2] & (one << i)) != 0)
                positionFEN += ConvertBit(ref blankSquareCounter, bitBoard[0], i, "p");
            else if ((bitBoard[3] & (one << i)) != 0)
                positionFEN += ConvertBit(ref blankSquareCounter, bitBoard[0], i, "n");
            else if ((bitBoard[4] & (one << i)) != 0)
                positionFEN += ConvertBit(ref blankSquareCounter, bitBoard[0], i, "b");
            else if ((bitBoard[5] & (one << i)) != 0)
                positionFEN += ConvertBit(ref blankSquareCounter, bitBoard[0], i, "r");
            else if ((bitBoard[6] & (one << i)) != 0)
                positionFEN += ConvertBit(ref blankSquareCounter, bitBoard[0], i, "q");
            else blankSquareCounter++;

            if (i % 8 == 7)
            {
                positionFEN += (blankSquareCounter == 0 ? "" : blankSquareCounter.ToString()) + (i != 63 ? "/" : "");
                blankSquareCounter = 0;
            }
        }

        return positionFEN;
    }

    private static string ConvertBit(ref int blankSquareCounter, ulong bitBoard, int square, string piece)
    {
        string returnString;
        if ((bitBoard & (one << square)) != 0)
        {
            returnString = (blankSquareCounter == 0 ? "" : blankSquareCounter.ToString()) + piece.ToUpper();
            blankSquareCounter = 0;
        }
        else
        {
            returnString = (blankSquareCounter == 0 ? "" : blankSquareCounter.ToString()) + piece.ToLower();
            blankSquareCounter = 0;
        }
        return returnString;
    }
    public static byte CountBitsSet(ulong number)
    {
        byte counter;
        for (counter = 0; number != 0; counter++)
        {
            number &= number - 1;
        }
        return counter;
    }
    public static Dictionary<byte, byte> flippedDirections = new Dictionary<byte, byte>(){
            {0, 2},
            {1, 3},
            {2, 0},
            {3, 1},
            {4, 6},
            {5, 7},
            {6, 4},
            {7, 5},
        };
}
