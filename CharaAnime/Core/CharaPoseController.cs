using HarmonyLib;
using RootMotion.FinalIK;
using Studio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharaAnime
{
    class CharaPoseController : MonoBehaviour
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

        public static CharaPoseController Install(GameObject container, ObjectCtrlInfo target)
        {
            CharaPoseController newCtrl = container.AddComponent<CharaPoseController>();
            newCtrl.ociTarget = target;
            newCtrl.manager = CharaAnimeMgr.Instance;
            newCtrl.manager.RegistPoseController(target, newCtrl);
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
            //IKExecutionOrder_Patches.onPostLateUpdate += DoBoneSync;
        }

        private void Start()
        {
            Console.WriteLine("CharaPoseController for {0} started!", ociTarget.treeNodeObject.textName);
            Initialized = true;
        }

        private void LateUpdate()
        {
            DoBoneSync();
        }

        private void OnDestroy()
        {
            Console.WriteLine("CharaPoseController for {0} destroyed!", ociTarget.treeNodeObject.textName);
            Initialized = false;
            Enable = false;
            manager.RemovePoseController(ociTarget);
            //IKExecutionOrder_Patches.onPostLateUpdate -= DoBoneSync;
        }

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
            //Console.WriteLine("DoBoneSync {0}: Pos = {1}, Rot = {2}, Scl = {3}", ociTarget.treeNodeObject.textName, BonePosModifier.Count, BoneRotModifier.Count, BoneSclModifier.Count);
        }



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
#region GC_Suspend_Patch

        public static bool SuspendGC { get; set; }

        private static bool JudgeGCRequest()
        {
            return true;
        }

        [HarmonyPatch(typeof(GC), "InternalCollect")]
        private static class GCCollect_patch0
        {
            public static bool Prefix()
            {
                CharaAnime.Logger.LogDebug($"GC.InternalCollect() triggered, suspend request = {SuspendGC}");
                return JudgeGCRequest();
            }
        }

        [HarmonyPatch(typeof(GC), nameof(GC.Collect), new Type[0])]
        private static class GCCollect_patch1
        {
            public static bool Prefix()
            {
                CharaAnime.Logger.LogDebug($"GC.Collect() triggered, suspend request = {SuspendGC}");
                return JudgeGCRequest();
            }
        }

        [HarmonyPatch(typeof(GC), nameof(GC.Collect), new Type[] { typeof(Int32) })]
        private static class GCCollect_patch2
        {
            public static bool Prefix()
            {
                CharaAnime.Logger.LogDebug($"GC.Collect(Int32) triggered, suspend request = {SuspendGC}");
                return JudgeGCRequest();
            }
        }

        [HarmonyPatch(typeof(GC), nameof(GC.Collect), new Type[] { typeof(Int32), typeof(GCCollectionMode) })]
        private static class GCCollect_patch3
        {
            public static bool Prefix()
            {
                CharaAnime.Logger.LogDebug($"GC.Collect(Int32, GCCollectionMode) triggered, suspend request = {SuspendGC}");
                return JudgeGCRequest();
            }
        }

        [HarmonyPatch(typeof(GC), nameof(GC.Collect), new Type[] { typeof(Int32), typeof(GCCollectionMode), typeof(Boolean) })]
        private static class GCCollect_patch4
        {
            public static bool Prefix()
            {
                CharaAnime.Logger.LogDebug($"GC.Collect(Int32, GCCollectionMode, Boolean) triggered, suspend request = {SuspendGC}");
                return JudgeGCRequest();
            }
        }

        [HarmonyPatch(typeof(GC), nameof(GC.Collect), new Type[] { typeof(Int32), typeof(GCCollectionMode), typeof(Boolean), typeof(Boolean) })]
        private static class GCCollect_patch5
        {
            public static bool Prefix()
            {
                CharaAnime.Logger.LogDebug($"GC.Collect(Int32, GCCollectionMode, Boolean, Boolean) triggered, suspend request = {SuspendGC}");
                return JudgeGCRequest();
            }
        }
#endregion
#endif
    }
}
