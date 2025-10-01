using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeNextLevel = 2f;
    [SerializeField] private GameObject YouWinUI;
    [SerializeField] private AudioClip victorySound;

    private bool levelCleared = false;
    private AudioSource audioSource;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {

        if (levelCleared) return;
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

        if (balls.Length == 0)
        {
            levelCleared = true;
            Invoke(nameof(LoadNextLevel), delayBeforeNextLevel);
        }
    }

    void LoadNextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        // Se è l'ultima scena, torna al menu o mostra schermata vittoria
        if (currentScene  >= SceneManager.sceneCountInBuildSettings-1)
        {
            StartCoroutine(HandleVictorySequence());
        }
        else
        {
            SceneManager.LoadScene(currentScene + 1);
        }
    }
    IEnumerator HandleVictorySequence()
    {
        // Mostra la UI "You Win"
        if (YouWinUI != null)
            YouWinUI.SetActive(true);

        // Ferma il movimento del personaggio
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false; // Disattiva lo script
            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
                anim.Play("Victory"); // L'animazione deve esistere nel tuo Animator
            audioSource.PlayOneShot(victorySound);
        }

        // Attendi 5 secondi
        yield return new WaitForSeconds(5f);

        // Carica il menu principale
        SceneManager.LoadScene("MainMenu");
    }

}
