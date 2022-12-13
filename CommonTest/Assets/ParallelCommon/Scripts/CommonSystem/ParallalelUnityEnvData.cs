using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelCommon {
    public class ParallelUnityEnvDataSO : ScriptableObject {
        public ParallelUnityEnvData envData;
    }

    [Serializable]
    public class ParallelUnityEnvData {
        public int standAloneURLIndex;
        public ParallelUnityEnvServerURL[] standAloneURLs;
        public string StandAloneURL {
            get {
                if (this.standAloneURLIndex < 0) return null;
                if (this.standAloneURLs.Length <= this.standAloneURLIndex) return null;
                return this.standAloneURLs[this.standAloneURLIndex].url;
            }
        }
    }

    [Serializable]
    public class ParallelUnityEnvDummyPlayerData {
        public int id;
        public string name;
        public string token;
        public string device_id;
    }

    [Serializable]
    public class ParallelUnityEnvServerURL {
        public string name;
        public string url;
    }
}