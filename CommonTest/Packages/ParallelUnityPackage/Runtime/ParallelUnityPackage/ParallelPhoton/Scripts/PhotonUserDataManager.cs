using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Photon.Pun;

namespace ParallelPhoton {
    [Serializable]
    public class PhotonTeamData {
        public int groupRoomID = 0;
        public int teamID = 0;
        public List<string> userIDs = new List<string>();

        public void Add(string uid) {
            this.userIDs.Add(uid);
        }

        public void Remove(string uid) {
            this.userIDs.Remove(uid);
        }

        public bool Contains(string uid) {
            return this.userIDs.Contains(uid);
        }

        public bool ContainsAll(string[] uids) {
            return ParallelCommon.Util.ContainsAll<string>(uids, this.userIDs);
        }

        public bool ContainsAll(List<string> uids) {
            return ParallelCommon.Util.ContainsAll<string>(uids, this.userIDs);
        }

        public int NumberOfPlayers => userIDs.Count;

        
    }

    [Serializable]
    public class PhotonTeamDataList {
        public List<PhotonTeamData> teamList = new List<PhotonTeamData>();
    }

    public class PhotonUserDataManager : SingletonMonoBehaviour<PhotonUserDataManager> {
        private List<string> _userIDs = new List<string>();
        private List<string> _readyUserIDs = new List<string>();

        private PhotonTeamDataList _teamData = new PhotonTeamDataList();
        public PhotonTeamDataList TeamDataAll => _teamData;
        public int NumberOfTeams => _teamData.teamList.Count;
        public int NumberOfPlayers {
            get {
                return this._userIDs.Count;
            }
        }
        
        public string CurrentUserId => PhotonNetwork.LocalPlayer.GetUserId();
        public int CurrentTeamId => this.GetTeamId(this.CurrentUserId);
        public PhotonTeamData CurrentTeamData => this.GetTeamData(this.GetTeamId(this.CurrentUserId));

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        public void Reset() {
            this._userIDs.Clear();
            this._readyUserIDs.Clear();
            this._teamData.teamList.Clear();
        }

        public void AddUserID(string photonUserID) {

            if (!this._userIDs.Contains(photonUserID)) {
                this._userIDs.Add(photonUserID);
            }
            Debug.Log($"ADD USER:{photonUserID}");
        }

        public void RemoveUserID(string photonUserID) {
            this._userIDs.Remove(photonUserID);
        }

        /// <summary>
        /// チーム作成
        /// MasterClientが呼ぶ想定
        /// </summary>
        /// <param name="friendMatch"></param>
        /// <param name="teamNum">friendMatch = trueのときのみ値を入れる</param>
        public bool CreateTeam(bool friendMatch, int numberOfTeams = 0) {
            if (friendMatch) {
                return this.createTeamInParallelRoom(numberOfTeams);
            } else {
                return this.createTeamByGroupRoomID();
            }
        }

        private bool createTeamInParallelRoom(int numberOfTeams) {
            int teamID = 1;
            int index = 1;
            int numberOfPlayers = this._userIDs.Count / numberOfTeams;
            PhotonTeamData teamData = null;
            foreach (string uid in this._userIDs) {
                if (index == 1) {
                    teamData = new PhotonTeamData();
                    teamData.teamID = teamID;
                    teamData.userIDs = new List<string>();
                    teamID++;
                }
                teamData.userIDs.Add(uid);
                if (index == numberOfPlayers) {
                    this._teamData.teamList.Add(teamData);
                    index = 1;
                } else {
                    index++;
                }
            }

            return true;
        }

        /// <summary>
        /// GroupRoomID（パラレルの部屋ID）に基づいてチームを作成する
        /// </summary>
        /// <returns></returns>
        private bool createTeamByGroupRoomID() {
            int teamID = 1;
            Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
            foreach (string photonUserID in this._userIDs) {
                int groupRoomID;
                int userID;
                PhotonUtil.GetParallelGroupRoomIDAndUserIDFromPhotonUserID(photonUserID, out groupRoomID, out userID);
                if (!dict.ContainsKey(groupRoomID)) {
                    dict.Add(groupRoomID, new List<string>());
                }

                dict[groupRoomID].Add(photonUserID);
            }


            foreach (var pair in dict) {
                PhotonTeamData teamData = new PhotonTeamData();
                teamData.teamID = teamID;
                teamData.groupRoomID = pair.Key;
                teamData.userIDs = pair.Value.ToList();
                this._teamData.teamList.Add(teamData);
                teamID++;
            }

            return true;
        }

