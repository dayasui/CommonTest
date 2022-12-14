using System;

namespace ParallelCommon {

    [Serializable]
    public class ParallelUserData : ParallelUserDataBase {

        public ParallelUserData() { }
        
        public ParallelUserData(ParallelUserData src) {
            this.id = src.id;
            this.name = src.name;
            this.user_image = src.user_image;
            this.gender = src.gender;
            this.twitter_id = src.twitter_id;
            this.is_connected = src.is_connected;
        }

        public ParallelUserData(ParalleWebGLlUserData src) {
            this.id = src.id;
            this.name = src.name;
            this.user_image = src.user_image;
        }
        public string gender;
        public string twitter_id;
        public bool is_connected;
        
    }
}
