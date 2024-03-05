using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Threading;
using UnityEngine;

public class EventHandlerScript : MonoBehaviour
{
    bool clicked = false;
    bool reset = true;
    bool passNplay = false;
    bool playerOnTurn = false;
    public Color lightAlphaColor;
    public Color darkAlphaColor;
    public Color lightColor;
    public Color darkColor;
    public Color moveColor;
    public int[] lastMoveCords;
    public Sprite wQueen;
    public Sprite bQueen;
    public GameObject Board;
    private GameObject Tile1;
    public Camera cam;
    public GameObject PieceCopy;
    public TextMeshProUGUI text;
    public TextMeshProUGUI GOtext;
    public GameObject GameOver;
    public List<int> legalSquares;
    public AudioSource audioSource;
    public AudioClip[] audioClips = new AudioClip[4];
    static readonly byte[][] distanceToEdge = ChessBotFunctions.DistanceToEdge();
    readonly bool[,] knightLegalMoves = ChessBotFunctions.LegalKnightMoves(distanceToEdge);

    private float moveTimer;
    public GameManager gmScript;
    public CreateBoard cbScript;

    readonly sbyte[] nMoves = new sbyte[] { -17, -15, -6, 10, 17, 15, 6, -10 };
    readonly sbyte[] qkMoves = new sbyte[8] { -9, -7, 9, 7, -8, 1, 8, -1 };

    private List<string> positions = new();

    private bool[] taskRunStatus = { false, false, false };
    private bool[] taskComplationStatus = { false, false, false };
    private Thread[] threads = new Thread[3];
    private int[] move = new int[2];
    public bool unreadyFlag = true;

    bool gameOver;

    readonly byte[][] rookPos = new byte[4][] {
        new byte[] { 63, 61 },
        new byte[] { 56, 59 },
        new byte[] { 7, 5 },
        new byte[] { 0, 3 }
    };
    private void Start()
    {
        audioSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);
        gameOver = false;
        moveTimer = Time.time;
        lightColor = Functions.ConvertToColor(PlayerPrefs.GetString("LightSquares", "rgba(0.9339623, 0.8006562, 0.6746681, 1)"));
        darkColor = Functions.ConvertToColor(PlayerPrefs.GetString("DarkSquares", "rgba(0.4339623, 0.2914913, 0.108783, 1)"));
        moveColor = Functions.ConvertToColor(PlayerPrefs.GetString("MoveSquares", "rgba(0.8588235, 0.9529411, 0.317647, 0.5)"));
        if (PlayerPrefs.GetString("Bot", "Pass & Play") == "Pass & Play")
            passNplay = true;
        else
            playerOnTurn = PlayerPrefs.GetString("PieceColor", "W") == "W";

