using UnityEngine;

namespace AstraBricks
{
    /// <summary>
    /// Кирпич. Пока любой тип разбивается с одного удара, тип влияет только на спрайт.
    /// </summary>
    public class Brick : MonoBehaviour
    {
        [Tooltip("Тип кирпича: 1 или 2.")]
        public int type = 1;

        BrickField field;

        public void Init(BrickField owner, int brickType)
        {
            field = owner;
            type = brickType;
        }

        /// <summary>Разбить кирпич: убрать из раскладки и уничтожить объект.</summary>
        public void Break()
        {
            if (field != null) field.Remove(this);
            Destroy(gameObject);
        }
    }
}
