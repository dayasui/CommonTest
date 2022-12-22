using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


namespace ParallelCommon {
    public class AssetManager : SingletonMonoBehaviour<AssetManager> {
        private Dictionary<string, List<UnityEngine.Object>> _assetsDict = new Dictionary<string, List<UnityEngine.Object>>();

        
        public void LoadAssetByLabel<T>(string label, UnityAction onComplete) where T : UnityEngine.Object {
            ResourceLoader.LoadAssetsAsync<T>(label, (assets) => {
                this._assetsDict[label] = new List<UnityEngine.Object>();
                foreach (var asset in assets) {
                    Debug.Log("loaded asset:" + asset.name);
                    this._assetsDict[label].Add(asset);
                }
                onComplete?.Invoke();
            });
        }

        public List<UnityEngine.Object> GetAssetsByLabel(string label) {
            if (!this._assetsDict.ContainsKey(label)) {
                return null;
            }
            List<UnityEngine.Object> assets = this._assetsDict[label];
            return assets;
        }

        public List<T> GetAssetsByLabel<T>(string label) where T : UnityEngine.Object {
            if (!this._assetsDict.ContainsKey(label)) {
                return null;
            }
            List<T> assets = new List<T>();
            foreach (var a in this._assetsDict[label]) {
                T asset = a as T;
                if (asset != null) {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public GameObject GetAssetByName(string name) {
            foreach (var pair in this._assetsDict) {
                System.Object o = pair.Value.FirstOrDefault(e => e.name == name);
                GameObject go = o as GameObject;
                if (go != null) {
                    return go;
                }
            }
            return null;
        }
    }
}