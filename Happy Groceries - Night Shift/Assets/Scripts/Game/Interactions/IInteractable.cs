using UnityEngine;

// Define um "contrato" que qualquer objeto interativo do jogo deve seguir.
// Usar interfaces facilita a comunicação entre o jogador e diferentes tipos de objetos.
public interface IInteractable
{
    // -------------------------------------------------------------------------
    // MÉTODOS DA INTERFACE
    // -------------------------------------------------------------------------

    // Este método será chamado quando o jogador interagir com o objeto (ex: pressionar 'E').
    // Cada objeto que "assinar" esta interface terá a sua própria versão da interação.
    void Interact();

    // Este método permite verificar se o objeto está disponível para interação num dado momento.
    // Útil para impedir interações repetidas ou quando um objeto está "bloqueado".
    bool CanInteract();
}