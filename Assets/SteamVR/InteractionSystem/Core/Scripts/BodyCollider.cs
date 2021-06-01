//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

//note: some edits have been made by Jacob Chesnut for the purpose of integration with
//the topographical map project

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( CapsuleCollider ) )]
	public class BodyCollider : MonoBehaviour
	{

		public Rigidbody myBody;

		public Transform head;

		//overall VR player object
		public Transform playerObject;

		private CapsuleCollider capsuleCollider;

		//-------------------------------------------------
		void Awake()
		{
			capsuleCollider = GetComponent<CapsuleCollider>();
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			
			float distanceFromFloor = Vector3.Dot( head.localPosition, Vector3.up );
			//capsuleCollider.height = Mathf.Max( capsuleCollider.radius, distanceFromFloor );
			transform.localPosition = head.localPosition - 0.5f * distanceFromFloor * Vector3.up;

			//shoot a ray downward to find the floor, and place the VR player there
			Vector3 startPosition = transform.position;
			Vector3 rayDirection = Vector3.down;
			Ray MouseRay = new Ray(startPosition, rayDirection);
			RaycastHit Hit;
			//int layerMask = LayerMask.GetMask("Ground");
			if (Physics.Raycast(MouseRay, out Hit))
            {
				Vector3 newPosition = playerObject.position;
				newPosition.y = Hit.point.y;
				playerObject.position = newPosition;
			}


			/*
			//the following will cause the player to fall with gravity
			//note: old movement system which caused a lot of bouncing for the VR player
			Vector3 newPosition = playerObject.position;
			newPosition.y = transform.position.y;
			//newPosition.x = transform.position.x;
			//newPosition.z = transform.position.z;
			playerObject.position = newPosition;
			if(myBody.velocity.y > 0)
            {
				myBody.velocity = Vector3.zero;
            }
            else
            {
				myBody.velocity = (0.5f * myBody.velocity);
            }
			*/
		}
	}
}
