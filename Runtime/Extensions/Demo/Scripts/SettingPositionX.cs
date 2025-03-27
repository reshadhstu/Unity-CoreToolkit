using UnityEngine;

namespace CoreToolkit.Runtime.Extensions.Demo.Scripts
{
    public class SettingPositionX : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;

        private void Start()
        {
            _target.SetPositionX(1f);
        }
    }
}