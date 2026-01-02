using System;
using System.Collections.Generic;
using NaughtyAttributes;
using QuickEye.Utility;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;

#endif

namespace GB
{
    [CreateAssetMenu(fileName = "SPRAnimationClip", menuName = "GB/SPRAnimationClip", order = 1)]
    public class SPRAnimationClip : ScriptableObject
    {

        [BoxGroup("Resources")]
        [SerializeField] Sprite[] _sprites;
        [BoxGroup("Resources")]
        public float Speed = 1;
        [BoxGroup("Resources")]
        public bool IsLoop = false;

        [BoxGroup("Resources")]
        public UnityDictionary<string, Sprite[]> skins;

        [BoxGroup("Resources")]
        public UnityDictionary<int, List<TriggerData>> Triggers;
     

        public int GetSpriteCount(string skinName)
        {
            if(string.IsNullOrEmpty(skinName))
            {
                if (_sprites != null) return _sprites.Length;
                else return 0;
            }
            else
            {
                if(skins.ContainsKey(skinName))
                {
                    if (skins[skinName] != null) return skins[skinName].Length;
                    else return 0;
                }
                else
                {
                    return GetSpriteCount(string.Empty);
                }

            }

        }



        // public int SpriteCount
        // {
        //     get
        //     {
        //         if (_sprites != null) return _sprites.Length;
        //         else return 0;
        //     }
        // }

        public Sprite GetSprite(int index, string skinName = null)
        {
            if (string.IsNullOrEmpty(skinName))
            {
                if (_sprites != null && index < _sprites.Length)
                    return _sprites[index];
                else
                    return null;
            }
            else
            {
                if(skins != null && skins.ContainsKey(skinName))
                {
                    if (index < skins[skinName].Length)
                        return skins[skinName][index];
                    else
                        return GetSprite(index);
                }
                else
                {
                    return GetSprite(index);
                }

            }
            
        }


        #region  EDIT

#if UNITY_EDITOR

        
        public void LoadSprite(string p,string skinName = "")
        {

            p = p.Replace(Application.dataPath + "/", "");
            string path = Application.dataPath + "/" + p;

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
            if (di.Exists == false)
            {
                _sprites = null;
                Debug.Log("<color=red>Directory - None Failed</color>");
                return;
            }

            List<string> fileList = new List<string>();

            foreach (System.IO.FileInfo File in di.GetFiles())
            {
                if (File.Extension.ToLower().CompareTo(".png") == 0)
                {
                    string FullFileName = File.FullName;
                    int len = FullFileName.Length - Application.dataPath.Length;
                    fileList.Add("Assets" + FullFileName.Substring(Application.dataPath.Length, len));
                }
            }
            List<Sprite> sprList = new List<Sprite>();

            for (int i = 0; i < fileList.Count; ++i)
            {
                UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(fileList[i]);
                foreach (UnityEngine.Object v in data)
                {
                    if (v.GetType() == typeof(Sprite))
                    {
                        sprList.Add((Sprite)v);
                    }
                }
            }
            if(string.IsNullOrEmpty(skinName))
            {
                _sprites = sprList.ToArray();
            }
            else
            {
                if(skins == null) skins = new UnityDictionary<string, Sprite[]>();
                skins[skinName] = sprList.ToArray();
            }

            

            if (_sprites.Length > 0)
                Debug.Log("<color=green>LoadSprite - Success</color>");
            else
                Debug.Log("<color=red>LoadSprite - Failed</color>");
        }



#endif
        #endregion
    }

    [Serializable]
    public class TriggerData
    {
        public string Str;
        public Vector3 Vec3;
        public float Float;
        public int Int;
    }
}