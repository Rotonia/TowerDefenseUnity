using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRandomizer : MonoBehaviour
    {
        public Sprite[] sprites;
        private SpriteRenderer _sprite;
        // Start is called before the first frame update
        void Start()
        {
            _sprite = GetComponent<SpriteRenderer>();
            Randomize();
        }

        public void OnEnable()
        {
            Randomize();
        }

        private void Randomize()
        {
            if (_sprite != null)
            {
                _sprite.sprite = sprites[Random.Range(0, sprites.Length)];
            }
        }
    }
}
