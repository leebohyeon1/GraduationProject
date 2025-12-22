// SubTree.cs
using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "SubTree", menuName = "BehaviorTree/Sub Tree")]
public class SubTree : ActionTree // ActionTree를 상속받아 Clone 등의 기능을 재사용
{
    // 내용은 ActionTree와 거의 동일합니다.
    // 이름만으로도 역할을 구분할 수 있게 됩니다.
}