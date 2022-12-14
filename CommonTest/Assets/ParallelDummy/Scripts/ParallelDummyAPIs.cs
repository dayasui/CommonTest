using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BestHTTP;
using System;
using ParallelCommon;


namespace ParallelDummy {

    [Serializable]
    public class NetChatGroupData {
        public int id;
        public string type;
        public string name;
        public ParallelUserData[] users;
        public enum Type {
            None,
            Individual,
            Group,
        }
        public Type GroupType {
            get {
                switch (type) {
                    case "ChatGroup::Individual":
                        return Type.Individual;

                    case "ChatGroup::Group":
                        return Type.Group;

                    default:
                        return Type.None;
                }
            }
        }
    }

    [Serializable]
    public class NetChatGroupDataArray {
        public NetChatGroupData[] chat_groups;
    }

    [Serializable]
    public class NetChatGroupRoomSession : ParallelChatRoomSessionData {
    }

    [Serializable]
    public class NetChatGroupRoomData {
        public int id;
        public string name;
        public int chat_group_id;
        public ParallelUserData[] users;
        public NetChatGroupRoomSession[] chat_group_room_sessions;
    }


    [Serializable]
    public class NetChatGroupRoomDataArray {
        public NetChatGroupRoomData[] chat_group_rooms;
    }

    [Serializable]
    public class NetJoinRoomData {
        public NetChatGroupRoomData chat_group_room;
    }

    [Serializable]
    public class NetUserListDataArray {
        public ParallelUserData[] users;
    }

    public sealed class ParallelDummyAPIs {
        /*
        curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -H "Content-Type: application/json" \
         -X GET ${SERVER}/v1/chat_groups
        */
        public void GetChatGroupData(IParallelNetwork parallelNetwork, UnityAction<NetChatGroupDataArray> onComplete) {
            string url = parallelNetwork.Server + "/v1/chat_groups";
            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);

