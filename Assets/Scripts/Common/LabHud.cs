using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shared HUD for both lab scenes. It draws four things the brief asked for:
///   1. a status bar            (title, coins, score, lives, level state)
///   2. an input visualisation  (Horizontal axis bar, jump, grounded, velocity)
///   3. a teaching-annotation toggle (world labels, key T)
///   4. a camera / rendering debug panel (key C)
/// Everything is optional: leave a field empty and that widget is simply skipped.
/// </summary>
public class LabHud : MonoBehaviour
{
    public static LabHud Instance { get; private set; }

    [Header("Scene wiring")]
    public PlayerController2D player;
    public Camera targetCamera;
    public GameObject annotationRoot;      // parent of the world-space teaching labels
    public GameObject debugPanel;          // the whole camera/render panel (toggled with C)

    [Header("Text widgets")]
    public Text titleText;
    public Text stateText;
    public Text scoreText;
    public Text helpText;
    public Text inputText;
    public Text debugText;
    public Text conceptsText;

    [Header("Input widgets")]
    public Image moveBarFill;
    public Image jumpDot;
    public Image groundDot;

    [Header("Content")]
    public string title = "CAT105TC - Lab";
    public string help = "A / D  move      Space  jump\nR  restart      T  teaching labels\nC  camera debug      H  hide this panel";
    public int totalCoins;

    private static readonly Color Ok = new Color(0.30f, 0.90f, 0.45f);
    private static readonly Color Bad = new Color(0.92f, 0.36f, 0.36f);
    private static readonly Color Dim = new Color(0.55f, 0.58f, 0.65f);

    private int score;
    private int coins;
    private int lives = 3;
    private bool goalReached;
    private bool showDebug;
    private bool showHelp = true;
    private string rayInfo = "";

    private int conceptsDone;
    private int conceptsTotal;
    private string conceptsList = "";

    private float fps;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyContent();
        RefreshScore();
        RefreshConcepts();
    }

    private void Update()
    {
        HandleHotkeys();
        UpdateInputPanel();
        UpdateDebugPanel();

        fps = Mathf.Lerp(fps, 1f / Mathf.Max(0.0001f, Time.smoothDeltaTime), 0.1f);
    }

    // ------------------------------------------------------------------ public API

    public void SetTitle(string t) { title = t; if (titleText != null) titleText.text = t; }

    public void SetStateText(string s) { if (stateText != null) stateText.text = s; }

    public void SetRayInfo(string s) { rayInfo = s; }

    public void AddCoin(int value)
    {
        coins += 1;
        score += value * 100;
        RefreshScore();
    }

    public void AddScore(int value) { score += value; RefreshScore(); }

    public void SetLives(int value) { lives = value; RefreshScore(); }

    public void LoseLife()
    {
        lives -= 1;
        if (lives <= 0)
        {
            lives = 3;
        }
        RefreshScore();
    }

    public void SetGoalReached(bool reached)
    {
        if (reached && !goalReached)
        {
            score += 1000;
        }
        goalReached = reached;
        RefreshScore();
    }

    public void SetConcepts(int done, int total, string listText)
    {
        conceptsDone = done;
        conceptsTotal = total;
        conceptsList = listText;
        RefreshConcepts();
    }

    public void SetTotalCoins(int total) { totalCoins = total; RefreshScore(); }

    public string GetScoreLine()
    {
        return string.Format("Coins {0}/{1}     Score {2}     Lives {3}", coins, totalCoins, score, lives);
    }

    // ------------------------------------------------------------------ internals

    private void ApplyContent()
    {
        if (titleText != null) titleText.text = title;
        if (helpText != null) helpText.text = help;
        if (annotationRoot != null) annotationRoot.SetActive(true);
        if (debugPanel != null) debugPanel.SetActive(showDebug);
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.T) && annotationRoot != null)
        {
            annotationRoot.SetActive(!annotationRoot.activeSelf);          // teaching labels
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            showDebug = !showDebug;
            if (debugPanel != null) debugPanel.SetActive(showDebug);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            showHelp = !showHelp;
            if (helpText != null) helpText.gameObject.SetActive(showHelp);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    private void RefreshScore()
    {
        if (scoreText == null)
        {
            return;
        }
        scoreText.text = GetScoreLine() + (goalReached ? "\n<color=#7CE38B>LEVEL COMPLETE  +1000</color>" : "");
    }

    private void RefreshConcepts()
    {
        if (conceptsText == null)
        {
            return;
        }
        conceptsText.text = string.Format("REVIEW  {0} / {1}\n{2}", conceptsDone, conceptsTotal, conceptsList);
    }

    private void UpdateInputPanel()
    {
        if (player == null)
        {
            return;
        }

        float h = player.Horizontal;
        bool jumpHeld = Input.GetKey(KeyCode.Space);
        bool grounded = player.Grounded;
        Vector2 v = player.currentVelocity;

        if (moveBarFill != null) moveBarFill.fillAmount = (h + 1f) * 0.5f;
        if (jumpDot != null) jumpDot.color = jumpHeld ? Ok : Dim;
        if (groundDot != null) groundDot.color = grounded ? Ok : Bad;

        if (inputText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Horiz. axis   ").Append(h.ToString("+0.00;-0.00; 0.00")).Append("   (A -1 .. D +1)\n");
            sb.Append("Space         ").Append(jumpHeld ? "HELD" : "up").Append('\n');
            sb.Append("IsGrounded    ").Append(grounded ? "true" : "false").Append('\n');
            sb.Append("Velocity      (").Append(v.x.ToString("0.0")).Append(", ").Append(v.y.ToString("0.0")).Append(")\n");
            sb.Append("Speed         ").Append((player.Speed01 * 100f).ToString("0")).Append("%");
            if (!string.IsNullOrEmpty(rayInfo))
            {
                sb.Append('\n').Append(rayInfo);
            }
            inputText.text = sb.ToString();
        }
    }

    private void UpdateDebugPanel()
    {
        if (debugText == null || !showDebug)
        {
            return;
        }

        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        StringBuilder sb = new StringBuilder();
        if (cam != null)
        {
            sb.Append("Camera        ").Append(cam.orthographic ? "Orthographic" : "Perspective").Append('\n');
            sb.Append("orthoSize     ").Append(cam.orthographicSize.ToString("0.0")).Append('\n');
            sb.Append("position      ").Append(cam.transform.position.x.ToString("0.00")).Append(", ")
              .Append(cam.transform.position.y.ToString("0.00")).Append('\n');
            sb.Append("depth         ").Append(cam.depth).Append('\n');
            sb.Append("clearFlags    ").Append(cam.clearFlags).Append('\n');
            sb.Append("cullingMask   0x").Append(cam.cullingMask.ToString("X8")).Append('\n');
        }
        sb.Append("fixedDeltaTime ").Append(Time.fixedDeltaTime.ToString("0.000")).Append('\n');
        sb.Append("FPS           ").Append(fps.ToString("0"));
        debugText.text = sb.ToString();
    }
}
