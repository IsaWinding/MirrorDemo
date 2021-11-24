using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAni : MonoBehaviour
{
    private Animator animator;
    private string curAniName;
    private int curAniProx = 0;
    private void Awake()
    {
        animator = this.gameObject.GetComponent<Animator>();
    }
    private System.Action onFinish;
    public bool PlayAni(string pAniName,int pProx,System.Action pOnFinish = null)
    {
        if (curAniName != null && IsPlayAning(curAniName) && pProx <= curAniProx)
            return false;
        animator.CrossFade(pAniName,0);
        curAniName = pAniName;
        curAniProx = pProx;
        onFinish = pOnFinish;
        return true;
    }
    private bool IsPlayAning(string pAniName)
    {
        AnimatorStateInfo animatorInfo = animator.GetCurrentAnimatorStateInfo(0);
        if ((animatorInfo.normalizedTime <= 1.0f) && (animatorInfo.normalizedTime > 0f) && (animatorInfo.IsName(pAniName)))
        {
            return true;
        }
        return false;
    }
    public void OnAttackFinish()
    {
        if (onFinish != null)
            onFinish.Invoke();
    }
}
