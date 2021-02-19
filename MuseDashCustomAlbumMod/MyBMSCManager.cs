using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.Commons;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Sirenix.Utilities;
using Assets.Scripts.GameCore;

namespace MuseDashCustomAlbumMod
{
    class MyBMSCManager
    {
		public static readonly MyBMSCManager instance = new MyBMSCManager();

		private MyBMSCManager() { }

		public Dictionary<string, float> bpmTones = new Dictionary<string, float>();

		// Assets.Scripts.GameCore.Managers.iBMSCManager
		public iBMSCManager.BMS Load(byte[] bytes, string bmsName)
		{
			bpmTones.Clear();

			JObject jInfo = new JObject();
			JArray jarray = new JArray();

			JArray jarray2 = new JArray();
			List<JObject> list = new List<JObject>();

			StreamReader streamReader = new StreamReader(new MemoryStream(bytes), AssetsUtils.GetBytesEncodeType(bytes));
			// Calculate MD5 of bms bytes
			byte[] md5Array = MD5.Create().ComputeHash(bytes);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < md5Array.Length; i++)
			{
				sb.Append(md5Array[i].ToString("x2"));
			}
			string md5 = sb.ToString();

			string bmsLine;
			// Parse bms command
			while ((bmsLine = streamReader.ReadLine()) != null)
			{
				bmsLine = bmsLine.Trim();
				if (!string.IsNullOrEmpty(bmsLine) && bmsLine.StartsWith("#"))
				{
					bmsLine = bmsLine.Remove(0, 1); // Remove left right space and start '#'
					
					if (bmsLine.Contains(" "))
					{
						// Parse info
						string infoKey = bmsLine.Split(' ')[0];
						string infoValue = bmsLine.Remove(0, infoKey.Length + 1);

						jInfo[infoKey] = infoValue;
						if (infoKey == "BPM")
						{
							float freq = 60f / float.Parse(infoValue) * 4f;
							JObject jobject2 = new JObject();
							jobject2["tick"] = 0f;
							jobject2["freq"] = freq;
							list.Add(jobject2);
						}
						else if (infoKey.Contains("BPM"))
						{
							this.bpmTones.Add(infoKey.Replace("BPM", string.Empty), float.Parse(infoValue));
						}
					}
					else if (bmsLine.Contains(":"))
					{
						string[] array3 = bmsLine.Split(new char[]
						{
					':'
						});
						string text4 = array3[0];
						string text5 = array3[1];
						int num = int.Parse(text4.Substring(0, 3));
						string text6 = text4.Substring(3, 2);
						if (text6 == "02")
						{
							JObject jobject3 = new JObject();
							jobject3["beat"] = num;
							jobject3["percent"] = float.Parse(text5);
							jarray2.Add(jobject3);
						}
						else
						{
							int num2 = text5.Length / 2;
							for (int j = 0; j < num2; j++)
							{
								string text7 = text5.Substring(j * 2, 2);
								if (!(text7 == "00"))
								{
									float theTick = (float)j / (float)num2 + (float)num;
									if (text6 == "03" || text6 == "08")
									{
										float value2 = 60f / ((!(text6 == "08") || !this.bpmTones.ContainsKey(text7)) ? ((float)Convert.ToInt32(text7, 16)) : this.bpmTones[text7]) * 4f;
										JObject jobject4 = new JObject();
										jobject4["tick"] = theTick;
										jobject4["freq"] = value2;
										list.Add(jobject4);
										list.Sort(delegate (JObject l, JObject r)
										{
											float num12 = (float)l["tick"];
											float num13 = (float)r["tick"];
											if (num12 > num13)
											{
												return -1;
											}
											return 1;
										});
									}
									else
									{
										float num3 = 0f;
										float num4 = 0f;
										List<JObject> list2 = list.FindAll((JObject b) => (float)b["tick"] < theTick);
										for (int k = list2.Count - 1; k >= 0; k--)
										{
											JObject jobject5 = list2[k];
											float num5 = 0f;
											float num6 = (float)jobject5["freq"];
											if (k - 1 >= 0)
											{
												JObject jobject6 = list2[k - 1];
												num5 = (float)jobject6["tick"] - (float)jobject5["tick"];
											}
											if (k == 0)
											{
												num5 = theTick - (float)jobject5["tick"];
											}
											float num7 = num4;
											num4 += num5;
											int num8 = Mathf.FloorToInt(num7);
											int num9 = Mathf.CeilToInt(num4);
											for (int m = num8; m < num9; m++)
											{
												int index = m;
												float num10 = 1f;
												if (m == num8)
												{
													num10 = (float)(m + 1) - num7;
												}
												if (m == num9 - 1)
												{
													num10 = num4 - (float)(num9 - 1);
												}
												if (num9 == num8 + 1)
												{
													num10 = num4 - num7;
												}
												JToken jtoken = jarray2.Find((JToken pc) => (int)pc["beat"] == index);
												float num11 = (jtoken == null) ? 1f : ((float)jtoken["percent"]);
												num3 += (float)Mathf.RoundToInt(num10 * num11 * num6 / 1E-06f) * 1E-06f;
											}
										}
										JObject jobject7 = new JObject();
										jobject7["time"] = num3;
										jobject7["value"] = text7;
										jobject7["tone"] = text6;
										jarray.Add(jobject7);
									}
								}
							}
						}
					}
				}
			}
			jarray.Sort(delegate (JToken l, JToken r)
			{
				float num12 = (float)l["time"];
				float num13 = (float)r["time"];
				if (num12 > num13)
				{
					return 1;
				}
				if (num13 > num12)
				{
					return -1;
				}
				return 0;
			});
			iBMSCManager.BMS bms = new iBMSCManager.BMS
			{
				info = jInfo,
				notes = jarray,
				notesPercent = jarray2,
				md5 = md5
			};
			bms.info["NAME"] = bmsName;
			bms.info["NEW"] = true;
			
			if (bms.info.Properties().ToList<JProperty>().Find((JProperty p) => p.Name == "BANNER") == null)
			{
				bms.info["BANNER"] = "cover/none_cover.png";
			}
			else
			{
				bms.info["BANNER"] = "cover/" + (string)bms.info["BANNER"];
			}
			return bms;
		}
	}
}
