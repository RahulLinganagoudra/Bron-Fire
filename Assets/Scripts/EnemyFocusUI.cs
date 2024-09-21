using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class EnemyFocusUI : MonoBehaviour
	{
		[SerializeField] Image enemyFocus;
		Camera cam;
		Transform focus;
		private Vector3 offset;
		float lastTimeSpriteUpdated;

		private void Start()
		{
			cam=Camera.main;
		}
		public void Focus(Transform newFocus, Vector3 offset)
		{
			focus = newFocus;
			this.offset= offset;
			if (newFocus == null)
			{
				enemyFocus.gameObject.SetActive(false);
				return;
			}
			else
			{
				enemyFocus.gameObject.SetActive(true);
			}

			RepositionTarget(newFocus, offset);
		}

		private void RepositionTarget(Transform newFocus, Vector3 offset)
		{
			if(cam==null)
			{
				cam= Camera.main;
			}
			enemyFocus.transform.position = cam.WorldToScreenPoint(newFocus.position + offset);
		}
		private void RepositionTarget()
		{
			RepositionTarget(focus, offset);
		}

		private void Update()
		{
			if(focus!=null&&Time.time-lastTimeSpriteUpdated>.2f)
			{
				RepositionTarget();
			}
		}
	}
}
