using Firebase.Auth;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class AuthManager : MonoBehaviour
{
    [SerializeField] int _gameSceneIndex = 1;
    [Header("Login")]
    [SerializeField] TMP_InputField _emailInputLogin;
    [SerializeField] TMP_InputField _passwordInputLogin;
    [Header("Register")]
    [SerializeField] TMP_InputField _emailInputRegister;
    [SerializeField] TMP_InputField _passwordInputRegister;

    [Header("Events")]
    [SerializeField] UnityEvent OnSuccessLogin;
    [SerializeField] UnityEvent OnSuccessRegister;
    [SerializeField] UnityEvent OnPlayerDataNull;
    public void RegisterUser() // Send Register Request
    {
        string email = _emailInputRegister.text;
        string password = _passwordInputRegister.text;

        StartCoroutine(RegisterUserCoroutine(email, password));
    }

    private IEnumerator RegisterUserCoroutine(string email, string password)
    {
        var registerTask = FirebaseInitializer.Auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.IsCanceled)
        {
            CaseLogger.Instance.Logger("RegisterUser was canceled.", Color.red);
            yield break;
        }
        if (registerTask.IsFaulted)
        {
            CaseLogger.Instance.Logger("RegisterUser encountered an error: " + registerTask.Exception, Color.red);
            yield break;
        }
        if (registerTask.IsCompletedSuccessfully)
        {
            Debug.Log("SuccesRegister");
            OnSuccessRegister?.Invoke();

        }
    }
    public void LoginUser()//Send Login Request.
    {
        string email = _emailInputLogin.text;
        string password = _passwordInputLogin.text;
        StartCoroutine(LoginCoroutine(email, password));
    }
    private IEnumerator LoginCoroutine(string email, string password)
    {

        var loginTask = FirebaseInitializer.Auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.IsCanceled)
        {
            CaseLogger.Instance.Logger("LoginUser was canceled.", Color.yellow);
            yield break;
        }
        if (loginTask.IsFaulted)
        {
            CaseLogger.Instance.Logger("LoginUser encountered an error: " + loginTask.Exception, Color.red);
            yield break;
        }

        FirebaseInitializer.User = loginTask.Result.User;
        OnSuccessLogin?.Invoke();
        CaseLogger.Instance.Logger("User logged in successfully!", Color.green);
        DatabaseManager.Instance.OnLoadData.AddListener(OnLoadPlayerData);
        yield return DatabaseManager.Instance.LoadDataCoroutine();
        if(DatabaseManager.Instance.PlayerData.PlayerName == null|| DatabaseManager.Instance.PlayerData.PlayerName.Length == 0)
        {
            OnPlayerDataNull?.Invoke();
            Debug.Log("OnPlayerDataNull");
            CaseLogger.Instance.Logger("User logged in successfully!", Color.green);
        }
    }
    public void OnLoadPlayerData()
    {
        SceneManager.LoadScene(_gameSceneIndex);
        DatabaseManager.Instance.OnLoadData?.RemoveListener(OnLoadPlayerData);
    }


}

