using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui
{
    public interface IUGUIBuilder<T> where T : MonoBehaviour
    {
        T Build();
    }
}