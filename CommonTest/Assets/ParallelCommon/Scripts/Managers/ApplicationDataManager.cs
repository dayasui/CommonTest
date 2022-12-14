using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelCommon {
    public class ApplicationDataManager : SingletonMonoBehaviour<ApplicationDataManager> {
        public readonly string AppName = "パラレル";
        public string MiniGameName => "マルチゲームUnityWebGL";
        public string HashTag => $"#{this.AppName} #{this.MiniGameName}";
        
        private string _build_env = string.Empty;

        private bool _isDebug = false;
        private string _appIdPUN;
        public bool IsDebug => this._isDebug;
        public string AppIdPUN => this._appIdPUN;
        public bool IsDevelopment => this._build_env.Equals("development");
        public bool IsStaging => this._build_env.Equals("staging");
        public bool IsProduction => this._build_env.Equals("production");

        public bool IsStandAlone {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
        private int _applicationID = 0;

        public int ApplicationID
        {
            get => this._applicationID;
            set => this._applicationID = value;
        }

        /// <summary>
        /// 観戦者かどうか
        /// </summary>
        public bool IsObserver
        {
            get; set;
        }

        /// <summary>
        /// マッチングモード
        /// </summary>
        public Constants.MatchingMode MatchingMode {
            get; set;
        } = Constants.MatchingMode.None;

        public bool IsFriendMode => MatchingMode == Constants.MatchingMode.Friend;
        public bool IsVersusMode => MatchingMode == Constants.MatchingMode.Versus;
    }
}