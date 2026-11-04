using UnityEngine;
using UnityEngine.UI;
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

        // Evitar que las imágenes/textos informativos de los pasos bloqueen los clics hacia los botones de TutoBase
        DisableBlockingRaycastsOnSteps();

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
    /// Avanza al siguiente paso del tutorial. Si ya no hay más elementos en la lista,
    /// dispara EndTutorial() para cerrar el tutorial y reanudar la partida.
    /// </summary>
    public void NextStep()
    {
        if (!isTutorialActive) return;

        // Si no hay elementos o ya estamos en el último elemento de la lista, finalizar el tutorial
        if (objects == null || currentStepIndex >= objects.Length - 1)
        {
            EndTutorial();
            return;
        }

        currentStepIndex++;
        RefreshStepVisibility();
    }

    /// <summary>
    /// Retrocede al paso anterior del tutorial si no está en el primer paso.
    /// </summary>
    public void PreviousStep()
    {
        if (!isTutorialActive || objects == null || objects.Length == 0) return;

        if (currentStepIndex <= 0)
        {
            currentStepIndex = 0;
            return;
        }

        currentStepIndex = Mathf.Clamp(currentStepIndex - 1, 0, objects.Length - 1);
        RefreshStepVisibility();
    }

    private void RefreshStepVisibility()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && objects[i] != tutoBase && objects[i] != tutorialCanvas)
            {
                objects[i].SetActive(i == currentStepIndex);
            }
        }

        // Garantizar que el canvas padre y TutoBase sigan siempre activos mientras dure el tutorial
        if (tutorialCanvas != null && !tutorialCanvas.activeSelf)
        {
            tutorialCanvas.SetActive(true);
        }

        if (tutoBase != null && !tutoBase.activeSelf)
        {
            tutoBase.SetActive(true);
        }
    }

    /// <summary>
    /// Desactiva raycastTarget en las imágenes y textos de cada paso que no sean botones interactivos,
    /// evitando que un paso superpuesto en la jerarquía bloquee los botones de "Siguiente" y "Anterior".
    /// </summary>
    private void DisableBlockingRaycastsOnSteps()
    {
        if (objects == null) return;

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject stepObj = objects[i];
            if (stepObj == null || stepObj == tutoBase || stepObj == tutorialCanvas) continue;

            Graphic[] graphics = stepObj.GetComponentsInChildren<Graphic>(true);
            for (int g = 0; g < graphics.Length; g++)
            {
                Graphic graphic = graphics[g];
                if (graphic == null) continue;

                // Solo mantener el raycast si el objeto tiene (o es parte de) un Button / Selectable
                if (graphic.GetComponentInParent<Selectable>() == null)
                {
                    graphic.raycastTarget = false;
                }
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