            List<int> groupIDList = new List<int>();

            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (jsonStr) => {
                Debug.Log("<color=cyan>[GetChatGroupData]</color>:" + jsonStr);
                NetChatGroupDataArray dataArray = JsonUtility.FromJson<NetChatGroupDataArray>(jsonStr);
                if (onComplete != null) {
                    onComplete(dataArray);
                }
            });
        }

        
        public void GetChatGroupID(IParallelNetwork parallelNetwork, int[] userIDs, UnityAction<int[]> onComplete) {
            string url = parallelNetwork.Server + "/v1/chat_groups";

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            ParallelCommon.Network n = new ParallelCommon.Network();
            List<int> groupIDList = new List<int>();

            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (jsonStr) => {
                Debug.Log("<color=cyan>[GetChatGroupID]</color>:" + jsonStr);
                NetChatGroupDataArray dataArray = JsonUtility.FromJson<NetChatGroupDataArray>(jsonStr);
                foreach (var group in dataArray.chat_groups) {
                    int count = userIDs.Length;
                    foreach (var user in group.users) {
                        if (Array.IndexOf(userIDs, user.id) >= 0) {
                            count--;
                            if (count == 0) {
                                groupIDList.Add(group.id);
                                Debug.Log("group id:" + group.id);
                                break;
                            }
                        }
                    }
                }

                if (onComplete != null) {
                    onComplete(groupIDList.ToArray());
                }
            });
        }
        
        public void GetChatGroupRoomID(IParallelNetwork parallelNetwork, UnityAction<NetChatGroupRoomDataArray> onComplete) {
            string url = string.Format("{0}/v1/chat_groups/{1}/chat_group_rooms", parallelNetwork.Server, parallelNetwork.ChatGroupID);

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (jsonStr) => {
                try {
                    NetChatGroupRoomDataArray dataArray = JsonUtility.FromJson<NetChatGroupRoomDataArray>(jsonStr);
                    if (onComplete != null) {
                        onComplete(dataArray);
                    }
                }
                catch {
                    Debug.Log("<color=yellow>Error GetChatGroupRoomID</color> in:" + url + " out:" + jsonStr);
                    if (onComplete != null) {
                        onComplete(null);
                    }
                }
            });
        }

        /*
        curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -X GET ${SERVER}/ v1 / chat_groups /${ CHAT_GROUP_ID}/ chat_group_rooms
        */
        public void GetChatGroupRoomID(IParallelNetwork parallelNetwork, int[] userIDs, UnityAction<int> onComplete) {
            string url = string.Format("{0}/v1/chat_groups/{1}/chat_group_rooms", parallelNetwork.Server, parallelNetwork.ChatGroupID);
            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (jsonStr) => {
                Debug.Log("<color=cyan>[GetChatGroupRoomID]</color>:" + jsonStr);
                NetChatGroupRoomDataArray dataArray = JsonUtility.FromJson<NetChatGroupRoomDataArray>(jsonStr);

                int roomID = -1;
                foreach (var room in dataArray.chat_group_rooms) {
                    int count = userIDs.Length;
                    foreach (var user in room.users) {
                        if (Array.IndexOf(userIDs, user.id) >= 0) {
                            count--;
                            if (count == 0) {
                                roomID = room.id;
                                Debug.Log($"room id:{room.id}");
                                break;
                            }
                        }
                    }

                    if (roomID >= 0) {
                        break;
                    }
                }

                if (onComplete != null) {
                    onComplete(roomID);
                }
            });
        }


        /*
        curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -X POST ${SERVER}/v1/chat_groups/${CHAT_GROUP_ID}/chat_group_rooms/${CHAT_GROUP_ROOM_ID}/chat_group_room_users
        */
        public void JoinRoom(IParallelNetwork parallelNetwork, UnityAction<NetJoinRoomData, string> onComplete) {
            string url = parallelNetwork.Server + "/v1/chat_groups/" + parallelNetwork.ChatGroupID + "/chat_group_rooms/" + parallelNetwork.ChatGroupRoomID +
                         "/chat_group_room_users";
            Debug.Log("<color=cyan>[JoinRoom]</color>:" + url);

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Post;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (str) => {
                JSONObject json = new JSONObject(str);
                string error = null;
                NetJoinRoomData data = null;
                if (json.HasField("errors")) {
                    error = json.GetField("errors")[0].str;
                } else {
                    data = JsonUtility.FromJson<NetJoinRoomData>(str);
                }

                if (onComplete != null) {
                    onComplete(data, error);
                }
            });
        }

        /*
        curl -H "Authorization: Bearer ${TOKEN}"
         -H "X-Device-Id: ${DEVICE_ID}"
         -H "Content-Type: application/json"
         -d "{\"chat_group_room_session_user\":{\"user_id\":${USER_ID}},\"is_owner\":false}"
         -X POST ${SERVER}/v1/chat_groups/${CHAT_GROUP_ID}/chat_group_rooms/${CHAT_GROUP_ROOM_ID}/chat_group_room_sessions/${CHAT_GROUP_ROOM_SESSION_ID}/chat_group_room_session_users
        */
        public void JoinSession(IParallelNetwork parallelNetwork, bool isObserver, int userId, UnityAction<JSONObject, string> onComplete) {
            string url =
                $"{parallelNetwork.Server}/v1/chat_groups/{parallelNetwork.ChatGroupID}/chat_group_rooms/{parallelNetwork.ChatGroupRoomID}/chat_group_room_sessions/{parallelNetwork.ChatGroupRoomSessionID}/chat_group_room_session_users";

            Debug.Log("path: " + url);

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            header.Add("Content-Type", "application/json");
            string is_observer = isObserver ? "true" : "false"; // {isObserver}だと"True" or "False"になってサーバが受付けない
            string dataStr =
                $"{{\"chat_group_room_session_user\":{{\"user_id\":{userId},\"is_owner\":false,\"is_observer\":{is_observer}}}}}";
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Post;
            nParam.header = header;
            nParam.data = dataStr;
            parallelNetwork.RequestCore(nParam, (str) => {
                JSONObject json = new JSONObject(str);
                string error = null;
                if (json.HasField("errors")) {
                    error = json.GetField("errors")[0].str;
                }

                if (onComplete != null) {
                    onComplete(json, error);
                }
            });
        }

        /*
    curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -X DELETE ${SERVER}/v1/chat_groups/${CHAT_GROUP_ID}/chat_group_rooms/${CHAT_GROUP_ROOM_ID}/chat_group_room_users
        */
        public void LeaveRoom(IParallelNetwork parallelNetwork, UnityAction onComplete) {
            string url =
                $"{parallelNetwork.Server}/v1/chat_groups/{parallelNetwork.ChatGroupID}/chat_group_rooms/{parallelNetwork.ChatGroupRoomID}/chat_group_room_users";
            Debug.Log("<color=cyan>[LeaveRoom]</color>:" + url);

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Delete;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (string str) => {
                if (onComplete != null) {
                    onComplete();
                }
            });
        }

        /*
            curl --location --request GET 'https://staging-v2.parallelgame.com/v1/users/:user_id/reset' \
    --header 'X-Device-Id: test' \
    --header 'Authorization: Bearer XXX'
         */
        public void Reset(IParallelNetwork parallelNetwork, int userID, UnityAction<string> onComplete) {
            string url = $"{parallelNetwork.Server}/v1/users/{userID}/reset";
            Debug.Log("<color=cyan>[Reset]</color>:" + url);

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (str) => {
                Debug.Log("<color=cyan>[reset result]</color>:" + str);
                if (onComplete != null) {
                    onComplete(str);
                }
            });
        }

        /*
        curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -X GET ${SERVER}/v1/chat_groups/${CHAT_GROUP_ID}/chat_group_rooms/${CHAT_GROUP_ROOM_ID}/chat_group_room_sessions/candidates
        */
        
        public void GetUserList(IParallelNetwork parallelNetwork, UnityAction<NetUserListDataArray> onComplete) {
            string url = parallelNetwork.Server + "/v1/chat_groups/" + parallelNetwork.ChatGroupID + "/chat_group_rooms/" + parallelNetwork.ChatGroupRoomID +
                         "/chat_group_room_sessions/candidates";
            Debug.Log("<color=cyan>[GetUserID]</color>" + url);

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Get;
            nParam.header = header;
            parallelNetwork.RequestCore(nParam, (jsonStr) => {
                Debug.Log("<color=cyan>[GetUserList out]</color>:" + jsonStr);
                NetUserListDataArray dataArray = JsonUtility.FromJson<NetUserListDataArray>(jsonStr);
                if (onComplete != null) {
                    onComplete(dataArray);
                }
            });
        }

        /*
        curl -H "Authorization: Bearer ${TOKEN}" \
         -H "X-Device-Id: ${DEVICE_ID}" \
         -H "Content-Type: application/json" \
         -d "{\"chat_group_room_session\":{\"app_type\":\"internal\",\"app_id\":3,\"user_ids\":${USER_IDS}}}" \
         -X POST ${SERVER}/v1/chat_groups/${CHAT_GROUP_ID}/chat_group_rooms/${CHAT_GROUP_ROOM_ID}/chat_group_room_sessions
        */
        public void CreateRoomSession(IParallelNetwork parallelNetwork, int appID, int[] userIDs, UnityAction<ParallelChatRoomSessionDataAll> onComplete) {
            string url = parallelNetwork.Server + "/v1/chat_groups/" + parallelNetwork.ChatGroupID + "/chat_group_rooms/" + parallelNetwork.ChatGroupRoomID +
                         "/chat_group_room_sessions";

            Dictionary<string, string> header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            header.Add("Content-Type", "application/json");

            string userIDsStr = "[";
            for (int i = 0; i < userIDs.Length; ++i) {
                string id = userIDs[i].ToString();
                if (i == 0) {
                    userIDsStr += id;
                } else {
                    userIDsStr += "," + id;
                }
            }

            userIDsStr += "]";
            int app_id = appID;

            string data =
                $"{{\"chat_group_room_session\":{{\"app_type\":\"internal\",\"app_id\":{app_id},\"user_ids\":{userIDsStr}}}}}";
            Debug.Log("<color=cyan>[CreateRoomSession in]</color>:" + data);
            NetworkParam nParam = new NetworkParam();
            nParam.url = url;
            nParam.httpMethods = HTTPMethods.Post;
            nParam.header = header;
            nParam.data = data;
            parallelNetwork.RequestCore(nParam, (jsonStr) => {
                Debug.Log("<color=cyan>[CreateRoomSession out]</color>:" + jsonStr);
                ParallelChatRoomSessionDataAll sessionData =
                    JsonUtility.FromJson<ParallelChatRoomSessionDataAll>(jsonStr);
                if (onComplete != null) {
                    onComplete(sessionData);
                }
            });
        }
        
        // GET /v1/chat_groups/:chat_group_id/chat_group_rooms/:chat_group_room_id/chat_group_room_users
        public void GetChatRoomSessionID(IParallelNetwork parallelNetwork, UnityAction<string> onComplete = null) {
            NetworkParam param = new NetworkParam();
            param.url = string.Format("{0}/v1/chat_groups/{1}/chat_group_rooms/{2}/chat_group_room_users",
                parallelNetwork.Server, parallelNetwork.ChatGroupID, parallelNetwork.ChatGroupRoomID);
            param.httpMethods = HTTPMethods.Get;
            param.header = parallelNetwork.CreateHeaderBase(parallelNetwork.Token, parallelNetwork.DeviceID);
            parallelNetwork.RequestCore(param, onComplete);
        }

        // UnityWebRequestのテストコード
        // 実際は使わない
        public IEnumerator GetChatGroupID2(IParallelNetwork parallelNetwork) {
            string url = parallelNetwork.Server + "/v1/chat_groups";
            var req = new UnityEngine.Networking.UnityWebRequest(url, "GET");
            req.SetRequestHeader("Authorization", "Bearer 8h2Rg3PQ4QAJFuBLizfNuYWS");
            req.SetRequestHeader("X-Device-Id", "D363C93B-A9DE-4F7C-BD99-FC5B69A826A6");
            req.downloadHandler = (UnityEngine.Networking.DownloadHandler) new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return req.SendWebRequest();
            if (req.isNetworkError || req.isHttpError) {
                Debug.Log("Network error:" + req.error);
            } else {
                Debug.Log("Succeeded:" + req.downloadHandler.text);
            }
        }
    }
}
