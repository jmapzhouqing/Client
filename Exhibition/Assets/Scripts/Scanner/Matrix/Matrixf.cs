using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Matrixf{

	private float m00;
	private float m01;
	private float m02;
	private float m03;
	private float m10;
	private float m11;
	private float m12;
	private float m13;
	private float m20;
	private float m21;
	private float m22;
	private float m23;
	private float m30;
	private float m31;
	private float m32;
	private float m33;

	public Matrixf(){
		Vector3 value = Vector3.zero;
	}



	public void SetTRS(Vector3 center,Quaternionf rotation,Vector3 scale){
		this.QuaternionConvertMatrix (rotation);

		this.m00 *= scale.x;
		this.m01 *= scale.x;
		this.m02 *= scale.x;


		this.m10 *= scale.y;
		this.m11 *= scale.y;
		this.m12 *= scale.y;

		this.m20 *= scale.z;
		this.m21 *= scale.z;
		this.m22 *= scale.z;

		this.m30 = center.x;
		this.m31 = center.y;
		this.m32 = center.z;
	}


	public Vector3 MultiplyVector(Vector3 vector){
		Vector3 res;
		res.x = this.m00 * vector.x + this.m10 * vector.y + this.m20 * vector.z;
		res.y = this.m01 * vector.x + this.m11 * vector.y + this.m21 * vector.z;
		res.z = this.m02 * vector.x + this.m12 * vector.y + this.m22 * vector.z;

		return res;
	}

	public Vector3 MultiplyPoint(Vector3 vertic){
		Vector3 res;
		res.x = this.m00 * vertic.x + this.m10 * vertic.y + this.m20 * vertic.z + this.m30;
		res.y = this.m01 * vertic.x + this.m11 * vertic.y + this.m21 * vertic.z + this.m31;
		res.z = this.m02 * vertic.x + this.m12 * vertic.y + this.m22 * vertic.z + this.m32;
		return res;
	}

	private void QuaternionConvertMatrix(Quaternionf q){
		this.m00 = 1.0f - 2 * (Mathf.Pow (q.y, 2) + Mathf.Pow (q.z, 2));
		this.m01 = 2 * (q.x * q.y + q.w * q.z);
		this.m02 = 2 * (q.x * q.z - q.w * q.y);
		this.m03 = 0.0f;

		this.m10 = 2 * (q.x * q.y - q.w * q.z);
		this.m11 = 1 - 2 * (Mathf.Pow(q.x, 2) + Mathf.Pow(q.z, 2));
		this.m12 = 2 * (q.y * q.z + q.w * q.x);
		this.m13 = 0.0f;

		this.m20 = 2 * (q.x * q.z + q.w * q.y);
		this.m21 = 2 * (q.y * q.z - q.w * q.x);
		this.m22 = 1 - 2 * (Mathf.Pow (q.x, 2) + Mathf.Pow (q.y, 2));
		this.m23 = 0.0f;

		this.m30 = 0.0f;
		this.m31 = 0.0f;
		this.m32 = 0.0f;
		this.m33 = 1.0f;
	}
}
