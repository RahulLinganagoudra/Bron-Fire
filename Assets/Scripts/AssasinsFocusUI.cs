using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
/*
The purpose for separating AssassinsFocusUI is this updates every frame and updating every frame whole canvas is very costly.
separating only assasins focus can prevent repainting every canvas element for no reason.
*/
public class AssasinsFocusUI : MonoBehaviour
{
	[SerializeField] Image focusPrefab;
	[SerializeField] Transform focusPointParent;
	[SerializeField] Sprite focused;
	[SerializeField] CanvasGroup canvasGroup;
	Camera cam;
	private void Start()
	{
		cam = Camera.main;
	}

	public void StartFocus()
	{
		focusPointParent.DestroyChildren();
		canvasGroup.DOFade(1, .5f);
	}
	public void EndFocus()
	{
		canvasGroup.DOFade(0, .4f);
	}
	public void Repaint(EnemyAI[] targets)
	{
		if (cam == null)
		{
			cam = Camera.main;
		}
		focusPointParent.DestroyChildren();
		foreach (EnemyAI target in targets)
		{
			Vector3 screenPoint = cam.WorldToScreenPoint(target.transform.position + Vector3.up);

			if (screenPoint.x <= Screen.width && screenPoint.x >= 0 &&
				screenPoint.y <= Screen.height && screenPoint.y >= 0)
			{
				var focusUI = Instantiate(focusPrefab, focusPointParent);
				focusUI.transform.position = screenPoint;
			}
		}
	}




}
