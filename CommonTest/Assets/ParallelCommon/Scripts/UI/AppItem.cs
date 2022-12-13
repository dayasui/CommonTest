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

        private Constants.ApplicationID _appID;
        public Constants.ApplicationID AppID => _appID;
        private UnityAction<Constants.ApplicationID> _onClick;

        public void Init(Constants.ApplicationID id, string text, UnityAction<Constants.ApplicationID> onClick) {
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