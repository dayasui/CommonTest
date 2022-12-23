var jsBridge = {
    execute: function(methodName, parameter) {
        console.log("called execute");
        // jsの文字列に変換する
        methodName = Pointer_stringify(methodName)
        parameter = Pointer_stringify(parameter)

        // 実行するメソッド名とパラメータをまとめる
        var jsonObj = {}
        jsonObj.methodName = methodName
        jsonObj.parameter = parameter

        var argsmentString = JSON.stringify(jsonObj)
        // カスタムイベントを作成して発行する
        var event = new CustomEvent('unityMessage', { detail: argsmentString })
        window.dispatchEvent(event)
    },

    getCookies: function() {
        var cookies = document.cookie;
        console.log("jsbridge getCookies", cookies);

        var bufferSize = lengthBytesUTF8(cookies) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(cookies, buffer, bufferSize);
        return buffer;
    },

    postMessage: function() {
        var w = window;

        if (w.webkit && w.webkit.messageHandlers && w.webkit.messageHandlers.firebase) {
            // Call iOS interface
            var message = { command: 'showNextContentList'};
            w.webkit.messageHandlers.firebase.postMessage(message);
        } else {
            // No Android or iOS interface found
            console.log('No native APIs found.');
        }

    },

    getInitValue: function() {
        var w = window;

        if (w.webkit && w.webkit.messageHandlers && w.webkit.messageHandlers.firebase) {
            // Call iOS interface
            var message = { command: 'getInitValue'};
            w.webkit.messageHandlers.firebase.postMessage(message);
        } else {
            // No Android or iOS interface found
            console.log('No native APIs found.');
        }

        console.log("call getInitValue in jsBridge");
    },

    getInitValueCallback: function(data) {
        console.log("call getInitValueCallback in jsBridge", data);

        MyGameInstance.SendMessage("JSMessageReceiver", "GetInitValueCallback", data);
    },
}

mergeInto(LibraryManager.library, jsBridge);