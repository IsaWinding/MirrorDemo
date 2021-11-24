using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public enum AIType
{ 
	Monster = 1,//会攻击玩家的怪物
	Npc = 2,// 不会进行攻击的npc
}
public enum AIState
{ 
	Idle = 1,
	
	MoveToTarget  = 2,
	AttackTarget = 3,
	SkillTarget = 4,
	MoveBack = 5,
}
public enum PathType
{ 
	Once = 1,
	Loop = 2,
	PingPong = 3,

}
[System.Serializable]
public class PathPoint
{
	public Vector3 pos;
	public PathPoint NextPoint;
	public PathPoint PrePoint;
}

[System.Serializable]
public class AIPath
{
	public List<PathPoint> Paths = new List<PathPoint>();
	public PathType pathType = PathType.Loop;
	private PathPoint nextPoint;
	public void Init(){
		for (var i = 0; i < Paths.Count; i++)
		{
			var path = Paths[i];
			if (i == 0)
			{
				path.NextPoint = Paths[i + 1];
				if (pathType == PathType.Loop)
					path.PrePoint = Paths[Paths.Count - 1];
			}
			else if (i == Paths.Count - 1)
			{
				if (pathType == PathType.Loop)
					path.NextPoint = Paths[0];
				path.PrePoint = Paths[i - 1];
			}
			else
			{
				path.NextPoint = Paths[i + 1];
				path.PrePoint = Paths[i - 1];
			}
		}
		nextPoint = Paths[0];
	}
	public PathPoint GetNextPoint()
	{
		return nextPoint;
	}
	public bool IsReachNextPoint(Vector3 pos)
	{
		if (Vector3.Distance(nextPoint.pos, pos) <= 1)
			return true;
		return false;
	}
	private bool isForward = true;
	public void OnReachNextPoint()
	{
		if (pathType == PathType.Once)
		{
			nextPoint = nextPoint.NextPoint;
		}
		else if (pathType == PathType.Loop)
		{
			nextPoint = nextPoint.NextPoint;
		}
		else if (pathType == PathType.PingPong)
		{
			if (isForward)
			{
				if (nextPoint.NextPoint != null)
					nextPoint = nextPoint.NextPoint;

				else
				{
					nextPoint = nextPoint.PrePoint;
					isForward = false;
				}
			}
			else
			{
				if (nextPoint.PrePoint != null)
					nextPoint = nextPoint.PrePoint;
				else
				{
					nextPoint = nextPoint.NextPoint;
					isForward = true;
				}
			}
		}
	}
}
public class MonsterAI : NetworkBehaviour
{
	public Vector3 reBornPos;
	public float moveSpeed = 8f;

	public float atkDistance = 2f;//攻击范围
	public float warnDistance = 10f;//警戒范围
	public float followDistance = 12f;//追踪范围

	public float atk = 10f;
	public float atkOffset = 1f;
	public bool isFaceRight = true;
	public int MaxHp = 100;
	[SyncVar(hook = "HpChange")]
	public int curHp = 100;

	public AIPath path;

	private bool isMove = false;
	private Vector3 moveDelta;
	private bool isAttack = false;
	private bool curIsFaceRight = true;
	private Vector3 oriScale;
	private NetworkIdentity identity;
	private CharacterAni characterAni;
	private CharacterHp characterHp;
	private bool isCanReborn = true;
	private CharacterInput curTarget;
	private Vector3 findTargetPos;
	private AIPolicy aiPolicy;

