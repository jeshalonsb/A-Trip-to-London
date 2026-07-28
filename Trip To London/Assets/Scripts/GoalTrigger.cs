using UnityEngine;

public class SoccerGoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SoccerBallKick ball = other.GetComponent<SoccerBallKick>();

        if (ball == null)
        {
            ball = other.GetComponentInParent<SoccerBallKick>();
        }

        if (ball == null)
            return;

        SoccerMinigameManager.Instance.ScoreGoal();
    }
}