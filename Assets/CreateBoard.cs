using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Piece
{
    public Sprite img;
}
public class CreateBoard : MonoBehaviour
{
    public GameObject testPrefab1;
    public GameObject testPrefab2;
    public Color lightColor;
    public Color darkColor;
    public List<Piece> pieces = new();
    public GameObject tile;
    public GameObject piece;
    public EventHandlerScript eventHandler;
    public string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0";
    public GameManager gmScript;

    void Start()
    {
        lightColor = Functions.ConvertToColor(PlayerPrefs.GetString("LightSquares", "rgba(0.9339623, 0.8006562, 0.6746681, 1)"));
        darkColor = Functions.ConvertToColor(PlayerPrefs.GetString("DarkSquares", "rgba(0.4339623, 0.2914913, 0.108783, 1)"));
        if (PlayerPrefs.GetString("FEN", "") != "")
            startingFEN = PlayerPrefs.GetString("FEN", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0");
        gmScript.gameStart(startingFEN);
    }
    public void GenerateBoard()
    {
        int cnt = 0;
        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                GameObject tileObject = Instantiate(tile, new Vector3(file - 3.5f - 10f, rank - 3.5f, 0), Quaternion.identity);
                tileObject.GetComponent<SpriteRenderer>().color = (rank + file) % 2 == 1 ? lightColor : darkColor;
                tileObject.transform.parent = gameObject.transform;
                tileObject.transform.name = cnt.ToString();
                Vector3 tileTransform = tileObject.transform.position;

                GameObject pieceObject = Instantiate(piece, new Vector3(tileTransform.x, tileTransform.y, tileTransform.z - 1), Quaternion.identity);
                pieceObject.GetComponent<SpriteRenderer>().sprite = pieces[gmScript.board[cnt]].img;
                pieceObject.transform.parent = tileObject.transform;

                cnt++;

                if (startingFEN == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0")
                {
                    if (PlayerPrefs.GetString("PieceColor", "W") == "B")
                        pieceObject.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
                    else if (PlayerPrefs.GetString("PieceColor", "W") == "R")
                    {
                        var rand = new System.Random();
                        string choice = "WB"[rand.Next(0, 2)].ToString();
                        PlayerPrefs.SetString("PieceColor", choice);
                        if (choice == "B")
                            pieceObject.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
                    }
                }
                else if (startingFEN.Split(" ")[1] == "b")
                    pieceObject.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
            }
        }
        if (startingFEN == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0")
        {
            if (PlayerPrefs.GetString("PieceColor", "W") == "B")
            {
                gameObject.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
                gameObject.transform.position = new Vector3(-16.5f, 4f, 1f);
            }
        }
        else
        {
            if (startingFEN.Split(" ")[1] == "b")
            {
                gameObject.transform.rotation = new Quaternion(0f, 0f, 180f, 0f);
                gameObject.transform.position = new Vector3(-16.5f, 4f, 1f);
            }
        }
        List<string> FEN = startingFEN.Split(" ").ToList();
        FEN.Insert(1, "\n");
        eventHandler.GetComponent<EventHandlerScript>().text.text = string.Join(" ", FEN);
        eventHandler.unreadyFlag = false;
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
            {'k', 12}
        };
    public string GenerateFEN()
    {
        string FEN = "";
        int tileCount = 0;
        for (int i = 0; i < 64; i++)
        {
            if (gmScript.board[i] == 6)
                tileCount++;
            else
            {
                FEN += (tileCount == 0 ? "" : tileCount.ToString()) + Functions.pieceCodes[gmScript.board[i]];
                tileCount = 0;
            }
            if (i % 8 == 7)
            {
                FEN += (tileCount == 0 ? "" : tileCount) + (i != 63 ? "/" : "");
                tileCount = 0;
            }
        }
        return FEN += " " + (gmScript.gameData[0] == 0 ? "w" : "b") + " "
            + CastlingRights() + " "
            + Functions.NumToTile(gmScript.gameData[5]) + " "
            + gmScript.gameData[6] + " "
            + gmScript.gameData[7];
    }
    string CastlingRights()
    {
        string cr = "";
        if (gmScript.gameData[1] == 1) cr += "K";
        if (gmScript.gameData[2] == 1) cr += "Q";
        if (gmScript.gameData[3] == 1) cr += "k";
        if (gmScript.gameData[4] == 1) cr += "q";
        if (cr == "")
            return "-";
        else return cr;
    }

    public void CopyFEN()
    {
        TextEditor te = new TextEditor();
        List<string> FEN = GenerateFEN().Split(" ").ToList();
        te.text = string.Join(" ", FEN);
        te.SelectAll();
        te.Copy();
    }
}