	void Awake() {
		oriScale = this.transform.localScale;
		identity = this.gameObject.GetComponent<NetworkIdentity>();
		characterAni = this.gameObject.GetComponent<CharacterAni>();
		characterHp = this.gameObject.GetComponent<CharacterHp>();
		characterHp.SetHpInfo(curHp, MaxHp);
		aiPolicy = new AIPolicy(this);
		path.Init();
	}
	private void Start()
	{
		InvokeRepeating("OnAiAction", 1f, 1f);
	}
	private void OnAiAction()
	{
		if (IsDead())
			return;
		allIdentitys = GameObject.FindObjectsOfType<CharacterInput>(false);
		aiPolicy.OnRun(allIdentitys);
	}
	public void HpChange(int pOld, int hp)
	{
		characterHp.SetHpInfo(hp, MaxHp);
		if (hp <= 0)
		{
			OnDead();
		}
	}
	private void OnDead()
	{
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
	public void StopMove() {
		isMove = false;
		isMoveToTargetPos = false;
	}
	public void FaceToCurTarget()
	{
		if (curTarget.transform.position.x > this.transform.position.x)
		{
			FaceRight(true);
		}
		else
		{
			FaceRight(false);
		}
	}
	public void StopAttack() {
		isAttack = false;
	}
	public void Attack(){
		isAttack = true;
	}

	private bool IsInRange(CharacterInput pTarget,float pRange)
	{
		var targetPos = pTarget.transform.position;
		var selfPos = this.transform.position;

		if (Vector3.Distance(targetPos,selfPos)<= pRange){
			return true;
		}
		return false;
	}

	private bool IsInAtkRange(CharacterInput pTarget)
	{
		var direction = curIsFaceRight ? Vector2.right * atkDistance : -Vector2.right * atkDistance;
		var targetPos = pTarget.transform.position;
		var selfPos = this.transform.position;
		if (Mathf.Abs(targetPos.y - selfPos.y) <= atkOffset && Mathf.Abs(targetPos.x - (selfPos.x + direction.x)) <= atkOffset)
		{
			return true;
		}
		return false;
	}
	private CharacterInput[] allIdentitys;
	public bool IsHaveTarget(){
		return curTarget != null && !curTarget.IsDead();
	}
	public CharacterInput GetOneCharacterInRange(float pRange)
	{
		allIdentitys = GameObject.FindObjectsOfType<CharacterInput>(false);
		for(var i = 0; i< allIdentitys.Length;i++)
		{
			if (!allIdentitys[i].IsDead() && IsInRange(allIdentitys[i], pRange))
				return allIdentitys[i];
		}
		return null;
	}
	public bool CurTargetInRange(float pRange) {
		if (curTarget != null && !curTarget.IsDead())
		{
			return IsInRange(curTarget, pRange);
		}
		return false;
	}
    public void OnLoopMove()
    {
		if (path.IsReachNextPoint(this.transform.position))
		{
			path.OnReachNextPoint();
		}
		var nextPos = path.GetNextPoint();
		if (nextPos != null)
			MoveToTargetPos(nextPos.pos);
		else
			Idle();
	}
	private bool isMoveToTargetPos = false;
	private Vector3 targetPos;
    public void MoveToTargetPos(Vector3 pTargetPos)
	{
		StopAttack();
		targetPos = pTargetPos;
		isMoveToTargetPos = true;
	}
	public void MoToCurTarget()
	{
		StopAttack();
		MoveToTargetPos(curTarget.transform.position);
	}
	public bool NeedSelectTarget()//是否需要重新选择追踪目标
	{
		if (curTarget != null && !curTarget.IsDead() && IsInRange(curTarget, followDistance))
			return false;
		var character = GetOneCharacterInRange(warnDistance);
		return character != null;
	}
	public void SelectAdjustTarget() {
		var character = GetOneCharacterInRange(warnDistance);
		SetCurTarget(character);
	}
	private bool IsNeedBackToHome = false;
	public void BackToHome()
	{
		var distance = Vector3.Distance(this.transform.position, findTargetPos);
		if (distance >= 1)
		{
			IsNeedBackToHome = true;
			SetCurTarget(null);
			MoveToTargetPos(findTargetPos);
		}
		else
		{
			IsNeedBackToHome = false;
		}	
	}
	public bool NeedBackToHome()
	{
		if (IsNeedBackToHome)
			return true;
		if (curTarget == null)
			return false;
		if (curTarget.IsDead())
			return true;
		var distance = Vector3.Distance(this.transform.position, findTargetPos);
		return distance >= followDistance;
	}
	
	public void SetCurTarget(CharacterInput pCharacterInput){
		curTarget = pCharacterInput;
		findTargetPos = this.transform.position;
	}
	public void CmdDoNormalAttack()
	{
		var direction = curIsFaceRight ? Vector2.right : -Vector2.right;
		allIdentitys = GameObject.FindObjectsOfType<CharacterInput>(false);
		for (var i = 0; i < allIdentitys.Length; i++)
		{
			if (allIdentitys[i] != this)
			{
				if (IsInAtkRange(allIdentitys[i]))
				{
					allIdentitys[i].OnDamage((int)atk);
				}
			}
		}
	}
	public void Idle()
	{
		SetCurTarget(null);
		StopAttack();
		StopMove();
		characterAni.PlayAni("idle", 1);
	}
	public void FaceRight(bool pIsFaceRight)
	{
		int xS = isFaceRight == pIsFaceRight ? 1 : -1;
		curIsFaceRight = xS == 1;
		this.transform.localScale = new Vector3(oriScale.x * xS, oriScale.y, oriScale.z);
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
	public void SetHpInfo()
	{
		characterHp.SetHpInfo(curHp, MaxHp);
	}
	private bool isPlayMoveAni = false;
	private void FixedUpdate()
	{
		//if (identity.isClient)
		//	return;
		if (IsDead())
			return;
		if (isAttack)
		{
			characterAni.PlayAni("attack", 4, () => {
				isAttack = false;
				CmdDoNormalAttack();
			});
		}
		if (isMoveToTargetPos)
		{
			if (Vector3.Distance(targetPos, this.transform.position) <= 1)
			{
				isMoveToTargetPos = false;
			}
			else
			{
				if (targetPos.x > this.transform.position.x)
					moveDelta.x = moveSpeed;
				else
					moveDelta.x = -moveSpeed;
				this.transform.localPosition += moveDelta * Time.deltaTime;
				int xS = isFaceRight == moveDelta.x > 0 ? 1 : -1;
				curIsFaceRight = xS == 1;
				this.transform.localScale = new Vector3(oriScale.x * xS, oriScale.y, oriScale.z);
				characterAni.PlayAni("move", 3);
			}
		}
		else
		{
			if (isMove)
			{
				this.transform.localPosition += moveDelta * Time.deltaTime;
				int xS = isFaceRight == moveDelta.x > 0 ? 1 : -1;
				curIsFaceRight = xS == 1;
				this.transform.localScale = new Vector3(oriScale.x * xS, oriScale.y, oriScale.z);
				characterAni.PlayAni("move", 3);
			}
		}
	
		if (!isAttack && !isMove && !isMoveToTargetPos)
		{
			characterAni.PlayAni("idle", 1);
		}
	}

}
