using UnityEngine;

namespace Prefabs
{
    public class ItemReference : MonoBehaviour
    {
        [SerializeField] private Scriptable_Objects_Scripts.Item item;

        public Scriptable_Objects_Scripts.Item GetItem()
        {
            return item;
        }
        
    }
}
