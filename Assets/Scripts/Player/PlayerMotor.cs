using UnityEngine;

namespace CottageMagic
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private readonly int[] _rowForDirection = { 0, 2, 4, 1 };
        private readonly Vector2[] _facing = { Vector2.down, Vector2.right, Vector2.up, Vector2.left };
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;
        private Camera _camera;
        private Vector2 _moveInput;
        private Vector2 _lastMove = Vector2.down;
        private float _animationTimer;
        private int _frame;
        private Vector2 _cameraVelocity;
        private Vector3 _cameraTarget;
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
            _body.linearDamping = 4f;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.25f;
            collider.offset = new Vector2(0, -0.12f);
            _sprite = GetComponent<SpriteRenderer>();
            _sprite.sprite = PixelArt.MaleFrame(0, 0);
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
            Vector3 desired = new Vector3(transform.position.x, transform.position.y + 0.55f, -10f);
            _cameraTarget = Vector3.SmoothDamp(_cameraTarget == Vector3.zero ? desired : _cameraTarget, desired, ref _cameraVelocity, 0.12f);
            const float pixelStep = 1f / 16f;
            _camera.transform.position = new Vector3(
                Mathf.Round(_cameraTarget.x / pixelStep) * pixelStep,
                Mathf.Round(_cameraTarget.y / pixelStep) * pixelStep,
                -10f);
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
            int direction = 0;
            float largest = 0;
            for (int i = 0; i < _facing.Length; i++)
            {
                float dot = Vector2.Dot(_lastMove, _facing[i]);
                if (dot > largest) { largest = dot; direction = i; }
            }
            if (!walking)
            {
                _frame = 0;
                _sprite.sprite = PixelArt.MaleFrame(0, _rowForDirection[direction]);
                return;
            }
            _animationTimer += Time.deltaTime;
            if (_animationTimer >= 0.105f)
            {
                _animationTimer -= 0.105f;
                _frame = (_frame + 1) % 5;
                _sprite.sprite = PixelArt.MaleFrame(_frame, _rowForDirection[direction]);
            }
        }
    }
}
