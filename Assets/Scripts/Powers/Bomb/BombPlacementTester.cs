// BombPlacementTester.cs — BORRAR ANTES DE ENTREGAR
// Prueba el raycast del crosshair 2D sin necesidad de la ruleta ni el inventario.

using UnityEngine;

public class BombPlacementTester : MonoBehaviour
{
    public Camera enemyCamera;
    public RectTransform crosshairUI;
    public LayerMask validPlacementLayer;
    public float crosshairSpeed = 300f;

    private Vector2 aimPosition;

    private void Update()
    {
        // Mueve el crosshair con las flechas del teclado independientemente del input system
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        aimPosition += new Vector2(x, y) * crosshairSpeed * Time.deltaTime;
        crosshairUI.anchoredPosition = aimPosition;

        // Al presionar Space lanza el raycast y dibuja dónde pegaría
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestRaycast();
        }
    }

    private void TestRaycast()
    {
        Ray ray = enemyCamera.ScreenPointToRay(GetScreenPosition());

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, validPlacementLayer))
        {
            Debug.Log($"Hit en: {hit.point} | Objeto: {hit.collider.name}");
            // Dibuja una esfera roja en el punto de impacto, visible en Scene view
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 3f);
        }
        else
        {
            Debug.Log("Sin hit — revisa que el suelo tenga el layer correcto");
            Debug.DrawRay(ray.origin, ray.direction * 200f, Color.yellow, 3f);
        }
    }

    private Vector3 GetScreenPosition()
    {
        Canvas canvas = crosshairUI.GetComponentInParent<Canvas>();
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 pos = crosshairUI.anchoredPosition;

        float screenX = (pos.x / canvasSize.x + 0.5f) * Screen.width;
        float screenY = (pos.y / canvasSize.y + 0.5f) * Screen.height;

        return new Vector3(screenX, screenY, 0f);
    }
}
