using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using BestHTTP;
using System;

namespace ParallelCommon {

    [System.Serializable]
    public class GameOrderData {
        public int order;
        public int user_id;
        public double start_time;
        public double end_time;
        public double schedule_end_time;
        public bool disabled;
        public string text;
        public bool chain_success;
    }


    public partial class NetworkManager : SingletonMonoBehaviour<NetworkManager> {
        public void Init(string server, string token, string device_id, string platform = "", string version = "",
            string language = "") {
            Server = server;
            Token = token;
            DeviceID = device_id;
            Platform = platform;
            Version = version;
            Language = language;

            Debug.Log($"Network Init:{server} {token} {device_id} {platform} {version} {language}");
        }

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

        public bool IsObserver {
            get {
                if (ApplicationDataManager.Instance != null) {
                    return ApplicationDataManager.Instance.IsObserver;
                }
                return false;
            }
        }

        private string BaseURL =>
            $"{this.Server}/v1/chat_groups/{this.ChatGroupID}/chat_group_rooms/{this.ChatGroupRoomID}/chat_group_room_sessions/{this.ChatGroupRoomSessionID}";

        protected override void Awake() {
            base.Awake();
#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
            // 理由をきちんと調査していないが、この設定をしないとPROTOCOL_ERRORが出る
            BestHTTP.HTTPManager.UseAlternateSSLDefaultValue = false;
#endif
        }

