using UnityEngine;

namespace Fodinae.Assets.Scripts.Player
{
    /// <summary>
    /// Simple camera follow script to keep the player centered on screen.
    /// This is crucial for testing because terrain generation depends on the camera position.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        
        private float _originalZ;

        private void Start()
        {
            // Preserve the camera's original Z position so it doesn't clip into the 2D plane
            _originalZ = transform.position.z;

            // If no target is assigned, try to find the player controller automatically
            if (_target == null)
            {
                var player = FindObjectOfType<PlayerMovementController>();
                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (_target != null)
            {
                Vector3 targetPosition = _target.position + new Vector3(_offset.x, _offset.y, 0f);
                Vector3 desiredPosition = new Vector3(targetPosition.x, targetPosition.y, _originalZ);
                
                // Smoothly interpolate towards the target position
                transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Change the camera's tracking target at runtime
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }
    }
}
