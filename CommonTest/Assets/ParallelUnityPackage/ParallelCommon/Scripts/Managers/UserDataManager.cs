using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace ParallelCommon {
    public class UserDataManager : SingletonMonoBehaviour<UserDataManager> {
        /// <summary>
        /// 自分のユーザID
        /// </summary>
        private static int _userID;
        public static int UserID {
            get => _userID;
            set => _userID = value;
        }

        // 観戦者の場合はオーナーのUserIDを使う
        public static int UserIDWithExceptObserver {
            get {
                return ApplicationDataManager.Instance.IsObserver ? OwnerUserID : UserID;
            }
        }

        /// <summary>
        /// 自分がオーナーかどうか
        /// </summary>
        private static bool _isOwner;
        public static bool IsOwner {
            get => _isOwner;
            set => _isOwner = value;
        }

        /// <summary>
        /// オーナーのユーザID
        /// </summary>
        private static int _ownerUserID = 0;
        public static int OwnerUserID {
            get => _ownerUserID;
            set => _ownerUserID = value;
        }
        
        /// <summary>
        /// オーナーが接続中かどうか
        /// </summary>
        private bool _isOwnerWithdrawal = false;
        public bool IsOwnerWithdrawal {
            get => _isOwnerWithdrawal;
            set => _isOwnerWithdrawal = value;
        }

        public int NumberOfUsers {
            get {
                if (!ReferenceEquals(this._userList, null)) {
                    return this._userList.Count;
                }
                return 0;
            }
        }

        // 観戦者を除いた数を取得
        public int NumberOfPlayers {
            get {
                if (!ReferenceEquals(this._userList, null)) {
                    return this._userList.Count(e => !e.is_observer);
                }
                return 0;
            }
        }

        public ParallelChatRoomSessionData.SessionUser CurrentUserData => this.GetUserData(UserID);
        public string CurrentIconUrl => this.CurrentUserData.user.user_image;
    
        public static bool IsMyself(int userID) => (userID == UserIDWithExceptObserver);

//        private string _userName;
        private List<ParallelChatRoomSessionData.SessionUser> _userList = new List<ParallelChatRoomSessionData.SessionUser>();
        
        public List<ParallelChatRoomSessionData.SessionUser> UserList => this._userList;
        private Dictionary<int, Sprite> _userImage = new Dictionary<int, Sprite>();
        
        void OnDestroy() {
            this.DestroyUsersImage();
        }

        public string GetPlayerName(int playerId) {
            ParallelChatRoomSessionData.SessionUser userData = this.GetUserData(playerId);
            if (userData != null) {
                return userData.user.name;
            }
            return "";
        }

        public string GetMyName() {
            return this.GetPlayerName(UserID);
        }

        public ParallelChatRoomSessionData.SessionUser GetUserData(int userID) {
            ParallelChatRoomSessionData.SessionUser userData = this._userList.FirstOrDefault(e => e.user.id == userID);
            return userData;
        }

        public void AddUsersData(ParallelChatRoomSessionData.SessionUser data) {
            if (this._userList.FirstOrDefault(e => e.user.id == data.user.id) == null) {
                data.user.name = StringUtil.ReplaceEmoji(data.user.name, "□");
                this._userList.Add(data);
            }
        }

        // TODO
        /*
        public void LoadTexture2D(string url, UnityAction<Texture2D> onComplete) {
            if (string.IsNullOrEmpty(url)) return;
            
            NetworkManager.Instance.DownLoadTexture2D(url, (tex) => {
                onComplete(tex);
            });
        }
        */
        

        // TODO
        /*
        /// <summary>
        /// ユーザアイコンのDLとSprite作成
        /// </summary>
        public void LoadUsersImage(UnityAction onComplete = null) {
            int count = this.UserList.Count;
            UnityAction decCount = () => {
                count--;
                if (count == 0) {
                    Debug.Log("LoadUserImage OnComplete");
                    onComplete?.Invoke();
                }
            };
            foreach (var userData in this.UserList) {
                if (!this._userImage.ContainsKey(userData.user.id)) {
                    NetworkManager.Instance.DownLoadTexture2D(userData.user, (tex) => {
                        //Texture2DをSpriteに変換
                        if (tex != null) {
                            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                            this._userImage.Add(userData.user.id, sprite);
                        }
                        decCount();
                    });
                } else {
                    decCount();
                }
            }
        }
        */

        /// <summary>
        /// ユーザアイコン Spriteのゲッター
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public Sprite GetUserIamge(int userID) {
            if (this._userImage.ContainsKey(userID)) {
                return this._userImage[userID];
            }
            return null;
        }
        
        /// <summary>
        /// 全ユーザのアイコン取得
        /// </summary>
        public Dictionary<int, Sprite> GetUserImages => this._userImage;

        /// <summary>
        /// ユーザアイコン Spriteの破棄
        /// </summary>
        public void DestroyUsersImage() {
            foreach (KeyValuePair<int, Sprite> pair in this._userImage) {
                Destroy(pair.Value);
            }
        }
        
        /// <summary>
        /// オーナーのユーザIDかを判定
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static bool IsOwnerUserID(int userID) {
            if (OwnerUserID == 0) {
                return false;
            }
            return OwnerUserID == userID;
        }

        public int[] GetUserIDs() {
            if (ReferenceEquals(this._userList, null)) {
                Debug.LogWarning("UserListが空っぽです");
                return new int[] {1, 2, 3};
            }
            return this._userList.Select(e => e.user.id).ToArray();
        }

        /// <summary>
        /// 観戦者を除いたuserIDを返す
        /// </summary>
        public int[] GetPlayerIDs() {
            if (ReferenceEquals(this._userList, null)) {
                Debug.LogWarning("UserListが空っぽです");
                return new int[] { 1, 2, 3 };
            }
            return this._userList.Where(e => !e.is_observer).Select(e => e.user.id).ToArray();
        }

        /// <summary>
        /// 観戦者と自分を除いたuserIDを返す
        /// PhotonのexpcetedUsersに使う想定
        /// </summary>
        public int[] GetPlayerIDsWithoutSelf() {
            if (ReferenceEquals(this._userList, null)) {
                Debug.LogWarning("UserListが空っぽです");
                return null;
            }
            return this._userList.Where(e => !e.is_observer && e.user.id != UserID).Select(e => e.user.id).ToArray();
        }
    }
}