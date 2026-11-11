using UnityEngine;

namespace CottageMagic
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;
        private Transform _spriteRoot;
        private Camera _camera;
        private Vector2 _moveInput;
        private Vector2 _lastMove = Vector2.down;
        private float _animationTimer;
        private int _frame;
        private Vector3 _cameraVelocity;
        private Vector3 _cameraTarget;
        private int _direction;
        private const float WalkSpeed = 3.15f;
        private const float Acceleration = 24f;
        private const float Braking = 30f;

        public WorldInteractable Nearby { get; private set; }
        public string Prompt { get; private set; }

        public void Initialize(Camera cameraToUse)
        {
            _camera = cameraToUse;
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.linearDamping = 0f;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.25f;
            collider.offset = new Vector2(0, 0.16f);
            var spriteObject = new GameObject("Pixel-Snapped Character");
            _spriteRoot = spriteObject.transform;
            _spriteRoot.SetParent(transform, false);
            _sprite = spriteObject.AddComponent<SpriteRenderer>();
            _sprite.sprite = PixelArt.SproutCharacterFrame(_direction, false, 0);
            _sprite.sortingOrder = 40;
        }

        private void Update()
        {
            bool block = MagicShopDirector.Instance != null && MagicShopDirector.Instance.UI != null && MagicShopDirector.Instance.UI.BlocksWorldInput;
            _moveInput = block ? Vector2.zero : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_moveInput.sqrMagnitude > 1) _moveInput.Normalize();
            if (_moveInput.sqrMagnitude > 0.02f) _lastMove = _moveInput.normalized;

            FindNearby();
            if (!block && Input.GetKeyDown(KeyCode.E) && Nearby != null)
                MagicShopDirector.Instance.Interact(Nearby);
            if (!block && Input.GetKeyDown(KeyCode.I)) MagicShopDirector.Instance.UI.ToggleInventory();
            if (!block && Input.GetKeyDown(KeyCode.J)) MagicShopDirector.Instance.UI.OpenOrders();
            if (Input.GetKeyDown(KeyCode.Escape) && MagicShopDirector.Instance.UI != null)
                MagicShopDirector.Instance.UI.ClosePanel();

            Animate(_moveInput.sqrMagnitude > 0.02f);
            _sprite.sortingOrder = 40 - Mathf.RoundToInt(transform.position.y * 4f);
        }

        private void FixedUpdate()
        {
            if (_body == null) return;
            Vector2 target = _moveInput * WalkSpeed;
            float change = (_moveInput.sqrMagnitude > 0.01f ? Acceleration : Braking) * Time.fixedDeltaTime;
            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity, target, change);
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            bool interior = MagicShopDirector.Instance != null && MagicShopDirector.Instance.World != null && MagicShopDirector.Instance.World.IsInterior;
            Vector3 desired = interior
                ? new Vector3(0f, -0.5f, -10f)
                : new Vector3(transform.position.x, transform.position.y + 0.55f, -10f);
            _camera.backgroundColor = interior ? new Color32(77, 19, 19, 255) : new Color32(133, 166, 67, 255);
            _cameraTarget = Vector3.SmoothDamp(_cameraTarget == Vector3.zero ? desired : _cameraTarget, desired, ref _cameraVelocity, 0.12f);
            // Snap both camera and visible sprite to the source-art grid. The
            // camera's output pixel step is too fine for 16px art and causes
            // tiny fractional shifts as the player walks.
            float pixelStep = 1f / PixelArt.PixelsPerUnit;
            _camera.transform.position = new Vector3(
                Mathf.Round(_cameraTarget.x / pixelStep) * pixelStep,
                Mathf.Round(_cameraTarget.y / pixelStep) * pixelStep,
                -10f);

            if (_spriteRoot != null)
            {
                _spriteRoot.localPosition = Vector3.zero;
                Vector3 renderPosition = transform.position;
                renderPosition.x = Mathf.Round(renderPosition.x / pixelStep) * pixelStep;
                renderPosition.y = Mathf.Round(renderPosition.y / pixelStep) * pixelStep;
                _spriteRoot.position = renderPosition;
            }
        }

        public void Teleport(Vector2 point)
        {
            if (_body == null) _body = GetComponent<Rigidbody2D>();
            _body.position = point;
            _body.linearVelocity = Vector2.zero;
            transform.position = point;
            _cameraTarget = Vector3.zero;
        }

        private void FindNearby()
        {
            Nearby = null; Prompt = null;
            var candidates = WorldInteractable.Active;
            if (candidates == null) return;
            float best = 1.45f;
            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.isActiveAndEnabled || !candidate.gameObject.activeInHierarchy) continue;
                float distance = Vector2.Distance(transform.position, candidate.transform.position);
                if (distance < best) { best = distance; Nearby = candidate; }
            }
            if (Nearby != null) Prompt = "E  ·  " + Nearby.Label;
        }

        private void Animate(bool walking)
        {
            // Keep the existing normalized 8-way movement while facing the
            // nearest cardinal direction supported by the reference sheet.
            if (Mathf.Abs(_lastMove.y) >= Mathf.Abs(_lastMove.x))
                _direction = _lastMove.y >= 0f ? 1 : 0;
            else
                _direction = _lastMove.x < 0f ? 2 : 3;

            if (!walking)
            {
                _frame = 0;
                _sprite.sprite = PixelArt.SproutCharacterFrame(_direction, false, _frame);
                return;
            }
            _animationTimer += Time.deltaTime;
            if (_animationTimer >= 0.12f)
            {
                _animationTimer -= 0.12f;
                _frame = (_frame + 1) % 2;
            }
            _sprite.sprite = PixelArt.SproutCharacterFrame(_direction, true, _frame);
        }
    }
}
