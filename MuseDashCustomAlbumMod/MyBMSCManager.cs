using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.Commons;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace MuseDashCustomAlbumMod
{
    internal class MyBMSCManager
    {
        public static readonly MyBMSCManager instance = new MyBMSCManager();

        public Dictionary<string, float> bpmTones = new Dictionary<string, float>();

        private MyBMSCManager()
        {
        }

        // Assets.Scripts.GameCore.Managers.iBMSCManager
        public iBMSCManager.BMS Load(byte[] bytes, string bmsName)
        {
            bpmTones.Clear();

            var info = new JObject();
            var notes = new JArray();

            var notesPercent = new JArray();
            var list = new List<JObject>();

            var streamReader =
                new StreamReader(new MemoryStream(bytes), AssetsUtils.GetBytesEncodeType(bytes));
            // Calculate MD5 of bms bytes
            var md5Array = MD5.Create().ComputeHash(bytes);
            var sb = new StringBuilder();
            for (var i = 0; i < md5Array.Length; i++) sb.Append(md5Array[i].ToString("x2"));

            var md5 = sb.ToString();

            string txtLine;
            // Parse bms
            while ((txtLine = streamReader.ReadLine()) != null)
            {
                txtLine = txtLine.Trim();
                if (!string.IsNullOrEmpty(txtLine) && txtLine.StartsWith("#"))
                {
                    txtLine = txtLine.Remove(0, 1); // Remove left right space and start '#'

                    if (txtLine.Contains(" "))
                    {
                        // Parse info
                        var infoKey = txtLine.Split(' ')[0];
                        var infoValue = txtLine.Remove(0, infoKey.Length + 1);

                        info[infoKey] = infoValue;
                        if (infoKey == "BPM")
                        {
                            var freq = 60f / float.Parse(infoValue) * 4f;
                            var jObject = new JObject();
                            jObject["tick"] = 0f;
                            jObject["freq"] = freq;
                            list.Add(jObject);
                        }
                        else if (infoKey.Contains("BPM"))
                        {
                            bpmTones.Add(infoKey.Replace("BPM", string.Empty), float.Parse(infoValue));
                        }
                    }
                    else if (txtLine.Contains(":"))
                    {
                        var keyValue = txtLine.Split(':');

                        //string text4 = array3[0];
                        var beat = int.Parse(keyValue[0].Substring(0, 3));
                        var type = keyValue[0].Substring(3, 2);

                        var value = keyValue[1];
                        if (type == "02")
                        {
                            var jObject = new JObject();
                            jObject["beat"] = beat;
                            jObject["percent"] = float.Parse(value);
                            notesPercent.Add(jObject);
                        }
                        else
                        {
                            var objLength = value.Length / 2;
                            for (var i = 0; i < objLength; i++)
                            {
                                var note = value.Substring(i * 2, 2);
                                if (note == "00") continue;

                                var theTick = i / (float) objLength + beat;
                                // 变速
                                if (type == "03" || type == "08")
                                {
                                    var freq = 60f / (type != "08" || !bpmTones.ContainsKey(note)
                                        ? Convert.ToInt32(note, 16)
                                        : bpmTones[note]) * 4f;
                                    var jObject = new JObject();
                                    jObject["tick"] = theTick;
                                    jObject["freq"] = freq;
                                    list.Add(jObject);
                                    list.Sort(delegate(JObject l, JObject r)
                                    {
                                        var num12 = (float) l["tick"];
                                        var num13 = (float) r["tick"];
                                        if (num12 > num13) return -1;

                                        return 1;
                                    });
                                }
                                else
                                {
                                    var num3 = 0f;
                                    var num4 = 0f;
                                    var list2 = list.FindAll(b => (float) b["tick"] < theTick);
                                    for (var k = list2.Count - 1; k >= 0; k--)
                                    {
                                        var jobject5 = list2[k];
                                        var num5 = 0f;
                                        var num6 = (float) jobject5["freq"];
                                        if (k - 1 >= 0)
                                        {
                                            var jobject6 = list2[k - 1];
                                            num5 = (float) jobject6["tick"] - (float) jobject5["tick"];
                                        }

                                        if (k == 0) num5 = theTick - (float) jobject5["tick"];

                                        var num7 = num4;
                                        num4 += num5;
                                        var num8 = Mathf.FloorToInt(num7);
                                        var num9 = Mathf.CeilToInt(num4);
                                        for (var m = num8; m < num9; m++)
                                        {
                                            var index = m;
                                            var num10 = 1f;
                                            if (m == num8) num10 = m + 1 - num7;

                                            if (m == num9 - 1) num10 = num4 - (num9 - 1);

                                            if (num9 == num8 + 1) num10 = num4 - num7;

                                            var jtoken = notesPercent.Find(pc => (int) pc["beat"] == index);
                                            var num11 = jtoken == null ? 1f : (float) jtoken["percent"];
                                            num3 += Mathf.RoundToInt(num10 * num11 * num6 / 1E-06f) * 1E-06f;
                                        }
                                    }

                                    var jobject7 = new JObject();
                                    jobject7["time"] = num3;
                                    jobject7["value"] = note;
                                    jobject7["tone"] = type;
                                    notes.Add(jobject7);
                                }
                            }
                        }
                    }
                }
            }

            notes.Sort(delegate(JToken l, JToken r)
            {
                var num12 = (float) l["time"];
                var num13 = (float) r["time"];
                if (num12 > num13) return 1;

                if (num13 > num12) return -1;

                return 0;
            });
            var bms = new iBMSCManager.BMS
            {
                info = info,
                notes = notes,
                notesPercent = notesPercent,
                md5 = md5
            };
            bms.info["NAME"] = bmsName;
            bms.info["NEW"] = true;

            if (bms.info.Properties().ToList().Find(p => p.Name == "BANNER") == null)
                bms.info["BANNER"] = "cover/none_cover.png";
            else
                bms.info["BANNER"] = "cover/" + (string) bms.info["BANNER"];

            return bms;
        }
    }
}