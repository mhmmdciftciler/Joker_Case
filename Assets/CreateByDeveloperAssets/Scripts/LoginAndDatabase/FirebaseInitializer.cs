using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseAuth Auth;
    public static FirebaseUser User;
    public UnityEvent OnFirebaseAppRun;
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted&&task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase App Running");
                Auth = FirebaseAuth.DefaultInstance;
                OnFirebaseAppRun?.Invoke();
                CaseLogger.Instance.Logger("Firebase App Running", Color.green);
            }
            else
            {
                CaseLogger.Instance.Logger($"Could not resolve all Firebase dependencies: {task.Result}", Color.red);
                Debug.Log($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
        
    }
    private void OnApplicationQuit()
    {
        Auth.Dispose();
        Auth = null;
    }
}
