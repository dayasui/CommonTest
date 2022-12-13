using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelCommon {

    [Serializable]
    public class ParallelChatRoomSessionData {
        //public Internal_app internallApp; // 使わないので定義しない
        public int id;
        public int app_id;
        public string app_type;

        [Serializable]
        public class SessionUser {
            public bool is_player;

            //  public int id; // userIDと紛らわしいので消します userIDはuser.idを参照してください
            public bool is_deleted;
            public bool is_owner;
            public bool is_observer;
            public ParallelUserData user;
        }

        public SessionUser[] chat_group_room_session_users;

        public int OwnerUserID {
            get {
                var userData = this.chat_group_room_session_users.FirstOrDefault(e => e.is_owner);
                if (ReferenceEquals(userData, null)) {
                    Debug.LogWarning("OwnerUserID Is 0");
                    return 0;
                }

                return userData.user.id;
            }
        }
    }
}
