using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.Commons;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Sirenix.Utilities;
using static Assets.Scripts.GameCore.Managers.iBMSCManager;
using System.Text.RegularExpressions;

namespace CustomAlbums
{
	public static class BMSCLoader
	{
		private static readonly Regex MainDataRegex = new Regex("^([0-9]{3})([0-9]{2}):(.*)$");
		/// <summary>
		/// A bms loader copied from MuseDash.
		/// 
		/// Ref: Assets.Scripts.GameCore.Managers.iBMSCManager
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="bmsName"></param>
		/// <returns></returns>
		public static BMS Load(Stream stream, string bmsName)
		{
			JObject header = new JObject();
			Dictionary<string, float> BPMExt = new Dictionary<string, float>();
			List<JObject> BPMList = new List<JObject>();

			List<JObject> notes = new List<JObject>();

			JArray notesPercent = new JArray();

			// Calculate MD5 of bms bytes
			string md5 = stream.ToArray().GetMD5().ToString("x2");
			stream.Position = 0;

			StreamReader streamReader = new StreamReader(stream);
			string line;
			// Parse bms file
			while ((line = streamReader.ReadLine()) != null)
			{
				line = line.Trim();
                if (!line.StartsWith("#"))
					continue;

				line = line.Substring(1); // Remove start '#'

				// Parse header field
				if (line.Contains(" "))
				{
					var fileds = line.Split(new char[] { ' ' }, 2);
					string key = fileds[0];
					string value = fileds[1];

					header[key] = value;

					if (key == "BPM")
					{
						// Set default bpm
						float freq = 60f / float.Parse(value) * 4f;
						JObject jObject = new JObject();
						jObject["tick"] = 0f;
						jObject["freq"] = freq;
						BPMList.Add(jObject);
					}
					else if (key.Contains("BPM"))
					{
						// Extended BPM
						BPMExt.Add(key.RemoveFromStart("BPM"), float.Parse(value));
					}
					continue;
				}

				// Parse main data field
				var match = MainDataRegex.Match(line);
				if (!match.Success)
					continue;

				var measure = int.Parse(match.Groups[1].Value);
				var channel = match.Groups[2].Value;
				var rawData = match.Groups[3].Value;

				int dataLength = rawData.Length / 2;
				for (int i = 0; i < dataLength; i++)
				{
					string data = rawData.Substring(i * 2, 2);
					if ("00" == data)
						continue; // Skip

					float currentTick = (float)i / (float)dataLength + (float)measure;

					if ("02" == channel)
					{
						// 小节的缩短
						JObject jObject = new JObject();
						jObject["beat"] = channel;
						jObject["percent"] = float.Parse(rawData);
						notesPercent.Add(jObject);
						break; // 跳过剩余数据
					}
					else if("03" == channel || "08" == channel)
					{
						// 变速
						float bpm = 0f;
						if ("08" == channel && BPMExt.ContainsKey(data))
							bpm = BPMExt[data];// 扩展变速
						else
							bpm = Convert.ToInt32(data, 16);
						JObject jObject = new JObject();
						jObject["tick"] = currentTick;
						jObject["freq"] = 60f / bpm * 4f;
						BPMList.Add(jObject);
						// 按照Tick从高到低排序
						BPMList.Sort((x,y)=>
						{
							if ((float)x["tick"] > (float)y["tick"])
							{
								return -1;
							}
							return 1;
						});
					}
                    else
                    {
						// note
						float num3 = 0f;
						float totalTick = 0f;
						var BPMListBeforeTick = BPMList.FindAll((o) => (float)o["tick"] < currentTick);
						for (int k = BPMListBeforeTick.Count - 1; k >= 0; k--)
						{
							var BPM = BPMListBeforeTick[k];
							var freq = (float)BPM["freq"];
							float keepTick = 0f;
							if (k - 1 >= 0)
							{
								// 下一个速度与上一个速度相差的tick
								var nextBPM = BPMListBeforeTick[k - 1];
								keepTick = (float)nextBPM["tick"] - (float)BPM["tick"];
							}
							if (k == 0)
							{
								// 最后一个
								keepTick = currentTick - (float)BPM["tick"];
							}

							float curTick = totalTick;
							totalTick += keepTick;

							int _curTick = Mathf.FloorToInt(curTick); // 最小取整
							int _totalTick = Mathf.CeilToInt(totalTick);	// 最大取整
							for (int m = _curTick; m < _totalTick; m++)
							{
								int index = m;
								float num10 = 1f;
								if (m == _curTick)
								{
									num10 = (float)(m + 1) - curTick;
								}
								if (m == _totalTick - 1)
								{
									num10 = totalTick - (float)(_totalTick - 1);
								}
								if (_totalTick == _curTick + 1)
								{
									num10 = totalTick - curTick;
								}
								JToken jtoken = notesPercent.Find((JToken pc) => (int)pc["beat"] == index);
								float num11 = (jtoken == null) ? 1f : ((float)jtoken["percent"]);
								num3 += (float)Mathf.RoundToInt(num10 * num11 * freq / 1E-06f) * 1E-06f;
							}

						}
						JObject jObject = new JObject();
						jObject["time"] = 0;
						jObject["value"] = data;
						jObject["tone"] = 0;
						notes.Add(jObject);
					}
				}
			}

			notes.Sort((x, y) =>
			{
				float num12 = (float)x["time"];
				float num13 = (float)y["time"];
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
			BMS bms = new BMS
			{
				info = info,
				notes = notes,
				notesPercent = notesPercent,
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
