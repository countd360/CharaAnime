//
// This unility source is provided by qwmnerbvqwmn@zodgame
// To reduce GC usage when playing MMDD
// Thanks!
//

using System;
using UnityEngine;

namespace CharaAnime
{
    static class MmddUtility
    {

        public static Vector3 BoneAdjustment_adjustAxis(int sign0, int sign1, int sign2, Vector3 v)
        {
            return new Vector3(sign0 * v.x, sign1 * v.y, sign2 * v.z);
        }
        

        // public static Quaternion BoneAdjustment_GetAdjustedRotation(
        //     Vector3 self_axisX, Vector3 self_axisY, Vector3 self_axisZ, 
        //     bool self_rotAxisAdjustment, 
        //     int self_sign0, int self_sign1, int self_sign2,
        //     Quaternion self_rotAdjustment,
        //     float self_rotationScale,
        //     Quaternion baseRotQ)
        // {
        //     Vector3 rotV = baseRotQ.eulerAngles;
        //     rotV = BoneAdjustment_adjustAxis(self_sign0, self_sign1, self_sign2, rotV);
        //     Quaternion quaternion;
        //     if (self_rotAxisAdjustment)
        //         quaternion = Quaternion.AngleAxis(rotV.y, self_axisY) * Quaternion.AngleAxis(rotV.x, self_axisX) * Quaternion.AngleAxis(rotV.z, self_axisZ) * self_rotAdjustment;
        //     else
        //         quaternion = Quaternion.Euler(rotV) * self_rotAdjustment;
        //     if (self_rotationScale != 1.0)
        //         quaternion = Quaternion.SlerpUnclamped(Quaternion.identity, quaternion, self_rotationScale);
        //     return quaternion;
        // }

        // def GetAdjustedRotation(self, baseRotQ):
        //     rotV = baseRotQ.eulerAngles
        //     rotV = self.adjustAxis(rotV)
        //     if self.rotAxisAdjusted:
        //         # rot by axis z->x->y
        //         #quaternion = Quaternion.AngleAxis(rotV.y, self.axisY) * Quaternion.AngleAxis(rotV.x, self.axisX) * Quaternion.AngleAxis(rotV.z, self.axisZ) * self.rotAdjustment
        //         quaternion = self.rotAxisAdjustment * Quaternion.Euler(rotV) * Quaternion.Inverse(self.rotAxisAdjustment) * self.rotAdjustment
        //     else:
        //         quaternion = Quaternion.Euler(rotV) * self.rotAdjustment
        //     if self.rotationScale != 1.0:
        //         quaternion = Quaternion.SlerpUnclamped(Quaternion.identity, quaternion, self.rotationScale)
        //     return quaternion

        public static Quaternion BoneAdjustment_GetAdjustedRotation(
            Vector3 self_axisX, Vector3 self_axisY, Vector3 self_axisZ,
            Quaternion self_rotAxisAdjustment, bool self_rotAxisAdjusted,
            int self_sign0, int self_sign1, int self_sign2,
            Quaternion self_rotAdjustment,
            float self_rotationScale,
            Quaternion baseRotQ)
        {
            Vector3 rotV = baseRotQ.eulerAngles;
            rotV = BoneAdjustment_adjustAxis(self_sign0, self_sign1, self_sign2, rotV);
            Quaternion quaternion;
            if (self_rotAxisAdjusted)
                // # rot by axis z->x->y
                quaternion = self_rotAxisAdjustment * Quaternion.Euler(rotV) * Quaternion.Inverse(self_rotAxisAdjustment) * self_rotAdjustment;
            else
                quaternion = Quaternion.Euler(rotV) * self_rotAdjustment;
            if (self_rotationScale != 1.0)
                quaternion = Quaternion.SlerpUnclamped(Quaternion.identity, quaternion, self_rotationScale);
            return quaternion;
        }




        //def GetAdjustedPosition(self, basePosV):
        //deltaPos = Vector3(self.sign[0] * self.positionScale* basePosV.x, self.sign[1] * self.positionScale* basePosV.y, self.sign[2] * self.positionScale* basePosV.z)
        //return self.basePosition + deltaPos
        public static Vector3 BoneAdjustment_GetAdjustedPosition(int self_sign0, int self_sign1, int self_sign2, float self_positionScale, Vector3 self_basePosition, Vector3 basePosV)
        {
            Vector3 deltaPos = new Vector3(self_sign0 * self_positionScale * basePosV.x, self_sign1 * self_positionScale * basePosV.y, self_sign2 * self_positionScale * basePosV.z);
            return self_basePosition + deltaPos;
        }

