using UnityEngine;
using System.Collections;

public class Tutorial : MonoBehaviour
{
    [Header("Config")]
    public GameObject[] objects;
    public float delayBetween = 3f;

    private void Start()
    {
        foreach (GameObject obj in objects)
            if (obj != null) obj.SetActive(false);

        Time.timeScale = 0f; // Pausar el tiempo al inicio
        StartCoroutine(ActivateSequence());
    }

    private IEnumerator ActivateSequence()
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null) obj.SetActive(true);
            yield return new WaitForSecondsRealtime(delayBetween); 
            if (obj != null) obj.SetActive(false);
        }

        Time.timeScale = 1f; 
    }
}