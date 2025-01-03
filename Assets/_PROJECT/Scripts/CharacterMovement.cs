using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    public float speed = 10f;
    public FloatingJoystick variableJoystick;
    public RectTransform imageRectTransform;
    public float moveDuration = 0.2f;
    public RectTransform canvasRectTransform;
    public RectTransform foodRectTransform;
    public AudioSource audioSource;
    public AudioClip eatSound;
    public TMP_Text fishCounterText;
    public Image timerBar;
    public RectTransform startPoint; 
    private Vector3 targetPosition;
    private int fishCount = 0;
    private float timeSinceLastEat = 0f;
    public float timeLimit = 10f;

    public Transform[] enemyList;

    void Start()
    {
        targetPosition = imageRectTransform.localPosition;
        UpdateFishCounter();
    }

    void Update()
    {
        CheckDamageEnemy();
        float count = fishCount;
        if (count == 0)
        {
            count = 1;
        }
        if (count > 10)
        {
            count = 10;
        }
        timeLimit = 10 / count;
        if (timeSinceLastEat < timeLimit)
        {
            timeSinceLastEat += Time.deltaTime;
            UpdateTimerBar();
        }
        else
        {
            ResetFishCount();
        }
    }

    void FixedUpdate()
    {
        float vertical = variableJoystick.Vertical;
        float horizontal = variableJoystick.Horizontal;

        Vector3 direction = new Vector3(horizontal, vertical, 0).normalized;
        targetPosition = imageRectTransform.localPosition + direction * speed * Time.deltaTime;
        targetPosition = GetClampedPosition(targetPosition);

        imageRectTransform.DOLocalMove(targetPosition, moveDuration).SetEase(Ease.Linear);
        RotateCharacter(direction);
        CheckFoodCollision();
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

    void CheckFoodCollision()
    {
        if (Vector3.Distance(imageRectTransform.localPosition, foodRectTransform.localPosition) < 60f)
        {
            SpawnFood();
            PlayEatSound();
            IncrementFishCount();
        }
    }

    void SpawnFood()
    {
        float randomX = Random.Range(-300f, 300f);
        float randomY = Random.Range(-300f, 300f);
        foodRectTransform.localPosition = new Vector3(randomX, randomY, foodRectTransform.localPosition.z);
    }

    void PlayEatSound()
    {
        if (audioSource != null && eatSound != null)
        {
            audioSource.PlayOneShot(eatSound);
        }
    }

    void RotateCharacter(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            imageRectTransform.DORotate(new Vector3(0, 0, angle - 90f), moveDuration).SetEase(Ease.Linear);
        }
    }

    void IncrementFishCount()
    {
        fishCount++;
        timeSinceLastEat = 0f;
        UpdateFishCounter();
    }

    void UpdateFishCounter()
    {
        if (fishCounterText != null)
        {
            fishCounterText.text = fishCount.ToString();
        }
    }

    void ResetFishCount()
    {
        fishCount = 0;
        UpdateFishCounter();
        ResetTimerBar();
    }

    void UpdateTimerBar()
    {
        if (timerBar != null)
        {
            timerBar.fillAmount = timeSinceLastEat / timeLimit;
        }
    }

    void ResetTimerBar()
    {
        if (timerBar != null)
        {
            timerBar.fillAmount = 0f;
        }
    }

    private void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CheckDamageEnemy()
    {
        foreach (var enemy in enemyList)
        {
            Vector3 myPosition = transform.position;
            Vector3 enemyPosition = enemy.transform.position;
            float distance = Vector3.Distance(myPosition, enemyPosition);

            if (distance <= 50f)
            {
                Reset();
            }
        }
        

        
    }
}
