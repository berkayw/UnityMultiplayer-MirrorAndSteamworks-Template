﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CMF
{
	//This script controls the character's animation by passing velocity values and other information ('isGrounded') to an animator component;
	public class AnimationControl : NetworkBehaviour {

		Controller controller;
		public Animator animator;
		public NetworkAnimator nAnimator;
		Transform animatorTransform;
		Transform tr;

		//Whether the character is using the strafing blend tree;
		public bool useStrafeAnimations = false;

		//Velocity threshold for landing animation;
		//Animation will only be triggered if downward velocity exceeds this threshold;
		public float landVelocityThreshold = 5f;

		private float smoothingFactor = 40f;
		Vector3 oldMovementVelocity = Vector3.zero;

		//Setup;
		void Awake () {
			controller = GetComponent<Controller>();
			animatorTransform = animator.transform;
			tr = transform;
		}

		//OnEnable;
		void OnEnable()
		{
			//Connect events to controller events;
			controller.OnLand += OnLand;
			controller.OnJump += OnJump;
		}

		//OnDisable;
		void OnDisable()
		{
			//Disconnect events to prevent calls to disabled gameobjects;
			controller.OnLand -= OnLand;
			controller.OnJump -= OnJump;
		}
		
		//Update;
		void Update () {
			
			//Get controller velocity;
			Vector3 _velocity = controller.GetVelocity();

			//Split up velocity;
			Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, tr.up);
			Vector3 _verticalVelocity = _velocity - _horizontalVelocity;

			//Smooth horizontal velocity for fluid animation;
			_horizontalVelocity = Vector3.Lerp(oldMovementVelocity, _horizontalVelocity, smoothingFactor * Time.deltaTime);
			oldMovementVelocity = _horizontalVelocity;

			//animator.SetFloat("VerticalSpeed", _verticalVelocity.magnitude * VectorMath.GetDotProduct(_verticalVelocity.normalized, tr.up));
			//animator.SetFloat("HorizontalSpeed", _horizontalVelocity.magnitude);
			//If animator is strafing, split up horizontal velocity;
			if(useStrafeAnimations)
			{
				Vector3 _localVelocity = animatorTransform.InverseTransformVector(_horizontalVelocity);
				animator.SetFloat("ForwardSpeed", _localVelocity.z);
				animator.SetFloat("StrafeSpeed", _localVelocity.x);
			}

			//Pass values to animator;
			//animator.SetBool("IsGrounded", controller.IsGrounded());
			//animator.SetBool("IsStrafing", useStrafeAnimations);
			CmdUpdateAnimatorParams(_verticalVelocity.magnitude, _horizontalVelocity.magnitude, controller.IsGrounded(), useStrafeAnimations);

		}

		[Command]
		void CmdUpdateAnimatorParams(float verticalSpeed, float horizontalSpeed, bool isGrounded, bool isStrafing)
		{
			// Sunucuda parametreleri güncelle:
			nAnimator.animator.SetFloat("VerticalSpeed", verticalSpeed);
			nAnimator.animator.SetFloat("HorizontalSpeed", horizontalSpeed);
			nAnimator.animator.SetBool("IsGrounded", isGrounded);
			animator.SetBool("IsStrafing", isStrafing);
			SyncAnims(verticalSpeed, horizontalSpeed, isGrounded, isStrafing);
		}

		[ClientRpc]
		void SyncAnims(float verticalSpeed, float horizontalSpeed, bool isGrounded , bool isStrafing)
		{
			nAnimator.animator.SetFloat("VerticalSpeed", verticalSpeed);
			nAnimator.animator.SetFloat("HorizontalSpeed", horizontalSpeed);
			nAnimator.animator.SetBool("IsGrounded", isGrounded);
			animator.SetBool("IsStrafing", isStrafing);
		}
		
		void OnLand(Vector3 _v)
		{
			//Only trigger animation if downward velocity exceeds threshold;
			if(VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
				return;

			//animator.SetTrigger("OnLand");
			nAnimator.SetTrigger("OnLand");
		}

		void OnJump(Vector3 _v)
		{
			
		}
	}
}
