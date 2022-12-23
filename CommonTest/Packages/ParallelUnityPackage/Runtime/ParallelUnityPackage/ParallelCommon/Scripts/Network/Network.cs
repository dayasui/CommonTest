using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using BestHTTP;
using UniRx;

namespace ParallelCommon {

    /// <summary>
    /// 通信処理
    /// </summary>
    public class Network {

        public enum NetworkResultCode {
            Success = 0,
            Error = 1,
            OtherError = 2,
        }

        private NetworkParam _param;
        private HTTPRequest _req = null;
        private HTTPResponse _resp = null;
        public HTTPResponse Response => _resp;
        private NetworkResultCode _resultCode;

        public NetworkResultCode ResultCode {
            get => this._resultCode;
            set => this._resultCode = value;
        }

        public string DataAsText => this._resp?.DataAsText ?? string.Empty;
        public int StatusCode => this._resp?.StatusCode ?? 0;
        private int _retryCount = 0;


        private UnityAction<Network> _onComplete;

        public UnityAction<Network> OnComplete {
            get => this._onComplete;
            set => this._onComplete = value;
        }

        /// <summary>
        /// 簡易送信処理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        public void Send(string url, string data) {
            System.Uri uri = new System.Uri(url + "/send");
            HTTPRequest req = new HTTPRequest(uri, HTTPMethods.Post, OnRequestFinished);
            req.AddHeader("Content-Type", "application/json");
            req.AddHeader("X-Kuiperbelt-Session", "user1");
            req.AddHeader("X-Kuiperbelt-Session", "user2");
            req.AddHeader("X-Kuiperbelt-Session", "user3");
            req.AddHeader("X-Kuiperbelt-Session", "user4");
            req.RawData = Encoding.UTF8.GetBytes(data);
            req.Timeout = TimeSpan.FromSeconds(60);
            req.ConnectTimeout = TimeSpan.FromSeconds(60);
            req.IsKeepAlive = true;
            req.Send();
        }

        /// <summary>
        /// 送信処理　呼び出し
        /// </summary>
        /// <param name="param"></param>
        /// <param name="onComplete"></param>
        public void Send(NetworkParam param, UnityAction<Network> onComplete) {
            this._param = param;
            this._onComplete = onComplete;
            this.SendCore(param);
        }

        /// <summary>
        /// 送信処理のコア
        /// </summary>
        /// <param name="param"></param>
        private void SendCore(NetworkParam param) {
            // HttpRequest設定
            Debug.Log("<color=magenta>[Network Send]</color>:" + param.url + "\n" + param.data + "</color>");
            System.Uri uri = new System.Uri(param.url);
            HTTPRequest req = new HTTPRequest(uri, param.httpMethods, OnRequestFinished);
            foreach (var pair in param.header) {
                req.AddHeader(pair.Key, pair.Value);
            }

            if (!string.IsNullOrEmpty(param.data)) {
                req.RawData = Encoding.UTF8.GetBytes(param.data);
            }

            req.Timeout = TimeSpan.FromSeconds(60);
            req.ConnectTimeout = TimeSpan.FromSeconds(60);
            req.IsKeepAlive = true;
            req.Send();
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
                if (!ReferenceEquals(null, this.OnComplete)) {
                    this.OnComplete(this);
                }
            }
        }

        /// <summary>
        /// リクエスト取得時の処理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="resp"></param>
        protected void OnRequestFinished(HTTPRequest req, HTTPResponse resp) {
            //  Debug.Log("Network OnRequestFinished state = " + req.State);
            this._req = req;
            this._resp = resp;
            switch (req.State) {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess) {
                        this._resultCode = NetworkResultCode.Success;
                    } else {
                        this._resultCode = NetworkResultCode.Error;
                    }

                    if (!ReferenceEquals(null, this.OnComplete)) {
                        Debug.Log("<color=magenta>[Network Complete]</color>:" + resp.StatusCode + "\n" +
                                  this.DataAsText);
                        this.OnComplete(this);
                    }

                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("<color=red>Request Finished with Error!</color> " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));
                    this.Retry(this._param);
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("<color=red>Request Aborted!</color>");
                    this.Retry(this._param);
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("<color=red>Connection Timed Out!</color>");
                    this.Retry(this._param);
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("<color=red>Processing the request Timed Out!</color>");
                    this.Retry(this._param);
                    break;

                default:
                    Debug.LogError("<color=red>Processing the request Other Error</color>");
                    this.Retry(this._param);
                    break;
            }
        }
    }
}