        private Dictionary<string, string> createHeaderBase(string token, string deviceId, bool isJson = false) {
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

        public void RequestCore(NetworkParam param, UnityAction<string> onComplete = null) {
            Network n = new Network();
            n.Send(param, (Network network) => {
                this.debugOutputResponseLog(network);
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

        private void debugOutputResponseLog(Network network) {
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

        private void RequestCore(NetworkParam param, UnityAction<Network> onComplete = null) {
            Network n = new Network();
            n.Send(param, (Network network) => {
                var jsonObj = new JSONObject(network.DataAsText);
                Debug.Log("DataAsText:" + network.DataAsText);
                if (jsonObj.HasField("message")) {
                    // 通常のAPIのレスポンス
                    network.ResultCode = Network.NetworkResultCode.Success;
                    var msg = jsonObj.GetField("message").list;
                    if (msg[0]?.str == "Success") {
                        // 特に今は何もしていない。
                    }

                    if (!ReferenceEquals(null, onComplete)) {
                        onComplete(network);
                    }
                } else if (jsonObj.HasField("ok")) {
                    // 共通APIのレスポンス
                    bool ok = jsonObj.GetField("ok").b;
                    network.ResultCode = ok ? Network.NetworkResultCode.Success : Network.NetworkResultCode.Error;
                    if (!ReferenceEquals(null, onComplete)) {
                        onComplete(network);
                    }

                } else {
                    if (jsonObj.HasField("errors")) {
                        var errors = jsonObj.GetField("errors").list;
                        network.ResultCode = Network.NetworkResultCode.Error;
                        if (!ReferenceEquals(null, onComplete)) {
                            onComplete(network);
                        }
                    } else {
                        // 分岐は作ったが他のパターンは仕様で決まっていない。
                        // resultCode は statusCode に依存
                        if (!ReferenceEquals(null, onComplete)) {
                            onComplete(network);
                        }
                    }
                }
            });
        }

        private void RequestCore(NetworkParam param, UnityAction<string, int> onComplete = null) {
            Network n = new Network();
            n.Send(param, (Network network) => {
                if (!ReferenceEquals(null, onComplete)) {
                    onComplete(network.DataAsText, network.StatusCode);
                }
            });
        }

        public void UpdateWebSocketEndPoint(UnityAction onComplete = null) {
            this.UpdateWebSocketEndPoint(this.Token, this.DeviceID, onComplete);
        }

        public void UpdateWebSocketEndPoint(string token, string deviceId, UnityAction onComplete) {
            string url = $"{this.Server}/websocket_endpoint";
            Dictionary<string, string> header = this.createHeaderBase(token, deviceId);

            NetworkParam param = new NetworkParam();
            param.url = url;
            param.httpMethods = HTTPMethods.Get;
            param.header = header;
            this.RequestCore(param, (json) => {
                JSONObject jsonObj = new JSONObject(json);
                this.WsEndPoint = jsonObj.GetField("endpoint").str;
                Debug.Log("WebSocket EndPoint:" + this.WsEndPoint);
                if (onComplete != null) {
                    onComplete();
                }
            });
        }

        //-d '{"command":{"payload": {"version":1,"type":"gameCommand","game":{"command":{}}}}}' \
        /// <summary>
        /// 共通API用の共通設定を生成
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public JSONObject CreateGeneralAPIBaseJson(string channelStr, JSONObject data = null) {
            JSONObject channel = new JSONObject();
            JSONObject payload = new JSONObject();
            JSONObject command = new JSONObject();
            JSONObject jObjRoot = new JSONObject();

            channel.AddField("channel", channelStr);
            payload.AddField("identifier", channel);
            if (data != null) {
                payload.AddField("message", data);
            }

            command.AddField("payload", payload);
            jObjRoot.AddField("command", command);
            return jObjRoot;
        }

        // GET /v1/chat_groups/:chat_group_id/chat_group_rooms/:chat_group_room_id/chat_group_room_users
        public void GetChatRoomSessionID(UnityAction<string> onComplete = null) {
            NetworkParam param = new NetworkParam();
            param.url = string.Format("{0}/v1/chat_groups/{1}/chat_group_rooms/{2}/chat_group_room_users",
                this.Server, this.ChatGroupID, this.ChatGroupRoomID);
            param.httpMethods = HTTPMethods.Get;
            param.header = this.createHeaderBase(this.Token, this.DeviceID);
            this.RequestCore(param, onComplete);
        }

        /*
        curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -H "Content-Type: application/json" \
         -d "{\"pictchain\":{\"draw_time\":45,\"input_time\":30,\"question_num\":2}}" \
         -X POST ${SERVER}/v1/chat_groups/${CHAT_GROUP_ID}/chat_group_rooms/${CHAT_GROUP_ROOM_ID}/chat_group_room_sessions/${CHAT_GROUP_ROOM_SESSION_ID}/pictchain
        */
        public void StartGame(int drawTime, int inputTime, int questionNum, UnityAction<string> onComplete) {
            this.StartGame(this.ChatGroupID, this.ChatGroupRoomID, this.ChatGroupRoomSessionID, drawTime, inputTime,
                questionNum, this.Token, this.DeviceID, onComplete);
        }

        public void StartGame(int chatGroupId, int chatGroupRoomId, int sessionID, int drawTime, int inputTime,
            int questionNum, string token, string deviceId, UnityAction<string> onComplete) {
            string url =
                $"{this.Server}/v1/chat_groups/{chatGroupId}/chat_group_rooms/{chatGroupRoomId}/chat_group_room_sessions/{sessionID}/pictchain";
            Dictionary<string, string> header = this.createHeaderBase(token, deviceId);
            header.Add("Content-Type", "application/json");
            string dataStr =
                $"{{\"pictchain\":{{\"draw_time\":{drawTime},\"input_time\":{inputTime},\"question_num\":{questionNum}}}}}";
            NetworkParam param = new NetworkParam();
            param.url = url;
            param.httpMethods = HTTPMethods.Post;
            param.header = header;
            param.data = dataStr;
            this.RequestCore(param, onComplete);
        }

        /// <summary>
        /// 部屋を抜ける
        /// /v1/chat_groups/1422/chat_group_rooms/xxx/chat_group_room_sessions/xxx/chat_group_room_session_users/${CHAT_GROUP_ROOM_SESSION_USER_ID}
        /// </summary>
        /// <param name="onComplete"></param>
        public void CloseRoomSession(UnityAction<string> onComplete) {
            string url =
                $"{this.Server}/v1/chat_groups/{this.ChatGroupID}/chat_group_rooms/{this.ChatGroupRoomID}/chat_group_room_sessions/{this.ChatGroupRoomSessionID}/chat_group_room_session_users/{this.ChatGroupRoomSessionUserID}";
            Dictionary<string, string> header = this.createHeaderBase(this.Token, this.DeviceID);
            NetworkParam param = new NetworkParam();
            param.url = url;
            param.httpMethods = HTTPMethods.Delete;
            param.header = header;
            this.RequestCore(param, onComplete);
        }

        /// <summary>
        /// アプリを終了して部屋を抜ける
        /// /v1/chat_groups/1422/chat_group_rooms/2996/chat_group_room_sessions/7441
        /// </summary>
        /// <param name="onComplete"></param>
        public void CloseRoomSessions(UnityAction<string> onComplete) {
            string url =
                $"{this.Server}/v1/chat_groups/{this.ChatGroupID}/chat_group_rooms/{this.ChatGroupRoomID}/chat_group_room_sessions/{this.ChatGroupRoomSessionID}";
            Dictionary<string, string> header = this.createHeaderBase(this.Token, this.DeviceID);
            NetworkParam param = new NetworkParam();
            param.url = url;
            param.httpMethods = HTTPMethods.Delete;
            param.header = header;
            this.RequestCore(param, onComplete);
        }

        /*
        PATCH /v1/chat_groups/{chat_group_id}/chat_group_rooms/{chat_group_room_id}/chat_group_room_sessions/{chat_group_room_session_id}/pictchain/pictchain_orders/{order}
        */
        public void EndTextInput(string text, int order, UnityAction<string> onComplete) {
            this.EndTextInput(text, this.ChatGroupID, this.ChatGroupRoomID, this.ChatGroupRoomSessionID, order,
                this.Token, this.DeviceID, onComplete);
        }

        public void EndTextInput(string text, int chatGroupId, int chatGroupRoomId, int chatGroupRoomSessionId,
            int order, string token, string deviceId, UnityAction<string> onComplete) {
            string url =
                $"{this.Server}/v1/chat_groups/{chatGroupId}/chat_group_rooms/{chatGroupRoomId}/chat_group_room_sessions/{chatGroupRoomSessionId}/pictchain/pictchain_orders/{order}";
            Dictionary<string, string> header = this.createHeaderBase(token, deviceId);
            header.Add("Content-Type", "application/json");
            string dataStr = $"{{\"pictchain_order\":{{\"text\":\"{text}\"}}}}";
            NetworkParam param = new NetworkParam();
            param.url = url;
            param.httpMethods = HTTPMethods.Patch;
            param.header = header;
            param.data = dataStr;
            this.RequestCore(param, onComplete);
        }

        /*
        DELETE /v1/chat_groups/{chat_group_id}/chat_group_rooms/{chat_group_room_id}/chat_group_room_sessions/{chat_group_room_session_id}/pictchain
        */

        public void GameQuit(UnityAction<string> onComplete) {
            this.GameQuit(this.ChatGroupID, this.ChatGroupRoomID, this.ChatGroupRoomSessionID, this.Token,
                this.DeviceID, onComplete);
        }

        public void GameQuit(int chatGroupId, int chatGroupRoomId, int chatGroupRoomSessionId, string token,
            string deviceId, UnityAction<string> onComplete) {
            string url =
                $"{this.Server}/v1/chat_groups/{chatGroupId}/chat_group_rooms/{chatGroupRoomId}/chat_group_room_sessions/{chatGroupRoomSessionId}/pictchain";
            Dictionary<string, string> header = this.createHeaderBase(token, deviceId);
            NetworkParam param = new NetworkParam();
            param.url = url;
            param.httpMethods = HTTPMethods.Delete;
            param.header = header;
            this.RequestCore(param, onComplete);
        }

        public void DownLoadTexture2D(ParallelUserData userData, UnityAction<Texture2D> onComplete) {
            string url = userData.user_image;
            this.DownLoadTexture2D(url, onComplete);
        }

        public void DownLoadTexture2D(string url, UnityAction<Texture2D> onComplete) {
            NetworkTextureDownload n = new NetworkTextureDownload();
            n.Send(url, onComplete);
        }
    }
}
