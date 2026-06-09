using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui
{
    public interface IUGUIBuilder<C> where C : MonoBehaviour
    {
        C Build();
    }
}