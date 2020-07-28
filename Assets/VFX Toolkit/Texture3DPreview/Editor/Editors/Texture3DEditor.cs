using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a custom Inspector for Texture3D assets
/// Actually, we will draw the default inspector but use the ability to draw a custom preview and render a custom asset's thumbnail
/// </summary>
[CustomEditor(typeof(Texture3D))]
public class Texture3DEditor : Editor
{
    #region Members
    /// <summary>
    /// The angle of the camera preview
    /// </summary>
    private Vector2 _cameraAngle = new Vector2(127.5f, -22.5f); // This default value will be used when rendering the asset thumbnail (see RenderStaticPreview)
    /// <summary>
    /// The raymarch interations
    /// </summary>
    private int _samplingIterations = 64;
    /// <summary>
    /// The factor of the Texture3D
    /// </summary>
    private float _density = 1;

    //// TODO : Investigate to access those variables as the default inspector is ugly
    //private SerializedProperty wrapModeProperty;
    //private SerializedProperty filterModeProperty;
    //private SerializedProperty anisotropyLevelProperty;
    #endregion

    #region Functions
    /// <summary>
    /// Sets back the camera angle
    /// </summary>
    public void ResetPreviewCameraAngle()
    {
        _cameraAngle = new Vector2(127.5f, -22.5f);
    }
    #endregion

    // Plain Texture
    protected SerializedProperty m_WrapU;
    protected SerializedProperty m_WrapV;
    protected SerializedProperty m_WrapW;   
    protected SerializedProperty m_FilterMode;
    protected SerializedProperty m_Aniso;
    public readonly string wrapModeText = "Wrap Mode";
    public readonly string wrapUText = "U axis";
    public readonly string wrapVText = "V axis";
    public readonly string wrapWText = "W axis";
    public readonly string filterModeText = "Filter Mode";
    public readonly string anisoLevelText = "Aniso Level";

    public readonly string[] wrapModeContents = { "Repeat","Clamp","Mirror","Mirror Once","Per-axis"};
    public readonly string[] wrapModeValues = { "Repeat","Clamp","Mirror","Mirror Once"};
    public readonly string[] filterModeValues = { "Point","Bilinear","Trilinear"};
    public void InitializeVariable()
    {
        if(m_WrapU != null) return;

        m_WrapU = serializedObject.FindProperty("m_TextureSettings.m_WrapU");
        m_WrapV = serializedObject.FindProperty("m_TextureSettings.m_WrapV");
        m_WrapW = serializedObject.FindProperty("m_TextureSettings.m_WrapW");
        m_FilterMode = serializedObject.FindProperty("m_TextureSettings.m_FilterMode");
        m_Aniso = serializedObject.FindProperty("m_TextureSettings.m_Aniso");
    }


    #region Overrides base class functions (https://docs.unity3d.com/ScriptReference/Editor.html)
    /// <summary>
    /// Draws the content of the Inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //// Had to disable the default Inspector as it makes preview lag
        // DrawDefaultInspector();

        InitializeVariable();
        DoWrapModePopup();
        DoFilterModePopup();
        DoAnisoLevelSlider();

        serializedObject.ApplyModifiedProperties();
    }

    bool enableWrapModePerAxis = false;
    private void DoWrapModePopup()
    {
        int newIndex = EditorGUILayout.Popup(wrapModeText, FindIndex(), wrapModeContents, EditorStyles.popup);
        enableWrapModePerAxis = newIndex == 4 ? true : false;

        if(newIndex == 4 || enableWrapModePerAxis)
        {
            m_WrapU.intValue = EditorGUILayout.Popup(wrapUText, m_WrapU.intValue, wrapModeValues, EditorStyles.popup);
            m_WrapV.intValue = EditorGUILayout.Popup(wrapVText, m_WrapV.intValue, wrapModeValues, EditorStyles.popup);
            m_WrapW.intValue = EditorGUILayout.Popup(wrapWText, m_WrapW.intValue, wrapModeValues, EditorStyles.popup);
        }else{
            m_WrapU.intValue = newIndex;
            m_WrapV.intValue = newIndex;
            m_WrapW.intValue = newIndex;
        }
    }
    private void DoFilterModePopup()
    {
        m_FilterMode.intValue = EditorGUILayout.Popup(filterModeText, m_FilterMode.intValue, filterModeValues, EditorStyles.popup);
    }

    private void DoAnisoLevelSlider()
    {
        EditorGUILayout.IntSlider (m_Aniso, 0, 16, anisoLevelText);
    }

    private int FindIndex()
    {
        if(m_WrapU.intValue == m_WrapV.intValue && m_WrapU.intValue == m_WrapW.intValue && !enableWrapModePerAxis)
            return m_WrapU.intValue;
        else
            return 4;
    }

    #region Preview
    /// <summary>
    /// Tells if the Object has a custom preview
    /// </summary>
    public override bool HasPreviewGUI()
    {
        return true;
    }

    /// <summary>
    /// Draws the toolbar area on top of the preview window
    /// </summary>
    public override void OnPreviewSettings()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Camera", EditorStyles.miniButton))
        {
            ResetPreviewCameraAngle();
        }
        EditorGUILayout.LabelField("Quality", GUILayout.MaxWidth(50));
        _samplingIterations = EditorGUILayout.IntPopup(_samplingIterations, new string[] { "16", "32", "64", "128", "256", "512" }, new int[] { 16, 32, 64, 128, 256, 512 }, GUILayout.MaxWidth(50));
        EditorGUILayout.LabelField("Density", GUILayout.MaxWidth(50));
        _density = EditorGUILayout.Slider(_density, 0, 5, GUILayout.MaxWidth(200));
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the preview area
    /// </summary>
    /// <param name="rect">The area of the preview window</param>
    /// <param name="backgroundStyle">The default GUIStyle used for preview windows</param>
    public override void OnPreviewGUI(Rect rect, GUIStyle backgroundStyle)
    {
        _cameraAngle = PreviewRenderUtilityHelpers.DragToAngles(_cameraAngle, rect);

        if (Event.current.type == EventType.Repaint)
        {
            GUI.DrawTexture(rect, ((Texture3D)serializedObject.targetObject).RenderTexture3DPreview(rect, EditorStyles.helpBox, _cameraAngle, 6.5f /*TODO : Find distance with fov and boundingsphere, when non uniform size will be supported*/, _samplingIterations, _density), ScaleMode.StretchToFill, true);
        }
    }

    /// <summary>
    /// Draws the custom preview thumbnail for the asset in the Project window
    /// </summary>
    /// <param name="assetPath">Path of the asset</param>
    /// <param name="subAssets">Array of children assets</param>
    /// <param name="width">Width of the rendered thumbnail</param>
    /// <param name="height">Height of the rendered thumbnail</param>
    /// <returns></returns>
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        return ((Texture3D)serializedObject.targetObject).RenderTexture3DStaticPreview(new Rect(0, 0, width, height), _cameraAngle, 6.5f /*TODO : Find distance with fov and boundingsphere, when non uniform size will be supported*/, _samplingIterations, _density);
    }

    /// <summary>
    /// Allows to give a custom title to the preview window
    /// </summary>
    /// <returns></returns>
    public override GUIContent GetPreviewTitle()
    {
        return new GUIContent(serializedObject.targetObject.name + " preview");
    }
    #endregion
    #endregion
}
