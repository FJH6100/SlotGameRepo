using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    [Range(3, 5)]
    private int boardHeight;

    [SerializeField]
    [Range(3, 7)]
    private int boardWidth;

    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private GameObject[] gamePieces;
    [SerializeField]
    private GameObject slot;

    private AudioSource _audio;
    private GameObject[,] _gameBoard;
    private GameObject _board;
    private List<GameObject> _matchLines;
    private int _score = 0;
    private Vector3 _offset = new Vector3(0, 0, -1);
    // Start is called before the first frame update
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _board = GameObject.Find("GameBoard");
        _gameBoard = new GameObject[boardHeight, boardWidth];
        _matchLines = new List<GameObject>();
        int heightLimit = boardHeight - 1;
        int widthLimit = boardWidth - 1;
        int heightIndex = 0;
        int widthIndex = 0;
        for (int i = heightLimit; i >= -heightLimit; i -= 2)
        {
            for (int j = -widthLimit; j <= widthLimit; j += 2)
            {
                GameObject thisSlot = Instantiate(slot, new Vector3(j, i, 0), Quaternion.identity);
                thisSlot.name = heightIndex + " " + widthIndex;
                thisSlot.transform.parent = _board.transform;
                widthIndex++;
            }
            heightIndex++;
            widthIndex = 0;
        }
    }

    public void Spin()
    {
        if (_audio != null)
            _audio.Play();
        //Clear lines from previous spin
        foreach (GameObject l in _matchLines)
        {
            GameObject.Destroy(l);
        }
        _matchLines.Clear();
        //Destroy previous piece and assign new piece to each slot
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                GameObject gridPosition = _board.transform.Find(i + " " + j).gameObject;
                if (gridPosition.transform.childCount > 0)
                {
                    GameObject destroyPiece = gridPosition.transform.GetChild(0).gameObject;
                    Destroy(destroyPiece);
                }
                GameObject pieceType = gamePieces[Random.Range(0, gamePieces.Length)];
                GameObject thisPiece = Instantiate(pieceType, gridPosition.transform.position + _offset, Quaternion.identity);
                thisPiece.name = pieceType.name;
                thisPiece.transform.parent = gridPosition.transform;
                _gameBoard[i, j] = thisPiece;
            }
        }
        CheckForMatches();
    }

    private void CheckForMatches()
    {
        //Vertical Matches
        for (int i = 0; i < boardWidth; i++)
        {
            int matchLength = 1;
            GameObject matchBegin = _gameBoard[0, i];
            GameObject matchEnd = null;
            for (int j = 0; j < boardHeight - 1; j++)
            {
                if (_gameBoard[j, i].name == _gameBoard[j + 1, i].name)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        matchEnd = _gameBoard[j, i];
                        _score += 10 * (matchLength - 2);
                        DrawLine(matchBegin.transform.position + _offset, matchEnd.transform.position + _offset);
                    }
                    matchBegin = _gameBoard[j + 1, i];
                    matchLength = 1;
                }
            }
            if (matchLength >= 3)
            {
                matchEnd = _gameBoard[boardHeight - 1, i];
                _score += 10 * (matchLength - 2);
                DrawLine(matchBegin.transform.position + _offset, matchEnd.transform.position + _offset);
            }
        }
        //Horizontal Matches
        for (int i = 0; i < boardHeight; i++)
        {
            int matchLength = 1;
            GameObject matchBegin = _gameBoard[i, 0];
            GameObject matchEnd = null;
            for (int j = 0; j < boardWidth - 1; j++)
            {
                if (_gameBoard[i, j].name == _gameBoard[i, j + 1].name)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        matchEnd = _gameBoard[i, j];
                        _score += 10 * (matchLength - 2);
                        DrawLine(matchBegin.transform.position + _offset, matchEnd.transform.position + _offset);
                    }
                    matchBegin = _gameBoard[i, j + 1];
                    matchLength = 1;
                }
            }
            if (matchLength >= 3)
            {
                matchEnd = _gameBoard[i, boardWidth - 1];
                _score += 10 * (matchLength - 2);
                DrawLine(matchBegin.transform.position + _offset, matchEnd.transform.position + _offset);
            }
        }
        //X in a row/only if width is greater than 3
        if (boardWidth > 3)
        {
            List<GameObject> points = new List<GameObject>();
            List<string> names = new List<string>();
            for (int i = 0; i < boardHeight; i++)
            {
                points.Clear();
                GameObject startPoint = _gameBoard[i, 0];
                if (names.Contains(startPoint.name))
                    continue;
                else
                    names.Add(startPoint.name);
                points.Add(startPoint);
                for (int j = 1; j < boardWidth; j++)
                {
                    bool notFound = false;
                    for (int k = 0; k < boardHeight; k++)
                    {
                        if (startPoint.name == _gameBoard[k, j].name)
                        {
                            points.Add(_gameBoard[k, j]);
                            break;
                        }
                        if (k == boardHeight - 1)
                            notFound = true;
                    }
                    if (notFound)
                        break;
                }
                if (points.Count == boardWidth)
                {
                    GameObject myLine = new GameObject();
                    myLine.transform.position = startPoint.transform.position + _offset;
                    myLine.AddComponent<LineRenderer>();
                    LineRenderer lr = myLine.GetComponent<LineRenderer>();
                    lr.positionCount = boardWidth;
                    lr.startWidth = .1f;
                    lr.endWidth = .1f;
                    for (int a = 0; a < points.Count; a++)
                        lr.SetPosition(a, points[a].transform.position + _offset);
                    _matchLines.Add(myLine);
                    _score += boardWidth * 10;
                }
            }
        }
        scoreText.text = _score.ToString();
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.startWidth = .1f;
        lr.endWidth = .1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        _matchLines.Add(myLine);
    }
}