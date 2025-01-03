using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Transform ropeEnd; 
    public Transform[] allowedAreas; // Oyuncunun içinde kalması gereken alanlar
    public float shakeAmount = 0.1f; // Titreşim miktarı
    public int shakeIterations = 5; // Titreşim tekrarı
    private Rigidbody2D rb;
    private Vector3 originalPosition;
    
    public bool swiping = false;
    public bool joystick = false;
    public bool raft = false;
    
    public FloatingJoystick variableJoystick;
    private Vector3 targetPosition;
    public RectTransform canvasRectTransform;
    public float moveDuration = 0.2f;
    public float speed = 2000f;
    public RectTransform imageRectTransform;
    public TMP_Dropdown gameModeDropdown;

    private void Start()
    {
        targetPosition = transform.localPosition;
        swiping = true;
        RopeRotation.instance.thisRope.SetActive(false);
        RopeRotation.instance.thisRope = this.gameObject.transform.GetChild(0).gameObject;
        scoreText.text = score.ToString();
        Application.targetFrameRate = 600;
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void ChangedGameMode()
    {
        switch (gameModeDropdown.value)
        {
            case 0:
                swiping = true;
                joystick = false;
                raft = false;
                break;
            case 1:
                swiping = false;
                joystick = true;
                raft = false;
                break;
            case 2:
                swiping = false;
                joystick = false;
                raft = true;
                break;
        }
    }

    private void Update()
    {
        CheckDamageEnemy();
        CheckItemProximity(); 
        
        
        if(swiping)
        {
            RopeRotation.instance.thisRope.SetActive(false);

            foreach (var area in allowedAreas)
            {
                area.gameObject.SetActive(false);
            }
            
            
            if (!_isMoving)
            {
                HandleSwipeInput();
            }
            else
            {
                MoveToTarget();
            }
        }
        else if (joystick)
        {
            float vertical = variableJoystick.Vertical;
            float horizontal = variableJoystick.Horizontal;

            Vector3 direction = new Vector3(horizontal, vertical, 0).normalized;

            Vector3 newPosition = imageRectTransform.localPosition + direction * speed * Time.deltaTime;
    
            targetPosition = GetClampedPosition(newPosition);
            if (IsPositionValid(targetPosition))
            {
                imageRectTransform.DOLocalMove(targetPosition, moveDuration).SetEase(Ease.Linear);
                RotateCharacter(direction);
            }
        }
        else if(raft)
        {
            RopeRotation.instance.thisRope.SetActive(true);
            
            foreach (var area in allowedAreas)
            {
                area.gameObject.SetActive(true);
            }
            
            if (Input.GetMouseButtonDown(0)) 
            {
                if (IsWithinAllowedAreas(ropeEnd.localPosition))
                {
                    MoveToRopeEnd();
                }
            }
        }
    }
    
    bool IsPositionValid(Vector3 targetPosition)
    {
        float radius = 0.5f;

        Collider2D hit = Physics2D.OverlapCircle(targetPosition, radius, LayerMask.GetMask("Obstacle"));

        return hit == null;
    }
    
    void RotateCharacter(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            imageRectTransform.DORotate(new Vector3(0, 0, angle - 90f), moveDuration).SetEase(Ease.Linear);
        }
    }
    
    
    Vector3 GetClampedPosition(Vector3 targetPos)
    {
        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;
        float characterWidth = imageRectTransform.rect.width;
        float characterHeight = imageRectTransform.rect.height;

        float clampedX = Mathf.Clamp(targetPos.x, -canvasWidth / 2 + characterWidth / 2, canvasWidth / 2 - characterWidth / 2);
        float clampedY = Mathf.Clamp(targetPos.y, -canvasHeight / 2 + characterHeight / 2, canvasHeight / 2 - characterHeight / 2);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
    


    bool IsWithinAllowedAreas(Vector3 position)
    {
        foreach (Transform area in allowedAreas)
        {
            Collider2D collider = area.GetComponent<Collider2D>();
            if (collider != null)
            {
                Debug.Log($"Checking area: {collider.bounds}");
                Debug.Log($"RopeEnd position: {ropeEnd.position}");
                if (collider.bounds.Contains(ropeEnd.position))
                {
                    Debug.Log("Position is within allowed area.");
                    return true;
                }
            }
        }
        return false;
    }


    
    
    void MoveToRopeEnd()
    {
        transform.position = ropeEnd.position;
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
