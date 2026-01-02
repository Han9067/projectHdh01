#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using QuickEye.Utility;

namespace GB
{
    public class EditorSPRAnimation : EditorWindow
    {
        [MenuItem("GB/Core/SPRAnimation")]
        static void init()
        {
            EditorWindow.GetWindow(typeof(EditorSPRAnimation));
        }
        private void OnEnable()
        {
            Load();

        }

        private void OnFocus() {
            Load();
        }

        public UnityDictionary<string, SPRAnimationClip> dataAssets = new UnityDictionary<string, SPRAnimationClip>();

        
        void Load()
        {
            dataAssets = TryGetAsset<SPRAnimationClip>(Application.dataPath);
            _serializedObject = new SerializedObject(this);
            _myUser = _serializedObject.FindProperty("dataAssets");

        }

        Vector2 scrollPos;
        
        private SerializedObject _serializedObject;
        private SerializedProperty _myUser;
        private Vector2 scrollPosition;


        private void OnGUI()
        {
            
            GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));
            GB.EditorGUIUtil.DrawHeaderLabel("GB SPRAnimation");
            GB.EditorGUIUtil.Space(5);

            if (dataAssets != null && _myUser != null)
            {
                GUILayout.BeginVertical("box");
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                _serializedObject.Update();
                EditorGUILayout.PropertyField(_myUser);
                _serializedObject.ApplyModifiedProperties();
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            if(GB.EditorGUIUtil.DrawSyleButton("Create SRPAnimationClip"))
            {

                string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                if (!string.IsNullOrEmpty(path) && path.Contains("Assets"))
                {
                    Debug.Log(path);
                    Save(path);
                }
                else
                {
                    Debug.LogWarning("Path Error :" + path);

                }



            }





            GUILayout.EndArea();


        }

        void Save(string path)
        {
            string filePath = path + "/SPRAnimationClip.asset";
            if(File.Exists(filePath))
            {
                int fileNumber = 1;
                string newFilePath;
                do
                {
                    newFilePath = path + "/SPRAnimationClip_"+ fileNumber+".asset";
                    fileNumber++;
                } while (File.Exists(newFilePath));

                filePath = newFilePath;
            }
            int idx = filePath.IndexOf("Assets/");
            string fixPath = filePath.Substring(idx, filePath.Length - idx);
            var clip = ScriptableObject.CreateInstance<SPRAnimationClip>();
            
            
            AssetDatabase.CreateAsset(clip, fixPath); // 파일 생성
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            
            Selection.activeObject = clip;
            Load();
            Repaint();
            AssetDatabase.Refresh();

        }





         public static UnityDictionary<string, T> TryGetAsset<T>(string folderPath, string optionalName = "") where T : UnityEngine.Object
        {
            // Gets all files with the Directory System.IO class
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);


            UnityDictionary<string, T> dict = new UnityDictionary<string, T>();

            // move through all files
            foreach (var file in files)
            {

                string path = FixFilePath(file);

                // Then I try and load the asset at the current path.
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                // check the asset to see if it's not null
                if (asset)
                {
                    var sprits = folderPath.Split("/");
                    var fName = sprits[sprits.Length - 1];

                    int idx = file.IndexOf(fName);
                    idx += fName.Length + 1;
                    string paserName = file.Substring(idx, file.Length - idx);

                    sprits = paserName.Split(".");


                    dict[sprits[0].Replace("\\", "/")] = asset;

                }
            }

            Debug.Log("files :" + dict.Count);


            return dict;
        }

        static string FixFilePath(string path)
        {
            if (path.Contains("Assets"))
            {
                int idx = path.IndexOf("Assets");
                int len = path.Length - idx;

                return path.Substring(idx, len);

            }

            return string.Empty;
        }




    }
}
#endif