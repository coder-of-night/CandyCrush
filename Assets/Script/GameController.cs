using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// 糖果容器管理,交换,删除,添加,动画,特效
/// </summary>
public class GameController : MonoBehaviour 
{
	public enum Condition 
	{
		NORMAL,
		CRUSH,
		EXCHANGE
	}
	public Condition state = Condition.EXCHANGE;
	//糖果容器对象
	public CandyBox candyBox;
	//第一次点击选择的CandyBox
	private CandyBox whichTimeClickBox;
	//行列总数
	public int rowNum = 8, columnNum = 8;
	//保存糖果容器矩阵的二维数组
	private ArrayList candyArr;
	//保存消除糖果容器的数组
	private ArrayList matches;
	//爆炸特效
	public ParticleSystem fx; 
	//相机震动用transform
	public Transform c_transform;
	//时间进度条
	public Slider slider;
	public AudioSource au;
	public AudioClip crushSound, wrongSound, comboSound4, comboSound5;
	//震动总时间
	public float shakeTime = 0.2f;
	//振幅
	public float shakeLevel = 1f;
	//震动速率
	public float shakeSpeed = 5f;
	//背景切换的时间
	private float playTime = 6;
	//背景索引
	private int bgIndex = 0;
	public SpriteRenderer bgRenderer;
	//背景图
	public Sprite[] bg;
	//重随提示信息
	public Text message;
	//分数组件
	public Text showScore;
	private int presentScore = 0;
	//每个Candy的分数
	public int eachGrade = 50;
	//单例
	private GameController instance;
	public GameController Instance()
	{
		if(instance == null)
			instance = this;
		return instance;
	}
	void Start () 
	{
		state = Condition.EXCHANGE;
		presentScore = 0;
		bgIndex = 0;
		//保存了每一行的ArrayList
		candyArr = new ArrayList();
		for (int rowIndex = 0; rowIndex < rowNum; rowIndex++) 
		{
			//保存了一行中每个Candybox
			ArrayList temp = new ArrayList();
			for (int columnIndex = 0; columnIndex < columnNum; columnIndex++) 
			{	
				temp.Add(AddCandyBox(rowIndex, columnIndex, false));
			}
			candyArr.Add(temp);
		}
		StartCoroutine(WaitForCheck(0.8f));
	}
	/// <summary>
	/// 向界面上特定位置添加新的CandyBox
	/// </summary>
	/// <param name="rowIndex">行索引</param>
	/// <param name="columnIndex">列索引</param>
	/// <param name="ifTop">是否是顶部添加元素</param>
	/// <returns>返回添加的新元素对象CandyBox</returns>
	private CandyBox AddCandyBox(int rowIndex, int columnIndex, bool ifTop)
	{
		CandyBox c;
		//初始生成位置
		if(!ifTop)
		{
			c = Instantiate (candyBox,this.transform.position,this.transform.rotation) as CandyBox;
		}
		else
		{
			Vector3 insTopPos = new Vector3(columnIndex, rowNum);
			c = Instantiate (candyBox,insTopPos,this.transform.rotation) as CandyBox;
		}
		//将CandyBox设为GameController子物体
		c.transform.parent = this.transform; 
		//将本脚本引用传给CandyBox脚本
		c.gameController = this; 
		c.columnIndex = columnIndex;
		c.rowIndex = rowIndex;
		//设置CandyBox位置
	//	if(!ifTop)
	//	{
	//		c.UpdatePosition(); 
	//	}
	//	else
	//	{
			c.TweenToPosition();
	//	}
		
		return c;
	}
	void Update () 
	{
		//随时间切换背景
		playTime -= Time.deltaTime;
		if(playTime <= 0)
		{
			playTime = 6;
			SwitchBG();
		}
		//随时间流逝进度条减少,归零则游戏结束
		slider.value = Mathf.Clamp(slider.value -= Time.deltaTime * 0.1f, 0, 1);
		if(slider.value == 0)
		{
			SceneManager.LoadScene("OverScene");
		}

		if(Input.touchCount == 1)
		{
			if(Input.GetTouch(0).phase == TouchPhase.Began)
			{
				if(state == GameController.Condition.NORMAL)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
					RaycastHit hit;
					if(Physics.Raycast(ray , out hit))
					{
						Select(hit.transform.GetComponent<CandyBox>());					
					}
					
				}	
			}
			if(Input.GetTouch(0).phase == TouchPhase.Ended)
			{				
				if(state == GameController.Condition.NORMAL)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
					RaycastHit hit;
					if(Physics.Raycast(ray , out hit))
					{
						Select(hit.transform.GetComponent<CandyBox>());					
					}	
				}	
			}
		}
	}
	/// <summary>
	/// 切换背景
	/// </summary>
	void SwitchBG()
	{
		bgRenderer.sprite = bg[bgIndex];
		bgIndex++;
		if(bgIndex >= 3)
		{
			bgIndex = 0;
		}
	}
	/// <summary>
	/// 点选CandyBox,先后判断,交换逻辑控制
	/// </summary>
	/// <param name="cb">第一次点选的元素</param>
	public void Select(CandyBox cb)
	{
		if(whichTimeClickBox == null)
		{
			whichTimeClickBox = cb;
			return ;
		} 
		else if(whichTimeClickBox != cb)
		{
			//邻接元素才可交换
			if(Mathf.Abs(whichTimeClickBox.rowIndex - cb.rowIndex) + Mathf.Abs(whichTimeClickBox.columnIndex - cb.columnIndex) == 1)
			{
				state = Condition.EXCHANGE;
				Exchange(whichTimeClickBox,cb);
				StartCoroutine(WaitForCheck(0.8f));
			}
			//重置首次点选元素
			whichTimeClickBox = null;
		}
	}
	/// <summary>
	/// 交换两元素(数组,界面都换)(一层封装)
	/// </summary>
	/// <param name="cb1">选择的元素1</param>
	/// <param name="cb2">选择的元素2</param>
	private void Exchange(CandyBox cb1, CandyBox cb2)
	{
		DoExchange(cb1, cb2);
		StartCoroutine(Wait(0.4f, cb1, cb2));
		
	}
	IEnumerator Wait(float t, CandyBox cb1, CandyBox cb2)
	{
		yield return new WaitForSeconds(t);
		//如果交换后不能实现消除,则换回两元素(只是换回,不再循环检测消除了,否则会无限交换)
		if(CheckMatches(false))
		{
			au.PlayOneShot(crushSound);
			RemoveMatches();
		}
		else
		{
			au.PlayOneShot(wrongSound);
			DoExchange(cb1, cb2);
		}
	}
	/// <summary>
	/// 交换两元素(数组,界面都换)(实现层)
	/// </summary>
	/// <param name="cb1">选择的元素1</param>
	/// <param name="cb2">选择的元素2</param>
	void DoExchange(CandyBox cb1, CandyBox cb2)
	{
		state = Condition.EXCHANGE;
		//数组矩阵中也交换元素
		SetCandyBox(cb1.rowIndex, cb1.columnIndex, cb2);
		SetCandyBox(cb2.rowIndex, cb2.columnIndex, cb1);
		//交换行列索引
		cb1.rowIndex = cb1.rowIndex + cb2.rowIndex;
		cb2.rowIndex = cb1.rowIndex - cb2.rowIndex;
		cb1.rowIndex = cb1.rowIndex - cb2.rowIndex;
		cb1.columnIndex = cb1.columnIndex + cb2.columnIndex;
		cb2.columnIndex = cb1.columnIndex - cb2.columnIndex;
		cb1.columnIndex = cb1.columnIndex - cb2.columnIndex;
		//更新位置
		cb1.TweenToPosition();
		cb2.TweenToPosition();
	}
	/// <summary>
	/// 获取数组矩阵特定元素
	/// </summary>
	/// <param name="rowIndex">行索引</param>
	/// <param name="columnIndex">列索引</param>
	/// <returns>返回获取的元素对象CandyBox</returns>
	private CandyBox GetCandyBox(int rowIndex, int columnIndex)
	{
	
		ArrayList tmp = candyArr[rowIndex] as ArrayList;
		CandyBox cb = tmp[columnIndex] as CandyBox;
		return cb; 
	}
	/// <summary>
	/// 向数组矩阵中添加元素
	/// </summary>
	/// <param name="rowIndex">行索引</param>
	/// <param name="columnIndex">列索引</param>
	/// <param name="cb">要添加的元素对象</param>
	private void SetCandyBox(int rowIndex, int columnIndex, CandyBox cb)
	{
		ArrayList tmp  = candyArr[rowIndex] as ArrayList;
		tmp[columnIndex] = cb;
	}
	/// <summary>
	/// 删除元素,下移元素,添加新元素
	/// </summary>
	/// <param name="willRemoveCandyBox">将要删除的元素</param>
	private void Remove(CandyBox willRemoveCandyBox)
	{
		//删除传入元素
		willRemoveCandyBox.Dispose();
		int col = willRemoveCandyBox.columnIndex;
		//使其上元素都下降一格
		for(int row = willRemoveCandyBox.rowIndex + 1; row < rowNum; row++)
		{
			CandyBox upCandyBox = GetCandyBox(row, col);
			upCandyBox.rowIndex--;
			upCandyBox.TweenToPosition();
			//在数组中也改动
			SetCandyBox(row - 1, col, upCandyBox);
		}
		//顶部添加新元素
		CandyBox newCandyBox = AddCandyBox(rowNum - 1, col, true);
		//在数组中也改动
		SetCandyBox(rowNum - 1, col, newCandyBox);
	}
	/// <summary>
	/// 检测有无可消除行列
	/// </summary>
	/// <param name="isTry">标识是否用于检测死图</param>
	/// <returns></returns>
	private bool CheckMatches(bool isTry)
	{
		return CheckHorizontalMatches(isTry) || CheckVerticalMatches(isTry);
	}
	/// <summary>
	/// 检测行有无可消除
	/// </summary>
    /// <param name="isTry">标识是否用于检测死图</param>
	/// <returns></returns>
	private bool CheckHorizontalMatches(bool isTry)
	{
		bool result	= false;
		for(int row = 0; row < rowNum; row++)
		{
			for(int col = 0; col < columnNum - 2; col++)
			{
				if((GetCandyBox(row,col).type == GetCandyBox(row,col + 1).type) &&
			   	   (GetCandyBox(row, col + 1).type == GetCandyBox(row, col + 2).type))
				{
					result = true;
					if(!isTry)
					{
						AddMatch(GetCandyBox(row,col));
						AddMatch(GetCandyBox(row,col + 1));
						AddMatch(GetCandyBox(row, col + 2));
					}
				}
			}
		}
		return result;
	}
	/// <summary>
	/// 检测列有无消除
	/// </summary>
	/// <param name="isTry">标识是否用于检测死图</param>
	/// <returns></returns>
	private bool CheckVerticalMatches(bool isTry)
	{
		bool result = false;
		for(int col = 0; col < columnNum; col++)
		{
			for(int row = 0; row < rowNum - 2; row++)
			{
				if((GetCandyBox(row,col).type == GetCandyBox(row + 1,col).type) &&
			   	   (GetCandyBox(row + 1, col).type == GetCandyBox(row + 2, col).type))
				{
					result = true;
					if(!isTry)
					{
						AddMatch(GetCandyBox(row,col));
						AddMatch(GetCandyBox(row + 1,col));
						AddMatch(GetCandyBox(row + 2, col));
					}
				}
			}
		}
		return result;
	}
	/// <summary>
	/// 将可消除的元素加入待消除数组
	/// </summary>
	/// <param name="cb">可消除元素</param>
	private void AddMatch(CandyBox cb)
	{
		if(matches == null)
		{
			matches = new ArrayList();
		}
		//如果此元素不存在于数组,就添加,防止重复添加
		int index = matches.IndexOf(cb);
		if(index == -1)
		{
			matches.Add(cb);
		}
	}
	/// <summary>
	/// 遍历删除待消除数组中的元素
	/// </summary>
	private void RemoveMatches()
	{
		state = Condition.CRUSH;
		//相机震动
		StartCoroutine(Shake());
		CandyBox temp;
		
		if(matches.Count == 4 || matches.Count > 5)
		{
			au.PlayOneShot(comboSound4);
		}
		else if(matches.Count == 5)
		{
			au.PlayOneShot(comboSound5);
		}

		for(int i = 0; i < matches.Count; i++)
		{
			temp = matches[i] as CandyBox;
			//播放爆炸特效
			ParticleSystem ps = Instantiate(fx,temp.transform.position,temp.transform.rotation);
			Destroy(ps.gameObject, 1);
			Remove(temp);
			//加分
			AddScore(eachGrade);
			//重置进度条
			ResetSlider();
		}
		//重置数组
		matches = new ArrayList();
		StartCoroutine(WaitForCheck(0.8f));
	}
	/// <summary>
	/// 重置进度条
	/// </summary>
	private void ResetSlider()
	{
		slider.value = 1;
	}
	/// <summary>
	/// 加分
	/// </summary>
	/// <param name="eachNum">每个Candy的分数</param>
	private void AddScore(int eachNum)
	{
		presentScore += eachNum;
		Global.score = presentScore;
		showScore.text = presentScore.ToString();
	}
	/// <summary>
	/// 延迟检测消除协程
	/// </summary>
	/// <param name="t">延迟时间</param>
	IEnumerator WaitForCheck(float t)
	{
		yield return new WaitForSeconds(t);
		//再次检测消除
		if(CheckMatches(false))
		{
			au.PlayOneShot(crushSound);
			RemoveMatches();
		}
		else
		{
			state = Condition.NORMAL;
			//检测死图
			if(CheckDeadMap())
			{
				StartCoroutine(Message(3));
				//重随
				Refresh();
			}
		}
	}
	IEnumerator Message(float t)
	{
		message.enabled = true;
		iTween.ShakeScale(message.gameObject,new Vector3(1,1,1),0.5f);
		yield return new WaitForSeconds(t);
		message.enabled = false;
	}
	/// <summary>
	/// 检测是否死图
	/// 思想是,在数组里模拟交换所有纵横相邻的元素,然后检测是否可消
	/// 注意点是,模拟交换后要记得还原回去
	/// </summary>
	/// <returns>是死图就返回true</returns>
	private bool CheckDeadMap()
	{
		bool isdeadmap = true;
		//横向交换检测
		for(int row = 0; row < rowNum; row++)
		{
			for(int col = 0; col < columnNum - 1; col++)
			{
				CandyBox cb1 = GetCandyBox(row, col);
				CandyBox cb2 = GetCandyBox(row, col + 1);
				SetCandyBox(cb1.rowIndex, cb1.columnIndex, cb2);
				SetCandyBox(cb2.rowIndex, cb2.columnIndex, cb1);
				if(CheckMatches(true))
				{
					isdeadmap = false;
				}
				SetCandyBox(cb1.rowIndex, cb1.columnIndex, cb1);
				SetCandyBox(cb2.rowIndex, cb2.columnIndex, cb2);
				if(!isdeadmap)
				{
					return isdeadmap;
				}
			}
		}
		//纵向交换检测
		for(int col = 0; col < columnNum; col++)
		{
			for(int row = 0; row < rowNum - 1; row++)
			{
				CandyBox cb1 = GetCandyBox(row, col);
				CandyBox cb2 = GetCandyBox(row + 1, col);
				SetCandyBox(cb1.rowIndex, cb1.columnIndex, cb2);
				SetCandyBox(cb2.rowIndex, cb2.columnIndex, cb1);
				if(CheckMatches(true))
				{
					isdeadmap = false;
				}
				SetCandyBox(cb1.rowIndex, cb1.columnIndex, cb1);
				SetCandyBox(cb2.rowIndex, cb2.columnIndex, cb2);
				if(!isdeadmap)
				{
					return isdeadmap;
				}
			}
		}
		return isdeadmap;
	}
	/// <summary>
	/// 重新随机全部元素(清除数组,重新添加元素(界面,数组))
	/// </summary>
	public void Refresh()
	{
		state = Condition.EXCHANGE;
		for (int row = 0; row < rowNum; row++) 
		{
			for (int col = 0; col < columnNum; col++) 
			{
				GetCandyBox(row,col).Dispose();
			}
		}
		candyArr.Clear();
		candyArr = new ArrayList();
		for (int row = 0; row < rowNum; row++) 
		{
			ArrayList temp = new ArrayList();
			for (int col = 0; col < columnNum; col++) 
			{	
				temp.Add(AddCandyBox(row, col, false));
			}
			candyArr.Add(temp);
		}
		StartCoroutine(WaitForCheck(0.8f));
	}
	/// <summary>
	/// 相机震动协程
	/// </summary>
	IEnumerator Shake()
	{
		Vector3 oriPos = c_transform.localPosition;
		float spendTime = 0;
		while(spendTime < shakeTime)
		{
			Vector3 randomPoint = oriPos + Random.insideUnitSphere * shakeLevel;
			c_transform.localPosition = Vector3.Lerp(c_transform.localPosition, randomPoint, Time.deltaTime * shakeSpeed);
			yield return null;
			spendTime += Time.deltaTime;
		}
		c_transform.localPosition = oriPos;
	}
}
