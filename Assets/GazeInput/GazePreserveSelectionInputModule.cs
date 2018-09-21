using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR;

namespace Assets.Scripts.GazeInput
{
	/// This script provides an implemention of Unity's `BaseInputModule` class, so
	/// that Canvas-based (_uGUI_) UI elements can be selected by looking at them and
	/// pulling the viewer's trigger or touching the screen.
	/// This uses the player's gaze and the trigger as a raycast generator.
	///
	/// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
	/// other modules, such as _TouchInputModule_ and _StandaloneInputModule_, in order
	/// for the user's gaze to take priority in the event system.
	///
	/// Next, set the **Canvas** object's _Render Mode_ to **World Space**, and set its _Event Camera_
	/// to a (mono) camera that is controlled by a GvrHead.  If you'd like gaze to work
	/// with 3D scene objects, add a _PhysicsRaycaster_ to the gazing camera, and add a
	/// component that implements one of the _Event_ interfaces (_EventTrigger_ will work nicely).
	/// The objects must have colliders too.
	///
	/// GazePreserveSelectionInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
	/// _Deselect_, and _UpdateSelected_.  Scroll, move, and submit/cancel events are not emitted.
	[AddComponentMenu("VR/GazePreserveSelectionInputModule")]
	public class GazePreserveSelectionInputModule : BaseInputModule
	{
		/// Time in seconds between the pointer down and up events sent by a trigger.
		/// Allows time for the UI elements to make their state transitions.
		[HideInInspector]
		public float clickTime = 0.1f;  // Based on default time for a button to animate to Pressed.

		/// The pixel through which to cast rays, in viewport coordinates.  Generally, the center
		/// pixel is best, assuming a monoscopic camera is selected as the `Canvas`' event camera.
		[HideInInspector]
		public Vector2 hotspot = new Vector2(0.5f, 0.5f);

		public Transform Gaze;

		public Reticle Reticle;

		private PointerEventData pointerData;
		private Vector2 lastHeadPose;

		public override void DeactivateModule()
		{
			base.DeactivateModule();
			if (pointerData != null)
			{
				HandlePendingClick();
				HandlePointerExitAndEnter(pointerData, null);
				pointerData = null;
			}
			eventSystem.SetSelectedGameObject(null, GetBaseEventData());
		}

		public override bool IsPointerOverGameObject(int pointerId)
		{
			return pointerData != null && pointerData.pointerEnter != null;
		}

		public override void Process()
		{
			CastRayFromGaze();
			UpdateCurrentObject();

			// Handle input
			if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0) || !Input.GetButtonDown("Fire1") && Input.GetButton("Fire1"))
			{
				HandleDrag();
			}
			else if (Time.unscaledTime - pointerData.clickTime < clickTime)
			{
				// Delay new events until clickTime has passed.
			}
			else if (!pointerData.eligibleForClick && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1")))
			{
				// New trigger action.
				HandleTrigger();
			}
			else if (!Input.GetMouseButton(0) && !Input.GetButton("Fire1"))
			{
				// Check if there is a pending click to handle.
				HandlePendingClick();
			}
		}
		/// @endcond

		private void CastRayFromGaze()
		{
			Vector2 headPose = NormalizedCartesianToSpherical(Gaze.rotation * Vector3.forward);

			if (pointerData == null)
			{
				pointerData = new PointerEventData(eventSystem);
				lastHeadPose = headPose;
			}

			// Cast a ray into the scene
			pointerData.Reset();

			//Unity devs comment:
			//We've made some changes internally to the Camera viewport rects to fix viewport issues in VR. 
			//With this change, the Screen.width and height no longer represent the resolution of the HMD. 
			//Screen represents the game view and the HMD resolution is now represented by VRSettings.eyeTextureWidth and VRSettings.eyeTextureHeight.
			pointerData.position = new Vector2(hotspot.x * Screen.width, hotspot.y * Screen.height);

			eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

			RaycastResult raycastResult = FindFirstRaycast(m_RaycastResultCache);
			pointerData.pointerCurrentRaycast = raycastResult;

			if (Reticle)
			{
				Vector3 position;
				float distance;

				GetIntersectionParameters(out position, out distance);

				if (distance > 0.01f)
				{
					Reticle.SetPosition(position, distance);
				}
				else
				{
					Reticle.SetDefaultPosition();
				}
			}

			m_RaycastResultCache.Clear();
			pointerData.delta = headPose - lastHeadPose;
			lastHeadPose = headPose;
		}

