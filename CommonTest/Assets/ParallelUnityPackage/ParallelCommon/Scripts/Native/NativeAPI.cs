using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;

namespace ParallelCommon {
    public class NativeAPI {
#if UNITY_IOS
    [DllImport("__Internal")] public static extern string getSession();
    [DllImport("__Internal")] public static extern string getInitValue();
    [DllImport("__Internal")] public static extern void resetSession();
    [DllImport("__Internal")] public static extern void sendLog(string eventName, string key, string value);
    [DllImport("__Internal")] public static extern int getAppId();
    [DllImport("__Internal")] public static extern void showHeader();
    [DllImport("__Internal")] public static extern void hideHeader();
    [DllImport("__Internal")] public static extern void showFooter();
    [DllImport("__Internal")] public static extern void hideFooter();
    [DllImport("__Internal")] public static extern void showNextContentList();
    [DllImport("__Internal")] public static extern void hideNextContentList();
    [DllImport("__Internal")] public static extern void openWebView(string url);
    [DllImport("__Internal")] public static extern void closeWebView();
#endif
        
#if UNITY_EDITOR || !UNITY_IOS
    public static string GetSession() {
        Debug.Log("called GetSession on editor");
        return "";
    }

    public static string GetInitValue() {
        Debug.Log("called GetInitValue on editor");
        return "";
    }

    public static void ResetSession() {
        Debug.Log("called ResetSession on editor");
    }

    public static void SendLog(string eventName, string key, string value) {
        Debug.Log($"called sendLog on editor param:{eventName} {key} {value}");
    }

    public static int GetAppId() {
        Debug.Log("called GetAppId on editor");
        return 0;
    }

    public static void ShowHeader() {
        Debug.Log("called ShowHeader on editor");
    }

    public static void HideHeader() {
        Debug.Log("called HideHeader on editor");
    }

    public static void ShowFooter() {
        Debug.Log("called ShowFooter on editor");
    }

    public static void HideFooter() {
        Debug.Log("called HideFooter on editor");
    }
    
    public static void ShowNextContentList() {
        Debug.Log("called ShowNextContentList on editor");
    }
    
    public static void HideNextContentList() {
        Debug.Log("called HideNextContentList on editor");
    }
    
    public static void OpenWebView(string url) {
        Debug.Log("called OpenWebView on editor  url:" + url);
    }
    
    public static void CloseWebView() {
        Debug.Log("called CloseWebView on editor");
    }
#elif UNITY_IOS
    public static string GetSession() {
        return getSession();
    }

    public static string GetInitValue() {
        return getInitValue();
    }

    public static void ResetSession() {
        resetSession();
    }

    public static void SendLog(string eventName, string key, string value) {
        sendLog(eventName, key, value);
    }

    public static int GetAppId() {
        return getAppId();
    }

    public static void ShowHeader() {
        showHeader();
    }

    public static void HideHeader() {
        hideHeader();
    }

    public static void ShowFooter() {
        showFooter();
    }

    public static void HideFooter() {
        hideFooter();
    }
    
    public static void ShowNextContentList() {
        showNextContentList();
    }
    
    public static void HideNextContentList() {
        hideNextContentList();
    }
    
    public static void OpenWebView(string url) {
        openWebView(url);
    }
    
    public static void CloseWebView() {
        closeWebView();
    }
#endif
    }
}
