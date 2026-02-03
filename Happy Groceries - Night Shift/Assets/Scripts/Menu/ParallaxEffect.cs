using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("Movement Settings")]
    public float intensity = 20f;  // Quanto maior o valor, mais o fundo se move
    public float smoothness = 5f;  // Velocidade de suavizacao do movimento

    private Vector3 initialPosition;

    void Start()
    {
        // Salva a posicao original do objeto
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Pega a posicao do mouse na tela normalizada (0 a 1)
        float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;

        // Calcula a posicao alvo para o efeito de parallax
        Vector3 targetPosition = new Vector3(
            initialPosition.x + (mouseX * intensity),
            initialPosition.y + (mouseY * intensity),
            initialPosition.z
        );

        // Move suavemente o objeto para a posicao alvo
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothness);
    }
}