        public static void SetTransformLocalRotation(Transform transform, Quaternion rotation)
        {
            transform.localRotation = rotation;
        }

        public static void SetTransformLocalPosition(Transform transform, Vector3 position)
        {
            transform.localPosition = position;
        }

        public static void SetWorkBoneIndexLocalRotation(Transform wbi_transform, Quaternion boneRot, Vector3 wbi_referAxis, Vector3 wbi_limitAxis)
        {

            boneRot.ToAngleAxis(out float angle, out Vector3 axis);
            if ((axis + wbi_referAxis).magnitude < 1)
                angle = -angle;
            wbi_transform.localRotation = Quaternion.AngleAxis(angle, wbi_limitAxis);
        }


        public static void SetWorkBoneIndexTrackLocalRotation(Transform wbi_transform, Transform wbi_trackSrcTransform, float wbi_trackScale)
        {
            // rotBase = wbi.transform.localRotation
            // rotTSrc = wbi.trackSrcTransform.localRotation
            // wbi.transform.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, rotTSrc, wbi.trackScale) * rotBase
            Quaternion rotBase = wbi_transform.localRotation;
            Quaternion rotTSrc = wbi_trackSrcTransform.localRotation;
            wbi_transform.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, rotTSrc, wbi_trackScale) * rotBase;
        }

        public static void SetWorkBoneIndexTrackLocalPosition(Transform wbi_transform, Transform wbi_trackSrcTransform, float wbi_trackScale)
        {
            // posBase = wbi.transform.localPosition
            // posTSrc = wbi.trackSrcTransform.localPosition
            // wbi.transform.localPosition = Vector3(posBase.x + posTSrc.x * wbi.trackScale, posBase.y + posTSrc.y * wbi.trackScale, posBase.z + posTSrc.z * wbi.trackScale)
            Vector3 posBase = wbi_transform.localPosition;
            Vector3 posTSrc = wbi_trackSrcTransform.localPosition;
            wbi_transform.localPosition = new Vector3(posBase.x + posTSrc.x * wbi_trackScale, posBase.y + posTSrc.y * wbi_trackScale, posBase.z + posTSrc.z * wbi_trackScale);
        }

        public static bool IsLocalRotationIdentify(Transform transform)
        {
            return transform.localRotation == Quaternion.identity;
        }

        public static void SetTransformTwistDisperse(Transform tdi_twistBoneTransform, Transform tdi_disperseBoneTransform, float tdi_disperseRate)
        {
            // orgQ = tdi.twistBoneTransform.localRotation
            // remainQ = Quaternion.Lerp(Quaternion.identity, orgQ, 1.0 - tdi.disperseRate)
            // disperseQ = orgQ * Quaternion.Inverse(remainQ)
            // tdi.twistBoneTransform.localRotation = remainQ
            // tdi.disperseBoneTransform.localRotation = tdi.disperseBoneTransform.localRotation * disperseQ
            Quaternion orgQ = tdi_twistBoneTransform.localRotation;
            Quaternion remainQ = Quaternion.Lerp(Quaternion.identity, orgQ, 1.0f - tdi_disperseRate);
            Quaternion disperseQ = orgQ * Quaternion.Inverse(remainQ);
            tdi_twistBoneTransform.localRotation = remainQ;
            tdi_disperseBoneTransform.localRotation = tdi_disperseBoneTransform.localRotation * disperseQ;
        }





        // @staticmethod
        // def sampleBezier(handle1, handle2, t):
        //     zero = Vector2.zero
        //     full = Vector2(1.0, 1.0)
        //     v1 = Vector2.Lerp(zero, handle1, t)
        //     v2 = Vector2.Lerp(handle1, handle2, t)
        //     v3 = Vector2.Lerp(handle2, full, t)
        //     v4 = Vector2.Lerp(v1, v2, t)
        //     v5 = Vector2.Lerp(v2, v3, t)
        //     v6 = Vector2.Lerp(v4, v5, t)
        //     return v6 
        public static Vector2 VmdLib_sampleBezier(Vector2 handle1, Vector2 handle2, float t)
        {
            Vector2 zero = Vector2.zero;
            Vector2 full = new Vector2(1.0f, 1.0f);
            Vector2 v1 = Vector2.Lerp(zero, handle1, t);
            Vector2 v2 = Vector2.Lerp(handle1, handle2, t);
            Vector2 v3 = Vector2.Lerp(handle2, full, t);
            Vector2 v4 = Vector2.Lerp(v1, v2, t);
            Vector2 v5 = Vector2.Lerp(v2, v3, t);
            Vector2 v6 = Vector2.Lerp(v4, v5, t);
            return v6;
        }

