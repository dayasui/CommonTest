using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ParallelCommon {
    public class AppItem : MonoBehaviour {
        [SerializeField]
        private Text _text;

        private int _appID;
        public int AppID => _appID;
        private UnityAction<int> _onClick;

        public void Init(int id, string text, UnityAction<int> onClick) {
            this._appID = id;
            this._text.text = text;
            this._onClick = onClick;
        }

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        public void OnClick() {
            if (this._onClick != null) {
                this._onClick(this._appID);
            }
        }

        public void SetColor(Color c) {
            Image image = GetComponentInChildren<Image>();
            if (image != null) {
                image.color = c;
            }
        }
    }
}