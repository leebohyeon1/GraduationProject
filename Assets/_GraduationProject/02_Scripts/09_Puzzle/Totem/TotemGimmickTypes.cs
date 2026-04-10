public enum TotemDestructibleType
{
    Fragile,
    Hardened
}

public enum TotemGimmickState
{
    Alive,
    Processing,
    Destroyed
}

public enum TotemGimmickFeedbackType
{
    Interact,
    HitSuccess,
    HitBlocked,
    DestroyedStart,
    DestroyedComplete,
    DissolveStart,
    DissolveLoop,
    DissolveThreshold,
    DissolveComplete
}
