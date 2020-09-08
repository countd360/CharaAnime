using Studio;
using System;
using UnityEngine;


namespace CharaAnime
{
    class MainGUI : MonoBehaviour
    {
        private readonly int windowID = 10023;
        private readonly string windowTitle = "Chara Anime Plugin";
        private Rect windowRect = new Rect(0f, 300f, 630f, 500f);
        private bool mouseInWindow = false;

        private TreeNodeObject lastSelectedTreeNode;
        private ObjectCtrlInfo target;
        
        public GizmosManager GizmosMgr { get; private set; }


        public bool VisibleGUI
        {
            get;
            set;
        }

        private void Start()
        {
            GizmosMgr = this.gameObject.AddComponent<GizmosManager>();
            Console.WriteLine("CharaAnime MainGUI started.");
        }

        private void OnGUI()
        {
            if (VisibleGUI)
            {
                try
                {
                    GUIStyle guistyle = new GUIStyle(GUI.skin.window);
                    windowRect = GUI.Window(windowID, windowRect, new GUI.WindowFunction(FuncWindowGUI), windowTitle, guistyle);

                    mouseInWindow = windowRect.Contains(Event.current.mousePosition);
                    if (mouseInWindow)
                    {
                        Studio.Studio.Instance.cameraCtrl.noCtrlCondition = (() => mouseInWindow && VisibleGUI);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void Update()
        {
            // hotkey check
            if (CharaAnime.KeyShowUI.Value.IsDown())
            {
                VisibleGUI = !VisibleGUI;
                if (VisibleGUI)
                {
                    windowRect = new Rect(CharaAnime.UIXPosition.Value, CharaAnime.UIYPosition.Value, CharaAnime.UIWidth.Value, CharaAnime.UIHeight.Value);
                }
                else
                {
                    CharaAnime.UIXPosition.Value = (int)windowRect.x;
                    CharaAnime.UIYPosition.Value = (int)windowRect.y;
                }
            }

            // change select check
            if (VisibleGUI)
            {
                TreeNodeObject curSel = GetCurrentSelectedNode();
                if (curSel != lastSelectedTreeNode)
                {
                    OnSelectChange(curSel);
                }
            }
        }

        private void FuncWindowGUI(int winID)
        {
            
            try
            {
                if (GUIUtility.hotControl == 0)
                {
                    
                }
                if (Event.current.type == EventType.MouseDown)
                {
                    GUI.FocusControl("");
                    GUI.FocusWindow(winID);
                    
                }
                
                GUI.enabled = true;
                
                GUILayout.BeginVertical();
                CharaPoseController cpc = CharaAnimeMgr.Instance.GetPoseController(target);
                if (target == null || cpc == null)
                {
                    if (lastSelectedTreeNode != null || target != null) OnSelectChange(null);
                    GUILayout.Label("No charactor selected!");
                }
                else
                {
                    if (GUILayout.Button("Hello"))
                    {
                        Console.WriteLine("Hello, {0}.", target.treeNodeObject.textName);
                    }
                    cpc.Enable = GUILayout.Toggle(cpc.Enable, "Enable");
                }
                GUILayout.EndVertical();
                GUI.DragWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("target = {0}", target);
                target = null;
            }
            finally
            {
                
            }
        }

        private void OnSelectChange(TreeNodeObject newSel)
        {
            lastSelectedTreeNode = newSel;
            target = GetOCIfromNode(newSel);

            if (target != null)
            {
                CharaPoseController cpc = target.guideObject.transformTarget.gameObject.GetComponent<CharaPoseController>();
                if (cpc == null)
                {
                    cpc = CharaPoseController.Install(target.guideObject.transformTarget.gameObject, target);
                }
            }

            Console.WriteLine("Select change to {0}", target);
        }

        protected TreeNodeObject GetCurrentSelectedNode()
        {
            return Studio.Studio.Instance.treeNodeCtrl.selectNode;
        }

        protected ObjectCtrlInfo GetOCIfromNode(TreeNodeObject node)
        {
            if (node == null) return null;

            var dic = Studio.Studio.Instance.dicInfo;
            if (dic.ContainsKey(node))
            {
                return dic[node];
            }
            else
            {
                return null;
            }
        }


    }
}
