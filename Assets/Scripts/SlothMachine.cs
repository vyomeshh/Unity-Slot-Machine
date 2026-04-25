using System.Collections;
using UnityEngine;
using TMPro; 

public class SlothMachine : MonoBehaviour
{
    [Header("Reel Settings")]
    // Array holding the transforms of the 3 slot machine reels
    public Transform[] reels;
    // Speed at which the reels move downwards during a spin
    public float spinSpeed = 25f;
    // The Y-axis coordinate where a symbol resets to create an infinite loop
    public float bottomLimit = -2f;
    // The Y-axis coordinate where the symbol respawns
    public float topLimit = 2f;
    // Distance used to snap symbols perfectly into the center when stopping
    public float snapDistance = 1f;

    [Header("UI & Panels")]
    // Text elements for game feedback (Jackpot/Try Again), coins, and current bet
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI betDisplayTM; 
    
    // UI Panel displayed when the player runs out of currency
    public GameObject gameOverPanel;
    // UI Panel containing betting options, toggled off during active spins
    public GameObject betPanel; 

    [Header("Economy System")]
    // Starting currency for the player
    public int currentCoins = 1000;
    // Default bet amount per pull
    public int currentBet = 10;
    // Payout multiplier applied when all 3 symbols match
    public int jackpotMultiplier = 50;

    [Header("Lever Animation Setup")]
    // Components required to animate the physical lever
    public SpriteRenderer leverRenderer; 
    public Sprite leverUpSprite;         
    public Sprite leverDownSprite;       

    // State lock to prevent multiple inputs while reels are active
    private bool isSpinning = false;
    // Counter to track when all reels have successfully stopped
    private int stoppedReelsCount = 0;

    void Start()
    {
        // Initialize UI elements to their default states on game start
        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (betPanel != null) betPanel.SetActive(true); 
        if (resultText != null) resultText.text = "PULL LEVER TO START";
    }

    /// <summary>
    /// Triggered by the Lever's collider. Validates funds and starts the game loop.
    /// </summary>
    public void OnLeverClicked()
    {
        // Verify the machine is idle and the player has sufficient funds
        if (!isSpinning && currentCoins >= currentBet)
        {
            currentCoins -= currentBet; // Process the wager
            UpdateUI();                 
            StartCoroutine(AnimateLeverAndSpin()); 
        }
        // Trigger Game Over sequence if funds are insufficient
        else if (currentCoins < currentBet && !isSpinning)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Handles the visual lever pull animation and UI state locking.
    /// </summary>
    IEnumerator AnimateLeverAndSpin()
    {
        isSpinning = true; 

        // Hide betting UI to prevent mid-spin wager adjustments
        if (betPanel != null) betPanel.SetActive(false);

        // Animate lever downwards
        if (leverRenderer != null && leverDownSprite != null)
            leverRenderer.sprite = leverDownSprite;

        yield return new WaitForSeconds(0.2f); 

        // Return lever to resting position
        if (leverRenderer != null && leverUpSprite != null)
            leverRenderer.sprite = leverUpSprite;

        // Initialize reel movement
        StartCoroutine(SpinAllReels());
    }

    /// <summary>
    /// Starts individual coroutines for each reel with slightly varied durations.
    /// </summary>
    IEnumerator SpinAllReels()
    {
        if (resultText != null) resultText.text = "";
        stoppedReelsCount = 0;

        for (int i = 0; i < reels.Length; i++)
        {
            // Calculate a staggered spin duration for dramatic effect (RNG)
            float randomDuration = Random.Range(1.5f, 2.5f) + (i * Random.Range(0.4f, 0.8f));
            StartCoroutine(SpinSingleReel(reels[i], randomDuration));
        }
        yield return null;
    }

    /// <summary>
    /// Handles the translation, wrapping, and precise snapping of a single reel.
    /// </summary>
    IEnumerator SpinSingleReel(Transform reel, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            foreach (Transform s in reel)
            {
                // Move symbols downwards
                s.Translate(Vector3.down * spinSpeed * Time.deltaTime);
                
                // Wrap symbols back to the top when they pass the bottom threshold
                if (s.localPosition.y <= bottomLimit)
                {
                    // Prevent visual gaps by calculating precise overtravel
                    float overtravel = bottomLimit - s.localPosition.y;
                    s.localPosition = new Vector3(s.localPosition.x, topLimit - overtravel, 0);
                }
            }
            timer += Time.deltaTime;
            yield return null; 
        }

        // Snap logic: Ensure resting symbols align perfectly with the payout line
        foreach (Transform s in reel)
        {
            Vector3 pos = s.localPosition;
            pos.y = Mathf.Round(pos.y / snapDistance) * snapDistance;
            s.localPosition = pos;
        }

        stoppedReelsCount++;
        
        // Evaluate the board once the final reel stops
        if (stoppedReelsCount == reels.Length)
        {
            CheckResults();
            isSpinning = false; // Release the machine lock
        }
    }

    /// <summary>
    /// Evaluates the center payline to determine if a jackpot was hit.
    /// </summary>
    void CheckResults()
    {
        string[] results = new string[3];
        
        // Identify the symbol occupying the center position (y ≈ 0) for each reel
        for (int i = 0; i < reels.Length; i++)
        {
            foreach (Transform s in reels[i])
            {
                if (Mathf.Abs(s.localPosition.y) < 0.1f)
                {
                    results[i] = s.GetComponent<SpriteRenderer>().sprite.name;
                    break;
                }
            }
        }

        // Win Condition: All three center symbols are identical
        if (results[0] == results[1] && results[1] == results[2])
        {
            int winAmount = currentBet * jackpotMultiplier;
            if (resultText != null)
            {
                resultText.text = "JACKPOT! +" + winAmount;
                resultText.color = Color.yellow; 
            }
            currentCoins += winAmount; // Issue payout
        }
        else // Loss Condition
        {
            if (resultText != null)
            {
                resultText.text = "TRY AGAIN";
                resultText.color = Color.white;
            }
        }

        // Restore betting UI visibility for the next round
        if (betPanel != null) betPanel.SetActive(true);

        UpdateUI(); 
    }

    /// <summary>
    /// Called by UI buttons to update the current wager.
    /// </summary>
    public void SetBet(int amount)
    {
        // Prevent wager changes during active gameplay
        if (!isSpinning)
        {
            currentBet = amount;
            UpdateUI();
        }
    }

    /// <summary>
    /// Refreshes all text displays to reflect current data states.
    /// </summary>
    void UpdateUI()
    {
        if (coinText != null) coinText.text = "COINS: " + currentCoins;
        if (betDisplayTM != null) betDisplayTM.text = "BET: " + currentBet;
    }

    /// <summary>
    /// Resets the game economy when the player opts to restart after a Game Over.
    /// </summary>
    public void RestartGame()
    {
        currentCoins = 1000;
        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resultText != null) 
        {
            resultText.text = "PULL LEVER TO START";
            resultText.color = Color.white;
        }
    }

    /// <summary>
    /// Handles the "No" selection on the Game Over screen. 
    /// WebGL does not support Application.Quit(), so we simulate a final game state.
    /// </summary>
    public void QuitGame()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resultText != null) 
        {
            resultText.text = "GAME OVER";
            resultText.color = Color.red;
        }
    }
}