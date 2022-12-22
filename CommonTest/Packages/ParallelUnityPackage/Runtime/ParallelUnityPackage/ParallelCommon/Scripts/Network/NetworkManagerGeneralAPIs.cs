using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;
using BestHTTP;
using UnityEngine;
using System.Linq;

namespace ParallelCommon {
    [Serializable]
    public class ParalleWebGLlUserData : ParallelUserDataBase {

    }

    [Serializable]
    public class ParalleWebGLlUserDataAll {
        [Serializable]
        public class ParalleWebGLlUserPayload {
            public ParalleWebGLlUserData user;
        };

        public bool ok;
        public ParalleWebGLlUserPayload payload;
    }


    [Serializable]
    public class ParallelGeneralAPISessionData {
        //public Internal_app internallApp; // 使わないので定義しない
        public int chat_group_room_session_id;

        //ParallelUserData のうち、gender、twitter_id、is_connectedは値が入らないが気にしない。
        public ParalleWebGLlUserData owner;
        public ParalleWebGLlUserData[] guests;
        public ParalleWebGLlUserData[] web_guests;

        public int OwnerUserID {
            get {
                if (ReferenceEquals(owner, null)) {
                    Debug.LogWarning("OwnerUserID Is 0");
                    return 0;
                }

                return owner.id;
            }
        }

        public string websocket_endpoint;

        public ParalleWebGLlUserData GetUser(int id) {
            if (owner.id == id) {
                return owner;
            }

            var user = guests.FirstOrDefault(e => e.id == id);
            if (user != null) {
                return user;
            }

            user = web_guests.FirstOrDefault(e => e.id == id);
            return user;
        }

        public List<ParalleWebGLlUserData> GetAllUsers() {
            List<ParalleWebGLlUserData> users = new List<ParalleWebGLlUserData>();
            users.Add(this.owner);
            users.AddRange(guests);
            users.AddRange(web_guests);
            return users;
        }
    }


    [Serializable]
    public class ParallelGeneralAPISessionDataAll {
        public bool ok;
        public ParallelGeneralAPISessionData payload;
    }

    public partial class NetworkManager {
        private string GeneralBaseURL => Path.Combine(this.BaseURL, "general_game");

        /*
        curl \
        -H "Accept: application/json" \
        -H "Authorization: Bearer ${token}" \
        -H "X-Device-Id: ${device_id}" \
        -H "X-Client-Platform: ${platform}" \
        -H "X-App-Version: ${app_version}" \
        -H "Accept-Language: ${accept_language}" \
        -X GET \
        ${api_host}/v1/chat_groups/${chat_group_id}/chat_group_rooms/${chat_group_room_id}/chat_group_room_sessions/${chat_group_room_session_id}/general_game
        */

        /*
        {
            "ok": true,
            "payload": {
                "chat_group_room_session_id": 134,
                "owner": {
                    "id": 78,
                    "name": "♪なつき♪",
                    "user_image": "https://storage.googleapis.com/cocalero-dev/user_image/6a130b19-63a9-4c8e-acd6-5f4a4bd32a19.png"
                },
                "guests": [
                {
                    "id": 79,
                    "name": "あいみん",
                    "user_image": "https://storage.googleapis.com/cocalero-dev/user_image/f0d71609-e962-493f-a872-f8e660a34d65.png"
                }
                ],
                "websocket_endpoint": "ws://127.0.0.1:12345/connect"
            }
        }
        */



        private Dictionary<string, string> createGeneralAPIHeaderBase(string token, string deviceId) {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Accept", "application/json");
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

            return header;
        }

        public void GeneralAPIGetInfo(UnityAction<ParallelGeneralAPISessionData> onComplete = null) {
            string url = GeneralBaseURL;
            Dictionary<string, string> header = this.createHeaderBase(this.Token, this.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            this.RequestCore(nParam, (string json) => {
                ParallelGeneralAPISessionDataAll data = JsonUtility.FromJson<ParallelGeneralAPISessionDataAll>(json);
                this.ChatGroupRoomSessionID = data.payload.chat_group_room_session_id;
                this.WsEndPoint = data.payload.websocket_endpoint;
                Debug.Log("end_point:" + this.WsEndPoint);
                if (onComplete != null) {
                    onComplete(data.payload);
                }
            });
        }

        /*
        curl \
        -H "Accept: application/json" \
        -H "Authorization: Bearer ${token}" \
        -H "X-Device-Id: ${device_id}" \
        -H "X-Client-Platform: ${platform}" \
        -H "X-App-Version: ${app_version}" \
        -H "Accept-Language: ${accept_language}" \
        -H "X-App-Library-Id: webgame" \
        -H "Content-Type: application/json" \
        -d '{"command":{"payload": {"version":1,"type":"gameCommand","game":{"command":{}}}}}' \
        -X POST \
        ${api_host}/v1/chat_groups/${chat_group_id}/chat_group_rooms/${chat_group_room_id}/chat_group_room_sessions/${chat_group_room_session_id}/general_game/commands
        */

        /*
        {
            "version": 1,
            "type": "gameCommand",
            "game": {
                "command": {}
            }
        }
        */
        public void GeneralAPISendCommand(string dataJson, UnityAction<Network> onComplete = null) {
            string url = Path.Combine(GeneralBaseURL, "commands");
            Dictionary<string, string> header = this.createGeneralAPIHeaderBase(this.Token, this.DeviceID);
            header.Add("X-App-Library-Id", "unity");
            header.Add("Content-Type", "application/json");
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Post;
            nParam.header = header;
            nParam.data = dataJson;
            this.RequestCore(nParam, onComplete);
        }

        public void GeneralAPIAuth(UnityAction<ParalleWebGLlUserData> onComplete = null) {
            string url = Path.Combine(GeneralBaseURL, "auth");
            Dictionary<string, string> header = this.createGeneralAPIHeaderBase(this.Token, this.DeviceID);
            header.Add("X-App-Library-Id", "unity");
            header.Add("Content-Type", "application/json");
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            this.RequestCore(nParam, (string json) => {
                ParalleWebGLlUserDataAll dataAll = JsonUtility.FromJson<ParalleWebGLlUserDataAll>(json);
                if (onComplete != null) {
                    onComplete(dataAll.payload.user);
                }
            });
        }

        // test code
        public void Ready() {

            JSONObject game = new JSONObject();
            JSONObject command = new JSONObject();
            JSONObject payload = new JSONObject();
            JSONObject jObjRoot = new JSONObject();
            payload.AddField("version", 1);
            payload.AddField("type", "Ready");
            payload.AddField("game", game);
            command.AddField("payload", payload);
            jObjRoot.AddField("command", command);

            Debug.Log("テストだよ " + jObjRoot.ToString());
            this.GeneralAPISendCommand(jObjRoot.ToString());
        }
    }
}
