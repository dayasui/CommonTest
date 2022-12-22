using UnityEngine;

/// <summary>
/// MonoBehaviourのSingleton
/// </summary>
/// <typeparam name="T">SingletonにするClass</typeparam>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;

    /// <summary>
    /// Instanceへのアクセス
    /// </summary>
    public static T Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType(typeof(T)) as T;

                if (instance == null) {
                    // Debug.LogError(typeof(T) + " is nothing");
                }
            }

            return instance;
        }
    }

    protected virtual void Awake() {
        // Awake時に別のInstanceが無いか確認する
        CheckInstance();
    }

    /// <summary>
    /// 自身以外にInstanceが無いか確認し、存在する場合は自身を削除する
    /// </summary>
    /// <returns>True:自身が唯一のInstance False:既存のInstanceがあった（自身を削除）</returns>
    protected bool CheckInstance() {
        if (this == Instance) {
            return true;
        }

        Debug.Log("Destroy" + this.name);
        Destroy(this);

        return false;
    }

    /// <summary>
    /// 型Uでinstanceを取得する
    /// 多段継承した場合に使われることを想定
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public static U GetInstance<U>() where U : MonoBehaviour {
        return instance as U;       
    }
}
