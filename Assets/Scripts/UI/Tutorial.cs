using UnityEngine;
using JamSabana.Core;

public class Tutorial : MonoBehaviour
{
    [Header("Jerarquía del Tutorial")]
    [Tooltip("Canvas principal del tutorial (TutorialCanvas). Si se deja vacío usa este mismo GameObject.")]
    public GameObject tutorialCanvas;

    [Tooltip("Objeto base que permanece activo durante todo el tutorial (TutoBase).")]
    public GameObject tutoBase;

    [Tooltip("Lista ordenada de pasos del tutorial (tuto1, tuto2, tuto3...).")]
    public GameObject[] objects;

    private int currentStepIndex = 0;
    private bool isTutorialActive = false;

    public bool IsTutorialActive => isTutorialActive;

    private void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        if (tutorialCanvas == null)
        {
            tutorialCanvas = gameObject;
        }

        if (objects == null || objects.Length == 0)
        {
            Debug.LogWarning("[Tutorial] No hay pasos asignados en el arreglo 'objects'. Iniciando partida directamente.");
            EndTutorial();
            return;
        }

        currentStepIndex = 0;
        isTutorialActive = true;

        // Activar el Canvas padre y la base del tutorial
        tutorialCanvas.SetActive(true);
        if (tutoBase != null)
        {
            tutoBase.SetActive(true);
        }

        // Activar únicamente el primer paso (tuto1) y apagar los demás
        RefreshStepVisibility();

        // Pausar el juego a través del evento global
        GameEventsB.TriggerGamePaused(true);
    }

    /// <summary>
    /// Avanza al siguiente paso del tutorial. Si ya se encuentra en el último paso,
    /// cierra el tutorial y reanuda la partida. Conectar al evento OnClick del botón "Siguiente".
    /// </summary>
    public void NextStep()
    {
        if (!isTutorialActive) return;

        currentStepIndex++;

        if (currentStepIndex < objects.Length)
        {
            RefreshStepVisibility();
        }
        else
        {
            EndTutorial();
        }
    }

    private void RefreshStepVisibility()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(i == currentStepIndex);
            }
        }
    }

    public void EndTutorial()
    {
        isTutorialActive = false;

        if (objects != null)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].SetActive(false);
                }
            }
        }

        if (tutoBase != null)
        {
            tutoBase.SetActive(false);
        }

        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }

        // Reanudar la partida a través del evento global
        GameEventsB.TriggerGamePaused(false);
    }

    private void OnDestroy()
    {
        if (isTutorialActive)
        {
            GameEventsB.TriggerGamePaused(false);
        }
    }
}
