using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPolicy
{
    private List<AIAction> allActions = new List<AIAction>() { 
        new BackToHomeAction(),new AttackToTargetAction(),new MoveToTargetAction(),new SelectTargetAction(),
        new MoveAction(),new IdleAction()
    };
    private AIAction curAiAction;
    private MonsterAI bindAi;
    public AIPolicy(MonsterAI pMonsterAi) {
        bindAi = pMonsterAi;
    }
    public void OnRun(CharacterInput[] pAllCharacters)
    {
        for (var i = 0; i < allActions.Count; i++)
        {
            if (allActions[i].IsPass(bindAi, pAllCharacters))
            {
                curAiAction = allActions[i];
                break;
            }
        }
        curAiAction.OnAcion(bindAi);
    }
}
public class BackToHomeAction : AIAction
{
    public override AIConditioner Conditioner { get { if (conditioner == null) conditioner = new BackToHomeConditioner(); return conditioner; } }
    public override void OnAcion(MonsterAI pBindAi)
    {
        pBindAi.BackToHome();
    }
}
public class AttackToTargetAction : AIAction
{
    public override AIConditioner Conditioner { get { if (conditioner == null) conditioner = new AttackToTargetConditioner(); return conditioner; } }
    public override void OnAcion(MonsterAI pBindAi)
    {
        pBindAi.StopMove();
        pBindAi.FaceToCurTarget();
        pBindAi.Attack();
    }
}
public class MoveToTargetAction : AIAction
{
    public override AIConditioner Conditioner { get { if (conditioner == null) conditioner = new MoveToTargetConditioner(); return conditioner; } }
    public override void OnAcion(MonsterAI pBindAi)
    {
        pBindAi.MoToCurTarget();
    }
}
public class SelectTargetAction : AIAction
{
    public override AIConditioner Conditioner { get { if (conditioner == null) conditioner = new ForcusTargetConditioner(); return conditioner; } }
    public override void OnAcion(MonsterAI pBindAi)
    {
        pBindAi.SelectAdjustTarget();
    }
}
public class MoveAction : AIAction {
    public override AIConditioner Conditioner { get { if (conditioner == null) conditioner = new MoveConditioer(); return conditioner; } }
    public override void OnAcion(MonsterAI pBindAi)
    {
        pBindAi.OnLoopMove();
    }
}
public class IdleAction : AIAction {
    public override AIConditioner Conditioner { get {if (conditioner == null) conditioner = new IdleConditoner(); return conditioner; } }
    public override void OnAcion(MonsterAI pBindAi)
    {
        pBindAi.Idle();
    }
}
public class AIAction
{
    protected AIConditioner conditioner;

    public virtual AIConditioner Conditioner { get { if (conditioner == null) conditioner = new AIConditioner(); return conditioner; } }
    public virtual void OnAcion(MonsterAI pBindAi) { }
    public bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters) {
        return Conditioner.IsPass(pBindAi, pAllCharacters);
    }
}

public class BackToHomeConditioner : AIConditioner//目标超出追踪范围，返回原地
{
    public override bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return pBindAi.NeedBackToHome();
    }
}
public class AttackToTargetConditioner : AIConditioner//攻击目标
{
    public override bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return pBindAi.CurTargetInRange(pBindAi.atkDistance);
    }
}
public class MoveToTargetConditioner : AIConditioner//追踪目标
{
    public override bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return pBindAi.CurTargetInRange(pBindAi.followDistance);
    }
}
public class ForcusTargetConditioner : AIConditioner//锁定目标
{
    public override bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return pBindAi.NeedSelectTarget();
    }
}
public class MoveConditioer : AIConditioner //巡逻d 
{
    public override bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return !pBindAi.IsHaveTarget();
    }
}
public class IdleConditoner: AIConditioner//待机
{
    public override bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return !pBindAi.IsDead();
    }
}
public class AIConditioner
{
    public virtual bool IsPass(MonsterAI pBindAi, CharacterInput[] pAllCharacters)
    {
        return true;
    }
}

