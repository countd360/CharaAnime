using HarmonyLib;
using KKAPI.Studio;
using Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharaAnime
{
    class CharaAnimeMgr : MonoBehaviour
    {
        public MainGUI gui;
        public Dictionary<ObjectCtrlInfo, MmddPoseController> ociPoseCtrlDic;

        public static Harmony HarmonyInstance { get; set; }
        public static CharaAnimeMgr Instance { get; private set; }

        public static CharaAnimeMgr Install(GameObject container)
        {
            if (CharaAnimeMgr.Instance == null)
            {
                CharaAnimeMgr.Instance = container.AddComponent<CharaAnimeMgr>();
            }
            return CharaAnimeMgr.Instance;
        }

        public bool VisibleGUI
        {
            get => gui.VisibleGUI;
            set => gui.VisibleGUI = value;
        }

        private void Awake()
        {
            ociPoseCtrlDic = new Dictionary<ObjectCtrlInfo, MmddPoseController>();
        }

        private void Start()
        {
            StartCoroutine(LoadingCo());
        }

        //[Warning: Unity Log] OnLevelWasLoaded was found on ConsolePlugin
        //This message has been deprecated and will be removed in a later version of Unity.
        //Add a delegate to SceneManager.sceneLoaded instead to get notifications after scene loading has completed
        private IEnumerator LoadingCo()
        {
            yield return new WaitUntil(() => StudioAPI.StudioLoaded);
            // Wait until fully loaded
            yield return null;

            gui = new GameObject("GUI").AddComponent<MainGUI>();
            gui.transform.parent = base.transform;
            gui.VisibleGUI = false;
            System.Console.WriteLine("CharaAnime CharaAnimeMgr Started.");
        }

        public void RegistPoseController(ObjectCtrlInfo target, MmddPoseController poseCtrl)
        {
            ociPoseCtrlDic[target] = poseCtrl;
        }

        public void RemovePoseController(ObjectCtrlInfo target)
        {
            ociPoseCtrlDic.Remove(target);
        }

        public MmddPoseController GetPoseController(ObjectCtrlInfo target)
        {
            if (target == null) return null;

            if (ociPoseCtrlDic.ContainsKey(target))
            {
                return ociPoseCtrlDic[target];
            }
            else
            {
                return MmddPoseController.Install(target.guideObject.transformTarget.gameObject, target);
            }
        }
    }
}
