using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerCatchTrigger : MonoBehaviour
{
    [Header("UI da Transição")]
    public CanvasGroup fadeCanvas;
    public TextMeshProUGUI transitionText;
    public string customCaughtText = "The killer caught you.";

    [Header("Tempos")]
    public float fadeSpeed = 1.0f;
    public float waitTime = 2.0f;

    [Header("Referências para o Reset (Soft Reset)")]
    public Transform playerTransform;
    public Transform killerTransform;
    public Killer killerScript;

    [Header("Áudio e Efeitos")]
    public AudioSource backgroundMusic;
    public AudioSource jumpScareSource;
    public AudioClip jumpScareClip;

    [Header("Tarefas e Objetos a Reiniciar")]
    public KeyInteractable keyScript;
    public ExitDoorInteractable exitDoorScript;

    private Vector2 playerStartPos;
    private Vector2 killerStartPos;
    private bool isCaught = false;
    private float originalBgmVolume;

    // Guardamos referências aos Animators para os forçar a olhar para a frente no reset
    private Animator playerAnim;
    private Animator killerAnim;

    void Start()
    {
        if (playerTransform != null)
        {
            playerStartPos = playerTransform.position;
            playerAnim = playerTransform.GetComponent<Animator>(); // Vai buscar o Animator do jogador
        }

        if (killerTransform != null)
        {
            killerStartPos = killerTransform.position;
            killerAnim = killerTransform.GetComponent<Animator>(); // Vai buscar o Animator do assassino
        }

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }

        if (backgroundMusic != null)
        {
            originalBgmVolume = backgroundMusic.volume;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCaught && collision.CompareTag("Killer"))
        {
            StartCoroutine(SoftResetSequence());
        }
    }

    private IEnumerator SoftResetSequence()
    {
        isCaught = true;

        if (jumpScareSource != null && jumpScareClip != null)
        {
            jumpScareSource.ignoreListenerPause = true;
            jumpScareSource.PlayOneShot(jumpScareClip);
        }

        Time.timeScale = 0f;

        if (transitionText != null)
        {
            transitionText.text = customCaughtText;
        }

        float t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeSpeed;

            if (fadeCanvas != null) fadeCanvas.alpha = Mathf.Clamp01(progress);
            if (backgroundMusic != null) backgroundMusic.volume = Mathf.Lerp(originalBgmVolume, 0f, progress);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(waitTime);

        // ==========================================
        // 5. REPOSICIONAMENTO E RESET DE ANIMAÇÕES
        // ==========================================
        if (playerTransform != null)
        {
            playerTransform.position = playerStartPos;

            // NOVO: Chama a função no teu script para garantir que a memória dele também limpa
            PlayerController pController = playerTransform.GetComponent<PlayerController>();
            if (pController != null)
            {
                pController.ResetDirection();
            }
        }

        if (killerTransform != null)
        {
            killerTransform.position = killerStartPos;
        }

        if (killerScript != null)
        {
            killerScript.currentState = Killer.State.Patrol;
            killerScript.playerTransform = null;

            // NOVO: Força o Assassino a olhar para a frente usando a própria variável interna dele
            killerScript.GetComponent<Animator>().SetBool("isMoving", false);
            killerScript.GetComponent<Animator>().SetFloat("moveX", 0f);
            killerScript.GetComponent<Animator>().SetFloat("moveY", -1f);
        }

        if (keyScript != null) keyScript.ResetKey();
        if (exitDoorScript != null) exitDoorScript.ResetDoorState();

        // ==========================================

        t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeSpeed;

            if (fadeCanvas != null) fadeCanvas.alpha = 1f - Mathf.Clamp01(progress);
            if (backgroundMusic != null) backgroundMusic.volume = Mathf.Lerp(0f, originalBgmVolume, progress);

            yield return null;
        }

        if (backgroundMusic != null) backgroundMusic.volume = originalBgmVolume;

        Time.timeScale = 1f;
        isCaught = false;
    }
}