using System;

namespace ParallelCommon {
    [Serializable]
    public class ParallelInitData {
        public int chat_group_id;
        public string api_host;
        public string device_id;
        public string token;
        public int user_id;
        public int chat_group_room_id;
        public string platform;
        public string app_version;
        public string accept_language;
        public bool is_observer;

        public int chat_group_room_session_id; // for WebGL
        public Constants.MatchingMode matching_mode;
    }
}