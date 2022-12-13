using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ParallelCommon {
	public class Util {
		private static readonly string ELLIPSIS = "...";
		
		public static void DestroyChildObject(Transform parent_trans){
			for (int i = 0; i < parent_trans.childCount; ++i) {
				GameObject.Destroy(parent_trans.GetChild(i).gameObject);
			}
		}

		public static GameObject FindChildObject(Transform parent_trans, string name) {
			Transform[] transforms = parent_trans.GetComponentsInChildren<Transform>();
			foreach (Transform t in transforms) {
				if (t.name == name) {
					return t.gameObject;
                }
            }

			return null;
		}

		public static void SetLayerChildren(GameObject obj, int layer) {
			Transform[] transforms = obj.GetComponentsInChildren<Transform>();
			foreach (Transform transform in transforms) {
				transform.gameObject.layer = layer;
			}
		}

		public static List<GameObject> FindChildObjects(Transform parent_trans, string name) {
			List<GameObject> objects = new List<GameObject>();
			Transform[] transforms = parent_trans.GetComponentsInChildren<Transform>();
			foreach (Transform t in transforms) {
				if (t.name == name) {
					objects.Add(t.gameObject);
				}
			}

			return objects;
		}

		public static void MoveGameObjectToScene(GameObject obj, UnityEngine.SceneManagement.Scene scene) {
			if (!scene.IsValid()) {
				return;
            }
			obj.transform.SetParent(null);
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, scene);
		}
		
		public static bool IsUGUIHit(Vector3 scrPos) {
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			pointer.position = scrPos;
			List<RaycastResult> result = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointer, result);
			return (result.Count > 0);
		}
	}
}