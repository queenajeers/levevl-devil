using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    public GameObject deathPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Finish"))
        {
            Debug.Log("Level Finished!");
        }
        else if (other.gameObject.CompareTag("Trigger"))
        {
            Debug.Log("Entering here!");
            other.gameObject.GetComponent<TrapVisual>().Activate();
        }
        else if (other.gameObject.CompareTag("GameOver"))
        {
            Debug.Log("Game Over");
            Instantiate(deathPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
