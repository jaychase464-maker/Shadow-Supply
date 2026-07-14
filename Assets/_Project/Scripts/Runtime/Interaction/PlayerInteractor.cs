using ShadowSupply.Player;
using ShadowSupply.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowSupply.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera interactionCamera;
        [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private InteractionPromptHUD promptHUD;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[8];
        private FirstPersonController firstPersonController;
        private InputAction interactAction;
        private IInteractable currentInteractable;

        private void Awake()
        {
            firstPersonController = GetComponent<FirstPersonController>();

            if (interactionCamera == null)
            {
                interactionCamera = GetComponentInChildren<Camera>(true);
            }

            if (promptHUD == null)
            {
                promptHUD = FindFirstObjectByType<InteractionPromptHUD>();
            }

            interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            interactAction.AddBinding("<Gamepad>/buttonSouth");
        }

        private void OnEnable()
        {
            interactAction.Enable();
        }

        private void OnDisable()
        {
            interactAction.Disable();
            SetCurrentInteractable(null);
        }

        private void OnDestroy()
        {
            interactAction?.Dispose();
        }

        private void Update()
        {
            if (interactionCamera == null ||
                (firstPersonController != null && !firstPersonController.IsCursorLocked))
            {
                SetCurrentInteractable(null);
                return;
            }

            SetCurrentInteractable(FindTarget());

            if (currentInteractable != null &&
                currentInteractable.CanInteract(gameObject) &&
                interactAction.WasPressedThisFrame())
            {
                currentInteractable.Interact(gameObject);
            }
        }

        private IInteractable FindTarget()
        {
            Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
            int hitCount = Physics.RaycastNonAlloc(
                ray,
                hitBuffer,
                interactionDistance,
                interactionMask,
                QueryTriggerInteraction.Collide
            );

            if (hitCount <= 0)
            {
                return null;
            }

            int nearestIndex = -1;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (hitBuffer[i].distance < nearestDistance)
                {
                    nearestDistance = hitBuffer[i].distance;
                    nearestIndex = i;
                }
            }

            if (nearestIndex < 0)
            {
                return null;
            }

            MonoBehaviour[] behaviours =
                hitBuffer[nearestIndex].collider.GetComponentsInParent<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IInteractable interactable && interactable.CanInteract(gameObject))
                {
                    return interactable;
                }
            }

            return null;
        }

        private void SetCurrentInteractable(IInteractable interactable)
        {
            if (ReferenceEquals(currentInteractable, interactable))
            {
                return;
            }

            currentInteractable = interactable;

            if (promptHUD == null)
            {
                return;
            }

            if (currentInteractable == null)
            {
                promptHUD.ClearPrompt();
            }
            else
            {
                promptHUD.SetPrompt(currentInteractable.InteractionPrompt);
            }
        }
    }
}
