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
			capsuleCollider.height = Mathf.Max( capsuleCollider.radius, distanceFromFloor );
			//transform.localPosition = head.localPosition - 0.5f * distanceFromFloor * Vector3.up;

			//the following will cause the player to fall with gravity
			Vector3 newPosition = playerObject.position;
			newPosition.y = transform.position.y;
			playerObject.position = newPosition;
		}
	}
}