		private void UpdateCurrentObject()
		{
			// Send enter events and update the highlight.
			var go = pointerData.pointerCurrentRaycast.gameObject;
			HandlePointerExitAndEnter(pointerData, go);
			// Update the current selection, or clear if it is no longer the current object.
			var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
			if (selected == eventSystem.currentSelectedGameObject)
			{
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
					ExecuteEvents.updateSelectedHandler);
			}
			else
			{
                // we do not deselect objects by gaze.
    			//eventSystem.SetSelectedGameObject(null, pointerData);
			}
		}

		private void HandleDrag()
		{
			bool moving = pointerData.IsPointerMoving();

			if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
			{
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
					ExecuteEvents.beginDragHandler);
				pointerData.dragging = true;
			}

			// Drag notification
			if (pointerData.dragging && moving && pointerData.pointerDrag != null)
			{
				// Before doing drag we should cancel any pointer down state
				// And clear selection!
				if (pointerData.pointerPress != pointerData.pointerDrag)
				{
					ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

					pointerData.eligibleForClick = false;
					pointerData.pointerPress = null;
					pointerData.rawPointerPress = null;
				}
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
			}
		}

		private void HandlePendingClick()
		{
			if (!pointerData.eligibleForClick && !pointerData.dragging)
			{
				return;
			}

			var go = pointerData.pointerCurrentRaycast.gameObject;

			if (pointerData.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
			}
			else if (pointerData.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
			}

			// Clear the click state.
			pointerData.pointerPress = null;
			pointerData.rawPointerPress = null;
			pointerData.eligibleForClick = false;
			pointerData.clickCount = 0;
			pointerData.clickTime = 0;
			pointerData.pointerDrag = null;
			pointerData.dragging = false;
		}

		private void HandleTrigger()
		{
			var go = pointerData.pointerCurrentRaycast.gameObject;

			// Send pointer down event.
			pointerData.pressPosition = pointerData.position;
			pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
			pointerData.pointerPress =
				ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

		    if (!pointerData.pointerPress)
		    {
                // We deselect objects if pressed on the void
                eventSystem.SetSelectedGameObject(null, pointerData);
            }

            // Save the drag handler as well
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
			if (pointerData.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
			}

			// Save the pending click state.
			pointerData.rawPointerPress = go;
			pointerData.eligibleForClick = true;
			pointerData.delta = Vector2.zero;
			pointerData.dragging = false;
			pointerData.useDragThreshold = true;
			pointerData.clickCount = 1;
			pointerData.clickTime = Time.unscaledTime;
		}

		private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
		{
			cartCoords.Normalize();
			if (cartCoords.x == 0)
				cartCoords.x = Mathf.Epsilon;
			float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
			if (cartCoords.x < 0)
				outPolar += Mathf.PI;
			float outElevation = Mathf.Asin(cartCoords.y);
			return new Vector2(outPolar, outElevation);
		}

		GameObject GetCurrentGameObject()
		{
			if (pointerData != null && pointerData.enterEventCamera != null)
			{
				return pointerData.pointerCurrentRaycast.gameObject;
			}

			return null;
		}

		void GetIntersectionParameters(out Vector3 position, out float distance)
		{
			// Check for camera
			Camera cam = pointerData.enterEventCamera;
			if (cam == null)
			{
				position = Vector3.zero;
				distance = 0f;
				return;
			}

			distance = pointerData.pointerCurrentRaycast.distance;
			position = cam.transform.position + cam.transform.forward * distance;
		}
	}
}

