using UnityEngine;

namespace CharaAnime
{
    public class CCDIKSolver
    {
        public void Solve()
        {
            if (this.useLeg)
            {
                this.SolveLeg();
                return;
            }
            this.SolveNormal();
        }

        public void SolveLeg()
        {
            if (this.lastFrameQ != null)
            {
                for (int i = 0; i < this.chains.Length; i++)
                {
                    Quaternion localRotation = Quaternion.Slerp(this.chains[i].localRotation, this.lastFrameQ[i], this.lastFrameWeight);
                    this.chains[i].localRotation = localRotation;
                }
            }
            else
            {
                this.lastFrameQ = new Quaternion[this.chains.Length];
            }
            float magnitude = (this.ikBone.position - this.chains[1].position).magnitude;
            float magnitude2 = (this.chains[0].position - this.chains[1].position).magnitude;
            float magnitude3 = (this.target.position - this.chains[1].position).magnitude;
            float num = magnitude2 + magnitude3;
            float num2 = -0.5f;
            int j = 0;
            int num3 = this.iterations;
            while (j < num3)
            {
                int k = 0;
                int num4 = this.chains.Length;
                while (k < num4)
                {
                    Transform transform = this.chains[k];
                    Vector3 position = transform.position;
                    Vector3 lhs = this.target.position - position;
                    Vector3 rhs = this.ikBone.position - position;
                    bool flag = this.drawRay;
                    float sqrMagnitude = lhs.sqrMagnitude;
                    float sqrMagnitude2 = rhs.sqrMagnitude;
                    lhs = lhs.normalized;
                    rhs = rhs.normalized;
                    float num5 = Mathf.Acos(Vector3.Dot(lhs, rhs));
                    Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
                    if (!float.IsNaN(num5) && num5 != 0f)
                    {
                        goto IL_219;
                    }
                    if (k == 1 && this.chains[0].localEulerAngles.x == 0f && magnitude >= num)
                    {
                        this.StoreLastQ();
                        return;
                    }
                    if (k == 1 && this.chains[0].localEulerAngles.x == 0f && sqrMagnitude2 < sqrMagnitude)
                    {
                        normalized = new Vector3(1f, 0f, 0f);
                        num5 = num2;
                        goto IL_219;
                    }
                IL_2D6:
                    k++;
                    continue;
                IL_219:
                    float num6 = this.controll_weight;
                    if (num5 > num6)
                    {
                        num5 = num6;
                    }
                    if (num5 < -num6)
                    {
                        num5 = -num6;
                    }
                    num5 *= 57.29578f;
                    if (float.IsNaN(normalized.x) || float.IsNaN(normalized.y) || float.IsNaN(normalized.z))
                    {
                        goto IL_2D6;
                    }
                    Quaternion lhs2 = Quaternion.AngleAxis(num5, normalized);
                    transform.rotation = lhs2 * transform.rotation;
                    this.limitter(transform);
                    if (this.minDelta != 0f && (this.target.position - this.ikBone.position).sqrMagnitude < this.minDelta)
                    {
                        this.StoreLastQ();
                        return;
                    }
                    goto IL_2D6;
                }
                j++;
            }
            this.StoreLastQ();
        }