        public void UpdateTeamData(string json) {
            this._teamData = JsonUtility.FromJson<PhotonTeamDataList>(json);
        }

        public void Ready(string userID) {
            if (!this._readyUserIDs.Contains(userID)) {
                Debug.Log($"ADD READY USER:{userID}");
                this._readyUserIDs.Add(userID);
            }
        }

        public bool IsReady(string userID) {
            return this._readyUserIDs.Contains(userID);
        }

        public bool IsAllReadyUsers() {
            if (this._userIDs.Count == 0) {
                return false;
            }

            var sortedUserIDs = this._userIDs.OrderBy(e => e);
            var readyIDs = this._readyUserIDs.OrderBy(e => e);
            return sortedUserIDs.SequenceEqual(readyIDs);
        }

        public int GetTeamId(string userID) {
            foreach (var team in this._teamData.teamList) {
                if (team.userIDs.IndexOf(userID) >= 0) {
                    return team.teamID;
                }
            }
            Debug.Log($"not find in team:{userID}");
            return 0;
        }

        public PhotonTeamData GetTeamData(int teamID) {
            return this._teamData.teamList.FirstOrDefault(e => e.teamID == teamID);
        }

        public List<PhotonTeamData> GetTeamDataList() {
            return this._teamData.teamList;
        }

        public bool IsMyTeam(int teamID) {
            int myTeamID = this.GetTeamId(PhotonNetwork.LocalPlayer.GetUserId());
            return myTeamID == teamID;
        }

        public int GetActorNumber(string userID) {
            var player = PhotonNetwork.PlayerList.FirstOrDefault(e => e.GetUserId() == userID);
            if (player != null) {
                return player.ActorNumber;
            }
            return -1;
        }

        public int[] GetActorNumbers() {
            return PhotonNetwork.PlayerList.Select(e => e.ActorNumber).ToArray();
        }
        
        public List<int> GetActorNumbers(int teamID) {
            List<int> actorNumbers = new List<int>();
            var team = this.GetTeamData(teamID);
            foreach (var userId in team.userIDs) {
                var player = PhotonNetwork.PlayerList.FirstOrDefault(e => e.GetUserId() == userId);
                actorNumbers.Add(player.ActorNumber);
            }

            return actorNumbers;
        }
        
        /// <summary>
        /// 離脱（Photon切断）したユーザーを取得
        /// </summary>
        /// <returns>離脱ユーザーのIDリスト</returns>
        public List<string> GetLeftUsers() {
            List<string> leftUsers = new List<string>();
            if (!PhotonNetwork.IsConnectedAndReady) {
                return leftUsers;
            }
            List<string> remainUsers = PhotonNetwork.PlayerList.Select(e => e.GetUserId()).ToList();
            leftUsers = this._userIDs.Except(remainUsers).ToList();
            return leftUsers;
        }

        /// <summary>
        /// 対戦相手のチームデータ取得
        /// </summary>
        /// <returns></returns>
        public PhotonTeamData[] OpponentTeamData => this.GetTeamDataList().Where(e => e.teamID != this.CurrentTeamId).ToArray();

        /// <summary>
        /// 離脱者がいるかどうか
        /// </summary>
        public bool IsUsersHaveLeft => this.GetLeftUsers().Count > 0;
        
        public List<string> GetUserIDByTeamID(int teamID) {
            var teamData = this.GetTeamData(teamID);
            return teamData?.userIDs;
        }
        
        public List<string> GetCurrentTeamUserID() {
            return this.GetUserIDByTeamID(this.CurrentTeamId);
        }
    }
}