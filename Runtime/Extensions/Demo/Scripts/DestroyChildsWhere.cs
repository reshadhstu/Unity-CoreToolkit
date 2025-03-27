using UnityEngine;

namespace CoreToolkit.Runtime.Extensions.Demo.Scripts
{
    public class DestroyChildsWhere : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;

        private void Update()
        {
            _target.DestroyChildsWhere(c => c.name == "Enemy");
        }
    }
}