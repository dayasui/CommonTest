using System.Collections.Generic;
using UnityEngine.Events;

namespace ParallelCommon {
    public interface IParallelNetwork {
        string Server { get; set; }
        string WsEndPoint { get; set; }
        string Token { get; set; }
        string DeviceID { get; set; }
        string Platform { get; set; }
        string Version { get; set; }
        string Language { get; set; }
        int ChatGroupID { get; set; }
        int ChatGroupRoomID { get; set; }
        int ChatGroupRoomSessionID { get; set; }
        int ChatGroupRoomSessionUserID { get; set; }
        bool IsObserver { get; }
        int Port { get; set; }
        void Init(string server, string token, string device_id, string platform = "", string version = "",
            string language = "");

        Dictionary<string, string> CreateHeaderBase(string token, string deviceId, bool isJson = false);
        void RequestCore(ParallelCommon.NetworkParam param, UnityAction<string> onComplete = null);
        void DebugOutputResponseLog(ParallelCommon.Network network);
    }
}
