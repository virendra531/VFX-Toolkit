// DFVolume - Distance field volume generator for Unity
// https://github.com/keijiro/DFVolume

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DFVolume
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VolumeSampler))]
    class VolumeSamplerEditor : Editor
    {
        SerializedProperty _resolution;
        SerializedProperty _extent;
        SerializedProperty _distanceGradient;

        void OnEnable()
        {
            _resolution = serializedObject.FindProperty("_resolution");
            _extent = serializedObject.FindProperty("_extent");
            // _distanceGradient = serializedObject.FindProperty("_distanceGradient");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_resolution);
            EditorGUILayout.PropertyField(_extent);
            // EditorGUILayout.PropertyField(_distanceGradient);


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create SDF (Unsigned)")){
                // _distanceGradient.boolValue = false;
                CreateVolumeData(false);
            }
            if (GUILayout.Button("Create Distance Gradient")){
                // _distanceGradient.boolValue = true;
                CreateVolumeData(true);
            }
            GUILayout.EndHorizontal();
                
            serializedObject.ApplyModifiedProperties();

            CheckSkewedTransform();
        }

        void CreateVolumeData(bool distanceGradient = false)
        {
            var output = new List<Object>();

            foreach (VolumeSampler sampler in targets)
            {
                string distanceGradientText = (distanceGradient) ? "Distance Gradient" : "SDF (Unsigned)";
                var path = $"Assets/{sampler.gameObject.name} {distanceGradientText} Data.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                var asset = ScriptableObject.CreateInstance<VolumeData>();
                asset.Initialize(sampler,distanceGradient);

                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.AddObjectToAsset(asset.texture, asset);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.objects = output.ToArray();
        }

        void CheckSkewedTransform()
        {
            if (targets.Any(o => ((Component)o).transform.lossyScale != Vector3.one))
                EditorGUILayout.HelpBox(
                    "Using scale in transform may introduce error in output volumes.",
                    MessageType.Warning
                );
        }
    }
}
