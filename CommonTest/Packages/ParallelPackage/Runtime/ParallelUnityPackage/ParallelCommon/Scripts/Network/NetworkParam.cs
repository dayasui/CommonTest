using System.Collections;
using System.Collections.Generic;
using BestHTTP;

namespace ParallelCommon {
    public class NetworkParam {
        public string url;
        public HTTPMethods httpMethods;
        public string data;
        public Dictionary<string, string> header;
        public int retryIntervalMS = 1000;
        public int retryMax = 3;
    }
}
