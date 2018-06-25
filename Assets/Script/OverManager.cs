using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Text;
using FileOperate;
public class OverManager : MonoBehaviour {
	public Text pScoreText, hScoreText;
	public AudioSource au;
	public AudioClip button;
	// Use this for initialization
	void Start () {
		Dictionary<string,string> data = FileHandle.ReadINIFile(Application.persistentDataPath, "CandyScoreData.ini");
		Global.highscore = Global.score > int.Parse(data["highScore"]) ? Global.score : int.Parse(data["highScore"]);
		StreamWriter sw = FileHandle.CreateFile(Application.persistentDataPath, "CandyScoreData.ini");
		sw.WriteLine("highScore=" + Global.highscore);
		sw.Close();
		sw.Dispose();
		pScoreText.text = Global.score.ToString();
		hScoreText.text = Global.highscore.ToString();
	}
	public void Restart()
	{
		iTween.ShakeScale(this.gameObject,new Vector3(2,2,1),0.3f);
		StartCoroutine(WaitRestart(0.3f));
	}
	
	public void ShutDown()
	{
		iTween.ShakeScale(this.gameObject,new Vector3(2,2,1),0.3f);
		StartCoroutine(WaitExit(0.3f));
	}
	IEnumerator WaitRestart(float t)
	{
		au.PlayOneShot(button);
		yield return new WaitForSeconds(t);
		SceneManager.LoadScene("PreScene");
	}
	IEnumerator WaitExit(float t)
	{
		au.PlayOneShot(button);
		yield return new WaitForSeconds(t);
		Application.Quit();
	}
}
