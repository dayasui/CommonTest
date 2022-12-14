namespace ParallelCommon {
    public class Constants {
        public static readonly int FRAME_RATE = 30;
        public enum NetResultCode {
            Ok = 200,
        }

        public enum MatchingMode {
            None,
            Friend, // 友達と遊ぶ（ルーム内）
            Versus, // 対戦（ルーム同士）
        }
    }
}