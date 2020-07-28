// DFVolume - Distance field volume generator for Unity
// https://github.com/keijiro/DFVolume

using UnityEngine;
using UnityEditor;

namespace DFVolume
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VolumeVisualizer))]
    class VolumeVisualizerEditor : Editor
    {
        SerializedProperty _data;
        SerializedProperty _mode;
        SerializedProperty _depth;
        // SerializedProperty _shader;
        // SerializedProperty _material;

        void OnEnable()
        {
            _data = serializedObject.FindProperty("_data");
            _mode = serializedObject.FindProperty("_mode");
            _depth = serializedObject.FindProperty("_depth");
            // _shader = serializedObject.FindProperty("_shader");
            // _material = serializedObject.FindProperty("_material");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_data);
            EditorGUILayout.PropertyField(_mode);
            EditorGUILayout.PropertyField(_depth);
            // EditorGUILayout.PropertyField(_shader);
            // EditorGUILayout.PropertyField(_material);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
