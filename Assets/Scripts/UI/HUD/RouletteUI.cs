// Responsibility: Visual animation of the roulette wheel and return of the chosen power.

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class RouletteUI : MonoBehaviour
{
    [Header("References UI")]
    public Image[] powerIcons;        
    public Sprite bombSprite;
    public Sprite riftSprite;
    public Sprite conversionSprite;

    [Header("Config")]
    public float spinDuration = 2f;   
    public float initialInterval = 0.08f;  
    public float finalInterval = 0.3f;    

    [Header("Arrow")]
    public RouletteArrow arrow;
    
    private PowerType result;
    private bool spinDone = false;
    
    public static event System.Action OnRouletteTick;
    public static event System.Action OnRouletteResult;
    
    public IEnumerator Spin(List<PowerType> availablePowers)
    {
        spinDone = false;
        gameObject.SetActive(true);

        yield return StartCoroutine(AnimateSpin(availablePowers));

        gameObject.SetActive(false);
    }
    
    public PowerType GetResult() => result;

    private IEnumerator AnimateSpin(List<PowerType> availablePowers)
    {
        float elapsed = 0f;
        int currentIndex = 0;
        
        if (arrow != null) arrow.StartSpin();

        while (elapsed < spinDuration)
        {
            float t = elapsed / spinDuration;
            float interval = Mathf.Lerp(initialInterval, finalInterval, t);

            HighlightPower(currentIndex, availablePowers);
            currentIndex = (currentIndex + 1) % availablePowers.Count;

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        
        result = availablePowers[Random.Range(0, availablePowers.Count)];
        HighlightPower(availablePowers.IndexOf(result), availablePowers);

        if (arrow != null)
        {
            float targetAngle = arrow.GetAngleForIndex(
                availablePowers.IndexOf(result),
                powerIcons.Select(i => i.GetComponent<RectTransform>()).ToArray(),
                GetComponent<RectTransform>()
            );
            arrow.BrakeTo(targetAngle);
        }
        
        yield return new WaitUntil(() => !arrow.IsSpinning);
        OnRouletteResult?.Invoke();
    }

    private void HighlightPower(int index, List<PowerType> availablePowers)
    {
        OnRouletteTick?.Invoke();
        
        for (int i = 0; i < powerIcons.Length; i++)
        {
            if (i >= availablePowers.Count)
            {
                powerIcons[i].gameObject.SetActive(false);
                continue;
            }

            powerIcons[i].gameObject.SetActive(true);
            Color c = powerIcons[i].color;
            c.a = (i == index) ? 1f : 0.3f;
            powerIcons[i].color = c;
            powerIcons[i].sprite = GetSprite(availablePowers[i]);
        }
    }

    private Sprite GetSprite(PowerType power)
    {
        return power switch
        {
            PowerType.Bomb => bombSprite,
            PowerType.Rift => riftSprite,
            PowerType.Conversion => conversionSprite,
            _ => null
        };
    }
}