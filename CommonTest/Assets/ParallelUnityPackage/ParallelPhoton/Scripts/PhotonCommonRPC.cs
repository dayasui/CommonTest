using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace ParallelPhoton {
    public class PhotonCommonRPC : SingletonMonoBehaviour<PhotonCommonRPC> {
        private PhotonView _photonView;
        public UnityAction OnForecStartGame { set; get; }

        // Start is called before the first frame update
        void Start() {
            this._photonView = GetComponent<PhotonView>();
        }

        // Update is called once per frame
        void Update() {

        }

        public void UpdateTeamData(PhotonTeamDataList teamData) {
            string json = JsonUtility.ToJson(teamData);
            this._photonView.RPC("RPCUpdateTeamData", RpcTarget.Others, json);
        }

        [PunRPC]
        public void RPCUpdateTeamData(string json) {
            PhotonUserDataManager.Instance.UpdateTeamData(json);
        }

        public void Ready(string userID) {
            this._photonView.RPC("RPCReady", RpcTarget.All, userID);
        }

        [PunRPC]
        public void RPCReady(string userID) {
            PhotonUserDataManager.Instance.Ready(userID);
        }

        /// <summary>
        /// 強制ゲーム開始
        /// 暫定関数であとで消す予定
        /// </summary>
        public void ForceStartGame() {
            this._photonView.RPC("RPCForceStartGame", RpcTarget.All);
        }

        [PunRPC]
        public void RPCForceStartGame() {
            this.OnForecStartGame?.Invoke();
        }
    }
}