// Responsibility: Match countdown shown in a TextMeshPro field.
// When it reaches zero, the team with more assimilation wins.
// On a perfect tie the match goes to sudden death: the next change in the balance decides it.
// The match itself is closed by GameManager.EndMatch.

using TMPro;
using UnityEngine;

namespace JamSabana.Core
{
    [DisallowMultipleComponent]
    public class MatchTimer : MonoBehaviour
    {
        // Balance: 0.0 = Cute wins, 1.0 = Dark wins (see AssimilationManager).
        private const float Neutral = 0.5f;
        private const float TieTolerance = 0.001f;

        [Header("Config")]
        [SerializeField, Min(1f)] private float matchDurationSeconds = 180f;

        [Header("UI")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private string suddenDeathLabel = "¡MUERTE SÚBITA!";
        [Tooltip("Por debajo de estos segundos el texto cambia al color de advertencia.")]
        [SerializeField] private float warningSeconds = 30f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.red;

        public float RemainingSeconds { get; private set; }
        public bool IsSuddenDeath { get; private set; }

        private int displayedSeconds = -1;

        private void Awake()
        {
            RemainingSeconds = matchDurationSeconds;
            RefreshText(true);
        }

        private void OnEnable()
        {
            GameEventsB.OnAssimilationChanged += HandleAssimilationChanged;
        }

        private void OnDisable()
        {
            GameEventsB.OnAssimilationChanged -= HandleAssimilationChanged;
        }

        private void Update()
        {
            // Only counts while the match is being played (not in tutorial, pause or game over).
            if (!IsMatchPlaying() || IsSuddenDeath) return;

            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Time.deltaTime);
            RefreshText();

            if (RemainingSeconds <= 0f)
                HandleTimeExpired();
        }

        private void HandleTimeExpired()
        {
            float balance = AssimilationManager.Instance != null
                ? AssimilationManager.Instance.CurrentBalance
                : Neutral;

            if (TryGetLeader(balance, out PlayerTeam leader))
            {
                GameManager.Instance.EndMatch(leader);
                return;
            }

            // Perfect tie: nobody wins yet.
            IsSuddenDeath = true;
            RefreshText();
        }

        // In sudden death, the first balance change that breaks the tie ends the match.
        private void HandleAssimilationChanged(float balance)
        {
            if (!IsSuddenDeath || !IsMatchPlaying()) return;

            if (TryGetLeader(balance, out PlayerTeam leader))
                GameManager.Instance.EndMatch(leader);
        }

        private static bool TryGetLeader(float balance, out PlayerTeam leader)
        {
            if (balance > Neutral + TieTolerance)
            {
                leader = PlayerTeam.Dark;
                return true;
            }

            if (balance < Neutral - TieTolerance)
            {
                leader = PlayerTeam.Cute;
                return true;
            }

            leader = default;
            return false;
        }

        private static bool IsMatchPlaying() =>
            GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;

        private void RefreshText(bool force = false)
        {
            if (timerText == null) return;

            if (IsSuddenDeath)
            {
                timerText.text = suddenDeathLabel;
                timerText.color = warningColor;
                return;
            }

            // Only touch the text when the displayed second changes.
            int seconds = Mathf.CeilToInt(RemainingSeconds);
            if (!force && seconds == displayedSeconds) return;

            displayedSeconds = seconds;
            timerText.text = $"{seconds / 60}:{seconds % 60:00}";
            timerText.color = RemainingSeconds <= warningSeconds ? warningColor : normalColor;
        }
    }
}
