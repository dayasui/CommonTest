using System.Text.RegularExpressions;
using UnityEngine;
using Photon.Pun;

namespace ParallelPhoton {
    public class PhotonUtil : MonoBehaviour {
        // パラレル未使用のときにChatGroupRoomIDが取れないのでそれの対策
        static public int GetGroupRoomSessionID() {
            return ParallelCommon.NetworkManager.Instance != null ? ParallelCommon.NetworkManager.Instance.ChatGroupRoomSessionID : 0;
        }

        static public string[] GetPlayerIDsWithoutSelf() {
            int[] userIDs = ParallelCommon.UserDataManager.Instance != null ? ParallelCommon.UserDataManager.Instance.GetPlayerIDsWithoutSelf() : null;
            string[] result = new string[userIDs.Length];
            for (int index = 0; index < userIDs.Length; ++index) {
                result[index] = $"{GetGroupRoomSessionID()}_{userIDs[index]}";
            }

            return result;
        }

        /// <summary>
        /// PhotonのuserIDからパラレルのroomIDとuserIDを取得する
        /// </summary>
        static public void GetParallelGroupRoomIDAndUserIDFromPhotonUserID(string photonUserID, out int groupRoomID, out int userID) {
            groupRoomID = 0;
            userID = 0;
            Regex reg = new Regex(@"(\d+)_(\d+)");
            Match m = reg.Match(photonUserID);
            if (m.Groups.Count != 3) {
                return;
            }

            groupRoomID = int.Parse(m.Groups[1].Value);
            userID = int.Parse(m.Groups[2].Value);
        }

        static public GameObject Instantiate(GameObject prefab, object[] data = null) {
            return Instantiate(prefab, Vector3.zero, Quaternion.identity, data);
        }


        static public GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, object[] data = null) {
            GameObject go = null;
            if (PhotonNetwork.IsConnectedAndReady) {
                go = PhotonNetwork.Instantiate(prefab.name, position, rotation, data: data);
            } else {
                go = GameObject.Instantiate(prefab, position, rotation);
            }
            return go;
        }
    }
}