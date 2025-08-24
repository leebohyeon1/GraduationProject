using UnityEngine;

public class Boss : Enemy {
    public override void parryied()
    {
        base.parryied();
        // Boss-specific logic when parried
        Debug.Log("Boss parried! Increasing gauge.");
        // player.IncreaseGauge(5); // Increase gauge more for the boss
    }
    
    // Additional Boss-specific methods can be added here   
}