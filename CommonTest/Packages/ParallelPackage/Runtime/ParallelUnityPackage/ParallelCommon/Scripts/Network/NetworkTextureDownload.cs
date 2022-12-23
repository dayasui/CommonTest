using System;
using UnityEngine;
using UnityEngine.Events;
using BestHTTP;
using UniRx;

namespace ParallelCommon {
    public class NetworkTextureDownload {

        private NetworkParam _param = new NetworkParam();
        private UnityAction<Texture2D> _onComplete;

        public UnityAction<Texture2D> OnComplete {
            get => this._onComplete;
            set => this._onComplete = value;
        }

        UnityAction _onError;
        bool _isDone;
        bool _isRequestComplete;
        private int _retryCount = 0;
        private string _requestUrl = string.Empty;


        public void Send(string url, UnityAction<Texture2D> onComplete) {
            this._param.url = url;
            this._onComplete = onComplete;
            this.SendCore(this._param);
        }

        public void SendCore(NetworkParam param) {
            // HttpRequest設定
            Uri uri = new System.Uri(param.url);
            HTTPRequest req = new HTTPRequest(uri, OnRequestFinished);
            req.Timeout = TimeSpan.FromSeconds(60);
            req.ConnectTimeout = TimeSpan.FromSeconds(60);
            req.IsKeepAlive = false;
            req.Send();
        }

        protected void OnRequestFinished(HTTPRequest req, HTTPResponse resp) {
            //  Debug.Log("Network OnRequestFinished state = " + req.State);
            switch (req.State) {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    this._onComplete?.Invoke(resp.DataAsTexture2D);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("Request Finished with Error! " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));
                    this.Retry(this._param);
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    this.Retry(this._param);
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("Connection Timed Out!");
                    this.Retry(this._param);
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("Processing the request Timed Out!");
                    this.Retry(this._param);
                    break;

                default:
                    this.Retry(this._param);
                    break;
            }
        }

        /// <summary>
        /// 通信エラー時のリトライ
        /// </summary>
        /// <param name="param"></param>
        private void Retry(NetworkParam param) {
            //HTTPManager.OnQuit();
            if (this._retryCount < param.retryMax) {
                this._retryCount++;
                Observable.Timer(TimeSpan.FromMilliseconds(param.retryIntervalMS)).Subscribe(_ => {
                    // 一定時間後にリトライ
                    this.SendCore(param);
                });
            } else {
                this.OnComplete?.Invoke(null);
            }
        }
    }
}