using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ParallelDummy {
    public class RoomItem : MonoBehaviour {
        [SerializeField] private Text _roomNameText = null;
        private UnityAction<RoomItem> _onComplete = null;
        private NetChatGroupData _data;

        public NetChatGroupData Data {
            get { return this._data; }
        }

        public void Init(NetChatGroupData data, UnityAction<RoomItem> onComplete) {
            this._data = data;
            this._roomNameText.text = data.name;
            this._onComplete = onComplete;
        }

        public void Disacitvate() {
            Button button = GetComponentInChildren<Button>();
            button.interactable = false;
        }
        
        public void OnClick() {
            this._onComplete?.Invoke(this);
        }
    }
}