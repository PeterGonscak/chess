using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary> Class <c>ChessBot_v1</c> is first version of this chess engine.
/// <para>
/// The only public method is <c>byte[] GetMove(string FEN)</c> which returns the best move from chess position specified with FEN
/// </para>
/// </summary>
public static class ChessBot_v1
{
    static readonly byte[][] distanceToEdge = Functions.DistanceToEdge();
    static readonly bool[,] knightLegalMoves = Functions.LegalKnightMoves(distanceToEdge);
    static readonly sbyte[,][] possibleSlidingMoves = Functions.PossibleSlidingMoves(distanceToEdge);
    static readonly sbyte[][] possibleKnightMoves = Functions.PossibleKnightMoves(distanceToEdge);
    static readonly byte[,][] possiblePawnMoves = Functions.PossiblePawnMoves(distanceToEdge);
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
    const ulong one = 1;
    const byte b_one = 1;
    const byte b_zero = 0;
    static int counter = 0;
    static byte activeDepth = 5;
    static bool[] boolGameData;
    static byte[] byteGameData;
    static Move bestMove = new Move(false,0, 0, 0, 0, 0, 0);
    /// <summary>
    /// Returns the best move from specified FEN
    /// </summary>
    /// <param name="FEN">FEN string of the position.</param>
    /// <returns> Starting and landing coordinates stored in <c>byte[2]</c>.</returns>
    public static byte[] GetMove(string FEN)
    {
        
       
        string[] divFEN = FEN.Split(" ");

        boolGameData = new bool[]
        {
            divFEN[1] == "w",
            divFEN[2].Contains('k'),
            divFEN[2].Contains('q'),
            divFEN[2].Contains('K'),
            divFEN[2].Contains('Q')
        };

        byteGameData = new byte[]
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
                    if (pieceToNum[c] < 6) bitBoard[0] |= one << cnt;
                    if (pieceToNum[c] > 6) bitBoard[1] |= one << cnt;
                    for (int i = 0; i < 5; i++)
                        if (pieceToNum[c] % 7 == i) bitBoard[i + 2] |= one << cnt;
                    if (pieceToNum[c] == 5) byteGameData[3] = cnt;
                    if (pieceToNum[c] == 12) byteGameData[4] = cnt;
                    cnt++;
                }
                else cnt += (byte)int.Parse(c.ToString());
            }

        Span<Move> moves = stackalloc Move[activeDepth];
        Span<Position> positions = stackalloc Position[activeDepth]; // test later
        bestMove.eval = boolGameData[0] ? 999999 : -999999;
        FindMoves(ref moves, ref bitBoard, activeDepth);
        
        return new byte[] { 7, (byte) 15 };
    }
    static void FindMoves(ref Span<Move> moves, ref Span<ulong> bitBoard, byte depth) {                                                                     // main search tree
        if (boolGameData[0]) {                                                                                                                              // if white on turn
            for (byte squareNum = 0; squareNum < 64; squareNum++) {                                                                                         // cycle board squares
                ulong shift = one << squareNum;                                                                                                             // shift (select) variable
                if ((bitBoard[0] & shift) == 0) continue;                                                                                                   // skip not white
                if ((bitBoard[2] & shift) != 0) {                                                                                                           // is pawn
                    if ((one << possiblePawnMoves[squareNum, 0][0] & (bitBoard[0] | bitBoard[1])) == 0){                                                    // if 1 forwards clear
                        TryMove(ref moves, ref bitBoard, depth, squareNum, (sbyte)possiblePawnMoves[squareNum, 0][0], 0, false);                            // try 1 forwards
                        if (squareNum > 47 && (one << possiblePawnMoves[squareNum, 0][1] & (bitBoard[0] | bitBoard[1])) == 0)                               // if 2 forwards clear
                            TryMove(ref moves, ref bitBoard, depth, squareNum, (sbyte)possiblePawnMoves[squareNum, 0][1], 0, false);                        // try 2 forwards
                    }
                    if (squareNum % 8 != 0 && (((one << squareNum - 7) & bitBoard[1]) != 0) || squareNum - 7 == byteGameData[0])                            // if not on left edge 
                        TryMove(ref moves, ref bitBoard, depth, squareNum,                                                                                  // try take right
                            (sbyte)(squareNum - 7), 0, squareNum - 7 == byteGameData[0]);
                    if (squareNum % 7 != 0 && (((one << squareNum - 9) & bitBoard[1]) != 0) || squareNum - 9 == byteGameData[0])                            // if not on right edge 
                        TryMove(ref moves, ref bitBoard, depth, squareNum,                                                                                  // try take left
                            (sbyte)(squareNum - 9), 0, squareNum - 9 == byteGameData[0]);
                }
                PiecesTree(ref moves, ref bitBoard, depth, shift, squareNum);                                                                               // piece tree search function
                if (squareNum == byteGameData[3]) {                                                                                                         // is king
                    for (byte y = 0; y < 8; y++)                                                                                                            // for 8 directions
                        if (possibleSlidingMoves[squareNum, y].Length != 0)                                                                                 // if available direction
                            if ((bitBoard[0] & one << possibleSlidingMoves[squareNum, y][0]) != 0)                                                          // if not blocked by white
                                TryMove(ref moves, ref bitBoard, depth, squareNum, possibleSlidingMoves[squareNum, y][0], 5, false);
                    if (boolGameData[1] && NotInCheck(bitBoard, true, 60) && NotInCheck(bitBoard, true, 61) && NotInCheck(bitBoard, true, 62))              // if available
                        TryMove(ref moves, ref bitBoard, depth, 60, 62, 5, false);                                                                          // king side castle
                    if (boolGameData[2] && NotInCheck(bitBoard, true, 60) && NotInCheck(bitBoard, true, 59) && NotInCheck(bitBoard, true, 58))              // if available
                        TryMove(ref moves, ref bitBoard, depth, 60, 58, 5, false);                                                                          // queen side caste
                }
            }
        }
        else {                                                                                                                                              // if black on turn
            for (byte squareNum = 0; squareNum < 64; squareNum++) {                                                                                         // cycle board squares
                ulong shift = one << squareNum;                                                                                                             // shift (select) variable
                if ((bitBoard[1] & shift) == 0) continue;                                                                                                   // skip not black
                if ((bitBoard[2] & shift) != 0) {                                                                                                           // is pawn
                    if ((one << possiblePawnMoves[squareNum, 1][0] & (bitBoard[0] | bitBoard[1])) == 0){                                                    // if 1 forwards clear 
                        TryMove(ref moves, ref bitBoard, depth, squareNum, (sbyte) possiblePawnMoves[squareNum, 1][0], 0, false);                           // try 1 forwards
                        if (squareNum < 16 && (one << possiblePawnMoves[squareNum, 1][1] & (bitBoard[0] | bitBoard[1])) == 0)                               // if 2 forwards clear
                            TryMove(ref moves, ref bitBoard, depth, squareNum, (sbyte) possiblePawnMoves[squareNum, 1][1], 0, false);                       // try 2 forwards
                    }
                    if (squareNum % 8 != 0 && (((one << squareNum + 9) & bitBoard[1]) != 0) || squareNum + 9 == byteGameData[0])                            // if not on left edge
                        TryMove(ref moves, ref bitBoard, depth, squareNum,                                                                                  // try take left
                            (sbyte)(squareNum + 9), 0, squareNum + 9 == byteGameData[0]);
                    if (squareNum % 7 != 0 && (((one << squareNum + 7) & bitBoard[1]) != 0) || squareNum + 7 == byteGameData[0])                            // if not on right edge
                        TryMove(ref moves, ref bitBoard, depth, squareNum,                                                                                  // try take right
                            (sbyte)(squareNum + 7), 0, squareNum + 7 == byteGameData[0]);
                }
                PiecesTree(ref moves, ref bitBoard, depth, shift, squareNum);                                                                               // piece tree search function
                if (squareNum == byteGameData[4]) {                                                                                                         // is king
                    for (byte y = 0; y < 8; y++)                                                                                                            // for 8 directions
                        if(possibleSlidingMoves[squareNum, y].Length != 0)                                                                                  // if available direction
                            if ((bitBoard[1] & one << possibleSlidingMoves[squareNum, y][0]) != 0)                                                          // if not blocked by white
                                TryMove(ref moves, ref bitBoard, depth, squareNum, possibleSlidingMoves[squareNum, y][0], 5, false);
                    if (boolGameData[3] && NotInCheck(bitBoard, false, 4) && NotInCheck(bitBoard, false, 5) && NotInCheck(bitBoard, false, 6))              // if available
                        TryMove(ref moves, ref bitBoard, depth, 4, 6, 5, false);                                                                            // king side castle
                    if (boolGameData[4] && NotInCheck(bitBoard, false, 4) && NotInCheck(bitBoard, false, 3) && NotInCheck(bitBoard, false, 2))              // if available
                        TryMove(ref moves, ref bitBoard, depth, 4, 2, 5, false);                                                                            // queen side castle
                }
            }
        }
    }
    static void PiecesTree(ref Span<Move> moves, ref Span<ulong> bitBoard, byte depth, ulong shift, byte squareNum)
    {
        if ((bitBoard[3] & shift) != 0)                                                                                                                     // is knight
            for (byte x = 0; x < possibleKnightMoves[squareNum].Length; x++)                                                                                // for each possible
                if ((bitBoard[boolGameData[0] ? 0 : 1] & one << possibleKnightMoves[squareNum][x]) != 0)                                                    // if not same color
                    TryMove(ref moves, ref bitBoard, depth, squareNum, possibleKnightMoves[squareNum][x], 1, false);
        if ((bitBoard[4] & shift) != 0)                                                                                                                     // is bishop
            for (byte y = 0; y < 4; y++)                                                                                                                    // for each direction (0-4)
                for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                                        // for available length
                    if ((bitBoard[boolGameData[0] ? 0 : 1] & one << possibleSlidingMoves[squareNum, y][x]) != 0){                                           // if not same color
                        TryMove(ref moves, ref bitBoard, depth, squareNum, possibleSlidingMoves[squareNum, y][x], 2, false);
                        if ((bitBoard[boolGameData[0] ? 1 : 0] & one << possibleSlidingMoves[squareNum, y][x]) != 0) break;                                 // if is same color
                    }
        if ((bitBoard[5] & shift) != 0)                                                                                                                     // is rook
            for (byte y = 4; y < 8; y++)                                                                                                                    // for each direction (4-8)
                for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                                        // for available length
                    if ((bitBoard[boolGameData[0] ? 0 : 1] & one << possibleSlidingMoves[squareNum, y][x]) != 0){                                           // if not same color
                        TryMove(ref moves, ref bitBoard, depth, squareNum, possibleSlidingMoves[squareNum, y][x], 3, false);
                        if ((bitBoard[boolGameData[0] ? 1 : 0] & one << possibleSlidingMoves[squareNum, y][x]) != 0) break;                                 // if is same color
                    }
        if ((bitBoard[6] & shift) != 0)                                                                                                                     // is queen
            for (byte y = 0; y < 8; y++)                                                                                                                    // for each direction (0-8)
                for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                                        // for available length
                    if ((bitBoard[boolGameData[0] ? 0 : 1] & one << possibleSlidingMoves[squareNum, y][x]) != 0){                                           // if not same color
                        TryMove(ref moves, ref bitBoard, depth, squareNum, possibleSlidingMoves[squareNum, y][x], 3, false);
                        if ((bitBoard[boolGameData[0] ? 1 : 0] & one << possibleSlidingMoves[squareNum, y][x]) != 0) break;                                 // if is same color
                    }
    }
    static void TryMove(ref Span<Move> moves, ref Span<ulong> bitBoard, byte depth, byte startingSquare, sbyte landingSquare, byte pieceID, bool enP)
    {
        moves[depth] = MakeMove(ref bitBoard, startingSquare, landingSquare, pieceID, enP);                                                                 // make and save the move to stack
        if (NotInCheck(bitBoard, boolGameData[0], byteGameData[boolGameData[0] ? 3 : 4]))                                                                   // if not king in check
            if (depth == 0) {                                                                                                                               // if end of tree
                counter++;                                                                                                                                  //
                if (boolGameData[0] ? bestMove.eval < moves[0].eval : bestMove.eval > moves[0].eval)                                                        // if is best move so far
                    bestMove = new Move(moves[0].enP, moves[0].castlingRights, moves[0].from, 
                        moves[0].to, moves[0].piece, moves[0].capturedPiece, moves[0].eval);
            }
            else
                FindMoves(ref moves, ref bitBoard, (byte)(depth - 1));                                                                                      // search new postion
        UndoMove(ref bitBoard, ref moves, depth);                                                                                                           // undo move
    }
    static Move MakeMove(ref Span<ulong> bitBoard, byte startingSquare, sbyte landingSquare, byte pieceID, bool enP)
    {
        byte capturedPiece = 6;                                                                                                                             // set captured piece (CP) to blank
        for (int i = 0; i < 5; i++)                                                                                                                         // for 5 different pieces
        {
            if (enP)                                                                                                                                        // if en passant
            {
                capturedPiece = (byte)(((bitBoard[0] & (one << startingSquare)) != 0) ? 7 : 0);                                                             // set CP to opposite pawn
                break;
            }
            else if ((bitBoard[i+2] & (one << landingSquare)) != 0)                                                                                           // if found piece type on landingSq
            {
                capturedPiece = (byte)(i + (((bitBoard[1] & (one << landingSquare)) != 0) ? 7 : 0));                                                        // if black piece captured add 7
                break;
            }
            if ((bitBoard[i+2] & (one << startingSquare)) != 0)                                                                                               // if found piece type on startingSq
            {
                pieceID = (byte)(i + (((bitBoard[0] & (one << startingSquare)) != 0) ? 0 : 7));                                                             // if black piece moved add 7
                break;
            }
        }
        if ((bitBoard[2] & (one << startingSquare)) != 0 && startingSquare - landingSquare % 16 == 0) 
            byteGameData[0] = (byte)(startingSquare + ((startingSquare - landingSquare) / 2));
        byteGameData[1]++;
        if (capturedPiece != 6 || pieceID % 7 == 0) byteGameData[1] = 0;
        if (!boolGameData[0])
            byteGameData[2]++;
        if (pieceID == 5)
        {
            byteGameData[3] = (byte)landingSquare;
            boolGameData[1] = false;
            boolGameData[2] = false;
        }
        else if (pieceID == 12)
        {
            byteGameData[4] = (byte)landingSquare;
            boolGameData[3] = false;
            boolGameData[4] = false;
        }
        else
        {
            if (pieceID == 3)
            {
                if (startingSquare != 63) boolGameData[1] = false;
                else if (startingSquare != 56) boolGameData[2] = false;
            }
            else if (pieceID == 10)
            {
                if (startingSquare != 0) boolGameData[4] = false;
                else if (startingSquare != 7) boolGameData[3] = false;
            }
            for (int i = 0; i < 4; i++)
            {

            }
        }
        
        return new Move(enP, createCRbyte(), startingSquare, landingSquare, pieceID, capturedPiece, Evaluate(bitBoard));
    }
    static void UndoMove(ref Span<ulong> bitBoard, ref Span<Move> moves, byte depth)
    {

    }
    static byte createCRbyte()
    {
        return (byte)((boolGameData[1] ? 8 : 0) + (boolGameData[2] ? 4 : 0) + (boolGameData[3] ? 2 : 0) + (boolGameData[4] ? 1 : 0));
    }
    static bool NotInCheck(Span<ulong> bitBoard, bool colorWhite, byte kingSquare)
    {
        if (colorWhite) {                                                                                                                                   // if white
            if (kingSquare % 8 != 0)                                                                                                                        // if not on left edge
                if ((bitBoard[1] & bitBoard[2] & (one << (kingSquare - 9))) != 0)                                                                           // check left
                    return false;
            if (kingSquare % 7 != 0)                                                                                                                        // if not on right edge
                if ((bitBoard[1] & bitBoard[2] & (one << (kingSquare - 7))) != 0)                                                                           // check right
                    return false;
        } else {                                                                                                                                            // is black
            if (kingSquare % 8 != 0)                                                                                                                        // if not on left edge
                if ((bitBoard[0] & bitBoard[2] & (one << (kingSquare + 7))) != 0)                                                                           // check left
                    return false;                                                                                                                           //
            if (kingSquare % 7 != 0)                                                                                                                        // if not on right edge
                if ((bitBoard[0] & bitBoard[2] & (one << (kingSquare + 9))) != 0)                                                                           // check right
                    return false;
        }
        for (byte x = 0; x < possibleKnightMoves[kingSquare].Length; x++)                                                                                   // check knights
            if ((bitBoard[colorWhite ? 1 : 0] & bitBoard[3] & one << possibleKnightMoves[kingSquare][x]) != 0)                                              // if enemy knight in possible
                return false;
        for (byte y = 0; y < 4; y++)                                                                                                                        // check bishop & queen directions
            for (byte x = 0; x < possibleSlidingMoves[kingSquare, y].Length; x++)                                                                           // loop through possible
            {
                if ((bitBoard[colorWhite ? 0 : 1] & (bitBoard[4] | bitBoard[6]) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) break;             // if same color skip to next
                if ((bitBoard[colorWhite ? 1 : 0] & (bitBoard[4] | bitBoard[6]) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) return false;      // if opposite return false
            }
        for (byte y = 4; y < 8; y++)                                                                                                                        // check rook & queen directions
            for (byte x = 0; x < possibleSlidingMoves[kingSquare, y].Length; x++)                                                                           // loop through possible
            {
                if ((bitBoard[colorWhite ? 0 : 1] & (bitBoard[5] | bitBoard[6]) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) break;             // if same color skip to next
                if ((bitBoard[colorWhite ? 1 : 0] & (bitBoard[5] | bitBoard[6]) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) return false;      // if opposite return false
            }
        for (byte y = 0; y < 8; y++)                                                                                                                        // for 8 directions
            if (possibleSlidingMoves[kingSquare, y].Length != 0 &&                                                                                          // if not on edge
                ((one << byteGameData[colorWhite ? 4 : 3]) & (one << possibleSlidingMoves[kingSquare, y][0])) != 0)                                         // if enemy king
                return false;
        return true;                                                                                                                                        // nothing found - return true
    }
    static float Evaluate(Span<ulong> bitBoard)
    {




        return 0f;
    }
}

public struct Move
{
    public bool enP;
    public byte castlingRights;
    public byte from;
    public sbyte to;
    public byte piece;
    public byte capturedPiece;
    public float eval;

    public Move(bool enP, byte castlingRights, byte from, sbyte to, byte piece, byte capturedPiece, float eval)
    {
        this.enP = enP;
        this.castlingRights = castlingRights;
        this.from = from;
        this.to = to;
        this.piece = piece;
        this.capturedPiece = capturedPiece;
        this.eval = eval;
    }
}
public struct Position
{
    ulong whiteBoard;
    ulong blackBoard;
    ulong pawnBoard;
    ulong knightBoard;
    ulong bishopBoard;
    ulong rookBoard;
    ulong queenBoard;
    byte whiteKingPos;
    byte blackKingPos;
    byte enpSquare;
    byte halfMoveClock;
    byte fullMoveClock;
    bool whiteOnTurn;
    bool whiteKingCastle;
    bool whiteQueenCastle;
    bool blackKingCastle;
    bool blackQueenCastle;
}