using System.Collections.Generic;
using UnityEngine;

namespace CottageMagic
{
    public sealed class WorldInteractable : MonoBehaviour
    {
        public static readonly List<WorldInteractable> Active = new List<WorldInteractable>();
        public StationKind Kind;
        public int PlotIndex = -1;
        public string Label;

        private void OnEnable() { if (!Active.Contains(this)) Active.Add(this); }
        private void OnDisable() { Active.Remove(this); }
        private void OnDestroy() { Active.Remove(this); }
    }

    public sealed class PlotVisual : MonoBehaviour
    {
        public int PlotIndex;
        public SpriteRenderer PlantRenderer;
        private float _offsetTimer;
        private Vector3 _plantBase;
        private int _knownStage = -1;
        private string _knownSpecies;

        private void Start() { if (PlantRenderer != null) _plantBase = PlantRenderer.transform.localPosition; Refresh(); }

        private void Update()
        {
            if (PlantRenderer == null) return;
            var director = MagicShopDirector.Instance;
            if (director == null) return;
            var plot = director.PlotAt(PlotIndex);
            int stage = director.GrowthStage(plot);
            if (_knownStage != stage || _knownSpecies != (plot == null ? null : plot.species)) Refresh();
            if (PlantRenderer.sprite != null)
            {
                _offsetTimer += Time.deltaTime;
                int pixelOffset = Mathf.FloorToInt((Mathf.Sin(_offsetTimer * 2.1f + PlotIndex) + 1f) * 0.5f * 2f);
                PlantRenderer.transform.localPosition = _plantBase + Vector3.up * pixelOffset / 16f;
            }
        }

        public void Refresh()
        {
            var director = MagicShopDirector.Instance;
            if (director == null) return;
            var plot = director.PlotAt(PlotIndex);
            if (plot == null || string.IsNullOrEmpty(plot.species))
            {
                PlantRenderer.sprite = null;
                _knownStage = 0; _knownSpecies = null;
                return;
            }
            int stage = director.GrowthStage(plot);
            _knownStage = stage; _knownSpecies = plot.species;
            PlantRenderer.sprite = PlantArt(plot.species, stage);
            PlantRenderer.sortingOrder = 40 - Mathf.RoundToInt((transform.position.y + PlantRenderer.transform.localPosition.y) * 4f);
        }

        private Sprite PlantArt(string species, int stage)
        {
            int row;
            switch (species)
            {
                case "mooncap": row = 0; break;
                case "emberroot": row = 1; break;
                case "glowberry": row = 2; break;
                case "dreammint": row = 3; break;
                case "bitebloom": row = 4; break;
                default: row = 1; break;
            }
            int growthStage = Mathf.Clamp(stage, 0, 3);
            return PixelArt.Slice("MagicFarm/Crops", growthStage * 16, row * 16, 16, 16,
                PixelArt.PixelsPerUnit, new Vector2(0.5f, 0f));
        }
    }

    public sealed class IdlePixelAnimation : MonoBehaviour
    {
        public SpriteRenderer Target;
        public string CharacterSheet;
        public int FacingDirection;
        private Vector3 _start;
        private float _phase;
        private float _frameTimer;
        private int _frame;
        private void Start() { _start = transform.localPosition; _phase = transform.position.x * 0.7f; }
        private void Update()
        {
            _phase += Time.deltaTime * 2.0f;
            int pixels = Mathf.RoundToInt(Mathf.Sin(_phase) * 1.0f);
            transform.localPosition = _start + Vector3.up * pixels / 16f;
            if (Target != null && !string.IsNullOrEmpty(CharacterSheet))
            {
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= 0.75f) { _frameTimer = 0; _frame = (_frame + 1) % 3; }
                Target.sprite = PixelArt.CharacterFrame(CharacterSheet, FacingDirection, false, _frame);
            }
        }
    }
}