        // @staticmethod
        // def cubicBezierInterpolate_2(p1x, p2x, p1y, p2y, x):
        //     # another cubic bezier iterploate function
        //     # slower but more safe?
        //     handle1 = Vector2(p1x, p1y)
        //     handle2 = Vector2(p2x, p2y)
        //     xtgt = x
        //     x0 = 0.0
        //     x1 = 1.0
        //     c = 0
        //     #print("try to find point at %.3f"%x)
        //     #print("ref result:", VmdLib.cubicBezierInterpolate(p1x, p2x, p1y, p2y, x))
        //     while True:
        //         v = VmdLib.sampleBezier(handle1, handle2, x)
        //         c += 1
        //         #print("sample #%d x=%.3f get point (%.3f, %.3f)"%(c, x, v.x, v.y))
        //         if c >= 10: break
        //         if math.fabs(xtgt - v.x) < 0.001: break
        //         if v.x > xtgt:
        //             x1 = x
        //             x = (x1 + x0) / 2
        //             #print("   over! -> set seek range to %.3f to %.3f"%(x0, x1))
        //         else:
        //             x0 = x
        //             x = (x1 + x0) / 2
        //             #print("   less! -> set seek range to %.3f to %.3f"%(x0, x1))
        //     return v.y
        public static float VmdLib_cubicBezierInterpolate_2(float p1x, float p2x, float p1y, float p2y, float x)
        {
            // another cubic bezier iterploate function
            // slower but more safe?
            Vector2 handle1 = new Vector2(p1x, p1y);
            Vector2 handle2 = new Vector2(p2x, p2y);
            float xtgt = x;
            float x0 = 0.0f;
            float x1 = 1.0f;
            int c = 0;
            //print("try to find point at %.3f"%x)
            //print("ref result:", VmdLib.cubicBezierInterpolate(p1x, p2x, p1y, p2y, x))
            Vector2 v;
            while (true)
            {
                v = VmdLib_sampleBezier(handle1, handle2, x);
                c += 1;
                //print("sample #%d x=%.3f get point (%.3f, %.3f)"%(c, x, v.x, v.y))
                if (c >= 10) break;
                if (Mathf.Abs(xtgt - v.x) < 0.001f) break;
                if (v.x > xtgt)
                {
                    x1 = x;
                    x = (x1 + x0) / 2;
                    //print("   over! -> set seek range to %.3f to %.3f"%(x0, x1))
                }
                else
                {
                    x0 = x;
                    x = (x1 + x0) / 2;
                    //print("   less! -> set seek range to %.3f to %.3f"%(x0, x1))
                }
            }
            return v.y;
        }

        //public static void Lerp3(ref Vector3 rst, Vector3 a, Vector3 b, Vector3 t)
        //{
        //    rst = new Vector3(Mathf.Lerp(a.x, b.x, t.x),Mathf.Lerp(a.y, b.y, t.y),Mathf.Lerp(a.z, b.z, t.z));
        //}

        //public static void Lerp3(ref Vector3 rst, Vector3 a, Vector3 b, float tx, float ty, float tz)
        //{
        //    rst = new Vector3(Mathf.Lerp(a.x, b.x, tx),Mathf.Lerp(a.y, b.y, ty),Mathf.Lerp(a.z, b.z, tz));
        //}

        public static Vector3 Lerp3( Vector3 a, Vector3 b, float tx, float ty, float tz)
        {
            return new Vector3(Mathf.Lerp(a.x, b.x, tx), Mathf.Lerp(a.y, b.y, ty), Mathf.Lerp(a.z, b.z, tz));
        }

        public static void Quaternion_Slerp(ref Quaternion rst, Quaternion a, Quaternion b, float t)
        {
            rst = Quaternion.Slerp(a, b, t);
        }

    }
}
