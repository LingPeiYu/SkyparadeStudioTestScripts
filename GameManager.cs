using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    public GameObject[] playableObject;
    public GameObject startUI;
    public GameObject loadChooseUI;
    public GameObject scoreCountUI;
    public GameObject restartUI;
    public GameObject player1WinUI;
    public GameObject player2WinUI;
    public GameObject[] player1ScoreShower;
    public GameObject[] player2ScoreShower;

    public BallInitialize ballInitialize;
    public PlayerInitialize playerInitialize;//从第二回合玩家发生移动后才需要重置
    public EnamyAI enamyAI;//调用初始化函数重置，同样从第二回合开始

    public float pauseTime;

    private int player1Score = 0;
    private int player2Score = 0;
    private int turnCount = 0;

    public void StartGame()
    {
        startUI.SetActive(false);
        ContinueGame();
        //检测是否保存过游戏,保存过则询问玩家是否继续上次进度，如果玩家选择继续则加载游戏
        if (PlayerPrefs.GetInt("Saved",0)==1)
        {
            loadChooseUI.SetActive(true);
        }
        else
        {
            foreach (GameObject gob in playableObject)
                gob.SetActive(true);
            scoreCountUI.SetActive(true);
            StartCoroutine(ballInitialize.InitializeWithDelay(turnCount));
        }
    }

    public void RestartGame()
    {
        ContinueGame();
        restartUI.SetActive(false);
        player1WinUI.SetActive(false);
        player2WinUI.SetActive(false);
        player1Score = player2Score = 0;
        foreach (GameObject scoreShower in player1ScoreShower) scoreShower.SetActive(false);
        foreach (GameObject scoreShower in player2ScoreShower) scoreShower.SetActive(false);
        NewTurn();
    }

    public void CountScore(int numOfGetScorePlayer)
    {
        if (numOfGetScorePlayer == 1)
        {
            player1Score++;
            player1ScoreShower[player1Score - 1].SetActive(true);
        }
        else if (numOfGetScorePlayer == 2)
        {
            player2Score++;
            player2ScoreShower[player2Score - 1].SetActive(true);
        }

        //检查是否有人获胜
        if (player1Score == 5 || player2Score == 5)
            Win(numOfGetScorePlayer);
        else
        {
            StartCoroutine(Pause());
            NewTurn();
        }
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("Saved",1);//键值Saved保存是否保存过游戏
        PlayerPrefs.SetInt("Player1Score", player1Score);
        PlayerPrefs.SetInt("Player2Score", player2Score);
        PlayerPrefs.SetInt("TurnCount", turnCount);
    }

    public void LoadGame()
    {
        player1Score = PlayerPrefs.GetInt("Player1Score", 0);
        player2Score = PlayerPrefs.GetInt("Player2Score", 0);
        turnCount = PlayerPrefs.GetInt("TurnCount", 0);
        for (int i = 0; i < player1Score; i++)
            player1ScoreShower[i].SetActive(true);
        for (int i = 0; i < player2Score; i++)
            player2ScoreShower[i].SetActive(true);
    }

    public void LoadDecision(int choice)
    {
        foreach (GameObject gob in playableObject)
            gob.SetActive(true);
        scoreCountUI.SetActive(true);
        loadChooseUI.SetActive(false);
        //根据choice决定是否load game(0否1是)
        if (choice == 1)
            LoadGame();
        StartCoroutine(ballInitialize.InitializeWithDelay(turnCount));
    }

    private IEnumerator Pause()
    {
        StopGame();
        yield return new WaitForSecondsRealtime(pauseTime);
        ContinueGame();
    }

    private void Win(int numOfWinner)
    {
        StopGame();
        if(numOfWinner==1)
        {
            player1WinUI.SetActive(true);
        }
        else if (numOfWinner == 2)
        {
            player2WinUI.SetActive(true);
        }
        restartUI.SetActive(true);
    }

    private void NewTurn()
    {
        SaveGame();
        turnCount++;
        StartCoroutine(ballInitialize.InitializeWithDelay(turnCount));
        playerInitialize.Initialize();
        enamyAI.InitializeState();
    }

    private void StopGame()
    {
        Time.timeScale = 0;
    }

    private void ContinueGame()
    {
        Time.timeScale = 1;
    }

    private void Awake()
    {
        _instance = this;
        StopGame();
    }
}
