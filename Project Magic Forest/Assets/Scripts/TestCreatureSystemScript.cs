using UnityEngine;

public class TestCreatureSystemScript : MonoBehaviour
{
    [SerializeField] private string testMessage = "Creature system test script is active";

    private void Start()
    {
        Debug.Log(testMessage);
    }
}
