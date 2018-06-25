using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace FileOperate
{
	public class FileHandle : MonoBehaviour 
	{
	
		
		/// <summary>
		/// 从INI文件中读取数据到字典容器中
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <param name="name">文件名</param>
		/// <returns>返回key=string,value=string的字典容器</returns>
		public static Dictionary<string,string> ReadINIFile(string path, string name)
		{
			if(!File.Exists(path + "//" +name))
			{
				print("文件不存在,重建,0");
				StreamWriter sw = CreateFile(path, name);
				sw.WriteLine("highScore=0");
				sw.Close();
				sw.Dispose();
			}
			StreamReader sr = new StreamReader(path + "//" +name);
			Dictionary<string,string> iniFileDictionary = new Dictionary<string,string>();
			string line;
			//一行行读取
			while(!string.IsNullOrEmpty(line = sr.ReadLine()))
			{
				//移除首尾空格
				line.Trim();
				string[] parts = line.Split(new char[]{'='});
				iniFileDictionary.Add(parts[0].Trim(), parts[1].Trim());
			}
			//关闭流
			sr.Close();
			//销毁流
			sr.Dispose();
			return iniFileDictionary;
		}
		/// <summary>
		/// 创建新文件
		/// </summary>
		/// <param name="path">创建路径</param>
		/// <param name="name">文件名</param>
		/// <returns>返回指向所创建文件的输出流</returns>
		public static StreamWriter CreateFile(string path, string name)
		{
			StreamWriter sw;
			FileInfo fileInfo = new FileInfo(path + "//" + name);
			//创建新文件
			sw = fileInfo.CreateText();
			return sw;
		}

	}
}
