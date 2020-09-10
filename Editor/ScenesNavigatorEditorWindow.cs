using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.IO;
using System.Linq;


public class ScenesNavigatorEditorWindow : EditorWindow
{
    static ScenesNavigatorEditorWindow instance;

    const string m_title = "Scenes in project:";
    const string m_title_tooltip = "All scenes added to build in player settings:";
    const string m_build_count_format = "In build settings: {0}.";
    const string m_total_count_format = "Total: {0}.    ";

    static readonly GUIContent header = new GUIContent(m_title, m_title_tooltip);
    static GUIContent scenes_content;
    static GUIContent build_report_content;
    static GUIContent scripts_folder_content;

    static readonly Vector2 size = new Vector2(202, 20);
    static readonly Vector2 offset = new Vector2(5, 100);

    static int m_scene_to_load_id;
    static int m_all_scenes_count;

    static bool AutosaveScenes { get => EditorPrefs.GetBool(nameof(AutosaveScenes)); set => EditorPrefs.SetBool(nameof(AutosaveScenes), value); }

    [MenuItem("Window/"+nameof(ScenesNavigatorEditorWindow))]
    public static void Init()
    {
        instance = GetWindow<ScenesNavigatorEditorWindow>();
        instance.titleContent = new GUIContent(nameof(ScenesNavigatorEditorWindow));
        instance.Show();
    }
    void OnEnable()
    {
        var default_asset_store_content = EditorGUIUtility.IconContent("WelcomeScreen.AssetStoreLogo", " Asset Store Logo");

        build_report_content = new GUIContent(default_asset_store_content)
        {
            text = " Open Build Report Inspector"
        };

        var default_folder_content = EditorGUIUtility.IconContent("Folder Icon", " Folder");

        scenes_content = new GUIContent(default_folder_content)
        {
            text = " Scenes"
        };

        scripts_folder_content = new GUIContent(default_folder_content)
        {
            text = " Scripts"
        };

        var all_scenes = AssetDatabase.FindAssets("t: Scene");
        m_all_scenes_count = all_scenes.Length;
        //Debug.Log($"Scenes[{all_scenes.Length}]: {string.Join(", ", all_scenes)}");
    }
    void OnGUI()
    {
        ShowScenesGUI();

        ShowAdditionalGUI();
    }
    void OnInspectorUpdate()
    {
        Repaint();
    }

    static void ShowScenesGUI()
    {
        var build_scenes_count_text = string.Format(m_build_count_format, SceneManager.sceneCountInBuildSettings);
        var all_scenes_count_text = string.Format(m_total_count_format, m_all_scenes_count);

        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField(all_scenes_count_text + build_scenes_count_text);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Always save before transition: ");
        AutosaveScenes = EditorGUILayout.Toggle(string.Empty, AutosaveScenes);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        var scenes = EditorBuildSettings.scenes;
        var texts = scenes.Select(x => Path.GetFileNameWithoutExtension(x.path)).ToArray();

        m_scene_to_load_id = GUILayout.SelectionGrid(m_scene_to_load_id, texts, xCount: 3, EditorStyles.miniButton);


        var unity_scene_icon = EditorGUIUtility.IconContent("d_BuildSettings.SelectedIcon");
        var selected_scene_text = $" Load Scene: {Path.GetFileNameWithoutExtension(scenes[m_scene_to_load_id].path)}";
        var btn_content = new GUIContent(selected_scene_text, unity_scene_icon.image);

        if (GUILayout.Button(btn_content, GUILayout.MaxHeight(24)))
        {
            var scene = scenes[m_scene_to_load_id];
            var choice = 0;
            
            if (!AutosaveScenes)
            {
                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    choice = EditorUtility.DisplayDialogComplex("Save current scene?", "Do You want to save current scene?", "Yes", "No", "Cancel");
                }
                else
                {
                    choice = 1;
                }
            }
            switch (choice)
            {
                case 0: EditorSceneManager.SaveOpenScenes(); EditorSceneManager.OpenScene(scene.path); break;
                case 1: EditorSceneManager.OpenScene(scene.path); break;
                default: break;
            }
        }
        EditorGUILayout.Space();
    }


    static void ShowAdditionalGUI()
    {
        var cur_rect = GUILayoutUtility.GetRect(size.x, size.y);
        var rect = cur_rect;//new Rect(offset.x, offset.y + size.y * (/*scene_count*/ +2), size.x, size.y);

        GUI.Label(rect,"Additionals:",EditorStyles.boldLabel);
        rect.y += size.y;

#if BUILD_REPORT_INSPECTOR
        if (GUI.Button(rect, build_report_content))
        {            
            BuildReportInspector.OpenLastBuild();
        } 
        rect.y += size.y;
#endif
        rect.width /= 2;

        if (GUI.Button(rect, scenes_content))
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scenes");

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
        rect.x += rect.width;

        if (GUI.Button(rect, scripts_folder_content))
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scripts");

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }

}
