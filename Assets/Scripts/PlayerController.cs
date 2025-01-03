using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 _swipeStart;
    private Vector2 _swipeEnd;
    private bool _isMoving = false;
    private Vector3 _targetPosition;
    public int score = 0;
    public GameObject enemy;
    public GameObject item;

    public Transform[] spawnPoints; // Item spawn noktaları
    public float moveSpeed = 5000f;
    public float itemDetectionRadius = 50f; // Item'e yakınlık eşiği
    
    public TextMeshProUGUI scoreText;
    
    
    private void Start()
    {
        scoreText.text = score.ToString();
        Application.targetFrameRate = 600;
    }

    private void Update()
    {
        CheckDamageEnemy();
        CheckItemProximity(); // Item ile mesafe kontrolü
        if (!_isMoving)
        {
            HandleSwipeInput();
        }
        else
        {
            MoveToTarget();
        }
    }

    public void Reset()
    {
        transform.position = new Vector3(180, 1088, 0);
        score = 0;
        gameObject.SetActive(true);
        scoreText.text = score.ToString();
    }

    private void HandleSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _swipeStart = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _swipeEnd = Input.mousePosition;
            Vector2 swipe = _swipeEnd - _swipeStart;

            if (swipe.magnitude > 50f)
            {
                if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                {
                    if (swipe.x > 0) SetTarget(Vector3.right);
                    else SetTarget(Vector3.left);
                }
                else
                {
                    if (swipe.y > 0) SetTarget(Vector3.up);
                    else SetTarget(Vector3.down);
                }
            }
        }
    }

    void SetTarget(Vector3 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity);
        if (hit.collider && hit.collider.CompareTag($"Obstacle"))
        {
            _targetPosition = hit.point - (Vector2)direction * 0.5f;
        }
        _isMoving = true;
    }

    void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
        {
            _isMoving = false;
        }
    }

    void CheckDamageEnemy()
    {
        Vector3 myPosition = transform.position;
        Vector3 enemyPosition = enemy.transform.position;

        float distance = Vector3.Distance(myPosition, enemyPosition);

        if (distance <= 50f)
        {
            gameObject.SetActive(false);
            _isMoving = false;
            Reset();
        }
    }

    void CheckItemProximity()
    {
        Vector3 myPosition = transform.position;
        Vector3 itemPosition = item.transform.position;

        float distance = Vector3.Distance(myPosition, itemPosition);

        if (distance <= itemDetectionRadius)
        {
            MoveItemToRandomSpawn();
            score++; 
            scoreText.text = score.ToString();
        }
    }

    void MoveItemToRandomSpawn()
    {
        if (spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            item.transform.position = spawnPoints[randomIndex].position;
        }
    }
}
