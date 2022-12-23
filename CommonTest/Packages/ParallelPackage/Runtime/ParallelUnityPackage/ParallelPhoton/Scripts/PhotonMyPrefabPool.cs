using UnityEngine;
using Photon.Pun;
using System.Reflection;

// 参考ページ
// https://www.fast-system.jp/unity-photon-resources-to-prefablist/

namespace ParallelPhoton {
    // PPhotonNetwork.Instantiateを名前ではなくprefabから行うようにする
    // 注意:TitleLoadingBaseにロードされたprefabがあることを前提としています
    public class PhotonMyPrefabPool : SingletonMonoBehaviourPunCallbacks<PhotonMyPrefabPool>, IPunPrefabPool {
        [SerializeField] private Transform _objectRoot = null;

        public void Start() {
            // Poolの生成イベントを書き換える
            PhotonNetwork.PrefabPool = this;
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) {
            return this.Instantiate(prefabId, position, rotation, this._objectRoot);
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation, Transform parent) {
            GameObject prefab = ParallelCommon.AssetManager.Instance.GetAssetByName(prefabId);
            if (prefab == null) {
                Debug.Log($"prefabが見つからないのでInstantiateできません");
                return null;
            }
            GameObject go = Instantiate(prefab, position, rotation, parent);
            go.SetActive(false); // TODO falseにする必要あるのか？

            string className = this.GetType().Name;
            string methodName = MethodBase.GetCurrentMethod().Name;
            Debug.Log($"Photon Instantiate at {className}:{methodName}");

            return go;
        }

        public void Destroy(GameObject go) {
            GameObject.Destroy(go);
        }

    }
}