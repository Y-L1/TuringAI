using System;
using UnityEngine;

namespace Game
{
    public class ThinkController : MonoBehaviour
    {
        #region Define

        // private static readonly int Idle = Animator.StringToHash("Idle");
        // private static readonly int Talking = Animator.StringToHash("Talking");
        // private static readonly int Thinking = Animator.StringToHash("Thinking");
        
        public enum EThinkType
        {
            Idle = 0,
            Talking = 1,
            Thinking = 2
        }

        #endregion

        #region Property

        private Animator animator;

        #endregion

        #region Unity

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        #endregion

        #region API

        public void PlayByType(EThinkType thinkType)
        {
            if (!animator)
            {
                this.LogErrorEditorOnly("Thinking animation is disabled");
                return;
            }
            foreach (var type in Enum.GetValues(typeof(EThinkType)))
            {
                animator.SetBool(type.ToString(), type.Equals(thinkType));
            }
        }

        #endregion
    }
}