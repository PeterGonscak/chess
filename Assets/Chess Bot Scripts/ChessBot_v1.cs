using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class ChessBot_v1
{
    static readonly byte[][] distanceToEdge = Functions.DistanceToEdge();
    static readonly bool[,] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
    static readonly sbyte[,][] possibleSlidingMoves = Functions.PossibleSlidingMoves(distanceToEdge);
    static readonly sbyte[][] possibleKnightMoves = Functions.PossibleKnightMoves(distanceToEdge);
    static private readonly Dictionary<char, byte> pieceToNum = new() {
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
    public static byte[] GetMove(string FEN)
    {
        
        
        string[] divFEN = FEN.Split(" ");

        bool[] boolGameData = new bool[]
        {
            divFEN[1] == "w",
            divFEN[2].Contains('k'),
            divFEN[2].Contains('q'),
            divFEN[2].Contains('K'),
            divFEN[2].Contains('Q')
        };

        byte[] byteGameData = new byte[]
        {
            (byte) Functions.TileToNum(divFEN[3]),
            (byte) int.Parse(divFEN[4]),
            (byte) int.Parse(divFEN[5].Substring(0, 1)),
            0,
            0
        };

        Span<ulong> bitBoard = stackalloc ulong[7] { 0, 0, 0, 0, 0, 0, 0 }; // white / black / pawns / knights / bishops / rooks / queens

        byte cnt = 0;

        foreach (string row in divFEN[0].Split("/"))
            foreach (char c in row)
            {
                if ("PNBRQKpnbrqk".Contains(c))
                {
                    if (pieceToNum[c] < 6) bitBoard[0] |= (ulong) 1 << cnt;
                    if (pieceToNum[c] > 6) bitBoard[1] |= (ulong) 1 << cnt;
                    for (int i = 0; i < 5; i++)
                        if (pieceToNum[c] % 7 == i) bitBoard[i + 2] |= (ulong)1 << cnt;
                    if (pieceToNum[c] == 5) byteGameData[3] = cnt;
                    if (pieceToNum[c] == 12) byteGameData[4] = cnt;
                    cnt++;
                }
                else cnt += (byte)int.Parse(c.ToString());
                
            }

        Span<Move> moves = stackalloc Move[256];

        FindMoves(ref moves, bitBoard, boolGameData, byteGameData, 4);

        Move bestMove = moves.ToArray().ToList().OrderByDescending(item => item.eval).First();

        return new byte[] { bestMove.from, bestMove.to };
    }
    static void FindMoves(ref Span<Move> moves, Span<ulong> bitBoard, bool[] boolGameData, byte[] byteGameData, byte depth) {
        if (boolGameData[0]) {                                                                                                      // white on turn
            for (byte squareNum = 0; squareNum < 64; squareNum++) {                                                                 // cycle board
                ulong shift = (ulong)1 << squareNum;                                                                                // shift var
                if ((bitBoard[0] & shift) == 0) continue;                                                                           // not white
                if ((bitBoard[2] & shift) != 0) {                                                                                   // pawns

                }
                if ((bitBoard[3] & shift) != 0) {                                                                                   // knights

                }
                if ((bitBoard[4] & shift) != 0) {                                                                                   // bishops

                }
                if ((bitBoard[5] & shift) != 0) {                                                                                   // rooks

                }
                if ((bitBoard[6] & shift) != 0) {                                                                                   // queens

                }
                if (squareNum == byteGameData[3]) {                                                                                 // king

                }
            }
        }
        else {                                                                                                                      // black on turn
            for (byte squareNum = 0; squareNum < 64; squareNum++) {                                                                 // cycle board
                ulong shift = (ulong)1 << squareNum;                                                                                // shift var
                if ((bitBoard[1] & shift) == 0) continue;                                                                           // not black
                if ((bitBoard[2] & shift) != 0) {                                                                                   // pawns

                }
                if ((bitBoard[3] & shift) != 0) {                                                                                   // knights

                }
                if ((bitBoard[4] & shift) != 0) {                                                                                   // bishops

                }
                if ((bitBoard[5] & shift) != 0) {                                                                                   // rooks

                }
                if ((bitBoard[6] & shift) != 0) {                                                                                   // queens

                }
                if (squareNum == byteGameData[3]) {                                                                                 // king

                }
            }
        }
        moves[0] = new Move(8, 16, 0, 0);
    }
    static void MakeMove()
    {

    }
    static void UndoMove()
    {

    }
}

public struct Move
{
    public byte from;
    public byte to;
    public byte piece;
    public sbyte eval;

    public Move(byte from, byte to, byte piece, sbyte eval)
    {
        this.from = from;
        this.to = to;
        this.piece = piece;
        this.eval = eval;
    }
}
