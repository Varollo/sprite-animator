using UnityEngine;

namespace Varollo.SpriteAnimator.Demo
{
    public class SA_Demo1 : MonoBehaviour
    {
        private SpriteAnimatorBase anim;

        private void Start() => anim = GetComponent<SpriteAnimatorBase>();

        private void OnMouseDown()
        {
            if(anim.IsRunning) anim.StopPlayback();
            else anim.PlayAnimation(0);
        }
    }
}