using UnityEngine;

namespace HanabiCanvas.Runtime
{
    [CreateAssetMenu(fileName = "New Canvas Config", menuName = "Hanabi Canvas/Config/Canvas Config")]
    public class DrawingCanvasConfigSO : ScriptableObject
    {
        [SerializeField] private int _gridSize = 32;
        [SerializeField] private int _borderThickness = 2;

        [SerializeField] private Color _gridColor = Color.white;
        [SerializeField] private Color _backgroundColor = Color.darkGray;

        public int GridSize => _gridSize;
        public int BordersThickness => _borderThickness;

        public Color GridColor => _gridColor;
        public Color BackgroundColor => _backgroundColor;
    }
}
