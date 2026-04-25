# Unity Slot Machine Assignment 🎰

## 🎮 Game Overview
A fully functional 2D Slot Machine game built in Unity. The game features a complete core loop including an interactive betting system, smooth reel animations with random durations, and a robust economy system.

## ⚙️ Instructions to Run WebGL Build
1. Navigate to the `/Build/SlotMachine_FinalBuild` folder in this repository.
2. Download the folder to your local machine.
3. Because browsers block local WebGL files due to CORS policy, please host the folder using a local server (e.g., VS Code Live Server, Python HTTP server) or upload it to a platform like Playabl/Simmer.io to play.
   * Alternatively, you can open the Unity project and play directly in the Editor.

## ✨ Bonus Features Implemented
* **Dynamic Betting System:** Instead of a static spin, players can choose between Bet 10, Bet 50, and Bet 100 before pulling the lever.
* **Interactive UI States:** The betting panel hides during spins to prevent mid-game changes and reappears when reels stop.
* **Economy & Game Over State:** Tracks user balance. If the player runs out of coins, a "Game Over" UI panel restricts further play until restarted.

## 🧠 Thought Process & Approach
1. **Architecture:** Used a centralized `SlothMachine.cs` controller to manage UI, Economy, and Game Logic to keep the scope manageable and performant.
2. **Animation:** Avoided Unity's heavy Animator component for the reels. Instead, implemented custom Coroutines using `Vector3.Translate` and wrapping logic for precise control over speed and stopping behavior.
3. **Randomization (RNG):** Used `Random.Range` to assign slightly different total spin durations to each reel, ensuring unpredictable results and a satisfying stagger effect when stopping.
4. **Accuracy:** Added a "snap distance" calculation at the end of the spin coroutine to ensure symbols perfectly align in the center line before validation.