        public void SolveLeg2()
        {
            if (this.lastFrameQ != null)
            {
                for (int i = 0; i < this.chains.Length; i++)
                {
                    Quaternion localRotation = Quaternion.Slerp(this.chains[i].localRotation, this.lastFrameQ[i], this.lastFrameWeight);
                    this.chains[i].localRotation = localRotation;
                }
            }
            else
            {
                this.lastFrameQ = new Quaternion[this.chains.Length];
            }
            Vector3 a = this.ikBone.position - this.chains[1].position;
            float magnitude = a.magnitude;
            float magnitude2 = (this.chains[0].position - this.chains[1].position).magnitude;
            float magnitude3 = (this.target.position - this.chains[1].position).magnitude;
            float num = magnitude2 + magnitude3;
            Vector3 vector;
            if (magnitude > num && num > 0f)
            {
                vector = this.chains[1].position + a * (num / magnitude);
            }
            else
            {
                vector = this.ikBone.position;
            }
            float num2 = -0.5f;
            if (this.ikRotationWeight > 0f)
            {
                Vector3 a2 = this.ikBone.TransformPoint(new Vector3(0f, magnitude3, 0f));
                Transform transform = this.chains[1];
                Vector3 position = transform.position;
                Vector3 lhs = a2 - position;
                Vector3 rhs = vector - position;
                float sqrMagnitude = lhs.sqrMagnitude;
                float sqrMagnitude2 = rhs.sqrMagnitude;
                lhs = lhs.normalized;
                rhs = rhs.normalized;
                float num3 = Mathf.Acos(Vector3.Dot(lhs, rhs));
                Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
                if (!float.IsNaN(num3) && num3 != 0f && !float.IsNaN(normalized.x) && !float.IsNaN(normalized.y) && !float.IsNaN(normalized.z))
                {
                    Quaternion b = Quaternion.AngleAxis(num3, normalized) * transform.rotation;
                    transform.rotation = Quaternion.Slerp(transform.rotation, b, this.ikRotationWeight);
                }
            }
            int j = 0;
            int num4 = this.iterations;
            while (j < num4)
            {
                int k = 0;
                int num5 = this.chains.Length;
                while (k < num5)
                {
                    Transform transform2 = this.chains[k];
                    Vector3 position2 = transform2.position;
                    Vector3 lhs2 = this.target.position - position2;
                    Vector3 rhs2 = vector - position2;
                    bool flag = this.drawRay;
                    float sqrMagnitude3 = lhs2.sqrMagnitude;
                    float sqrMagnitude4 = rhs2.sqrMagnitude;
                    lhs2 = lhs2.normalized;
                    rhs2 = rhs2.normalized;
                    float num6 = Mathf.Acos(Vector3.Dot(lhs2, rhs2));
                    Vector3 normalized2 = Vector3.Cross(lhs2, rhs2).normalized;
                    if (!float.IsNaN(num6) && num6 != 0f)
                    {
                        goto IL_34F;
                    }
                    if (k == 1 && this.chains[0].localEulerAngles.x == 0f && magnitude >= num)
                    {
                        this.StoreLastQ();
                        return;
                    }
                    if (k == 1 && this.chains[0].localEulerAngles.x == 0f && sqrMagnitude4 < sqrMagnitude3)
                    {
                        normalized2 = new Vector3(1f, 0f, 0f);
                        num6 = num2;
                        goto IL_34F;
                    }
                IL_40C:
                    k++;
                    continue;
                IL_34F:
                    float num7 = 4f * this.controll_weight * (float)(k + 1);
                    if (num6 > num7)
                    {
                        num6 = num7;
                    }
                    if (num6 < -num7)
                    {
                        num6 = -num7;
                    }
                    num6 *= 57.29578f;
                    if (float.IsNaN(normalized2.x) || float.IsNaN(normalized2.y) || float.IsNaN(normalized2.z))
                    {
                        goto IL_40C;
                    }
                    Quaternion lhs3 = Quaternion.AngleAxis(num6, normalized2);
                    transform2.rotation = lhs3 * transform2.rotation;
                    this.limitter(transform2);
                    if (this.minDelta != 0f && (this.target.position - vector).sqrMagnitude < this.minDelta)
                    {
                        this.StoreLastQ();
                        return;
                    }
                    goto IL_40C;
                }
                j++;
            }
            this.StoreLastQ();
        }

        private void StoreLastQ()
        {
            for (int i = 0; i < this.chains.Length; i++)
            {
                this.lastFrameQ[i] = this.chains[i].localRotation;
            }
        }

