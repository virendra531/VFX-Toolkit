// DFVolume - Distance field volume generator for Unity
// https://github.com/keijiro/DFVolume

using UnityEngine;

namespace DFVolume
{
    public class VolumeSampler : MonoBehaviour
    {
        #region Exposed attributes

        // [SerializeField, HideInInspector] bool _distanceGradient = false;
        // public bool distanceGradient{
        //     get { return _distanceGradient;}
        // }
        [SerializeField] int _resolution = 50;

        public int resolution {
            get { return _resolution; }
        }

        [SerializeField] float _extent = 0.5f;

        public float extent {
            get { return _extent; }
        }

        #endregion

        #if UNITY_EDITOR

        #region Editor functions

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * _extent * 2);
        }

        public Color[] GenerateBitmap(bool distanceGradient = false)
        {
            // Generate a distance field.
            var df = new float[_resolution * _resolution * _resolution];

            for (var xi = 0; xi < _resolution; xi++)
            {
                var x = 2.0f * xi / (_resolution - 1) - 1;
                for (var yi = 0; yi < _resolution; yi++)
                {
                    var y = 2.0f * yi / (_resolution - 1) - 1;
                    for (var zi = 0; zi < _resolution; zi++)
                    {
                        var z = 2.0f * zi / (_resolution - 1) - 1;

                        var pt = new Vector3(x, y, z) * _extent;
                        pt = transform.TransformPoint(pt);

                        var dist = SearchDistance(pt) * 0.5f / _extent;
                        df[GetIndex(xi, yi, zi)] = dist;
                    }
                }
            }

            // Compute gradients and pack them into a bitmap.
            var bmp = new Color[df.Length];
            var dds2 = (_resolution - 1) / 2.0f;

            for (var xi = 0; xi < _resolution; xi++)
            {
                for (var yi = 0; yi < _resolution; yi++)
                {
                    for (var zi = 0; zi < _resolution; zi++)
                    {
                        var d = df[GetIndex(xi, yi, zi)];
                        var dx0 = df[GetIndex(xi - 1, yi, zi)];
                        var dx1 = df[GetIndex(xi + 1, yi, zi)];
                        var dy0 = df[GetIndex(xi, yi - 1, zi)];
                        var dy1 = df[GetIndex(xi, yi + 1, zi)];
                        var dz0 = df[GetIndex(xi, yi, zi - 1)];
                        var dz1 = df[GetIndex(xi, yi, zi + 1)];

                        if(d < 0) Debug.Log("D is Negative");
                        
                        // Debug.Log(xi + " " + yi + " " + zi);
                        Vector3 distanceV = new Vector3(xi,yi,zi);

                        // if( xi < 40f && yi < 40f && zi < 40f )  
                        // if( (xi > 40f || xi > 10f) || (yi > 40f || yi > 10f) )  
                        // if( (xi > 48f || xi < 2f) || (yi > 48f || yi < 2f) || (zi > 48f || zi < 2f) )
                        // {
                        //     // Debug.Log("In IF");
                        //     bmp[GetIndex(xi, yi, zi)] = new Color(0,0,0,0);
                        // }
                        // else
                        // {
                            Color newColor;
                            if(distanceGradient) // For Distance Gradient
                                newColor = new Color(
                                                        (dx1 - dx0) * dds2,
                                                        (dy1 - dy0) * dds2,
                                                        (dz1 - dz0) * dds2,
                                                        d
                                                    );
                            else // For SDF (Unsigned)
                                newColor = new Color(d,0,0,1);

                            bmp[GetIndex(xi, yi, zi)] = newColor;
                        // }
                        
                    }
                }
            }

            return bmp;
        }

        #endregion

        #region Private functions

        int GetIndex(int xi, int yi, int zi)
        {
            xi = Mathf.Clamp(xi, 0, _resolution - 1);
            yi = Mathf.Clamp(yi, 0, _resolution - 1);
            zi = Mathf.Clamp(zi, 0, _resolution - 1);
            return xi + _resolution * (yi + _resolution * zi);
        }

        float SearchDistance(Vector3 pt)
        {
            var r = _extent;
            var s = _extent * 0.5f;

            for (var i = 0; i < 10; i++)
            {
                r += (Physics.CheckSphere(pt, r) ? -1 : 1) * s;
                s *= 0.5f;
            }

            return r;
        }

        #endregion

        #endif
    }
}
