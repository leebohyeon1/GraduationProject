using BehaviorTree;
using UnityEngine;

namespace BehaviorTree
{
    public abstract class ServiceNode : Node
    {
        public float UpdateInterval = 0.3f;
        protected float lastExecutionTime = 0f;

        // [수정 1] Clone은 자식에서 구현하므로 여기선 추상화하거나, 기본 구현을 유지하되 주의해야 합니다.
        // 현재 작성하신 자식 클래스들(Service_PressureMove 등)이 Instantiate(this)를 잘 쓰고 있다면
        // 이 부분은 당장 에러는 안 나지만, abstract 클래스는 CreateInstance가 불가능하므로 수정하는 게 좋습니다.
        public override Node Clone()
        {
            // 자식 클래스에서 Instantiate(this)로 처리하므로 이 기본 함수는 호출되지 않아야 합니다.
            return Instantiate(this); 
        }

        protected override NodeState OnUpdate()
        {
            if(Time.time - lastExecutionTime >= UpdateInterval)
            {
                OnServiceLogic();
                lastExecutionTime = Time.time;
            }
            
            // [핵심 수정] SUCCESS -> RUNNING
            // RUNNING을 반환해야 OnExit가 호출되지 않고, 다음 프레임에도 OnEnter 없이 바로 Update가 불립니다.
            return NodeState.RUNNING;
        }

        protected abstract void OnServiceLogic();

        public override void initNode()
        {
            base.initNode();
            lastExecutionTime = -UpdateInterval; 
        }
    }
}