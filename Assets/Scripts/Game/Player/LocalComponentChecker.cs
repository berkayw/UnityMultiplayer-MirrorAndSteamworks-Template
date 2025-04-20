using CMF;
using UnityEngine;
using Mirror;

public class LocalComponentChecker : NetworkBehaviour
{
    private bool isComponentsSet;

    private Mover mover;
    private AdvancedWalkerController advancedWalkerController;
    private CharacterKeyboardInput characterKeyboardInput;
    [SerializeField] private GameObject playerCam; //attach in inspector
    
    private void Start()
    {
        mover = GetComponent<Mover>();
        advancedWalkerController = GetComponent<AdvancedWalkerController>();
        characterKeyboardInput = GetComponent<CharacterKeyboardInput>();
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            Destroy(mover);
            Destroy(advancedWalkerController);
            Destroy(characterKeyboardInput);
            playerCam.SetActive(false);
        }
    }
}
