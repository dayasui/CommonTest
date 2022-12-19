function test(parameter) {
    debuglog("called test:" + parameter.data);
    MyGameInstance.SendMessage(parameter.callbackGameObjectName, parameter.callbackFunctionName, 1000);
}

// Unityから呼ばれる
function getInitValue(parameter) {
    debuglog("called getInitValue in javascript");

    if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.firebase) {
        // Call iOS interface
        var message = { command: 'getInitValue'};
        window.webkit.messageHandlers.firebase.postMessage(message);
        debuglog('post command:getInitValue');
    } else {
        // No Android or iOS interface found
        debuglog('No native APIs found.');
    }
}

// ネイティブから呼ばれる
window.getInitValueCallback = function(data) {
    debuglog("call getInitValueCallback:" + data);

    MyGameInstance.SendMessage("JSMessageReceiver", "GetInitValueCallback", data);
}

// Unityから呼ばれる
function getSession(parameter) {
    debuglog("called getSession in javascript");

    if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.firebase) {
        // Call iOS interface
        var message = { command: 'getSession'};
        window.webkit.messageHandlers.firebase.postMessage(message);
        debuglog('post command:getSession');
    } else {
        // No Android or iOS interface found
        debuglog('No native APIs found.');
    }
}

// ネイティブから呼ばれる
window.getSessionCallback = function(data) {
    debuglog("call getSessionCallback:" + data);

    MyGameInstance.SendMessage("JSMessageReceiver", "GetSessionCallback", data);
}


// Unityから呼ばれる
function getAppId(parameter) {
    debuglog("called getAppId in javascript");

    if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.firebase) {
        // Call iOS interface
        var message = { command: 'getAppId'};
        window.webkit.messageHandlers.firebase.postMessage(message);
        debuglog('post command:getAppId');
    } else {
        // No Android or iOS interface found
        debuglog('No native APIs found.');
    }
}

// ネイティブから呼ばれる
window.getAppIdCallback = function(data) {
    debuglog("call getAppIdCallback:" + data);

    MyGameInstance.SendMessage("JSMessageReceiver", "GetAppIdCallback", data);
}


function recieveMessage(event) {
    var data = JSON.parse(event.detail)
    var methodName = data.methodName
    var parameter = data.parameter
    try {
      parameter = JSON.parse(parameter)
    } catch (e) {
      parameter = null
    }
    // C#から指定されているメソッドを呼び出しparameterを渡す
    eval(`${methodName}(parameter)`)
}
  
// unityMessageというCustomEventを受け取る
window.addEventListener('unityMessage', recieveMessage, false)

function debuglog(str) {
    console.log(str);
    MyGameInstance.SendMessage("JSMessageReceiver", "Log", str);
}

