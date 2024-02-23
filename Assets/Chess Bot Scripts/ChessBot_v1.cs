using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ChessBot_v1
{
    static readonly byte[][] distanceToEdge = ChessBotFunctions.DistanceToEdge();
    static readonly sbyte[,][] possibleSlidingMoves = ChessBotFunctions.PossibleSlidingMoves(distanceToEdge);
    static readonly sbyte[][] possibleKnightMoves = ChessBotFunctions.PossibleKnightMoves(distanceToEdge);
    static readonly byte[,][] possiblePawnMoves = ChessBotFunctions.PossiblePawnMoves(distanceToEdge);

    const long CKmask = 3;
    const long CQmask = 7;
    const ulong one = 1;
    static bool[] boolGameData = new bool[5];
    static byte[] byteGameData = new byte[5];

    static Dictionary<Move, float> movesAndEvals = new();
    /// <summary>
    /// Returns the best move from specified FEN
    /// </summary>
    /// <param name="FEN">FEN string of the position.</param>
    /// <returns> Starting and landing coordinates stored in <c>byte[2]</c>.</returns>
    public static int[] GetMove(string FEN, List<string> positions)
    {
        string[] divFEN = FEN.Split(" ");
        boolGameData = new bool[]
        {
            divFEN[1] == "w",
            divFEN[2].Contains('K'),
            divFEN[2].Contains('Q'),
            divFEN[2].Contains('k'),
            divFEN[2].Contains('q')
        };
        byteGameData = new byte[]
        {
            (byte) Functions.TileToNum(divFEN[3]),
            (byte) int.Parse(divFEN[4]),
            (byte) int.Parse(divFEN[5].Substring(0, 1)),
            0,
            0
        };


        Span<ulong> bitBoard = stackalloc ulong[7] { 0, 0, 0, 0, 0, 0, 0 };

        byte cnt = 0;

        foreach (string row in divFEN[0].Split("/"))
            foreach (char c in row)
            {
                if ("PNBRQKpnbrqk".Contains(c))
                {
                    if (Values.pieceToNum[c] < 6) bitBoard[0] |= one << cnt;
                    if (Values.pieceToNum[c] > 6) bitBoard[1] |= one << cnt;
                    for (int i = 0; i < 5; i++)
                        if (Values.pieceToNum[c] % 7 == i) bitBoard[i + 2] |= one << cnt;
                    if (Values.pieceToNum[c] == 5) byteGameData[3] = cnt;
                    if (Values.pieceToNum[c] == 12) byteGameData[4] = cnt;
                    cnt++;
                }
                else cnt += (byte)int.Parse(c.ToString());
            }


        byte depth = 3;
        float material = ChessBotFunctions.CountBitsSet(bitBoard[2]) * Values.pieceValues[0]
                       + ChessBotFunctions.CountBitsSet(bitBoard[3]) * Values.pieceValues[1]
                       + ChessBotFunctions.CountBitsSet(bitBoard[4]) * Values.pieceValues[2]
                       + ChessBotFunctions.CountBitsSet(bitBoard[5]) * Values.pieceValues[3]
                       + ChessBotFunctions.CountBitsSet(bitBoard[6]) * Values.pieceValues[4];
        if (material < 2000f) depth = 4;
        if (material < 600f) depth = 5;


        PseudoMove[] generatedMoves = FindMoves(bitBoard);
        foreach (PseudoMove pseudoMove in generatedMoves)
        {
            Move playedMove = MakeMove(ref bitBoard, pseudoMove.from, pseudoMove.to, pseudoMove.piece, pseudoMove.enP, pseudoMove.promoted);
            if (NotInCheck(bitBoard, !boolGameData[0], byteGameData[!boolGameData[0] ? 3 : 4]))
            {
                float eval;
                ulong[] bitBoardCopy = bitBoard.ToArray();
                if (positions.Count(x => x == ChessBotFunctions.GenerateFENposition(bitBoardCopy, byteGameData[3], byteGameData[4])) == 2)
                    eval = 0f;
                else 
                    eval = -Minimax(bitBoard, depth);

                movesAndEvals.Add(playedMove, eval);
            }
            UndoMove(ref bitBoard, playedMove);
        }


        var ordered = movesAndEvals.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        List<Move> bestMoves = new List<Move>();

        foreach (var item in ordered)
            if (item.Value >= (Values.botRandomisedPick ? ordered.Last().Value - Values.botRandomiseMaxDelta : ordered.Last().Value))
                bestMoves.Add(item.Key);

        System.Random rnd = new System.Random();
        int moveIndex = rnd.Next(0, bestMoves.Count() - 1);


        movesAndEvals.Clear();


        int[] move = new int[] { bestMoves[moveIndex].from, bestMoves[moveIndex].to };
        return move;
    }

    static float Minimax(Span<ulong> bitBoard, byte depth)
    {
        if (depth == 0)
            return Evaluate(bitBoard, boolGameData[0]);

        byte numOfMoves = 0;
        float bestEval = float.NegativeInfinity;
        PseudoMove[] generatedMoves = FindMoves(bitBoard);
        foreach (PseudoMove pseudoMove in generatedMoves)
        {
            Move playedMove = MakeMove(ref bitBoard, pseudoMove.from, pseudoMove.to, pseudoMove.piece, pseudoMove.enP, pseudoMove.promoted);
            if (NotInCheck(bitBoard, !boolGameData[0], byteGameData[!boolGameData[0] ? 3 : 4]))
            {
                numOfMoves++;
                float eval = -Minimax(bitBoard, (byte)(depth - 1));
                if (bestEval < eval)
                    bestEval = eval;
            }
            UndoMove(ref bitBoard, playedMove);
        }
        if (numOfMoves == 0)
        {
            if (NotInCheck(bitBoard, boolGameData[0], byteGameData[boolGameData[0] ? 3 : 4]))
                return 0;
            else
                return -99999f * depth;
        }

        return bestEval;
    }
    static PseudoMove[] FindMoves(Span<ulong> bitBoard)
    {
        List<PseudoMove> moves = new List<PseudoMove>();                                                                                        // main search tree
        if (boolGameData[0])
        {                                                                                                                                       // if white on turn
            for (byte squareNum = 0; squareNum < 64; squareNum++)
            {                                                                                                                                   // cycle board squares
                ulong shift = one << squareNum;                                                                                                 // shift (select) variable
                if ((bitBoard[0] & shift) == 0) continue;                                                                                       // skip not white
                if ((bitBoard[2] & shift) != 0)                                                                                                 // is pawn
                {
                    if (((one << possiblePawnMoves[squareNum, 0][0]) & (bitBoard[0] | bitBoard[1])) == 0)                                       // if 1 forwards clear
                    {
                        if (possiblePawnMoves[squareNum, 0][0] < 8)                                                                              // if on last rank
                            for (byte i = 1; i < 5; i++)
                                moves.Add(new PseudoMove(false, true, squareNum, (sbyte)possiblePawnMoves[squareNum, 0][0], i));
                        else
                            moves.Add(new PseudoMove(false, false, squareNum, (sbyte)possiblePawnMoves[squareNum, 0][0], 0));
                        if (squareNum > 47 && ((one << possiblePawnMoves[squareNum, 0][1]) & (bitBoard[0] | bitBoard[1])) == 0)                 // if 2 forwards clear
                            moves.Add(new PseudoMove(false, false, squareNum, (sbyte)possiblePawnMoves[squareNum, 0][1], 0));
                    }
                    if (squareNum % 8 != 7 && ((((one << (squareNum - 7)) & bitBoard[1]) != 0) || squareNum - 7 == byteGameData[0]))            // if not on left edge
                        if (squareNum - 7 < 8)                                                                                                  // if on last rank
                            for (byte i = 1; i < 5; i++)
                                moves.Add(new PseudoMove(false, true, squareNum, (sbyte)(squareNum - 7), i));
                        else
                            moves.Add(new PseudoMove(squareNum - 7 == byteGameData[0], false, squareNum, (sbyte)(squareNum - 7), 0));

                    if (squareNum % 8 != 0 && ((((one << (squareNum - 9)) & bitBoard[1]) != 0) || squareNum - 9 == byteGameData[0]))            // if not on right edge
                        if (squareNum - 9 < 8)                                                                                                  // if on last rank
                            for (byte i = 1; i < 5; i++)
                                moves.Add(new PseudoMove(false, true, squareNum, (sbyte)(squareNum - 9), i));
                        else
                            moves.Add(new PseudoMove(squareNum - 9 == byteGameData[0], false, squareNum, (sbyte)(squareNum - 9), 0));
                }
                if ((bitBoard[3] & shift) != 0)                                                                                                         // is knight
                    for (byte x = 0; x < possibleKnightMoves[squareNum].Length; x++)                                                                    // for each possible
                        if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleKnightMoves[squareNum][x])) == 0)                                      // if not same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleKnightMoves[squareNum][x], 1));
                if ((bitBoard[4] & shift) != 0)                                                                                                         // is bishop
                    for (byte y = 0; y < 4; y++)                                                                                                        // for each direction (0-4)
                        for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                            // for available length
                        {
                            if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][x], 2));
                            if ((bitBoard[boolGameData[0] ? 1 : 0] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if captured
                        }
                if ((bitBoard[5] & shift) != 0)                                                                                                         // is rook
                    for (byte y = 4; y < 8; y++)                                                                                                        // for each direction (4-8)
                        for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                            // for available length
                        {
                            if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][x], 3));
                            if ((bitBoard[boolGameData[0] ? 1 : 0] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if captured
                        }
                if ((bitBoard[6] & shift) != 0)                                                                                                         // is queen
                    for (byte y = 0; y < 8; y++)                                                                                                        // for each direction (0-8)
                        for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                            // for available length
                        {
                            if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][x], 4));
                            if ((bitBoard[boolGameData[0] ? 1 : 0] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if captured
                        }
                if (squareNum == byteGameData[3])
                {                                                                                             // is king
                    for (byte y = 0; y < 8; y++)                                                                                                // for 8 directions
                        if (possibleSlidingMoves[squareNum, y].Length != 0)                                                                     // if available direction
                            if ((bitBoard[0] & (one << possibleSlidingMoves[squareNum, y][0])) == 0)                                            // if not blocked by white
                                moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][0], 5));
                    if (boolGameData[1] && (((CKmask << 61) & (bitBoard[0] | bitBoard[1])) == 0)
                        && NotInCheck(bitBoard, true, 60) && NotInCheck(bitBoard, true, 61) && NotInCheck(bitBoard, true, 62))                  // if available
                        moves.Add(new PseudoMove(false, false, 60, 62, 5));
                    if (boolGameData[2] && (((CQmask << 57) & (bitBoard[0] | bitBoard[1])) == 0)
                        && NotInCheck(bitBoard, true, 60) && NotInCheck(bitBoard, true, 59) && NotInCheck(bitBoard, true, 58))                  // if available
                        moves.Add(new PseudoMove(false, false, 60, 58, 5));
                }
            }
        }
        else
        {                                                                                                                                       // if black on turn
            for (byte squareNum = 0; squareNum < 64; squareNum++)                                                                               // cycle board squares
            {
                ulong shift = one << squareNum;                                                                                                 // shift (select) variable
                if ((bitBoard[1] & shift) == 0) continue;                                                                                       // skip not black
                if ((bitBoard[2] & shift) != 0)                                                                                                 // is pawn
                {
                    if (((one << possiblePawnMoves[squareNum, 1][0]) & (bitBoard[0] | bitBoard[1])) == 0)                                       // if 1 forwards clear 
                    {
                        if (possiblePawnMoves[squareNum, 1][0] > 55)                                                                                                     // if on last rank
                            for (byte i = 1; i < 5; i++)
                                moves.Add(new PseudoMove(false, true, squareNum, (sbyte)possiblePawnMoves[squareNum, 1][0], i));
                        else
                            moves.Add(new PseudoMove(false, false, squareNum, (sbyte)possiblePawnMoves[squareNum, 1][0], 0));
                        if (squareNum < 16 && ((one << possiblePawnMoves[squareNum, 1][1]) & (bitBoard[0] | bitBoard[1])) == 0)                 // if 2 forwards clear
                            moves.Add(new PseudoMove(false, false, squareNum, (sbyte)possiblePawnMoves[squareNum, 1][1], 0));
                    }
                    if (squareNum % 8 != 7 && ((((one << (squareNum + 9)) & bitBoard[0]) != 0) || squareNum + 9 == byteGameData[0]))            // if not on left edge
                        if (squareNum + 9 > 55)                                                                                                 // if on last rank
                            for (byte i = 1; i < 5; i++)
                                moves.Add(new PseudoMove(false, true, squareNum, (sbyte)(squareNum + 9), i));
                        else
                            moves.Add(new PseudoMove(squareNum + 9 == byteGameData[0], false, squareNum, (sbyte)(squareNum + 9), 0));
                    if (squareNum % 8 != 0 && ((((one << (squareNum + 7)) & bitBoard[0]) != 0) || squareNum + 7 == byteGameData[0]))            // if not on right edge
                        if (squareNum + 7 > 55)                                                                                                 // if on last rank
                            for (byte i = 1; i < 5; i++)
                                moves.Add(new PseudoMove(false, true, squareNum, (sbyte)(squareNum + 7), i));
                        else
                            moves.Add(new PseudoMove(squareNum + 7 == byteGameData[0], false, squareNum, (sbyte)(squareNum + 7), 0));
                }
                if ((bitBoard[3] & shift) != 0)                                                                                                         // is knight
                    for (byte x = 0; x < possibleKnightMoves[squareNum].Length; x++)                                                                    // for each possible
                        if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleKnightMoves[squareNum][x])) == 0)                                      // if not same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleKnightMoves[squareNum][x], 1));
                if ((bitBoard[4] & shift) != 0)                                                                                                         // is bishop
                    for (byte y = 0; y < 4; y++)                                                                                                        // for each direction (0-4)
                        for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                            // for available length
                        {
                            if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][x], 2));
                            if ((bitBoard[boolGameData[0] ? 1 : 0] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if captured
                        }
                if ((bitBoard[5] & shift) != 0)                                                                                                         // is rook
                    for (byte y = 4; y < 8; y++)                                                                                                        // for each direction (4-8)
                        for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                            // for available length
                        {
                            if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][x], 3));
                            if ((bitBoard[boolGameData[0] ? 1 : 0] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if captured
                        }
                if ((bitBoard[6] & shift) != 0)                                                                                                         // is queen
                    for (byte y = 0; y < 8; y++)                                                                                                        // for each direction (0-8)
                        for (byte x = 0; x < possibleSlidingMoves[squareNum, y].Length; x++)                                                            // for available length
                        {
                            if ((bitBoard[boolGameData[0] ? 0 : 1] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if same color
                            moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][x], 4));
                            if ((bitBoard[boolGameData[0] ? 1 : 0] & (one << possibleSlidingMoves[squareNum, y][x])) != 0) break;                       // if captured
                        }
                if (squareNum == byteGameData[4])
                {                                                                                             // is king

                    for (byte y = 0; y < 8; y++)                                                                                                // for 8 directions
                        if (possibleSlidingMoves[squareNum, y].Length != 0)                                                                      // if available direction
                            if ((bitBoard[1] & (one << possibleSlidingMoves[squareNum, y][0])) == 0)                                            // if not blocked by black
                                moves.Add(new PseudoMove(false, false, squareNum, possibleSlidingMoves[squareNum, y][0], 5));
                    if (boolGameData[3] && (((CKmask << 5) & (bitBoard[0] | bitBoard[1])) == 0)
                        && NotInCheck(bitBoard, false, 4) && NotInCheck(bitBoard, false, 5) && NotInCheck(bitBoard, false, 6))                  // if available
                        moves.Add(new PseudoMove(false, false, 4, 6, 5));                                                             // king side castle
                    if (boolGameData[4] && (((CQmask << 1) & (bitBoard[0] | bitBoard[1])) == 0)
                        && NotInCheck(bitBoard, false, 4) && NotInCheck(bitBoard, false, 3) && NotInCheck(bitBoard, false, 2))                  // if available
                        moves.Add(new PseudoMove(false, false, 4, 2, 5));                                                              // queen side castle
                }
            }
        }
        return moves.ToArray();
    }
    static Move MakeMove(ref Span<ulong> bitBoard, byte startingSquare, sbyte landingSquare, byte pieceID, bool enP, bool promoted)
    {
        byte CRbyte = CreateCRbyte();
        byte capturedPiece = 6;                                                                                                                 // set captured piece (CP) to blank
        byteGameData[0] = 255;
        if (enP)
            capturedPiece = (byte)(boolGameData[0] ? 7 : 0);
        else
            for (byte i = 0; i < 5; i++)
                if ((bitBoard[i + 2] & (one << landingSquare)) != 0)
                    capturedPiece = (byte)(i + (boolGameData[0] ? 7 : 0));
        pieceID += (byte)(boolGameData[0] ? 0 : 7);
        if ((bitBoard[2] & (one << startingSquare)) != 0)
        {
            if ((startingSquare - landingSquare) % 16 == 0)                                                                                        // if pawn moved 2 squares
                byteGameData[0] = (byte)(boolGameData[0] ? startingSquare - 8 : startingSquare + 8);                                           // set enP square
        }
        byteGameData[1]++;                                                                                                                      // add to halfmove clock (HMC)
        if (capturedPiece != 6 || pieceID % 7 == 0 || promoted) byteGameData[1] = 0;                                                                        // if capture or pawn move reset HMC
        if (!boolGameData[0])                                                                                                                   // if black on turn
            byteGameData[2]++;                                                                                                                  // add to fullmove clock
        if (pieceID == 5)                                                                                                                       // if white king moved
        {
            byteGameData[3] = (byte)landingSquare;                                                                                              // set king square
            boolGameData[1] = false;                                                                                                            // unset king side castle (KSC)
            boolGameData[2] = false;                                                                                                            // unset queen side castle (QSC)
        }
        else if (pieceID == 12)                                                                                                                 // if black king moved
        {
            byteGameData[4] = (byte)landingSquare;                                                                                              // set king square
            boolGameData[3] = false;                                                                                                            // unset KSC
            boolGameData[4] = false;                                                                                                            // unset QSC
        }
        else
        {
            if (pieceID == 3)                                                                                                                   // if white rook moved
            {
                if (startingSquare == 63) boolGameData[1] = false;                                                                              // if king side moved unset KSC
                else if (startingSquare == 56) boolGameData[2] = false;                                                                         // if queen side moved unset QSC
            }
            else if (pieceID == 10)                                                                                                             // if black rook moved
            {
                if (startingSquare == 7) boolGameData[3] = false;                                                                               // if king side moved unset KSC
                else if (startingSquare == 0) boolGameData[4] = false;                                                                          // if queen side moved unset QSC
            }
            if (landingSquare == 63) boolGameData[1] = false;                                                                                   // if captured white rook unset KSC
            if (landingSquare == 56) boolGameData[2] = false;                                                                                   // if captured white rook unset QSC
            if (landingSquare == 7) boolGameData[3] = false;                                                                                    // if captured black rook unset KSC 
            if (landingSquare == 0) boolGameData[4] = false;                                                                                    // if captured black rook unset QSC
        }
        if (enP)                                                                                                                            // if en passant
        {
            bitBoard[boolGameData[0] ? 1 : 0] &= ~(one << ((boolGameData[0] ? 8 : -8) + landingSquare));                                    // remove on bitboard white or black
            bitBoard[2] &= ~(one << ((boolGameData[0] ? 8 : -8) + landingSquare));                                                          // remove on pawn bitboard
        }
        else if (capturedPiece != 6)
        {
            bitBoard[boolGameData[0] ? 1 : 0] &= ~(one << landingSquare);                                                                   // remove on bitboard white or black
            if (capturedPiece % 7 != 5) bitBoard[2 + (capturedPiece % 7)] &= ~(one << landingSquare);                                       // if not king remove from bitboard
        }

        bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << startingSquare)) | (one << landingSquare);            // move on bitboard white or black


        if (pieceID % 7 != 5)
            if (promoted)
            {
                bitBoard[2] &= ~(one << startingSquare);
                bitBoard[2 + (pieceID % 7)] |= (one << landingSquare);
            }
            else
                bitBoard[2 + (pieceID % 7)] = (bitBoard[2 + (pieceID % 7)] & ~(one << startingSquare)) | (one << landingSquare);                    // if not king move on piece bitboard
        else                                                                                                                                    // if king
        {
            if (startingSquare == 60)                                                                                                           // if start on white kings square
            {
                if (landingSquare == 62)                                                                                                        // if landing on KSC
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 63)) | (one << 61);                       // move rook on white
                    bitBoard[5] = (bitBoard[5] & ~(one << 63)) | (one << 61);                                                                   // move rook on piece board
                }
                if (landingSquare == 58)                                                                                                        // if landing on QSC
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 56)) | (one << 59);                       // move rook on white
                    bitBoard[5] = (bitBoard[5] & ~(one << 56)) | (one << 59);                                                                   // move rook on piece board
                }
            }
            else if (startingSquare == 4)                                                                                                       // if start on black kings square
            {
                if (landingSquare == 6)                                                                                                         // if landing on KSC
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 7)) | (one << 5);                         // move rook on black
                    bitBoard[5] = (bitBoard[5] & ~(one << 7)) | (one << 5);                                                                     // move rook on piece board
                }
                if (landingSquare == 2)                                                                                                         // if landing on QSC
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 0)) | (one << 3);                         // move rook on black
                    bitBoard[5] = (bitBoard[5] & ~(one << 0)) | (one << 3);                                                                     // move rook on piece board
                }
            }
        }
        boolGameData[0] = !boolGameData[0];                                                                                                     // flip on turn bool
        return new Move((bool)enP, (bool)promoted, (byte)byteGameData[0], (byte)byteGameData[1], (byte)CRbyte, (byte)startingSquare,
                        (sbyte)landingSquare, (byte)pieceID, (byte)capturedPiece);                                                             // return new move
    }
    static void UndoMove(ref Span<ulong> bitBoard, Move move)
    {
        byte piece = move.piece;
        byte cPiece = move.capturedPiece;
        byte startSQ = move.from;
        sbyte landSQ = move.to;
        boolGameData[0] = !boolGameData[0];                                                                                                     // flip on turn bool
        bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << landSQ)) | (one << startSQ);

        if (piece == 5)
        {
            byteGameData[3] = startSQ;
            if (startSQ == 60)
            {
                if (landSQ == 62)
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 61)) | (one << 63);
                    bitBoard[5] = (bitBoard[5] & ~(one << 61)) | (one << 63);
                }
                else if (landSQ == 58)
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 59)) | (one << 56);
                    bitBoard[5] = (bitBoard[5] & ~(one << 59)) | (one << 56);
                }
            }
        }
        else if (piece == 12)
        {
            byteGameData[4] = startSQ;
            if (startSQ == 4)
            {
                if (landSQ == 6)
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 5)) | (one << 7);
                    bitBoard[5] = (bitBoard[5] & ~(one << 5)) | (one << 7);
                }
                else if (landSQ == 2)
                {
                    bitBoard[boolGameData[0] ? 0 : 1] = (bitBoard[boolGameData[0] ? 0 : 1] & ~(one << 3)) | (one << 0);
                    bitBoard[5] = (bitBoard[5] & ~(one << 3)) | (one << 0);
                }
            }
        }
        else if (move.promoted)
        {
            bitBoard[2] |= (one << startSQ);
            bitBoard[2 + (piece % 7)] &= ~(one << landSQ);
        }
        else
            bitBoard[2 + (piece % 7)] = (bitBoard[2 + (piece % 7)] & ~(one << landSQ)) | (one << startSQ);
        if (cPiece != 6)
        {
            if (move.enP)
            {
                bitBoard[boolGameData[0] ? 1 : 0] |= one << ((boolGameData[0] ? 8 : -8) + landSQ);
                bitBoard[2] |= one << ((boolGameData[0] ? 8 : -8) + landSQ);
            }
            else
            {
                bitBoard[boolGameData[0] ? 1 : 0] |= one << landSQ;
                if (cPiece % 7 != 5) bitBoard[2 + (cPiece % 7)] |= one << landSQ;
            }
        }
        byteGameData[0] = move.enPsq;
        byteGameData[1] = move.halfMoveClock;
        if (!boolGameData[0])
            byteGameData[2]--;
        for (int i = 0; i < 4; i++)
        {
            boolGameData[4 - i] = (move.castlingRights & (one << i)) != 0;
        }
    }
    static byte CreateCRbyte()
    {
        return (byte)((boolGameData[1] ? 8 : 0) + (boolGameData[2] ? 4 : 0) + (boolGameData[3] ? 2 : 0) + (boolGameData[4] ? 1 : 0));
    }
    // color white == true => finding if white king is in check
    static bool NotInCheck(Span<ulong> bitBoard, bool colorWhite, byte kingSquare)
    {
        if (colorWhite)
        {                                                                                                                                                   // if white
            if (kingSquare % 8 != 0)                                                                                                                        // if not on left edge
                if ((bitBoard[1] & bitBoard[2] & (one << (kingSquare - 9))) != 0)                                                                           // check left
                    return false;
            if (kingSquare % 8 != 7)                                                                                                                        // if not on right edge
                if ((bitBoard[1] & bitBoard[2] & (one << (kingSquare - 7))) != 0)                                                                           // check right
                    return false;
        }
        else
        {                                                                                                                                                   // is black
            if (kingSquare % 8 != 0)                                                                                                                        // if not on left edge
                if ((bitBoard[0] & bitBoard[2] & (one << (kingSquare + 7))) != 0)                                                                           // check left
                    return false;                                                                                                                           //
            if (kingSquare % 8 != 7)                                                                                                                        // if not on right edge
                if ((bitBoard[0] & bitBoard[2] & (one << (kingSquare + 9))) != 0)                                                                           // check right
                    return false;
        }
        for (byte x = 0; x < possibleKnightMoves[kingSquare].Length; x++)                                                                                   // check knights
            if (((bitBoard[colorWhite ? 1 : 0] & bitBoard[3]) & (one << possibleKnightMoves[kingSquare][x])) != 0)                                          // if enemy knight in possible
                return false;
        for (byte y = 0; y < 4; y++)                                                                                                                        // check bishop & queen directions
            for (byte x = 0; x < possibleSlidingMoves[kingSquare, y].Length; x++)                                                                           // loop through possible
            {
                if ((bitBoard[colorWhite ? 0 : 1] & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) break;             // if same color skip to next
                if ((bitBoard[colorWhite ? 1 : 0] & (~(bitBoard[4] | bitBoard[6])) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) break;
                if ((bitBoard[colorWhite ? 1 : 0] & (bitBoard[4] | bitBoard[6]) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) return false;      // if opposite return false
            }
        for (byte y = 4; y < 8; y++)                                                                                                                        // check rook & queen directions
            for (byte x = 0; x < possibleSlidingMoves[kingSquare, y].Length; x++)                                                                           // loop through possible
            {
                if ((bitBoard[colorWhite ? 0 : 1] & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) break;             // if same color skip to next
                if ((bitBoard[colorWhite ? 1 : 0] & (~(bitBoard[5] | bitBoard[6])) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) break;
                if ((bitBoard[colorWhite ? 1 : 0] & (bitBoard[5] | bitBoard[6]) & (one << possibleSlidingMoves[kingSquare, y][x])) != 0) return false;      // if opposite return false
            }
        for (byte y = 0; y < 8; y++)                                                                                                                        // for 8 directions
            if (possibleSlidingMoves[kingSquare, y].Length != 0 &&                                                                                          // if not on edge
                ((one << byteGameData[colorWhite ? 4 : 3]) & (one << possibleSlidingMoves[kingSquare, y][0])) != 0)                                         // if enemy king
                return false;
        return true;                                                                                                                                        // nothing found - return true
    }
    static float Evaluate(Span<ulong> bitBoard, bool whiteOnTurn)
    {
        float eval = 0f;
        sbyte signMask = (sbyte)(whiteOnTurn ? 1 : -1);
        int material = 0;
        ulong board = bitBoard[0] | bitBoard[1];

        for (byte i = 0; i < 64; i++)
        {
            if ((board & (one << i)) == 0 || i == byteGameData[3] || i == byteGameData[4]) continue;

            sbyte sign = signMask;
            byte pstSquare;

            if ((bitBoard[0] & (one << i)) != 0)
            {
                sign *= 1;
                pstSquare = i;
            }
            else
            {
                sign *= -1;
                pstSquare = (byte)(i ^ 56);
            }

            if ((bitBoard[2] & (one << i)) != 0)
            {
                material += 100;                                                    //1600
            }
            else if ((bitBoard[3] & (one << i)) != 0)
            {
                eval += sign * (Values.pieceValues[1] + Values.knightPST[pstSquare]); //640
                material += 320;
            }
            else if ((bitBoard[4] & (one << i)) != 0)
            {
                eval += sign * (Values.pieceValues[2] + Values.bishopPST[pstSquare]); //660
                material += 330;
            }
            else if ((bitBoard[5] & (one << i)) != 0)
            {
                eval += sign * (Values.pieceValues[3] + Values.rookPST[pstSquare]); // 1000
                material += 500;
            }
            else if ((bitBoard[6] & (one << i)) != 0)
            {
                eval += sign * (Values.pieceValues[4] + Values.queenPST[pstSquare]); // 1800
                material += 900;
            }
        }

        float middleGameWeight;
        float endGameWeight;
        if (material < 2000) middleGameWeight = 0;
        else middleGameWeight = material;
        if (8000f - material < 2000) endGameWeight = 0;
        else endGameWeight = 8000f - material;

        for (byte i = 0; i < 64; i++)
        {
            if ((board & (one << i)) == 0 || i == byteGameData[3] || i == byteGameData[4]) continue;

            sbyte sign = signMask;
            byte pstSquare;

            if ((bitBoard[0] & (one << i)) != 0)
            {
                sign *= 1;
                pstSquare = i;
            }
            else
            {
                sign *= -1;
                pstSquare = (byte)(i ^ 56);
            }
            eval += sign * (Values.pieceValues[0] +
                    (((Values.mg_pawnPST[pstSquare] * middleGameWeight) + (Values.eg_pawnPST[pstSquare] * endGameWeight)) / 8000f)); //1600
        }

        eval += signMask * (((Values.mg_kingPST[byteGameData[3]] * middleGameWeight) + (Values.eg_kingPST[byteGameData[3]] * endGameWeight)) / 8000f) * 10;
        eval += -signMask * (((Values.mg_kingPST[byteGameData[4] ^ 56] * middleGameWeight) + (Values.eg_kingPST[byteGameData[4] ^ 56] * endGameWeight)) / 8000f * 10);

        int kingRank = byteGameData[whiteOnTurn ? 3 : 4] % 8;
        int kingFile = (int)Math.Floor((decimal)(byteGameData[whiteOnTurn ? 3 : 4] / 8));

        int oKingRank = byteGameData[whiteOnTurn ? 4 : 3] % 8;
        int oKingFile = (int)Math.Floor((decimal)(byteGameData[whiteOnTurn ? 4 : 3] / 8));


        eval += (14 - (Math.Abs(kingRank - oKingRank) + Math.Abs(kingFile - oKingFile))) * 20 * endGameWeight / 7000f;

        return eval;
    }
}