        public void SolveNormal()
        {
            if (this.lastFrameQ != null)
            {
                for (int i = 0; i < this.chains.Length; i++)
                {
                    Quaternion localRotation = Quaternion.Slerp(this.chains[i].localRotation, this.lastFrameQ[i], this.lastFrameWeight);
                    this.chains[i].localRotation = localRotation;
                }
            }
            else
            {
                this.lastFrameQ = new Quaternion[this.chains.Length];
            }
            int j = 0;
            int num = this.iterations;
            while (j < num)
            {
                int k = 0;
                int num2 = this.chains.Length;
                while (k < num2)
                {
                    Transform transform = this.chains[k];
                    Vector3 position = transform.position;
                    Vector3 lhs = this.target.position - position;
                    Vector3 rhs = this.ikBone.position - position;
                    bool flag = this.drawRay;
                    lhs = lhs.normalized;
                    rhs = rhs.normalized;
                    float num3 = Mathf.Acos(Vector3.Dot(lhs, rhs));
                    if (!float.IsNaN(num3))
                    {
                        float num4 = 4f * this.controll_weight * (float)(k + 1);
                        if (num3 > num4)
                        {
                            num3 = num4;
                        }
                        if (num3 < -num4)
                        {
                            num3 = -num4;
                        }
                        num3 *= 57.29578f;
                        Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
                        if (!float.IsNaN(normalized.x) && !float.IsNaN(normalized.y) && !float.IsNaN(normalized.z))
                        {
                            Quaternion lhs2 = Quaternion.AngleAxis(num3, normalized);
                            transform.rotation = lhs2 * transform.rotation;
                            this.limitter(transform);
                            if (this.minDelta != 0f && (this.target.position - this.ikBone.position).sqrMagnitude < this.minDelta)
                            {
                                for (int l = 0; l < this.chains.Length; l++)
                                {
                                    this.lastFrameQ[l] = this.chains[l].localRotation;
                                }
                                return;
                            }
                        }
                    }
                    k++;
                }
                j++;
            }
            for (int m = 0; m < this.chains.Length; m++)
            {
                this.lastFrameQ[m] = this.chains[m].localRotation;
            }
        }

        private void limitter(Transform bone)
        {
            if (bone.name.Contains("足首") || bone.name.Contains("foot"))
            {
                Vector3 localEulerAngles = bone.localEulerAngles;
                localEulerAngles.z = 0f;
                bone.localRotation = Quaternion.Euler(localEulerAngles);
                return;
            }
            if (!bone.name.Contains("ひざ") && !bone.name.Contains("lowleg"))
            {
                return;
            }
            Vector3 localEulerAngles2 = bone.localEulerAngles;
            if (this.adjust_rot(localEulerAngles2.y) == this.adjust_rot(localEulerAngles2.z))
            {
                localEulerAngles2.y = (float)this.adjust_rot(localEulerAngles2.y);
                localEulerAngles2.z = (float)this.adjust_rot(localEulerAngles2.z);
            }
            if (localEulerAngles2.x > 180f)
            {
                localEulerAngles2.x -= 360f;
            }
            if (localEulerAngles2.x < this.minKneeRot)
            {
                localEulerAngles2.x = this.minKneeRot;
            }
            else if (localEulerAngles2.x > 170f)
            {
                localEulerAngles2.x = 170f;
            }
            bone.localRotation = Quaternion.Euler(localEulerAngles2);
        }

        private int adjust_rot(float n)
        {
            if (Mathf.Abs(n) > Mathf.Abs(180f - n) && Mathf.Abs(360f - n) > Mathf.Abs(180f - n))
            {
                return 180;
            }
            return 0;
        }

        public Transform ikBone;

        public Transform target;

        public int iterations;

        public float controll_weight;

        public Transform[] chains;

        public bool drawRay;

        public float minDelta;

        public bool useLeg;

        public Quaternion[] lastFrameQ;

        public float lastFrameWeight = 0.9f;

        public float ikRotationWeight;

        public float minKneeRot = 5f;
    }

}
