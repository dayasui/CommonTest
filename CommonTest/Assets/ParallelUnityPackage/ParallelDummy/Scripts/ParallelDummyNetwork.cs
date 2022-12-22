using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ParallelDummy {
    
    public sealed class ParallelDummyNetwork : ParallelCommon.IParallelNetwork {
        public string Server { get; set; }
        public string WsEndPoint { get; set; }
        public string Token { get; set; }
        public string DeviceID { get; set; }
        public string Platform { get; set; }
        public string Version { get; set; }
        public string Language { get; set; }
        public int ChatGroupID { set; get; }
        public int ChatGroupRoomID { set; get; }
        public int ChatGroupRoomSessionID { set; get; }
        public int ChatGroupRoomSessionUserID { set; get; }
        public bool IsObserver { set; get; }
        public int Port { set; get; }
        public void Init(string server, string token, string device_id, string platform = "", string version = "",
            string language = "") {
            Server = server;
            Token = token;
            DeviceID = device_id;
            Platform = platform;
            Version = version;
            Language = language;
#if !UNITY_WEBGL
            // この設定をしないとPROTOCOL_ERRORが出る
            BestHTTP.HTTPManager.UseAlternateSSLDefaultValue = false;BestHTTP.HTTPManager.UseAlternateSSLDefaultValue = false;
#endif
        }
        
        public Dictionary<string, string> CreateHeaderBase(string token, string deviceId, bool isJson = false) {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", "Bearer " + token);
            header.Add("X-Device-Id", deviceId);
            if (!string.IsNullOrEmpty(this.Platform)) {
                header.Add("X-Client-Platform", this.Platform);
            }

            if (!string.IsNullOrEmpty(this.Version)) {
                header.Add("X-App-Version", this.Version);
            }

            if (!string.IsNullOrEmpty(this.Language)) {
                header.Add("Accept-Language", this.Language);
            }

            if (isJson) {
                header.Add("Content-Type", "application/json");
            }

            return header;
        }

        public void RequestCore(ParallelCommon.NetworkParam param, UnityAction<string> onComplete = null) {
            ParallelCommon.Network n = new ParallelCommon.Network();
            n.Send(param, (ParallelCommon.Network network) => {
                this.DebugOutputResponseLog(network);
                var jsonObj = new JSONObject(network.DataAsText);
                if (jsonObj.HasField("errors")) {
                    var errors = jsonObj.GetField("errors").list;
                    if (!ReferenceEquals(null, onComplete)) {
                        onComplete(network.DataAsText);
                    }
                } else {
                    if (!ReferenceEquals(null, onComplete)) {
                        onComplete(network.DataAsText);
                    }
                }
            });
        }

        public void DebugOutputResponseLog(ParallelCommon.Network network) {
            if (network.Response.Headers != null) {
                foreach (var pair in network.Response.Headers) {
                    Debug.Log($"Header key:{pair.Key}");
                    foreach (var s in pair.Value) {
                        Debug.Log($"Header Value:{s}");
                    }
                }
            } else {
                Debug.Log("no header");
            }

            if (network.Response.Cookies != null) {
                foreach (var cookie in network.Response.Cookies) {
                    Debug.Log($"cookie:{cookie.Name} {cookie.Value}");
                }
            } else {
                Debug.Log("Network Response no cookie");
            }
        }
    }
}
