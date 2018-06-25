using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 糖果容器,定义糖果类型、位置、一些基本操作
/// </summary>
public class CandyBox : MonoBehaviour {

	public int rowIndex = 0, columnIndex = 0;
	public int type = -1;
	//所有种类candy集合
	public GameObject[] showWhichCandy;
	//生成过程中存储本个candy
	private GameObject thisCandy;
	//GameController脚本的引用
	public GameController gameController;
	public bool once = true;
	void Start () 
	{
	
	}
	void Update () 
	{
		
	}
	private void AddRandomCandy()
	{
		if(thisCandy != null) return ;
		//生成0-5随机数
		type = Random.Range (0, showWhichCandy.Length);
		thisCandy = Instantiate (showWhichCandy [type], this.transform.position, this.transform.rotation) as GameObject;
		//将生成的candy设为CandyBox子物体
		thisCandy.transform.parent = this.transform; 
	}
	/// <summary>
	/// 设置界面CandyBox位置
	/// </summary>
	public void UpdatePosition()
	{
		AddRandomCandy();
		transform.position = new Vector3 (columnIndex, rowIndex, 0);
	}
	public void TweenToPosition()
	{
		AddRandomCandy();
		iTween.MoveTo(this.gameObject,iTween.Hash("x",columnIndex,"y",rowIndex,"z",0,"time",0.5,"islocal",true));
	}
	/// <summary>
	/// 鼠标单击触发事件
	/// </summary>
	// void OnMouseDown()
	// {
	// 	//控制在消除和交换时不可再操作CandyBox,避免Bug
	// 	if(gameController.Instance().state == GameController.Condition.NORMAL)
	// 	{
	// 		gameController.Instance().Select(this);
	// 	}	
	// }
	/// <summary>
	/// 删除的实际操作
	/// </summary>
	public void Dispose()
	{
		gameController = null;
		Destroy(thisCandy.gameObject);
		DestroyImmediate(this.gameObject);
	}
}
