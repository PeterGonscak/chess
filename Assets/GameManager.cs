using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CreateBoard cbScript;
    public EventHandlerScript ehScript;
    public List<byte> board;
    public int[] gameData = new int[8];
    public int[] kingPositions = new int[] {60, 4};
    public bool[] castlingInfo = { false, false, false, false };
    public int lastENP;

    public void gameStart(string FEN)
    {
        string[] divFEN = FEN.Split(" ");              // 0-pos FEN / 1-onTurn / 2-castle Rights / 3- enP / 4- halfC / 5- fullC
        gameData[0] = divFEN[1] == "w" ? 0 : 1;
        gameData[1] = divFEN[2].Contains('k') ? 1 : 0;
        gameData[2] = divFEN[2].Contains('q') ? 1 : 0;
        gameData[3] = divFEN[2].Contains('K') ? 1 : 0;
        gameData[4] = divFEN[2].Contains('Q') ? 1 : 0;
        gameData[5] = Functions.TileToNum(divFEN[3]);
        gameData[6] = int.Parse(divFEN[4]);
        gameData[7] = int.Parse(divFEN[5].Substring(0,1));
        foreach (string row in divFEN[0].Split("/"))
            foreach (char c in row)
                if ("12345678".Contains(c))
                    for (int z = 0; z < int.Parse(c.ToString()); z++)
                        board.Add(6);
                else
                    board.Add(pieceToNum[c]);
        kingPositions[0] = board.FindIndex(x => x == 5);
        kingPositions[1] = board.FindIndex(x => x == 12);
    }
    public void MakeMove(int startingSquare, int landingSquare)
    {
        if (board[startingSquare] == 0 || board[startingSquare] == 7 || board[landingSquare] != 6)
            gameData[6] = 0;
        else gameData[6]++;
        if (board[startingSquare] > 6) gameData[7]++;
        if (board[startingSquare] == 5) kingPositions[0] = landingSquare;
        if (board[startingSquare] == 12) kingPositions[1] = landingSquare;
        if (board[startingSquare] == 0 && startingSquare - landingSquare == 16 &&  startingSquare > 47)
            gameData[5] = landingSquare + 8;
        if (board[startingSquare] == 7 && startingSquare - landingSquare == -16 && startingSquare < 16)
            gameData[5] = landingSquare - 8;
        board[landingSquare] = board[startingSquare];
        board[startingSquare] = 6;
        if (landingSquare == gameData[5])
        {
            if (board[landingSquare] == 0) board[landingSquare + 8] = 6;
            else if (board[landingSquare] == 7) board[landingSquare - 8] = 6;
        }
        if (board[landingSquare] == 5)
        {
            gameData[1] = 0;
            gameData[2] = 0;

            if (landingSquare == startingSquare - 2) //cs tu matex lol
            {
                board[59] = 3;
                board[56] = 6;
                castlingInfo[1] = true;
            }
            else if (landingSquare == startingSquare + 2)
            {
                board[61] = 3;
                board[63] = 6;
                castlingInfo[0] = true;
            }
        }
        else if (board[landingSquare] == 12)
        {
            gameData[3] = 0;
            gameData[4] = 0;

            if (landingSquare == startingSquare - 2)
            {
                board[3] = 10;
                board[0] = 6;
                castlingInfo[3] = true;
            }
            else if (landingSquare == startingSquare + 2)
            {
                board[5] = 10;
                board[7] = 6;
                castlingInfo[2] = true;
            }
        }
        if (landingSquare == 0 || startingSquare == 0)
            gameData[4] = 0;
        if (landingSquare == 7 || startingSquare == 7)
            gameData[3] = 0;
        if (landingSquare == 56 || startingSquare == 56)
            gameData[2] = 0;
        if (landingSquare == 63 || startingSquare == 63)
            gameData[1] = 0;
    }
    public void UpdateData()
    {
        gameData[0] = 1 - gameData[0];                                              // change onTurn
        if (gameData[5] == lastENP)                                                 // enP reset
            gameData[5] = -1;
        lastENP = gameData[5];                                                      // enP set
    }
    private readonly Dictionary<char, byte> pieceToNum = new()
        {
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
            {'k', 12}};
}
