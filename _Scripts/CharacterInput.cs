using Mirror;
using Prime31;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : NetworkBehaviour
{

	public float moveSpeed = 8f;
	public float jumpSpeed = 20f;
	public float rayDistance = 4f;
	public float atkDistance = 2f;
	public float atk = 10f;
	public float atkOffset = 1f;
	public bool isFaceRight = true;
	public KeyCode moveLeftKey;
	public KeyCode moveRightKey;
	public KeyCode attackKey;
	public KeyCode jumpKey;
	public LayerMask platformMask = 0;
	public LayerMask enemyMask = 0;

	private bool isMove = false;
	private bool isJump = false;
	private bool isOnGround = true;
	private Vector3 moveDelta;
	private bool isAttack = false;

	private bool curIsFaceRight = true;
	private Rigidbody2D body;
	private Vector3 oriScale;
	private NetworkIdentity identity;
	private CharacterAni characterAni;
	private CharacterHp characterHp;
	public int MaxHp = 100;

	[SyncVar(hook = "HpChange")]
	public int curHp = 100;

	public NetworkIdentity Identity { get { return identity; } }
	void Awake() {
		body = this.gameObject.GetComponent<Rigidbody2D>();
		oriScale = this.transform.localScale;
		identity = this.gameObject.GetComponent<NetworkIdentity>();
		characterAni = this.gameObject.GetComponent<CharacterAni>();
		characterHp = this.gameObject.GetComponent<CharacterHp>();
		characterHp.SetHpInfo(curHp, MaxHp);
	}
    private void Start()
    {
		if(identity.isLocalPlayer)
			CameraSmoothFollow.Instance.SetTarget(this.transform);
    }

    private bool isCanReborn = true;
	private void OnDead() {
		if (isCanReborn)
		{
			Invoke("Reborn", 5f);
			isCanReborn = false;
		}	
	}

	[Command]
	private void Reborn()
	{
		curHp = MaxHp;
		SetHpInfo();
		RpcReborn();
	}
	[ClientRpc]
	private void RpcReborn()
	{
		this.transform.localPosition = Vector3.zero;
		isCanReborn = true;
	}
	public bool IsDead()
	{
		return curHp <= 0;
	}

    // the Update loop contains a very simple example of moving the character around and controlling the animation
    private void Update()
	{
		if (!identity.isLocalPlayer)
			return;
		if (IsDead())
			return;
		isMove = false;
		moveDelta = Vector3.zero;
		if (Input.GetKeyDown(jumpKey))
		{
			isOnGround = Physics2D.Raycast(this.transform.position, Vector2.down, rayDistance, platformMask);
			if(isOnGround)
				isJump = true;
		}
		if (Input.GetKeyDown(attackKey))
		{
			isAttack = true;
		}
		if (Input.GetKey(moveLeftKey)) {
			isMove = true;
			moveDelta.x = -moveSpeed;
		}
		else if (Input.GetKey(moveRightKey))
		{
			isMove = true;
			moveDelta.x = moveSpeed;
		}
	}
	public void HpChange(int pOld,int hp) {
		characterHp.SetHpInfo(hp, MaxHp);
		if (hp <= 0)
		{
			OnDead();	
		}
	}
	public void SetHpInfo()
	{
		characterHp.SetHpInfo(curHp, MaxHp);
	}
	public void OnDamage(int pDamage)
	{
		curHp -= pDamage;
		if (curHp > MaxHp)
			curHp = MaxHp;
		if (curHp < 0)
			curHp = 0;
		SetHpInfo();
	}


	private bool IsInAtkRange(Transform pTarget)
	{
		var direction = curIsFaceRight ? Vector2.right* atkDistance : -Vector2.right* atkDistance;
		var targetPos = pTarget.position;
		var selfPos = this.transform.position;
		if (Mathf.Abs(targetPos.y - selfPos.y) <= atkOffset && Mathf.Abs(targetPos.x - (selfPos.x + direction.x)) <= atkOffset)
		{
			return true;
		}
		return false;
	}
	[Command]
	public void CmdDoNormalAttack()
	{
		//Debug.LogError("CmdDoNormalAttack");
		var direction = curIsFaceRight ? Vector2.right : -Vector2.right;
		var identitys = GameObject.FindObjectsOfType<CharacterInput>(false);
		for (var i = 0; i < identitys.Length; i++)
		{ 
			if(identitys[i] != this)
			{
				if (IsInAtkRange(identitys[i].transform))
				{
					identitys[i].OnDamage((int)atk);
				}
			}
		}
		var ai = GameObject.FindObjectsOfType<MonsterAI>(false);
		for (var i = 0; i < ai.Length; i++)
		{
			if (ai[i] != this)
			{
				if (IsInAtkRange(ai[i].transform))
				{
					ai[i].OnDamage((int)atk);
				}
			}
		}
	}
	[Command]
	void CmdSetCurFaceRight(bool pSetRight)
	{
		curIsFaceRight = pSetRight;
	}
	private void FixedUpdate()
	{
		if (!identity.isLocalPlayer)
			return;
		if (IsDead())
			return;
		if (isAttack)
        {
			characterAni.PlayAni("attack",4,()=> {
				isAttack = false;
				CmdDoNormalAttack();
			});
		}
		if (isMove)
		{
			this.transform.localPosition += moveDelta * Time.deltaTime;
			int xS = isFaceRight == moveDelta.x > 0 ? 1 : -1;
			curIsFaceRight = xS == 1;
			CmdSetCurFaceRight(curIsFaceRight);
			this.transform.localScale = new Vector3(oriScale.x * xS, oriScale.y, oriScale.z);
			characterAni.PlayAni("move", 3);
		}
		if (isJump)
		{
			body.AddForce(new Vector2(0,jumpSpeed));
			isJump = false;
		}
		if (!isAttack && !isMove)
		{
			characterAni.PlayAni("idle", 1);
		}
	}
}
