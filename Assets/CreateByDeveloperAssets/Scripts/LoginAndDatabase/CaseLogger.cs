using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CaseLogger : MonoBehaviour
{
    public static CaseLogger Instance;
    [SerializeField] private TextMeshProUGUI _logger;
    [SerializeField] private GameObject _logObject;
    [SerializeField] private float logTime = 3;
     private  float logTimeCounter = 3;
    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if(logTimeCounter>0)
        {
            _logObject.SetActive(true);
        }
        else if(logTimeCounter <= 0 && _logObject.activeInHierarchy)
        {
            _logObject.SetActive(false);
        }
        logTimeCounter-= Time.deltaTime;
    }
    public void Logger(string message, Color color)
    {
        logTimeCounter = logTime;
        _logger.text = message;
        _logger.color = color;  
    }
}
