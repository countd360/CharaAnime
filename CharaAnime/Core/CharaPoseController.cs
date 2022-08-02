using BepInEx.Bootstrap;
using HarmonyLib;
using Studio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharaAnime
{
    public class CharaPoseController : MonoBehaviour
    {
        private ObjectCtrlInfo ociTarget;
        private CharaAnimeMgr manager;
        private bool enable;
        
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                if (Initialized)
                    enable = value;
                else
                    enable = false;
            }
        }
        public bool Initialized { get; private set; }
        public Dictionary<GameObject, Vector3> BonePosModifier;
        public Dictionary<GameObject, Quaternion> BoneRotModifier;
        public Dictionary<GameObject, Vector3> BoneSclModifier;
        public List<SyncSetting> BoneSync;

#if STUDIO_HS2 || STUDIO_AI
        // heelz 
        public enum HeelzPluginType
        {
            UNKNOWN = 0,
            HEELZ_14,
            HEELZ_15,
            UNSUPPORT,
            NOTFOUND,
        }

        public static bool HasHeelzPlugin { get; private set; }
        public static string HeelzPluginInfo { get; private set; }
        private static HeelzPluginType heelzPluginType = HeelzPluginType.UNKNOWN;
        public bool EnableHeelzUpdate { get; set; }
        public object heelsCtrl;

        public static void Foo()
        {
            Console.WriteLine("Foo10");
        }

        public static void DetectHeelzController()
        {
            // init only once
            if (heelzPluginType != HeelzPluginType.UNKNOWN)
            {
                return;
            }
            // search for 
            string heelzPluginName = null;
            string heelzPluginGuid = null;
            Version heelzPluginVer = null;
            Console.WriteLine("CharaPoseController search for heelz plugins...");
            foreach (var pname in Chainloader.PluginInfos.Keys)
            {
                if (pname.ToLower().Contains("heelz") || Chainloader.PluginInfos[pname].Metadata.Name.Contains("heelz"))
                {
                    var pinfo = Chainloader.PluginInfos[pname];
                    heelzPluginName = pinfo.Metadata.Name;
                    heelzPluginGuid = pinfo.Metadata.GUID;
                    heelzPluginVer = pinfo.Metadata.Version;
                    Console.WriteLine("  Found {0}, {1}, {2}!", heelzPluginName, heelzPluginGuid, heelzPluginVer);
                    break;
                }
            }
            if (string.IsNullOrEmpty(heelzPluginName) || heelzPluginVer == null)
            {
                Console.WriteLine("  No heelz plugin found!");
                HasHeelzPlugin = false;
                HeelzPluginInfo = "Heelz Plugin not found";
                heelzPluginType = HeelzPluginType.NOTFOUND;
                return;
            }
            else if (heelzPluginName.Equals("Heelz"))
            {
                if (heelzPluginVer.CompareTo(new Version(1, 14, 3)) == 0)
                {
                    PatchHeelz14();

                    HasHeelzPlugin = true;
                    HeelzPluginInfo = "Found supported plugin: Heelz 1.14.3 by Hooh";
                    heelzPluginType = HeelzPluginType.HEELZ_14;
                    return;
                }
            }
            else if (heelzPluginName.Equals("HS2Heelz"))
            {
                if (heelzPluginVer.CompareTo(new Version(1, 15, 3)) >= 0)
                {
                    HasHeelzPlugin = true;
                    HeelzPluginInfo = string.Format("Found supported plugin: Heelz {0} by Animal42069", heelzPluginVer.ToString());
                    heelzPluginType = HeelzPluginType.HEELZ_15;
                    return;
                }
            }
            // not supported
            HasHeelzPlugin = false;
            HeelzPluginInfo = string.Format("Found not-supported plugin: Name = {0}, GUID = {1}, Version = {2}", heelzPluginName, heelzPluginGuid, heelzPluginVer.ToString());
            heelzPluginType = HeelzPluginType.UNSUPPORT;
        }

        public static void PatchHeelz14()
        {
            try
            {
                // install patch
                Console.WriteLine("Patching Heelz 1.14.3 ...");
                CharaAnimeMgr.HarmonyInstance.PatchAll(typeof(Heelz14_Patch));
                Console.WriteLine("  ... Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail to patch Heelz: " + ex.Message);
            }

        }

        public void InitializeHeelzController()
        {
            DetectHeelzController();
            heelsCtrl = null;

            if (heelzPluginType == HeelzPluginType.HEELZ_14)
            {
                EnableHeelzUpdate = InitializeHeelzController14();
            }
            else if (heelzPluginType == HeelzPluginType.HEELZ_15)
            {
                EnableHeelzUpdate = false;
            }
            else
            {
                EnableHeelzUpdate = false;
            }
        }

        public bool InitializeHeelzController14()
        {
            try
            {
                Heelz14_Patch.caMgr = CharaAnimeMgr.Instance;
                heelsCtrl = (ociTarget as OCIChar).charInfo.gameObject.GetComponent<HeelsController>();
            }
            catch (Exception)
            {
                heelsCtrl = null;
            }
            return heelsCtrl != null;
        }

        public void HeelzUpdate()
        {
            if (heelzPluginType == HeelzPluginType.HEELZ_14)
            {
                HeelzUpdate14();
            }
            else if (heelzPluginType == HeelzPluginType.HEELZ_15)
            {

            }
            else
            {

            }
        }

        public void HeelzUpdate14()
        {
            if (EnableHeelzUpdate && heelsCtrl != null)
            {
                Heelz14_Patch.allowHeelzUpdate = true;
                (heelsCtrl as HeelsController)?.IKArray();
                Heelz14_Patch.allowHeelzUpdate = false;
            }
        }

        // heelz patch
        private class Heelz14_Patch
        {
            public static bool allowHeelzUpdate = false;
            public static CharaAnimeMgr caMgr;
 
            [HarmonyPatch(typeof(HeelsController), "IKArray")]
            [HarmonyPrefix]
            public static bool Prefix(HeelsController __instance)
            {
                foreach (CharaPoseController pc in caMgr.ociPoseCtrlDic.Values)
                {
                    if (pc.heelsCtrl != null && pc.heelsCtrl.Equals(__instance))
                    {
                        // this char has CharPoseController for it, depend on allowHeelzUpdate flag 
                        return allowHeelzUpdate && pc.EnableHeelzUpdate;
                    }
                }

                //Console.WriteLine("IKArray of HeelsController called");
                return true;
            }
        }

