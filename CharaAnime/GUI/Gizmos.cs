using System;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace CharaAnime
{
    class GizmosManager : MonoBehaviour
    {
        static public GizmosManager Instance { get; private set; }
        public List<Gizmos> GizmosList = new List<Gizmos>();
        public CameraEventsDispatcher _cameraEventsDispatcher;

        private void Start()
        {
            Instance = this;
            _cameraEventsDispatcher = Camera.main.gameObject.AddComponent<CameraEventsDispatcher>();
            _cameraEventsDispatcher.onPreRender += UpdateAllGizmos;
            Console.WriteLine("CharaAnime GizmosManager started.");
#if STUDIO_KKS
            VectorLine.SetEndCap("vector", EndCap.Back, 0, new Texture2D[] { Texture2D.whiteTexture, Texture2D.whiteTexture});
#endif
        }

        private void UpdateAllGizmos()
        {
            try
            {
                foreach (Gizmos g in GizmosList)
                {
                    if (g.GizmosEnabled())
                    {
                        g.UpdateGizmos();
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // pass
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ShowGizmos(Gizmos gizmos)
        {
            gizmos.SetEnable(true);
            if (!GizmosList.Contains(gizmos))
            {
                GizmosList.Add(gizmos);
            }
        }

        public void HideGizmos(Gizmos gizmos)
        {
            gizmos.SetEnable(false);
        }

        public void RemoveGizmos(Gizmos gizmos)
        {
            gizmos.SetEnable(false);
            if (GizmosList.Contains(gizmos))
            {
                GizmosList.Remove(gizmos);
            }
        }

        public void ClearGizmos()
        {
            foreach (Gizmos g in GizmosList)
            {
                g.SetEnable(false);
            }
            GizmosList.Clear();
        }
    }

    public class CameraEventsDispatcher : MonoBehaviour
    {
        public event Action onPreRender;

        private void OnPreRender()
        {
            if (this.onPreRender != null)
            {
                this.onPreRender();
            }
        }
    }

    abstract class Gizmos
    {
        abstract public bool GizmosEnabled();
        abstract public void SetEnable(bool enable);
        abstract public void UpdateGizmos();
        ~Gizmos()
        {
            GizmosManager.Instance.RemoveGizmos(this);
        }
    }

    class CubeGizmos : Gizmos
    {
        private readonly List<VectorLine> _cubeDebugLines = null;
        private Transform _boneTarget = null;
        private bool _enabled = false;
        private float _cubeSize = 0.12f;


        public CubeGizmos(Transform boneTarget = null, float cubeSize = 0.12f)
        {
            _boneTarget = boneTarget;
            _cubeSize = cubeSize;

            _cubeDebugLines = new List<VectorLine>();
            Vector3 vector = (Vector3.up + Vector3.left + Vector3.forward) * _cubeSize;
            Vector3 vector2 = (Vector3.up + Vector3.right + Vector3.forward) * _cubeSize;
            Vector3 vector3 = (Vector3.down + Vector3.left + Vector3.forward) * _cubeSize;
            Vector3 vector4 = (Vector3.down + Vector3.right + Vector3.forward) * _cubeSize;
            Vector3 vector5 = (Vector3.up + Vector3.left + Vector3.back) * _cubeSize;
            Vector3 vector6 = (Vector3.up + Vector3.right + Vector3.back) * _cubeSize;
            Vector3 vector7 = (Vector3.down + Vector3.left + Vector3.back) * _cubeSize;
            Vector3 vector8 = (Vector3.down + Vector3.right + Vector3.back) * _cubeSize;

            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector,
                vector2
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector2,
                vector4
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector4,
                vector3
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector3,
                vector
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector5,
                vector6
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector6,
                vector8
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector8,
                vector7
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector7,
                vector5
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector5,
                vector
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector6,
                vector2
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector8,
                vector4
            }));
            _cubeDebugLines.Add(VectorLine.SetLine(Color.white, new Vector3[]
            {
                vector7,
                vector3
            }));
            VectorLine vectorLine = VectorLine.SetLine(Color.red, new Vector3[]
            {
                Vector3.zero,
                Vector3.right * _cubeSize * 2f
            });
            vectorLine.endCap = "vector";
            _cubeDebugLines.Add(vectorLine);
            vectorLine = VectorLine.SetLine(Color.green, new Vector3[]
            {
                Vector3.zero,
                Vector3.up * _cubeSize * 2f
            });
            vectorLine.endCap = "vector";
            _cubeDebugLines.Add(vectorLine);
            vectorLine = VectorLine.SetLine(Color.blue, new Vector3[]
            {
                Vector3.zero,
                Vector3.forward * _cubeSize * 2f
            });
            vectorLine.endCap = "vector";
            _cubeDebugLines.Add(vectorLine);
            foreach (VectorLine vectorLine2 in _cubeDebugLines)
            {
                vectorLine2.lineWidth = 2f;
                vectorLine2.active = false;
            }
            
        }

        public override void UpdateGizmos()
        {
            Vector3 vector = _boneTarget.transform.position + (_boneTarget.up + -_boneTarget.right + _boneTarget.forward) * _cubeSize;
            Vector3 vector2 = _boneTarget.transform.position + (_boneTarget.up + _boneTarget.right + _boneTarget.forward) * _cubeSize;
            Vector3 vector3 = _boneTarget.transform.position + (-_boneTarget.up + -_boneTarget.right + _boneTarget.forward) * _cubeSize;
            Vector3 vector4 = _boneTarget.transform.position + (-_boneTarget.up + _boneTarget.right + _boneTarget.forward) * _cubeSize;
            Vector3 vector5 = _boneTarget.transform.position + (_boneTarget.up + -_boneTarget.right + -_boneTarget.forward) * _cubeSize;
            Vector3 vector6 = _boneTarget.transform.position + (_boneTarget.up + _boneTarget.right + -_boneTarget.forward) * _cubeSize;
            Vector3 vector7 = _boneTarget.transform.position + (-_boneTarget.up + -_boneTarget.right + -_boneTarget.forward) * _cubeSize;
            Vector3 vector8 = _boneTarget.transform.position + (-_boneTarget.up + _boneTarget.right + -_boneTarget.forward) * _cubeSize;
            int num = 0;
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector,
                vector2
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector2,
                vector4
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector4,
                vector3
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector3,
                vector
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector5,
                vector6
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector6,
                vector8
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector8,
                vector7
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector7,
                vector5
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector5,
                vector
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector6,
                vector2
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector8,
                vector4
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                vector7,
                vector3
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                this._boneTarget.transform.position,
                this._boneTarget.transform.position + this._boneTarget.right * _cubeSize * 2f
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                this._boneTarget.transform.position,
                this._boneTarget.transform.position + this._boneTarget.up * _cubeSize * 2f
            });
            _cubeDebugLines[num++].SetPoints(new Vector3[]
            {
                this._boneTarget.transform.position,
                this._boneTarget.transform.position + this._boneTarget.forward * _cubeSize * 2f
            });

            foreach (VectorLine vectorLine in _cubeDebugLines)
            {
                vectorLine.Draw();
            }
        }

        public override bool GizmosEnabled()
        {
            return _enabled && _boneTarget != null;
        }

        public override void SetEnable(bool enable)
        {
            if (_boneTarget != null)
            {
                _enabled = enable;
            }
            else
            {
                _enabled = false;
            }
            UpdateDebugLinesState();
        }

        private void UpdateDebugLinesState()
        {
            bool active = _enabled && _boneTarget != null;
            foreach (VectorLine vectorLine in _cubeDebugLines)
            {
                vectorLine.active = active;
            }
        }

    }

    class BoneGizmos : Gizmos
    {
        private VectorLine _boneLine = null;
        private VectorLine _circleLine = null;
        private Transform _boneBase = null;
        private Transform _boneParent = null;
        private bool _enabled = false;

        public float Size { get; set; } = 0.12f;
        public Color Color { get; set; } = Color.cyan;

        public BoneGizmos(Transform bone, Transform parent, int circleEdge)
        {
            _boneBase = bone;
            _boneParent = parent;

            Vector3 bonePos = bone.position;
            _circleLine = VectorLine.SetLine(Color, new Vector3[circleEdge+1]);
            _circleLine.lineWidth = 2f;
            _circleLine.MakeCircle(bonePos, Camera.main.transform.forward, Size);

            if (_boneParent != null)
            {
                Vector3 parentPos = parent.position;

                _boneLine = VectorLine.SetLine(Color, new Vector3[]
                {
                    parentPos,
                    bonePos
                });
                //_boneLine.endCap = "vector";
                _boneLine.lineWidth = 2f;
            }
            else
            {
                _boneLine = null;
            }
        }

        public override bool GizmosEnabled()
        {
            return _enabled && _boneBase != null;
        }

        public override void SetEnable(bool enable)
        {
            if (_boneBase != null)
            {
                _enabled = enable;
            }
            else
            {
                _enabled = false;
            }
            UpdateDebugLinesState();
        }

        public override void UpdateGizmos()
        {
            Vector3 bonePos = _boneBase.position;
            _circleLine.SetColor(Color);
            _circleLine.MakeCircle(bonePos, Camera.main.transform.forward, Size);
            _circleLine.Draw();
            

            if (_boneParent != null)
            {
                Vector3 parentPos = _boneParent.position;

                _boneLine.SetColor(Color);
                _boneLine.SetPoints(new Vector3[]
                {
                    parentPos,
                    bonePos
                });
                _boneLine.Draw();
            }
        }

        private void UpdateDebugLinesState()
        {
            bool active = _enabled && _boneBase != null;
            _circleLine.active = active;
            if (_boneLine != null) _boneLine.active = active;
        }
    }

    public static class GizmosExtensions
    {
        public static void SetPoints(this VectorLine self, params Vector3[] points)
        {
            for (int i = 0; i < self.points3.Count; i++)
            {
                self.points3[i] = points[i];
            }
        }

        public static void SetPoints(this VectorLine self, params Vector2[] points)
        {
            for (int i = 0; i < self.points3.Count; i++)
            {
                self.points2[i] = points[i];
            }
        }
    }
}
