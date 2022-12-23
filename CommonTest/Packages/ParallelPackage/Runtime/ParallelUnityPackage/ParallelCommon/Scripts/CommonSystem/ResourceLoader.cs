using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniRx;

namespace ParallelCommon {

    public static class ResourceLoader {
        public static void LoadAssetAsync<T>(string path, UnityAction<T> onComplete) where T : Object {
            /*
            Observable.FromCoroutine<T>(observer => ResoucesLoadAsync<T>(observer, path))
                .Subscribe(e => { onComplete(e); }, () => { Debug.Log("LoadAsync OnComplete");
            });
            */
            Observable.FromCoroutine<T>(observer => AddressableLoadAssetAsync<T>(observer, path))
                .Subscribe(e => { onComplete(e); }, () => { Debug.Log("LoadAsync OnComplete path:" + path); });
        }

        private static IEnumerator ResourcesLoadAsync<T>(System.IObserver<T> observer, string path) where T : Object {
            //非同期ロード開始
            ResourceRequest resourceRequest = Resources.LoadAsync<T>(path);
            while (!resourceRequest.isDone) {
                yield return 0;
            }

            observer.OnNext(resourceRequest.asset as T);
            observer.OnCompleted();
        }

        private static IEnumerator AddressableLoadAssetAsync<T>(System.IObserver<T> observer, string path)
            where T : Object {
            var handle = Addressables.LoadAssetAsync<T>(path);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                observer.OnNext(handle.Result as T);
            }

            observer.OnCompleted();

        }

        public static void LoadAssetsAsync<T>(string label, UnityAction<IList<T>> onComplete) where T : Object {
            Observable.FromCoroutine<IList<T>>(observer => AddressableLoadAssetsAsync<T>(observer, label))
                .Subscribe(e => { onComplete(e); }, () => { Debug.Log("LoadAsync OnComplete"); });
        }

        private static IEnumerator AddressableLoadAssetsAsync<T>(System.IObserver<IList<T>> observer, string label)
            where T : Object {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                observer.OnNext(handle.Result as IList<T>);
            }

            observer.OnCompleted();

        }
    }
}