#endif

        public static CharaPoseController Install(GameObject container, ObjectCtrlInfo target)
        {
            CharaPoseController newCtrl = container.AddComponent<CharaPoseController>();
            newCtrl.ociTarget = target;
            newCtrl.manager = CharaAnimeMgr.Instance;
            newCtrl.manager.RegistPoseController(target, newCtrl);

#if STUDIO_HS2 || STUDIO_AI
            // init heelz controller
            newCtrl.InitializeHeelzController();
#endif
            return newCtrl;
        }

        public void AddBoneSyncSetting(Transform tgt, Transform src, int flag)
        {
            foreach (SyncSetting cs in BoneSync)
            {
                if (cs.TgtTransform == tgt && cs.SrcTransform == src)
                {
                    cs.SyncFlag |= flag;
                    return;
                }
            }

            SyncSetting newcs = new SyncSetting(tgt, src, flag);
            BoneSync.Add(newcs);
        }

        public void AddBoneSyncSetting(GameObject tgt, GameObject src, int flag)
        {
            AddBoneSyncSetting(tgt.transform, src.transform, flag);
        }

        public void RemoveBoneSyncSetting(Transform tgt, Transform src, int flag = SyncSetting.MSK_ALL)
        {
            flag &= SyncSetting.MSK_ALL;
            foreach (SyncSetting cs in BoneSync)
            {
                if (cs.TgtTransform == tgt && cs.SrcTransform == src)
                {
                    int newflag = cs.SyncFlag & (~flag);
                    if (newflag == 0)
                    {
                        BoneSync.Remove(cs);
                    }
                    else
                    {
                        cs.SyncFlag = newflag;
                    }
                    return;
                }
            }
        }

        public void RemoveBoneSyncSetting(GameObject tgt, GameObject src, int flag = SyncSetting.MSK_ALL)
        {
            RemoveBoneSyncSetting(tgt.transform, src.transform, flag);
        }

        public void RemoveBoneSyncSetting(Transform tgt)
        {
            for (int i = BoneSync.Count - 1; i >= 0; i --)
            {
                if (BoneSync[i].TgtTransform == tgt)
                {
                    BoneSync.RemoveAt(i);
                }
            }
        }

        public void RemoveBoneSyncSetting(GameObject tgt)
        {
            RemoveBoneSyncSetting(tgt.transform);
        }

        public void ClearBoneSyncSetting()
        {
            if (Initialized)
            {
                BoneSync.Clear();
            }
        }

        private void Awake()
        {
            Enable = false;
            Initialized = false;
            BonePosModifier = new Dictionary<GameObject, Vector3>();
            BoneRotModifier = new Dictionary<GameObject, Quaternion>();
            BoneSclModifier = new Dictionary<GameObject, Vector3>();
            BoneSync = new List<SyncSetting>();
        }

        private void Start()
        {
            Console.WriteLine("CharaPoseController for {0} started!", ociTarget.treeNodeObject.textName);
            Initialized = true;
        }

        private void LateUpdate()
        {
            DoBoneSync();

#if STUDIO_HS2 || STUDIO_AI
            // heelz update
            HeelzUpdate();
#endif
        }

        public void ObiUpdate()
        {
            DoBoneSync(true);

#if STUDIO_HS2 || STUDIO_AI
            // heelz update
            HeelzUpdate();
#endif
        }

        private void OnDestroy()
        {
            Console.WriteLine("CharaPoseController for {0} destroyed!", ociTarget.treeNodeObject.textName);
            Initialized = false;
            Enable = false;
            manager.RemovePoseController(ociTarget);
        }

        public Action PostBoneSync = null;
        public void DoBoneSync(bool fouceUpdate = false)
        {
            if (!Enable && !fouceUpdate)
            {
                return;
            }
            foreach (SyncSetting cs in BoneSync)
            {
                cs.DoSync();
            }
            foreach (KeyValuePair<GameObject, Vector3> posPair in BonePosModifier)
            {
                if (posPair.Key == null)
                {
                    Console.WriteLine("Null key in BonePosModifier of {0}", ociTarget.treeNodeObject.textName);
                    continue;
                }
                posPair.Key.transform.localPosition = posPair.Value;
            }
            foreach (KeyValuePair<GameObject, Quaternion> rotPair in BoneRotModifier)
            {
                if (rotPair.Key == null)
                {
                    Console.WriteLine("Null key in BoneRotModifier of {0}", ociTarget.treeNodeObject.textName);
                    continue;
                }
                rotPair.Key.transform.localRotation = rotPair.Value;
            }
            foreach (KeyValuePair<GameObject, Vector3> sclPair in BoneSclModifier)
            {
                if (sclPair.Key == null)
                {
                    Console.WriteLine("Null key in BoneSclModifier of {0}", ociTarget.treeNodeObject.textName);
                    continue;
                }
                sclPair.Key.transform.localScale = sclPair.Value;
            }
            // post process
            PostBoneSync?.Invoke();
            //Console.WriteLine("DoBoneSync {0}: Pos = {1}, Rot = {2}, Scl = {3}", ociTarget.treeNodeObject.textName, BonePosModifier.Count, BoneRotModifier.Count, BoneSclModifier.Count);
        }

        public class SyncSetting
        {
            public const int SYN_LOCAL_POS_X = 0x00000001;
            public const int SYN_LOCAL_POS_Y = 0x00000002;
            public const int SYN_LOCAL_POS_Z = 0x00000004;
            public const int SYN_LOCAL_POS = SYN_LOCAL_POS_X + SYN_LOCAL_POS_Y + SYN_LOCAL_POS_Z;
            public const int SYN_LOCAL_ROT_X = 0x00000010;
            public const int SYN_LOCAL_ROT_Y = 0x00000020;
            public const int SYN_LOCAL_ROT_Z = 0x00000040;
            public const int SYN_LOCAL_ROT = SYN_LOCAL_ROT_X + SYN_LOCAL_ROT_Y + SYN_LOCAL_ROT_Z;
            public const int SYN_LOCAL_SCL_X = 0x00000100;
            public const int SYN_LOCAL_SCL_Y = 0x00000200;
            public const int SYN_LOCAL_SCL_Z = 0x00000400;
            public const int SYN_LOCAL_SCL = SYN_LOCAL_SCL_X + SYN_LOCAL_SCL_Y + SYN_LOCAL_SCL_Z;

            public const int SYN_POS_X = 0x00010000;
            public const int SYN_POS_Y = 0x00020000;
            public const int SYN_POS_Z = 0x00040000;
            public const int SYN_POS = SYN_POS_X + SYN_POS_Y + SYN_POS_Z;
            public const int SYN_ROT_X = 0x00100000;
            public const int SYN_ROT_Y = 0x00200000;
            public const int SYN_ROT_Z = 0x00400000;
            public const int SYN_ROT = SYN_ROT_X + SYN_ROT_Y + SYN_ROT_Z;

            public const int MSK_ALL = SYN_LOCAL_POS + SYN_LOCAL_ROT + SYN_LOCAL_SCL + SYN_POS + SYN_ROT;

            public Transform SrcTransform;
            public Transform TgtTransform;
            public int SyncFlag;

            public SyncSetting(Transform tgt, Transform src, int flag)
            {
                SrcTransform = src;
                TgtTransform = tgt;
                SyncFlag = flag;
            }

            public SyncSetting(GameObject tgt, GameObject src, int flag)
            {
                SrcTransform = src.transform;
                TgtTransform = tgt.transform;
                SyncFlag = flag;
            }

            public void DoSync()
            {
                if (SrcTransform == null || TgtTransform == null || SrcTransform == TgtTransform || (SyncFlag & MSK_ALL) == 0) return;

                // position
                if ((SyncFlag & SYN_POS) == SYN_POS)
                {
                    TgtTransform.position = SrcTransform.position;
                }
                else if ((SyncFlag & SYN_POS) != 0)
                {
                    Vector3 pos = TgtTransform.position;
                    if ((SyncFlag & SYN_POS_X) == SYN_POS_X) pos.x = SrcTransform.position.x;
                    if ((SyncFlag & SYN_POS_Y) == SYN_POS_Y) pos.y = SrcTransform.position.y;
                    if ((SyncFlag & SYN_POS_Z) == SYN_POS_Z) pos.z = SrcTransform.position.z;
                    TgtTransform.position = pos;
                }
                else if ((SyncFlag & SYN_LOCAL_POS) == SYN_LOCAL_POS)
                {
                    TgtTransform.localPosition = SrcTransform.localPosition;
                }
                else if ((SyncFlag & SYN_LOCAL_POS) != 0)
                {
                    Vector3 pos = TgtTransform.localPosition;
                    if ((SyncFlag & SYN_LOCAL_POS_X) == SYN_LOCAL_POS_X) pos.x = SrcTransform.localPosition.x;
                    if ((SyncFlag & SYN_LOCAL_POS_Y) == SYN_LOCAL_POS_Y) pos.y = SrcTransform.localPosition.y;
                    if ((SyncFlag & SYN_LOCAL_POS_Z) == SYN_LOCAL_POS_Z) pos.z = SrcTransform.localPosition.z;
                    TgtTransform.localPosition = pos;
                }

                // rotation
                if ((SyncFlag & SYN_ROT) == SYN_ROT)
                {
                    TgtTransform.rotation = SrcTransform.rotation;
                }
                else if ((SyncFlag & SYN_ROT) != 0)
                {
                    Vector3 rot = TgtTransform.eulerAngles;
                    if ((SyncFlag & SYN_ROT_X) == SYN_ROT_X) rot.x = SrcTransform.eulerAngles.x;
                    if ((SyncFlag & SYN_ROT_Y) == SYN_ROT_Y) rot.y = SrcTransform.eulerAngles.y;
                    if ((SyncFlag & SYN_ROT_Z) == SYN_ROT_Z) rot.z = SrcTransform.eulerAngles.z;
                    TgtTransform.rotation = Quaternion.Euler(rot);
                }
                else if ((SyncFlag & SYN_LOCAL_ROT) == SYN_LOCAL_ROT)
                {
                    TgtTransform.localRotation = SrcTransform.localRotation;
                }
                else if ((SyncFlag & SYN_LOCAL_ROT) != 0)
                {
                    Vector3 rot = TgtTransform.localEulerAngles;
                    if ((SyncFlag & SYN_LOCAL_ROT_X) == SYN_LOCAL_ROT_X) rot.x = SrcTransform.eulerAngles.x;
                    if ((SyncFlag & SYN_LOCAL_ROT_Y) == SYN_LOCAL_ROT_Y) rot.y = SrcTransform.eulerAngles.y;
                    if ((SyncFlag & SYN_LOCAL_ROT_Z) == SYN_LOCAL_ROT_Z) rot.z = SrcTransform.eulerAngles.z;
                    TgtTransform.localRotation = Quaternion.Euler(rot);
                }

                // scale
                if ((SyncFlag & SYN_LOCAL_SCL) == SYN_LOCAL_SCL)
                {
                    TgtTransform.localScale = SrcTransform.localScale;
                }
                else if ((SyncFlag & SYN_LOCAL_SCL) == SYN_LOCAL_SCL)
                {
                    Vector3 scl = TgtTransform.localScale;
                    if ((SyncFlag & SYN_LOCAL_SCL_X) == SYN_LOCAL_SCL_X) scl.x = SrcTransform.localScale.x;
                    if ((SyncFlag & SYN_LOCAL_SCL_Y) == SYN_LOCAL_SCL_Y) scl.y = SrcTransform.localScale.y;
                    if ((SyncFlag & SYN_LOCAL_SCL_Z) == SYN_LOCAL_SCL_Z) scl.z = SrcTransform.localScale.z;
                    TgtTransform.localScale = scl;
                }
            }
        }

#if false
        [HarmonyPatch(typeof(IKExecutionOrder), "LateUpdate")]
        private class IKExecutionOrder_Patches
        {
            public static event Action onPostLateUpdate;

            //public static void Prefix()
            public static void Postfix()
            {
                if (CharaPoseController.IKExecutionOrder_Patches.onPostLateUpdate != null)
                {
                    CharaPoseController.IKExecutionOrder_Patches.onPostLateUpdate();
                }
            }
        }
#endif
    }


}
