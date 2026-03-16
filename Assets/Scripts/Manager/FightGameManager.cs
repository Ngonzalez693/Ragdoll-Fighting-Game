using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class FightGameManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerHealth[] players;
    [SerializeField] private RagdollController[] ragdollControllers;

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject finalPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text winText;

    [Header("Start Countdown")]
    [SerializeField] private bool requireKeyToStart = true;
    [SerializeField] private KeyCode startKey = KeyCode.Space;
    [SerializeField] private float countdownStep = 1f;
    [SerializeField] private float fightTextDuration = 0.8f;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip fightMusic;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private bool stopMusicWhenFightEnds = false;

    private bool fightStarted = false;
    private bool fightEnded = false;

    private void Start()
    {
        ApplySavedMuteState();
        SetupPanels();
        SubscribeToPlayers();
        SetPlayersControlEnabled(false);

        if (playMusicOnStart)
        {
            StartFightMusic();
        }

        if (requireKeyToStart)
        {
            if (startPanel != null) startPanel.SetActive(true);
            if (instructionText != null) instructionText.text = "Press Space to Start";
        }
        else
        {
            StartCoroutine(StartFightRoutine());
        }
    }

    private void Update()
    {
        if (fightEnded) return;

        if (!fightStarted && requireKeyToStart && Input.GetKeyDown(startKey))
        {
            StartCoroutine(StartFightRoutine());
        }
    }

    private void SetupPanels()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        if (finalPanel != null)
            finalPanel.SetActive(false);
    }

    private void SubscribeToPlayers()
    {
        if (players == null) return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                players[i].OnPlayerDied += HandlePlayerDied;
            }
        }
    }

    private IEnumerator StartFightRoutine()
    {
        if (fightStarted) yield break;

        fightStarted = true;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (instructionText != null)
            instructionText.text = "3";
        yield return new WaitForSeconds(countdownStep);

        if (instructionText != null)
            instructionText.text = "2";
        yield return new WaitForSeconds(countdownStep);

        if (instructionText != null)
            instructionText.text = "1";
        yield return new WaitForSeconds(countdownStep);

        if (instructionText != null)
            instructionText.text = "FIGHT!";
        yield return new WaitForSeconds(fightTextDuration);

        if (startPanel != null)
            startPanel.SetActive(false);

        SetPlayersControlEnabled(true);
    }

    private void HandlePlayerDied(PlayerHealth deadPlayer)
    {
        if (fightEnded) return;

        CheckWinner();
    }

    private void CheckWinner()
    {
        List<PlayerHealth> alivePlayers = new List<PlayerHealth>();

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && !players[i].IsDead)
            {
                alivePlayers.Add(players[i]);
            }
        }

        if (alivePlayers.Count <= 1)
        {
            EndFight(alivePlayers.Count == 1 ? alivePlayers[0] : null);
        }
    }

    private void EndFight(PlayerHealth winner)
    {
        if (fightEnded) return;

        fightEnded = true;
        SetPlayersControlEnabled(false);

        if (stopMusicWhenFightEnds && musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        if (finalPanel != null)
            finalPanel.SetActive(true);

        if (winText != null)
        {
            if (winner != null)
                winText.text = $"Player {winner.PlayerId} Wins!";
            else
                winText.text = "Draw!";
        }
    }

    private void SetPlayersControlEnabled(bool enabledState)
    {
        if (ragdollControllers == null) return;

        for (int i = 0; i < ragdollControllers.Length; i++)
        {
            if (ragdollControllers[i] != null)
            {
                ragdollControllers[i].enabled = enabledState;
            }
        }
    }

    private void StartFightMusic()
    {
        if (musicSource == null || fightMusic == null) return;

        if (musicSource.clip != fightMusic)
        {
            musicSource.clip = fightMusic;
        }

        musicSource.loop = true;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void ApplySavedMuteState()
    {
        bool isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    private void OnDestroy()
    {
        if (players == null) return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                players[i].OnPlayerDied -= HandlePlayerDied;
            }
        }
    }

    public void FinishGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}