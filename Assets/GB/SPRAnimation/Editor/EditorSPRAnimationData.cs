using UnityEngine;
using System.Collections;
using System.IO;



#if UNITY_EDITOR
using UnityEditor;

namespace GB
{

    [CustomEditor(typeof(SPRAnimationClip))]
    public class EditorSPRAnimationClip : Editor
    {

        public int CurIDX;

        public bool IsPlaying;
        float _speed = 1;

        string SkinName;
        string AddSkinName;


        private void OnEnable()
        {
            CurIDX = 0;
            EditorApplication.update += OnEditorUpdate;
            deltaTime = (float)EditorApplication.timeSinceStartup;
        }

        private void OnDisable() 
        {
             EditorApplication.update -= OnEditorUpdate;

             
            
        }

        float _time;

        float deltaTime;

        void OnEditorUpdate()
        {
            

            if(IsPlaying)
            {
                float dt =(float)EditorApplication.timeSinceStartup - deltaTime;
                deltaTime = (float)EditorApplication.timeSinceStartup;

                _time += dt * _speed;
                UpdatePreview();
                Repaint(); 

            }

            


        }

        Vector2 scrollPos;


        public override void OnInspectorGUI()
        {
            GB.EditorGUIUtil.DrawHeaderLabel("SPRAnim Clip");
            base.OnInspectorGUI();


            var t = (SPRAnimationClip)target;

            GB.EditorGUIUtil.Start_VerticalBox();
            GUILayout.BeginVertical();

            if (GB.EditorGUIUtil.DrawSyleButton("Select Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                Debug.Log(path);
                if (!string.IsNullOrEmpty(path)) 
                {
                    t.LoadSprite(path);
                    EditorUtility.SetDirty(t); // 변경 사항 저장

                }
            }

            if (GB.EditorGUIUtil.DrawSyleButton("Current Folder Load"))
            {
                if (Selection.activeObject != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        string path = Path.Combine(Application.dataPath.Replace("/Assets",""),System.IO.Path.GetDirectoryName(assetPath)).Replace("\\","/");
                        if (!string.IsNullOrEmpty(path)) 
                        {
                            t.LoadSprite(path);
                            EditorUtility.SetDirty(t); // 변경 사항 저장

                        }
                    }
                }

                

            }

            
             

            string mySkinName = SkinName;
            if (string.IsNullOrEmpty(mySkinName)) mySkinName = "Default";

            GB.EditorGUIUtil.DrawLabel("SkinName : " + mySkinName);



            GUILayout.EndVertical();

            GUILayout.Space(20);

            if (t.GetSpriteCount(SkinName) > 0)
            {
                Rect r = EditorGUILayout.BeginVertical();
                GUILayout.Space(15);


                if (CurIDX > 0)
                    _hSliderValue = ((float)(CurIDX + 1) / (float)t.GetSpriteCount(SkinName));
                else
                    _hSliderValue = 0;


                EditorGUI.ProgressBar(r, _hSliderValue, CurIDX + "/" + (t.GetSpriteCount(SkinName) - 1));

                EditorGUILayout.EndVertical();


                GUILayout.BeginHorizontal();

                if (!IsPlaying)
                {
                    GB.EditorGUIUtil.BackgroundColor(Color.green);

                    if (GB.EditorGUIUtil.DrawSyleButton("▷"))
                    {
                        Play();
                    }

                    GB.EditorGUIUtil.BackgroundColor(Color.white);

                }
                else
                {
                    GB.EditorGUIUtil.BackgroundColor(Color.red);

                    if (GB.EditorGUIUtil.DrawSyleButton("II"))
                    {
                        Stop();
                    }
                    GB.EditorGUIUtil.BackgroundColor(Color.white);
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                if (GB.EditorGUIUtil.DrawSyleButton("«"))
                {
                    if (CurIDX > 0)
                        CurIDX--;
                }

                if (GB.EditorGUIUtil.DrawSyleButton("»"))
                {
                    if (CurIDX < t.GetSpriteCount(SkinName) - 1)
                        CurIDX++;
                }

                GUILayout.EndHorizontal();


            }

           


            GB.EditorGUIUtil.End_Vertical();



            scrollPos = GB.EditorGUIUtil.Start_ScrollView(scrollPos);
            GB.EditorGUIUtil.Start_VerticalBox();

            GB.EditorGUIUtil.DrawSectionStyleLabel("Skin");

            GB.EditorGUIUtil.Start_Horizontal();
            
            AddSkinName = GB.EditorGUIUtil.DrawSyleTextField("SkinName", AddSkinName);
            if (GB.EditorGUIUtil.DrawButton("Select Folder", GUILayout.Width(100)))
            {
                if (string.IsNullOrEmpty(AddSkinName)) 
                {
                    Debug.LogWarning("Skin Name Empty!!");
                    return;
                }
                if(t.skins.ContainsKey(AddSkinName))
                {
                    Debug.LogWarning("Same Skin Name!!");
                    return;
                }
                string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                if (!string.IsNullOrEmpty(path)) t.LoadSprite(path, AddSkinName);
            }

            GB.EditorGUIUtil.End_Horizontal();


            GB.EditorGUIUtil.Start_Horizontal();
            GB.EditorGUIUtil.DrawStyleLabel("", GUILayout.Width(200));
            GB.EditorGUIUtil.DrawStyleLabel("Default", GUILayout.Width(200));

            GB.EditorGUIUtil.BackgroundColor(string.Equals(mySkinName, "Default") ? Color.blue : Color.white);
            if (GB.EditorGUIUtil.DrawButton("Apply", GUILayout.Width(200)))
            {
                SkinName = string.Empty;
            }

            GB.EditorGUIUtil.BackgroundColor(Color.white);
            GB.EditorGUIUtil.End_Horizontal();

            if (t.skins == null) t.skins = new QuickEye.Utility.UnityDictionary<string, Sprite[]>();

            string removeName = string.Empty;

            foreach (var v in t.skins)
            {
                GB.EditorGUIUtil.Start_Horizontal();
                GB.EditorGUIUtil.DrawStyleLabel("", GUILayout.Width(200));
                GB.EditorGUIUtil.DrawStyleLabel(v.Key, GUILayout.Width(200));

                GB.EditorGUIUtil.BackgroundColor(string.Equals(SkinName, v.Key) ? Color.blue : Color.white);
                if (GB.EditorGUIUtil.DrawButton("Apply", GUILayout.Width(100)))
                {
                    SkinName = v.Key;
                }

                GB.EditorGUIUtil.BackgroundColor(Color.red);

                if (GB.EditorGUIUtil.DrawButton("Remove", GUILayout.Width(100)))
                {
                    removeName = v.Key;
                }


                GB.EditorGUIUtil.BackgroundColor(Color.white);
                GB.EditorGUIUtil.End_Horizontal();

            }

            if(!string.IsNullOrEmpty( removeName) )
            {
                t.skins.Remove(removeName);
            }


            GB.EditorGUIUtil.End_Vertical();
            GB.EditorGUIUtil.End_ScrollView();

        }

        float _hSliderValue;
        float _fixTimer;
        const float TIMER = 0.08f;

        public void Play()
        {
            var t = (SPRAnimationClip)target;
            _fixTimer = TIMER / t.Speed;
            CurIDX = 0;
            IsPlaying = true;
            _time = 0;
            
        }

        public void Stop()
        {
            IsPlaying = false;

            
        }

        public void UpdatePreview()
        {
            var t = (SPRAnimationClip)target;

            if (_time > _fixTimer)
            {

                _time = 0;
                CurIDX++;
                if (CurIDX >= t.GetSpriteCount(SkinName))
                {
                    CurIDX = 0;
                }

            }

        }


       

        public override void OnPreviewSettings()
        {
            var playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            var pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            var previewButtonSettingsStyle = new GUIStyle("preButton");
            var buttonContent = IsPlaying ? pauseButtonContent : playButtonContent;
            IsPlaying = GUILayout.Toggle(IsPlaying, buttonContent, previewButtonSettingsStyle);
            DrawSpeedSlider();
        }


        public override bool HasPreviewGUI()
        {
            return true;
        }



        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            var t = (SPRAnimationClip)target;
            var spr = t.GetSprite(CurIDX, SkinName);

            if (spr == null) return;
            var texture = AssetPreview.GetAssetPreview(spr);
            if (texture == null) return;


            EditorGUI.DrawTextureTransparent(r, texture, ScaleMode.ScaleToFit);


        }

        private void DrawSpeedSlider()
        {

            var preSlider = new GUIStyle("preSlider");
            var preSliderThumb = new GUIStyle("preSliderThumb");
            var preLabel = new GUIStyle("preLabel");
            var speedScale = EditorGUIUtility.IconContent("SpeedScale");
            var t = (SPRAnimationClip)target;
            GUILayout.Box(speedScale, preLabel);
            t.Speed =
                            GUILayout.HorizontalSlider(t.Speed, 0, 10, preSlider, preSliderThumb);
            GUILayout.Label(t.Speed.ToString("0.00"), preLabel, GUILayout.Width(40));

            _speed  = t.Speed;
        }

    }

}

#endif