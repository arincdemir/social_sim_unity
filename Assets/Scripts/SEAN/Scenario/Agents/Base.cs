using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SEAN.Scenario.Agents
{
    public abstract class Base : IVI.INavigable
    {
        //PARAMETERS
        public const float RADIUS = 0.2f;
        protected const float ROBOT_RADIUS = 0.2f;
        protected const float MASS = 80;
        protected const float PERCEPTION_RADIUS = 2;
        protected const float ANGULAR_SPEED = 120;
        protected const float ANIMATION_SMOOTHING = 0.6f;

        //private CapsuleCollider collisionCapsule;
        private Animator animator;

        //NAVIGATION
        NavMeshPath nmPath;
        protected NavMeshAgent nma;
        protected Rigidbody rb;
        protected CapsuleCollider collisionCapsule;

        //ANIMATION
        public Vector3 velocity { get; protected set; }
        private float animationScale = 1.0f;
        private float idleSpeed = 0.5f;
        private bool applyRootMotion = true;

        #region Unity Functions

        protected override void Start()
        {
            nmPath = new NavMeshPath();
            // Having a disabled navmesh agent allows it to move
            nma = gameObject.AddComponent<NavMeshAgent>();
            nma.radius = RADIUS;
            nma.enabled = false;

            rb = gameObject.GetComponent<Rigidbody>();
            if (!rb)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.mass = MASS;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            var agentMeshBounds = GetComponentInChildren<SkinnedMeshRenderer>().bounds;
            var agentHeight = agentMeshBounds.extents.y * 2;
            collisionCapsule = gameObject.GetComponent<CapsuleCollider>();
            if (collisionCapsule == null)
            {
                collisionCapsule = gameObject.AddComponent<CapsuleCollider>();
            }
            collisionCapsule.radius = RADIUS;
            collisionCapsule.height = agentHeight;
            collisionCapsule.center = Vector3.up * agentHeight / 2f;

            animator = GetComponent<Animator>();
            animator.applyRootMotion = applyRootMotion;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            base.Start();
        }

        private List<Vector3> locationHistory = new List<Vector3>();
        private float locationHistoryTimer = 0f;
        
        void Update()
        {
            velocity = UpdateVelocity();
            //print(name + " velocity: " + velocity);

            // Arinc edit
            locationHistoryTimer += Time.deltaTime;
            if (locationHistoryTimer >= 1f)
            {
                locationHistory.Add(transform.position);
                locationHistoryTimer = 0f;
            }

            Move();


            //if (path.Count > 1 && Util.Geometry.GroundPlaneDist(transform.position, path[0]) < Parameters.MIN_DIST)
            //{
            //    path.RemoveAt(0);
            //}
            //else if (path.Count == 1 && Util.Geometry.GroundPlaneDist(transform.position, path[0]) < Parameters.MIN_DIST)
            //{
            //    path.RemoveAt(0);

            //    StopAnimator();
            //}
        }

        #endregion

        protected override bool PlanNavigation()
        {
            //print(name + " StartNavigation");
            if (destPos[0] == Mathf.Infinity || destPos[1] == Mathf.Infinity || destPos[2] == Mathf.Infinity)
            {
                print(name + " goal set to infinity");
                return false;
            }
            ComputePath(destPos);
            return true;
        }

        protected override void StopNavigation()
        {
            if (nmPath != null) { nmPath.ClearCorners(); }
            destPos = Vector3.zero;
            StopAnimator();
        }

        protected override void StartGroup(IVI.GroupNavNode group)
        {
            group.AddMember(this);
        }

        protected override void StopGroup(IVI.GroupNavNode group)
        {
            group.RemoveMember(this);
        }

        #region Public Functions

        public void StopAnimator()
        {
            //animator.SetBool("Idling", true);
            animator.SetFloat("Forward", 0);
            animator.SetFloat("Strafe", 0);
        }

        public void ComputePath(Vector3 destination)
        {
            destPos = destination;
            if (nmPath == null) { return; }
            NavMesh.CalculatePath(transform.position, destPos, NavMesh.AllAreas, nmPath);
            //if (!nma) { return; }
            //nma.enabled = true;
            //var nmPath = new NavMeshPath();
            //if (nma.isOnNavMesh)
            //{
            //    //print("nma.CalculatePath(" + destination + "," + nmPath + ");");
            //    if (!nma.CalculatePath(destination, nmPath))
            //    {
            //        print("No path found for " + name + " to " + destination);
            //    }
            //    path = nmPath.corners.ToList();
            //    //print(name + " path count is " + path.Count);
            //}
            //else
            //{
            //    print(name + " is not on Navmesh");
            //}
            //print(name + " ComputePath " + destination);
            //nma.enabled = false;
        }

        #endregion

        #region Abstract Functions

        protected abstract Vector3 UpdateVelocity();

        #endregion

        #region Private Functions

        protected Vector3 nearestGoalPoint
        {
            get
            {
                // Skip points too close
                foreach (Vector3 position in nmPath.corners)
                {
                    if (Util.Geometry.GroundPlaneDist(transform.position, position) > Parameters.NEXT_NAV_MIN_DIST)
                    {
                        return position;
                    }
                }
                return destPos;
            }
        }
        private void Move()
        {
            float angle = 0;
            // compute angular velocity from next goal position
            if (!(GetType().Equals(typeof(Scenario.Agents.Playback.Agent)) || GetType().Equals(typeof(PlayerAgent))))
            {
                // $$$ FIX: can't move to 0,0,0
                if (destPos == Vector3.zero)
                {
                    //print(name + " destPos is zero");
                    return;
                }
                Vector3 goalDir = nearestGoalPoint - transform.position;
                float goalWeight = 0.5f;
                goalDir = goalWeight * goalDir.normalized + (1 - goalWeight) * velocity.normalized;
                goalDir.y = 0;
                angle = -Vector3.SignedAngle(goalDir, transform.forward, Vector3.up);
            }
            else
            {
                if (GetType().Equals(typeof(PlayerAgent)))
                {
                    angle = velocity.y * ANGULAR_SPEED * Time.deltaTime;
                }
                // read the angular velocity from the velocity field
                // note: this line is nearly identical to the following lines
                // but i didn't want to  change it in case it's specific to
                // SF code
                else
                {
                    angle = -Vector3.SignedAngle(velocity, transform.forward, Vector3.up);
                }
            }
            //Angular Velocity and rotation
            if (Mathf.Abs(angle) > ANGULAR_SPEED * Time.deltaTime)
            {
                angle = Mathf.Sign(angle) * ANGULAR_SPEED * Time.deltaTime;
            }
            //angle = Mathf.Sign(angle) * Mathf.Min(ANGULAR_SPEED, Mathf.Abs(angle)) * Time.deltaTime;
            transform.RotateAround(transform.position, Vector3.up, angle);

            // Motion
            Vector3 animParams = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * velocity;
            animParams *= animationScale;
            var idle = animParams.magnitude < idleSpeed && !applyRootMotion;

            animator.SetBool("Idling", idle);
            if (!GetType().Equals(typeof(PlayerAgent)))
            {
                animator.speed = velocity.magnitude;

            }
            animator.SetFloat("Forward", animParams.z/ANIMATION_SMOOTHING);
            animator.SetFloat("Strafe", animParams.x/ANIMATION_SMOOTHING);

            if (ShowDebug)
            {
        
                if (velocity.y < 1 && velocity.y > -1){
                    Debug.DrawLine(transform.position, transform.position + velocity, Color.red);
                }
                else{
                    Debug.DrawLine(transform.position, transform.position + velocity, Color.yellow);
                }
            }

        }

        protected override void OnDrawGizmosSelected()
        {
            if (!ShowDebug) { return; }
            Gizmos.color = Color.black;
            Vector3 lastPos = transform.position;
            foreach (Vector3 position in nmPath.corners)
            {
                if (Util.Geometry.GroundPlaneDist(transform.position, position) > Parameters.NEXT_NAV_MIN_DIST)
                {
                    Debug.DrawLine(lastPos, position);
                    Gizmos.DrawCube(position, new Vector3(0.15f, 0.15f, 0.15f));
                    lastPos = position;
                }
            }
            //Debug.DrawLine(transform.position, destPos);
            //Gizmos.DrawCube(destPos, new Vector3(0.25f, 0.25f, 0.25f));


            
            base.OnDrawGizmosSelected();
        }


        // Arinc edit
        // I added these
        protected void OnDrawGizmos()
        {
            // Draw red spheres at every 10th point in the location history
            if (locationHistory != null && locationHistory.Count > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < locationHistory.Count; i += 1)
                {
                    Gizmos.DrawSphere(locationHistory[i], 0.07f); // Adjust sphere radius as needed
                }
            }

        #if UNITY_EDITOR
            // Draw the planned future trajectory using Handles for thicker lines
            if (nmPath != null && nmPath.corners != null && nmPath.corners.Length > 0)
            {
                Handles.color = Color.magenta;
                // Set the line thickness (in pixels)
                float thickness = 5.0f;
                // Draw a polyline through the corners with the specified thickness
                Handles.DrawAAPolyLine(thickness, nmPath.corners);

                // Optionally, draw spheres at each corner
                foreach (Vector3 corner in nmPath.corners)
                {
                    Handles.SphereHandleCap(0, corner, Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
        #else
            // Fallback using Gizmos (fixed thin lines)
            if (nmPath != null && nmPath.corners != null && nmPath.corners.Length > 0)
            {
                Gizmos.color = Color.magenta;
                Vector3 prevCorner = nmPath.corners[0];
                Gizmos.DrawSphere(prevCorner, 0.10f);
                for (int j = 1; j < nmPath.corners.Length; j++)
                {
                    Vector3 currentCorner = nmPath.corners[j];
                    Gizmos.DrawLine(prevCorner, currentCorner);
                    Gizmos.DrawSphere(currentCorner, 0.1f);
                    prevCorner = currentCorner;
                }
            }
        #endif
        }

        #endregion
    }
}
