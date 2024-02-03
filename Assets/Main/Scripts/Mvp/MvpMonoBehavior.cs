using UnityEngine;

namespace Main.Scripts.Mvp
{
    public abstract class MvpMonoBehavior<T> : MonoBehaviour, MvpContract.View<T> where T : MvpContract.Presenter
    {
        protected abstract T? presenter { get; set; }
    }
}