// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// public class UtilityWindow : EditorWindow
// {
//     private SceneAsset sceneAsset;
//     private TextField welcomeMessageField;
//     private ObjectField startSceneField;
//     
//     private const string SCENE_PREF_KEY = "StartSceneGUID";
//     private const string LAST_SCENE_PREF_KEY = "LastEditedScenePath";
//     private const string WELCOME_MESSAGE_KEY = "WelcomeMessage";
//
//     void OnEnable()
//     {
//         string sceneGUID = EditorPrefs.GetString(SCENE_PREF_KEY, "");
//         if (!string.IsNullOrEmpty(sceneGUID))
//         {
//             string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
//             if (!string.IsNullOrEmpty(scenePath))
//             {
//                 sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
//                 EditorSceneManager.playModeStartScene = sceneAsset;
//             }
//         }
//     }
//
//     public void CreateGUI()
//     {
//         VisualElement root = rootVisualElement;
//         root.style.paddingTop = 10;
//         root.style.paddingBottom = 10;
//         root.style.paddingLeft = 10;
//         root.style.paddingRight = 10;
//
//         // Welcome Message Section
//         Label welcomeLabel = new Label("Welcome Message");
//         welcomeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
//         welcomeLabel.style.marginBottom = 5;
//         root.Add(welcomeLabel);
//
//         welcomeMessageField = new TextField();
//         welcomeMessageField.multiline = true;
//         welcomeMessageField.value = EditorPrefs.GetString(WELCOME_MESSAGE_KEY, "");
//         welcomeMessageField.style.height = 60;
//         welcomeMessageField.style.marginBottom = 20;
//         welcomeMessageField.RegisterValueChangedCallback(evt =>
//         {
//             EditorPrefs.SetString(WELCOME_MESSAGE_KEY, evt.newValue);
//         });
//         root.Add(welcomeMessageField);
//
//         // Start Scene Setup Section
//         Label startSceneLabel = new Label("Start Scene Setup");
//         startSceneLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
//         startSceneLabel.style.marginBottom = 5;
//         root.Add(startSceneLabel);
//
//         startSceneField = new ObjectField("Start Scene");
//         startSceneField.objectType = typeof(SceneAsset);
//         startSceneField.value = sceneAsset;
//         startSceneField.RegisterValueChangedCallback(evt =>
//         {
//             sceneAsset = evt.newValue as SceneAsset;
//             SaveSelectedScene();
//         });
//         root.Add(startSceneField);
//
//         Button setStartSceneButton = new Button(() => SaveSelectedScene());
//         setStartSceneButton.text = "Set as Start Scene";
//         setStartSceneButton.style.marginTop = 5;
//         setStartSceneButton.style.marginBottom = 20;
//         root.Add(setStartSceneButton);
//
//         // Scene Management Section
//         Label sceneManagementLabel = new Label("Scene Management");
//         sceneManagementLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
//         sceneManagementLabel.style.marginBottom = 5;
//         root.Add(sceneManagementLabel);
//
//         Button playButton = new Button(() => PlayFromBootstrapper());
//         playButton.text = "Play from Start Scene";
//         playButton.style.marginBottom = 5;
//         root.Add(playButton);
//
//         Button loadLastSceneButton = new Button(() => ReturnToLastScene());
//         loadLastSceneButton.text = "Load Last Edited Scene";
//         root.Add(loadLastSceneButton);
//     }
//
//     private void SaveSelectedScene()
//     {
//         if (sceneAsset != null)
//         {
//             string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset));
//             EditorPrefs.SetString(SCENE_PREF_KEY, guid);
//             EditorSceneManager.playModeStartScene = sceneAsset;
//             Debug.Log($"Start scene set to: {sceneAsset.name}");
//         }
//         else
//         {
//             EditorPrefs.DeleteKey(SCENE_PREF_KEY);
//             EditorSceneManager.playModeStartScene = null;
//             Debug.LogWarning("Please select a scene first!");
//         }
//     }
//
//     private void PlayFromBootstrapper()
//     {
//         var currentScene = EditorSceneManager.GetActiveScene();
//         string currentScenePath = currentScene.path;
//         
//         if (!string.IsNullOrEmpty(currentScenePath))
//         {
//             EditorPrefs.SetString(LAST_SCENE_PREF_KEY, currentScenePath);
//             Debug.Log($"Saved last edited scene: {currentScene.name}");
//         }
//         
//         EditorApplication.isPlaying = true;
//     }
//
//     private void ReturnToLastScene()
//     {
//         string lastScenePath = EditorPrefs.GetString(LAST_SCENE_PREF_KEY, "");
//         
//         if (!string.IsNullOrEmpty(lastScenePath))
//         {
//             if (System.IO.File.Exists(lastScenePath))
//             {
//                 EditorSceneManager.OpenScene(lastScenePath);
//                 Debug.Log($"Loaded last edited scene: {System.IO.Path.GetFileNameWithoutExtension(lastScenePath)}");
//             }
//             else
//             {
//                 Debug.LogWarning($"Could not find scene: {lastScenePath}");
//             }
//         }
//         else
//         {
//             Debug.LogWarning("No last scene information found!");
//         }
//     }
//
//     [MenuItem("BearStick Studio/Utility Window")]
//     static void Open()
//     {
//         GetWindow<UtilityWindow>("BearStick Studio Utility");
//     }
//
//     [InitializeOnLoadMethod]
//     static void OnProjectLoadedInEditor()
//     {
//         EditorApplication.delayCall += () =>
//         {
//             GetWindow<UtilityWindow>("BearStick Studio Utility");
//         };
//     }
// }