        Values.botRandomisedPick = PlayerPrefs.GetInt("BotRandom", 1) == 1;
        Values.botRandomiseMaxDelta = PlayerPrefs.GetFloat("BotDelta", 2f);
    }
    void Update()
    {
        if (unreadyFlag)
            return;
        if (gameOver)
            return;
        if(PlayerPrefs.GetString("Mode", "Player") == "Player")
        {
            if (passNplay || playerOnTurn)
                MovePlayer();
            else if (!taskRunStatus[0])
                MoveBot(PlayerPrefs.GetString("Bot", "700 ELO"), 0);
        }
        else
        {
            if(Time.time - moveTimer < 1)
                return;
            if (playerOnTurn && !taskRunStatus[1])
                MoveBot(PlayerPrefs.GetString("whiteBot", "700 ELO"), 1);
            else if ((!playerOnTurn) && !taskRunStatus[2])
                MoveBot(PlayerPrefs.GetString("blackBot", "700 ELO"), 2);
        }
    }

    private void MoveBot(string opponent, int taskIdentifier)
    {
        
        string fen = cbScript.GenerateFEN();
        if ((!taskComplationStatus[taskIdentifier]) && !taskRunStatus[taskIdentifier])
        {
            taskComplationStatus[taskIdentifier] = false;
            taskRunStatus[taskIdentifier] = true;
            threads[taskIdentifier] = new Thread(() =>
            {
                switch (opponent)
                {
                    case "700 ELO":
                        move = ChessBot_v1.GetMove(fen, positions);
                        break;
                    case "900 ELO":
                        move = ChessBot_v2.GetMove(fen, positions);
                        break;
                    case "1100 ELO":
                        move = ChessBot_v3.GetMove(fen, positions);
                        break;
                    case "1200 ELO":
                        move = ChessBot_v4.GetMove(fen, positions);
                        break;
                }


                taskComplationStatus[taskIdentifier] = true;
                taskRunStatus[taskIdentifier] = false;
            });
            threads[taskIdentifier].Start();
        }
        if (taskComplationStatus[taskIdentifier])
        {
            threads[taskIdentifier].Join();
            Tile1 = Board.transform.Find(move[0].ToString()).gameObject;
            GameObject sClickedGO = Board.transform.Find(move[1].ToString()).gameObject;
            Move(sClickedGO, move[1]);
            taskComplationStatus[taskIdentifier] = false;
            move = new int[2];
            moveTimer = Time.time;
            return;
        }
    } 

    private void MovePlayer()
    {
        if (Input.GetKey(KeyCode.Mouse0) && reset)
        {
            reset = false;
            if (clicked)
            {
                for (int i = 0; i < 64; i++)
                {
                    GameObject sClickedGO = Board.transform.Find(i.ToString()).GetComponent<ClickDetection>().ClickDetect(cam);
                    if (sClickedGO != null && legalSquares.Contains(int.Parse(sClickedGO.transform.name)))
                    {
                        Move(sClickedGO, i);
                        clicked = false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 64; i++)
                {
                    GameObject clickedGO = Board.transform.Find(i.ToString()).GetComponent<ClickDetection>().ClickDetect(cam);
                    if (clickedGO != null && clickedGO.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite != null)
                    {
                        Tile1 = clickedGO;
                        legalSquares = drawLegalSquares(false, byte.Parse(Tile1.name));
                        if (legalSquares.Count != 0)
                            clicked = true;
                        else
                            Tile1 = null;
                        break;
                    }
                    else clicked = false;
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Mouse1) && reset && Tile1 != null)
            {
                drawLegalSquares(true, byte.Parse(Tile1.name));
                colorMovedSquares(false);
                reset = false;
                clicked = false;
                Tile1 = null;
            }
        }
        if ((!Input.GetKey(KeyCode.Mouse0)) && (!Input.GetKey(KeyCode.Mouse1)))
            reset = true;
    }
    private void Move(GameObject sClickedGO, int i)
    {
        byte startingTile = byte.Parse(Tile1.name);
        byte landingTile = byte.Parse(sClickedGO.name);
        drawLegalSquares(true, byte.Parse(Tile1.name));
        colorMovedSquares(true);

        bool capture = gmScript.board[landingTile] != 6;

        gmScript.MakeMove(startingTile, landingTile);

        Tile1.transform.GetChild(0).transform.position = sClickedGO.transform.GetChild(0).transform.position;
        Tile1.transform.GetChild(0).transform.parent = sClickedGO.transform;
        Destroy(sClickedGO.transform.GetChild(0).gameObject);

        GameObject newObj = Instantiate(PieceCopy, Tile1.transform.position, Quaternion.identity, Tile1.transform);
        newObj.transform.position = new Vector3(newObj.transform.position.x, newObj.transform.position.y, -1f);

        // En Passant 

        if (i == gmScript.gameData[5] && sClickedGO.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite.name is "0" or "7")
        {
            GameObject enpTile = Board.transform.Find((gmScript.gameData[0] == 1 ? (i - 8) : (i + 8)).ToString()).gameObject;
            Destroy(enpTile.transform.GetChild(0).gameObject);
            GameObject enPObj = Instantiate(PieceCopy, enpTile.transform.position, Quaternion.identity, enpTile.transform);
            enPObj.transform.position = new Vector3(enPObj.transform.position.x, enPObj.transform.position.y, -1f);
        }

        // En Passant
        // Castling

        for (byte cnt = 0; cnt < 4; cnt++)
        {
            if (gmScript.castlingInfo[cnt])
            {
                MoveRook(rookPos[cnt][0], rookPos[cnt][1]);
                gmScript.castlingInfo[cnt] = false;
            }
        }

        // Castling
        // Promotion

        if (landingTile < 8 && gmScript.board[landingTile] == 0)
        {
            GameObject landingObj = Board.transform.Find(landingTile.ToString()).gameObject;
            landingObj.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = wQueen;
            gmScript.board[landingTile] = 4;
        }
        if (landingTile > 55 && gmScript.board[landingTile] == 7)
        {
            GameObject landingObj = Board.transform.Find(landingTile.ToString()).gameObject;
            landingObj.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = bQueen;
            gmScript.board[landingTile] = 11;
        }

        // Promotion

        gmScript.UpdateData();
        playerOnTurn = !playerOnTurn;
        Tile1 = null;
        List<string> FEN = cbScript.GenerateFEN().Split(" ").ToList();
        FEN.Insert(1, "\n");
        text.text = string.Join(" ", FEN);

        int gameStatus = GameStatusCheck(gmScript.gameData[0] == 0, FEN);

        if (gameStatus != 0)
        {
            if(gameStatus == 1)
            {
                GOtext.text = "The Game has ended in a draw.";
                GameOver.SetActive(true);
                gameOver = true;
            }
            if(gameStatus == 2)
            {
                GOtext.text = (gmScript.gameData[0] == 0 ? "Black" : "White") + " has won the game.";
                GameOver.SetActive(true);
                gameOver = true;
            }
        }
        lastMoveCords = new int[] { startingTile, landingTile };
        colorMovedSquares(false);

        if (gameOver)
            audioSource.clip = audioClips[3];
        else if (InCheck((byte)gmScript.kingPositions[playerOnTurn ? 1 : 0], 0, 0, gmScript.kingPositions[playerOnTurn ? 1 : 0], false))
            audioSource.clip = audioClips[2];
        else if(capture)
            audioSource.clip = audioClips[1];
        else
            audioSource.clip = audioClips[0];
        audioSource.Play();
    }
    public int GameStatusCheck(bool white, List<string> FEN)
    {
        int result = 0;
        int cnt = 0;
        int whitePieces = 0;
        int blackPieces = 0;

        for (byte i = 0; i < gmScript.board.Count; i++)
        {
            if (gmScript.board[i] < 6)
            {
                if(white)
                    cnt += drawLegalSquares(true, i).Count;
                whitePieces++;
            }

            if (gmScript.board[i] > 6)
            {
                if(!white)
                    cnt += drawLegalSquares(true, i).Count;
                blackPieces++;
            }

        }

        if (cnt == 0)
        {
            byte kingPosition = (byte)gmScript.kingPositions[white ? 0 : 1];
            if (InCheck(kingPosition, 0, 0, kingPosition, false))
                return 2;
            else 
                return 1;
        }
        else
        {
            if (gmScript.gameData[6] >= 50)
                return 1;
            positions.Add(FEN[0]);
            if (positions.Count(x => x == FEN[0]) == 3)
                return 1;

            if(gmScript.board.Count(x => x % 7 == 0) == 0)
            {
                int whiteKnights = gmScript.board.Count(x => x == 1);
                int whiteBishops = gmScript.board.Count(x => x == 2);
                int whiteRooks = gmScript.board.Count(x => x == 3);
                int whiteQueens = gmScript.board.Count(x => x == 4);
                int blackKnights = gmScript.board.Count(x => x == 1);
                int blackBishops = gmScript.board.Count(x => x == 2);
                int blackRooks = gmScript.board.Count(x => x == 3);
                int blackQueens = gmScript.board.Count(x => x == 4);

                if (whiteRooks + whiteQueens + blackRooks + blackQueens != 0)
                    return 0;
                
                if(whitePieces == 0)
                {
                    if (blackBishops == 0 && blackKnights <= 2)
                        return 1;
                    if (blackBishops == 1 && blackKnights == 0)
                        return 1;
                }
                else if (blackPieces == 0)
                {
                    if (whiteBishops == 0 && whiteKnights <= 2)
                        return 1;
                    if (whiteBishops == 1 && whiteKnights == 0)
                        return 1;
                }
            }
        }
        return result;
    }
    public void MoveRook(byte orgSquare, byte newSquare)
    {
        GameObject rookOrg = Board.transform.Find(orgSquare.ToString()).gameObject;
        GameObject rookNew = Board.transform.Find(newSquare.ToString()).gameObject;

        GameObject childOrg = rookOrg.transform.GetChild(0).gameObject;
        GameObject childNew = rookNew.transform.GetChild(0).gameObject;

        childOrg.transform.parent = rookNew.transform;
        childNew.transform.parent = rookOrg.transform;

        childOrg.transform.position = new Vector3(rookNew.transform.position.x, rookNew.transform.position.y, -5f);
        childNew.transform.position = new Vector3(rookOrg.transform.position.x, rookOrg.transform.position.y, -5f);
    }

    public void colorMovedSquares(bool clear)
    {
        if(lastMoveCords.Length != 0)
        {
            Board.transform.Find(lastMoveCords[0].ToString()).GetComponent<SpriteRenderer>().color = clear ?
                                Mathf.FloorToInt(lastMoveCords[0] / 8) % 2 == 0 ?
                                        lastMoveCords[0] % 2 == 1 ?
                                            darkColor : lightColor
                                        : lastMoveCords[0] % 2 == 1 ?
                                            lightColor : darkColor
                               : moveColor;

            Board.transform.Find(lastMoveCords[1].ToString()).GetComponent<SpriteRenderer>().color = clear ?
                                Mathf.FloorToInt(lastMoveCords[1] / 8) % 2 == 0 ?
                                        lastMoveCords[1] % 2 == 1 ?
                                            darkColor : lightColor
                                        : lastMoveCords[1] % 2 == 1 ?
                                            lightColor : darkColor
                               : moveColor;
        }
    }
        public List<int> drawLegalSquares(bool clear, byte startingSquare)
    {
        List<int> legalMoves = new();
        switch (Board.transform.GetChild(startingSquare).transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name)
        {
            case "0":
                if (gmScript.gameData[0] == 1)
                    break;
                if (gmScript.board[startingSquare - 8] == 6)   // up 1
                {
                    AddSquare(ref legalMoves, startingSquare - 8, startingSquare, clear, gmScript.kingPositions[0]);
                    if (distanceToEdge[startingSquare][4] != 1 && gmScript.board[startingSquare - 16] == 6 && startingSquare > 47)   // up 2
                        AddSquare(ref legalMoves, startingSquare - 16, startingSquare, clear, gmScript.kingPositions[0]);
                    
                }
                if (distanceToEdge[startingSquare][5] != 0
                    && (gmScript.board[startingSquare - 7] > 6 || startingSquare - 7 == gmScript.gameData[5])) //right
                    AddSquare(ref legalMoves, startingSquare - 7, startingSquare, clear, gmScript.kingPositions[0]);
                if (distanceToEdge[startingSquare][7] != 0
                    && (gmScript.board[startingSquare - 9] > 6 || startingSquare - 9 == gmScript.gameData[5])) //left
                    AddSquare(ref legalMoves, startingSquare - 9, startingSquare, clear, gmScript.kingPositions[0]);
                break;
            case "1":
                if (gmScript.gameData[0] == 1)
                    break;
                for (byte x = 0; x < 8; x++)
                {
                    byte testTile = (byte)(startingSquare + nMoves[x]);
                    if (knightLegalMoves[startingSquare, x] && (gmScript.board[testTile] > 5))
                        AddSquare(ref legalMoves, testTile, startingSquare, clear, gmScript.kingPositions[0]);
                }
                break;
            case "2":
                if (gmScript.gameData[0] == 1)
                    break;
                SlidingPieceSearch(0, 4, startingSquare, ref legalMoves, clear, true);
                break;
            case "3":
                if (gmScript.gameData[0] == 1)
                    break;
                SlidingPieceSearch(4, 8, startingSquare, ref legalMoves, clear, true);
                break;
            case "4":
                if (gmScript.gameData[0] == 1)
                    break;
                SlidingPieceSearch(0, 8, startingSquare, ref legalMoves, clear, true);
                break;
            case "5":
                if (gmScript.gameData[0] == 1)
                    break;
                if (!InCheck(60, 60, 60, 60, false))
                {
                    if (gmScript.gameData[1] == 1 && gmScript.board[62] == 6 && gmScript.board[61] == 6 && (!InCheck(61, 60, 61, 61, true)) && (!InCheck(62, 60, 62, 62, true)))
                        AddSquare(ref legalMoves, 62, startingSquare, clear, 62);
                    if (gmScript.gameData[2] == 1 && gmScript.board[59] == 6 && gmScript.board[58] == 6 && gmScript.board[57] == 6 && (!InCheck(59, 60, 59, 59, true)) && (!InCheck(58, 60, 58, 58, true)) && (!InCheck(57, 60, 57, 57, true)))
                        AddSquare(ref legalMoves, 58, startingSquare, clear, 58);
                }
                for (byte x = 0; x < 8; x++)
                {
                    sbyte testTile = (sbyte)(startingSquare + qkMoves[x]);
                    if (testTile >= 0 && testTile < 64 && gmScript.board[testTile] > 5
                        && (!(distanceToEdge[startingSquare][5] == 0 && x is 1 or 2 or 5))
                        && (!(distanceToEdge[startingSquare][7] == 0 && x is 0 or 3 or 7)))
                        AddSquare(ref legalMoves, testTile, startingSquare, clear, testTile);
                }
                break;
            case "7":
                if (gmScript.gameData[0] == 0)
                    break;
                if (gmScript.board[startingSquare + 8] == 6)   // down 1
                {
                    AddSquare(ref legalMoves, startingSquare + 8, startingSquare, clear, gmScript.kingPositions[1]);
                    if (distanceToEdge[startingSquare][6] != 1 && gmScript.board[startingSquare + 16] == 6 && startingSquare < 16)   // down 2
                        AddSquare(ref legalMoves, startingSquare + 16, startingSquare, clear, gmScript.kingPositions[1]);
                }
                if (distanceToEdge[startingSquare][5] != 0
                    && (gmScript.board[startingSquare + 9] < 6 || startingSquare + 9 == gmScript.gameData[5])) //right
                    AddSquare(ref legalMoves, startingSquare + 9, startingSquare, clear, gmScript.kingPositions[1]);
                if (distanceToEdge[startingSquare][7] != 0
                    && (gmScript.board[startingSquare + 7] < 6 || startingSquare + 7 == gmScript.gameData[5])) //left
                    AddSquare(ref legalMoves, startingSquare + 7, startingSquare, clear, gmScript.kingPositions[1]);
                break;
            case "8":
                if (gmScript.gameData[0] == 0)
                    break;
                for (byte x = 0; x < 8; x++)
                {
                    byte testTile = (byte)(startingSquare + nMoves[x]);
                    if (knightLegalMoves[startingSquare, x] && (gmScript.board[testTile] < 7))
                        AddSquare(ref legalMoves, testTile, startingSquare, clear, gmScript.kingPositions[1]);
                }
                break;
            case "9":
                if (gmScript.gameData[0] == 0)
                    break;
                SlidingPieceSearch(0, 4, startingSquare, ref legalMoves, clear, false);
                break;
            case "10":
                if (gmScript.gameData[0] == 0)
                    break;
                SlidingPieceSearch(4, 8, startingSquare, ref legalMoves, clear, false);
                break;
            case "11":
                if (gmScript.gameData[0] == 0)
                    break;
                SlidingPieceSearch(0, 8, startingSquare, ref legalMoves, clear, false);
                break;
            case "12":
                if (gmScript.gameData[0] == 0)
                    break;
                if(!InCheck(4, 4, 4, 4, false))
                {
                    if (gmScript.gameData[3] == 1 && gmScript.board[6] == 6 && gmScript.board[5] == 6 && (!InCheck(5, 4, 5, 5, true)) && (!InCheck(6, 4, 6, 6, true)))
                        AddSquare(ref legalMoves, 6, startingSquare, clear, 6);
                    if (gmScript.gameData[4] == 1 && gmScript.board[3] == 6 && gmScript.board[2] == 6 && gmScript.board[1] == 6 && (!InCheck(3, 4, 3, 3, true)) && (!InCheck(2, 4, 2, 2, true)) && (!InCheck(1, 4, 1, 1, true)))
                        AddSquare(ref legalMoves, 2, startingSquare, clear, 2);
                }
                for (byte x = 0; x < 8; x++)
                {
                    sbyte testTile = (sbyte)(startingSquare + qkMoves[x]);
                    if (testTile >= 0 && testTile < 64 && gmScript.board[testTile] < 7
                        && (!(distanceToEdge[startingSquare][5] == 0 && x is 1 or 2 or 5))
                        && (!(distanceToEdge[startingSquare][7] == 0 && x is 0 or 3 or 7)))
                        AddSquare(ref legalMoves, testTile, startingSquare, clear, testTile);
                }
                break;
        }
        return legalMoves;
    }

    void SlidingPieceSearch(byte startDirection, byte endDirection, byte startingTile, ref List<int> legalMoves, bool clear, bool white)
    {
        for (byte x = startDirection; x < endDirection; x++)
        {
            byte testTile = startingTile;
            for (byte y = 0; y < distanceToEdge[startingTile][x]; y++)
            {
                testTile = (byte)(testTile + qkMoves[x]);
                if (white ? gmScript.board[testTile] < 6 : gmScript.board[testTile] > 6)
                    break;
                AddSquare(ref legalMoves, testTile, startingTile, clear, white ? gmScript.kingPositions[0] : gmScript.kingPositions[1]);
                if (gmScript.board[testTile] != 6)
                    break;
            }
        }
    }
    void AddSquare(ref List<int> addList, int tile, int origin, bool clear, int kingSquare)
    {
        if (!InCheck((byte) kingSquare, origin, tile, kingSquare, true))
        {
            addList.Add(tile);
            ColorSquare(clear, tile);
        }
    }
    void ColorSquare(bool clear, int coloredSquare)
    {
        Board.transform.Find(coloredSquare.ToString()).GetComponent<SpriteRenderer>().color = clear ?
                                 Mathf.FloorToInt(coloredSquare / 8) % 2 == 0 ?
                                    coloredSquare % 2 == 1 ?
                                        darkColor : lightColor
                                    : coloredSquare % 2 == 1 ?
                                        lightColor : darkColor
                                : (Mathf.FloorToInt(coloredSquare) / 8) % 2 == 0 ?
                                    coloredSquare % 2 == 1 ?
                                        darkAlphaColor : lightAlphaColor
                                    : coloredSquare % 2 == 1 ?
                                        lightAlphaColor : darkAlphaColor;
    }

    bool InCheck(byte squareToCheck, int origin, int tile, int kingSquare, bool moveAhead)
    {
        byte[] testBoard = gmScript.board.ToArray();
        if (moveAhead)
        {
            testBoard[tile] = testBoard[origin];
            testBoard[origin] = 6;

            if (gmScript.gameData[5] == tile)
                if (testBoard[kingSquare] == 5) testBoard[tile + 8] = 6;
                else if (testBoard[kingSquare] == 12) testBoard[tile - 8] = 6;
        }

        // pawns

        if (testBoard[kingSquare] == 5)
            if ((distanceToEdge[squareToCheck][1] != 0 && testBoard[squareToCheck - 7] == 7)
            || (distanceToEdge[squareToCheck][0] != 0 && testBoard[squareToCheck - 9] == 7))
                return true;
        if (testBoard[kingSquare] == 12)
        {
            if ((distanceToEdge[squareToCheck][3] != 0 && testBoard[squareToCheck + 7] == 0)
                        || (distanceToEdge[squareToCheck][2] != 0 && testBoard[squareToCheck + 9] == 0))
                            return true;
        }
            

        // knights 

        for (byte x = 0; x < 8; x++)
        {
            byte testTile = (byte)(squareToCheck + nMoves[x]);
            if (knightLegalMoves[squareToCheck, x] &&
                ((testBoard[kingSquare] == 5 && testBoard[testTile] == 8)
                || (testBoard[kingSquare] == 12 && testBoard[testTile] == 1)))
                return true;
        }


        // sliding pieces

        for (byte x = 0; x < 8; x++)
        {
            byte testTile = squareToCheck;
            for (byte y = 0; y < distanceToEdge[squareToCheck][x]; y++)
            {
                testTile = (byte)(testTile + qkMoves[x]);
                if (x < 4 && ((testBoard[testTile] is 2 or 4 && testBoard[kingSquare] == 12) 
                    || (testBoard[testTile] is 9 or 11 && testBoard[kingSquare] == 5)
                    || (testBoard[testTile] is 5 && testBoard[kingSquare] == 12 && y == 0)
                    || (testBoard[testTile] is 12 && testBoard[kingSquare] == 5 && y == 0)))
                    return true;
                if (x > 3 && ((testBoard[testTile] is 3 or 4 && testBoard[kingSquare] == 12) 
                    || (testBoard[testTile] is 10 or 11 && testBoard[kingSquare] == 5)
                    || (testBoard[testTile] is 5 && testBoard[kingSquare] == 12 && y == 0)
                    || (testBoard[testTile] is 12 && testBoard[kingSquare] == 5 && y == 0)))
                    return true;
                if (testBoard[testTile] != 6) break;
            }
        }



        return false;
    }
}

