using UnityEngine;
using BehaviorTree;

public class Condition_DistanceHysteresis : ConditionNode
{
    public enum CheckType
    {
        Inside,  // 踰붿쐞 ?덉뿉 ?덉뼱???깃났 (嫄곕━媛 媛源뚯썙吏硫?True)
        Outside  // 踰붿쐞 諛뽰뿉 ?덉뼱???깃났 (嫄곕━媛 硫?댁?硫?True)
    }

    [Header("Target Settings")]
    [Tooltip("嫄곕━瑜?????곸엯?덈떎. 鍮꾩뼱?덉쑝硫??먮룞?쇰줈 ?뚮젅?댁뼱(runner.player)瑜??寃잛쑝濡??⑸땲??")]
    public GameObject target;

    [Header("Distance Settings")]
    [Tooltip("吏꾩엯 ?먯젙 嫄곕━ (??嫄곕━蹂대떎 媛源뚯썙吏硫?吏꾩엯?쇰줈 媛꾩＜)")]
    public float minRange = 5.0f;

    [Tooltip("?댄깉 ?먯젙 嫄곕━ (??嫄곕━蹂대떎 硫?댁?硫??댄깉濡?媛꾩＜)")]
    public float maxRange = 6.0f;

    [Tooltip("寃?????(Inside: ?덉そ?대㈃ ?깃났, Outside: 諛붽묑履쎌씠硫??깃났)")]
    public CheckType checkType = CheckType.Inside;

    // --- ?대? ?곹깭 蹂??---
    private bool _currentState = false;
    private bool _hasInitialized = false;

    // Clone ???고???蹂??珥덇린??
    public override Node Clone()
    {
        Condition_DistanceHysteresis node = Instantiate(this);
        node.target = this.target; // ?몃??먯꽌 二쇱엯???寃잛씠 ?덈떎硫?蹂듭궗
        node.minRange = this.minRange;
        node.maxRange = this.maxRange;
        node.checkType = this.checkType;
        
        node._hasInitialized = false;
        node._currentState = false;
        return node;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // ?寃잛씠 ?좊떦?섏? ?딆븯?ㅻ㈃ ?뚮젅?댁뼱濡?珥덇린???쒕룄
        if (target == null && runner != null)
        {
            target = runner.player.gameObject;
            // 二쇱쓽: ?ш린???좊떦?섎㈃ Clone???몄뒪?댁뒪?먮쭔 ?곸슜?섎?濡??덉쟾??
            // ?섏?留?GameObject????媛앹껜?대?濡?SO????λ릺吏 ?딄쾶 二쇱쓽 (Clone?대?濡?愿쒖갖??
            // 紐낆떆?곸쑝濡?蹂?섏뿉 ?ｌ? ?딄퀬 CheckCondition?먯꽌 泥섎━?섎뒗 寃껋씠 ??源붾걫?????덉쓬
        }
    }

    protected override bool CheckCondition()
    {
        // 1. ?ㅼ젣 ?寃?寃곗젙 (?곗꽑?쒖쐞: 吏곸젒 ?좊떦??target > runner.player)
        Transform currentTargetTrans = null;

        if (target != null)
        {
            currentTargetTrans = target.transform;
        }
        else if (runner != null && runner.player != null)
        {
            currentTargetTrans = runner.player.transform;
        }

        // ?寃잛씠 ?좏슚?섏? ?딆쑝硫?(二쎄굅???щ씪吏? -> False 諛섑솚
        if (currentTargetTrans == null)
        {
            _currentState = false;
            _hasInitialized = false; // ?寃잛쓣 ?껋뿀?쇰?濡?珥덇린???곹깭濡?由ъ뀑
            return false;
        }

        // 2. 嫄곕━ 怨꾩궛 (Y異?臾댁떆 - ?됰㈃ 嫄곕━)
        float dist = Vector3.Distance(
            new Vector3(runner.transform.position.x, 0, runner.transform.position.z),
            new Vector3(currentTargetTrans.position.x, 0, currentTargetTrans.position.z)
        );

        // 3. ?덉뒪?뚮━?쒖뒪 濡쒖쭅
        if (!_hasInitialized)
        {
            // ?곹솴 B: 泥??ㅽ뻾 ???꾧꺽??寃??
            if (checkType == CheckType.Inside)
            {
                _currentState = dist <= minRange;
            }
            else // Outside
            {
                _currentState = dist >= maxRange;
            }
            _hasInitialized = true;
        }
        else
        {
            // 珥덇린???댄썑: 踰꾪띁 援ш컙(Hysteresis) ?곸슜
            if (checkType == CheckType.Inside)
            {
                if (_currentState)
                {
                    // True ?곹깭 ?좎?: MaxRange 諛뽰쑝濡??섍?湲??꾧퉴吏
                    if (dist > maxRange) _currentState = false;
                }
                else
                {
                    // False ?곹깭 ?좎?: MinRange ?덉쑝濡??ㅼ뼱?ㅺ린 ?꾧퉴吏
                    if (dist <= minRange) _currentState = true;
                }
            }
            else // Outside
            {
                if (_currentState)
                {
                    // True ?곹깭 ?좎?: MinRange ?덉쑝濡??ㅼ뼱?ㅺ린 ?꾧퉴吏
                    if (dist <= minRange) _currentState = false;
                }
                else
                {
                    // False ?곹깭 ?좎?: MaxRange 諛뽰쑝濡??섍?湲??꾧퉴吏
                    if (dist > maxRange) _currentState = true;
                }
            }
        }
        return _currentState;
    }

    public override void initNode()
    {
        base.initNode();
        _hasInitialized = false;
        _currentState = false;
    }
}
