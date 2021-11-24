using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CharacterHp : MonoBehaviour
{
    public SpriteRenderer hpBg;
    public SpriteRenderer hp;
    public int maxHp = 100;
 
    public float curHp = 100f;

    private float oriScale = 8;
    void Awake()
    {
        oriScale = hpBg.transform.localScale.x;
    }
    public void SetHpInfo(int pCurHp,int pMaxHp)
    {
        curHp = pCurHp;
        maxHp = pMaxHp;
        SetCurHp();
    }
    private void SetCurHp()
    {
        var hpProgress = curHp / maxHp;

        hp.transform.localScale = new Vector3(hpProgress * oriScale,1,1);
        hp.transform.localPosition = new Vector3(-(1- hpProgress) * oriScale/2, 0,0);
    }
}
