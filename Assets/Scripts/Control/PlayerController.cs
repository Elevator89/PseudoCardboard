using Assets.Scripts.Control.Rotation;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Control
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public abstract class PlayerController : MonoBehaviour
	{
		[SerializeField]
		private Transform _cam;

		[SerializeField]
		protected readonly MovementSettings MovementSettings = new MovementSettings();

		[SerializeField]
		protected readonly AdvancedSettings AdvancedSettings = new AdvancedSettings();

		protected abstract IInstantRotator InstantRotator { get; }

		//Need to position players menu. 
		//Logically, if there is a match the height of the player controller capsule collider.
		public float PlayerHeight = 1.7f;

		protected Rigidbody RigidBody;
		private CapsuleCollider _capsule;
		private float _yRotation;
		private Vector3 _groundContactNormal;
		private bool _previouslyGrounded;
		private bool _isGrounded;
		private bool _isMove;

		public UnityEvent StartMoving = new UnityEvent();
		public UnityEvent StopMoving = new UnityEvent();

		public Vector3 Velocity
		{
			get { return RigidBody.velocity; }
		}

		public bool Grounded
		{
			get { return _isGrounded; }
		}

		public bool Running
		{
			get
			{
#if !MOBILE_INPUT
				return MovementSettings.Running;
#else
				return false;
#endif
			}
		}


		protected virtual void Start()
		{
			RigidBody = GetComponent<Rigidbody>();
			_capsule = GetComponent<CapsuleCollider>();
			ApplyInitialCameraRotation(_cam);
		}

		protected virtual void Update()
		{
			UpdateRootRotation(transform);
			UpdateCameraRotation(_cam);
		}

		protected virtual void UpdateRootRotation(Transform root)
		{
			if (InstantRotator != null)
				InstantRotator.UpdateRotation(root);
		}

		// These methods are usually not needed in controllers for VR
		protected virtual void ApplyInitialCameraRotation(Transform cam) { }
		protected virtual void UpdateCameraRotation(Transform cam) { }

		private void FixedUpdate()
		{
			if (!MovementSettings.AllowMovement)
				return;

			GroundCheck();
			Vector2 input = GetMovementInput();
			MovementSettings.UpdateDesiredTargetSpeed(input);

			if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (AdvancedSettings.AirControl || _isGrounded))
			{
				CheckMovingEvents(true);

				// always move along the camera forward as it is the direction that it being aimed at
				Vector3 desiredMove = _cam.forward * input.y + _cam.right * input.x;
				desiredMove = Vector3.ProjectOnPlane(desiredMove, _groundContactNormal).normalized;

				desiredMove.x = desiredMove.x * MovementSettings.CurrentTargetSpeed;
				desiredMove.z = desiredMove.z * MovementSettings.CurrentTargetSpeed;
				desiredMove.y = desiredMove.y * MovementSettings.CurrentTargetSpeed;
				if (RigidBody.velocity.sqrMagnitude <
					(MovementSettings.CurrentTargetSpeed * MovementSettings.CurrentTargetSpeed))
				{
					RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
				}
			}
			else if (RigidBody.velocity.sqrMagnitude < 0.01f)
			{
				CheckMovingEvents(false);
			}

			if (_isGrounded)
			{
				RigidBody.drag = 5f;

				if (Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && RigidBody.velocity.magnitude < 1f)
				{
					RigidBody.Sleep();
				}
			}
			else
			{
				RigidBody.drag = 0f;
				if (_previouslyGrounded)
				{
					StickToGroundHelper();
				}
			}
		}

		private void CheckMovingEvents(bool inMoveNow)
		{
			if (inMoveNow)
			{
				if (!_isMove)
				{
					_isMove = true;
					StartMoving.Invoke();
				}
			}
			else
			{
				if (_isMove)
				{
					_isMove = false;
					StopMoving.Invoke();
				}
			}
		}

		private float SlopeMultiplier()
		{
			float angle = Vector3.Angle(_groundContactNormal, Vector3.up);
			return MovementSettings.SlopeCurveModifier.Evaluate(angle);
		}


		private void StickToGroundHelper()
		{
			RaycastHit hitInfo;
			Vector3 bottomSphereCenter = transform.position + _capsule.center + Vector3.down * (0.5f * _capsule.height - _capsule.radius);

			if (Physics.SphereCast(bottomSphereCenter, _capsule.radius * (1.0f - AdvancedSettings.ShellOffset), Vector3.down, out hitInfo, _capsule.radius +
																																		   AdvancedSettings.StickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
				{
					RigidBody.velocity = Vector3.ProjectOnPlane(RigidBody.velocity, hitInfo.normal);
				}
			}
		}


		protected abstract Vector2 GetMovementInput();



		/// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
		private void GroundCheck()
		{
			_previouslyGrounded = _isGrounded;
			RaycastHit hitInfo;
			Vector3 bottomSphereCenter = transform.position + _capsule.center + Vector3.down * (0.5f * _capsule.height - _capsule.radius);

			if (Physics.SphereCast(bottomSphereCenter, _capsule.radius * (1.0f - AdvancedSettings.ShellOffset), Vector3.down, out hitInfo,
				_capsule.radius + AdvancedSettings.GroundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				_isGrounded = true;
				_groundContactNormal = hitInfo.normal;
			}
			else
			{
				_isGrounded = false;
				_groundContactNormal = Vector3.up;
			}
		}

	}
}