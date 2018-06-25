using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour {
	private bool onceClick = true;
	public GameObject start;
	public AudioSource au;
	public void DoStart()
	{
		if(onceClick)
		{
			onceClick = false;
			StartCoroutine(Wait(0.3f));
		}
	}
	IEnumerator Wait(float t)
	{
		au.Play();
		iTween.ShakeScale(start,new Vector3(2,2,1),0.3f);
		yield return new WaitForSeconds(t);
		SceneManager.LoadScene("GameScene");
	}

}
