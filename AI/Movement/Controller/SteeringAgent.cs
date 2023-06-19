using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using System;
using JStuff.Threading;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    public abstract class SteeringAgent : MonoBehaviour, ISteeringAgent, IConsumer
    {
        [Header("Attributes")]
        [Min(0f)]
        public float speed = 10;
        [Min(0f)]
        public float maxAcceleration = 50;
        [Min(0f)]
        public float maxNormalRotation = 1;
        [Min(0.1f)]
        public float radius = 1;
        [Min(0f)]
        public float drag = 0.1f;
        [Min(0f)]
        public float speedThreshold = 0;
        [Min(0f)]
        public float defaultBehaviorWeight = 1;
        public float gravityAcceleration = 12;
        public float jumpHeight = 3f;

        public bool disableDefaultBehaviorOnIdle = false;

        public SteeringSO defaultBehavior;

        public PathfindingStrategy pathfindingStrategy = PathfindingStrategy.None;

        [Header("Runtime")]
        public float orientation;
        public Vector2 velocity = Vector2.zero;
        public Func<SteeringOutput> steeringBehavior;
        public float stationaryLastFrame = 0f;
        public bool waitingForPath = false;
        public bool initialized = false;
        public NavigationSystem navSystem;
        public float stationary;

        protected Task steeringTask;
        protected float targetStopDistance;
        protected bool hug; // If agent should stop once target is reached
        protected Action callback;
        protected bool hasReached = false;
        protected float pathfindingTime;

        public Task SteeringTask => steeringTask;
        public abstract Vector2 Position { get; }
        public float Radius => radius;
        public abstract Vector2 MinPosition { get; }
        public abstract Vector2 MaxPosition { get; }
        public float MaxSpeed => speed;
        public float MaxAcceleration => maxAcceleration;
        public float MaxRotation => maxNormalRotation * 360;
        public float Drag => drag;
        public float SpeedThreshold => speedThreshold;
        public Vector2 Velocity => velocity;
        public float Orientation => orientation;
        public abstract UpAxis Up { get; }
        public virtual float Stationary
        {
            get => stationary;
            set => stationary = value;
        }

        public virtual Transform CollisionTransform => GetComponent<Collider>().transform;

        protected virtual void Start()
        {
            navSystem = NavigationSystem.instance;
            stationary = navSystem.agentMaxStationary;
            navSystem.AddAgent(this);

            initialized = true;

            pathfindingTime = UnityEngine.Random.value * navSystem.pathfindingCooldown + 1;
        }

        public virtual SteeringOutput GetSteering()
        {
            SteeringOutput change = new SteeringOutput();
            SteeringOutput movement = new SteeringOutput(Vector2.zero);
            bool isSteering = false;

            if (steeringBehavior != null)
            {
                movement = steeringBehavior();//.GetBounded(MaxAcceleration, MaxRotation);
                isSteering = true;
            }

            // Get weighted steering
            if (defaultBehavior != null && (isSteering || !disableDefaultBehaviorOnIdle))
            {
                change = defaultBehavior.GetSteering(this);//.GetBounded(MaxAcceleration, MaxRotation);
                change *= defaultBehaviorWeight;
            }

            change += movement;

            change.SetBounds(MaxAcceleration, MaxRotation);

            return change;
        }

        public virtual void UpdateSteering(SteeringOutput change)
        {
            // Path finding
            if (pathfindingTime > 0)
                pathfindingTime -= Time.deltaTime;

            switch (pathfindingStrategy)
            {
                case PathfindingStrategy.None:
                    break;
                case PathfindingStrategy.SearchWhenStuck:
                    if (pathfindingTime <= 0 && // time for path
                        !waitingForPath && // when not already queueing for a path
                        steeringTask != null && // has target
                        steeringTask.ShouldFollow() && // when not reached goal
                        Stationary >= navSystem.stationaryPathingThreshold && // when agent is stationary enough
                        stationaryLastFrame < Stationary) // when agent is more stationary than last frame
                    {
                        waitingForPath = true;
                        pathfindingTime = navSystem.pathfindingCooldown;
                        navSystem.QueuePath(this, steeringTask.GetPosition());
                    }
                    break;
                case PathfindingStrategy.SearchEveryXSecond:
                    if (pathfindingTime <=0 && // time for path
                        !waitingForPath && // when not already queueing for a path
                        steeringTask != null && // has target
                        steeringTask.ShouldFollow() && // when not reached goal
                        Stationary >= navSystem.stationaryPathingThreshold && // when agent is stationary enough
                        stationaryLastFrame < Stationary) // when agent is more stationary than last frame
                    {
                        waitingForPath = true;
                        pathfindingTime = navSystem.pathfindingCooldown;
                        navSystem.QueuePath(this, steeringTask.GetPosition());
                    }
                    break;
            }

            stationaryLastFrame = Stationary;

            change.SetBounds(MaxAcceleration, MaxRotation);

            velocity += change.linear * Time.deltaTime;
            orientation += change.angular * Time.deltaTime;
            
            float currentSpeed = velocity.magnitude;

            if (currentSpeed > speed)
            {
                velocity = velocity.normalized * speed;
            }

            if (orientation > 360) orientation -= 360;
            if (orientation < 0) orientation += 360;

            if (change.linear == Vector2.zero)
            {
                velocity -= velocity * drag;
            }

            Stationary = Mathf.Clamp01(1 - currentSpeed / speed) * navSystem.agentMaxStationary;
        }

        public void SetBehavior<T>(ISteeringBehavior<T> behavior, T target)
        {
            this.steeringBehavior = delegate { return behavior.GetSteering(this, target); };
        }

        public void SetBehavior(Func<SteeringOutput> steeringTask)
        {
            this.steeringBehavior = steeringTask;
        }

        public void SetBehavior()
        {
            this.steeringBehavior = null;
        }

        public void SetTarget(Transform target, float distance = 0, bool hug = false, Action callback = null)
        {
            Task task = new TransformTask(this, target);

            SetTargetTask(task, target, distance, hug, callback);
        }

        public void SetTarget(Vector2 target, float distance = 0, bool hug = false, Action callback = null)
        {
            Task task = new PositionTask(this, target);

            SetTargetTask(task, target, distance, hug, callback);
        }

        public void SetTarget(SteeringAgent target, float distance = 0, bool hug = false, Action callback = null)
        {
            Task task = new AgentTask(this, target);

            SetTargetTask(task, target.transform, distance, hug, callback);
        }

        //public void SetTarget(IList<Vector2> target, float distance = 0, bool hug = false, Action callback = null)
        //{
        //    Task task = new PositionTask(this, )
        //}

        public void SetTarget()
        {
            hug = false;
            targetStopDistance = 0;
            callback = null;
            steeringTask = null;
            SetBehavior();
        }

        private void SetTargetTask(Task task, object t, float distance = 0, bool hug = false, Action callback = null)
        {
            if (t == null)
                throw new Exception("Target object is null.");

            this.hug = hug;
            this.targetStopDistance = distance;
            this.callback = callback;
            steeringTask = task;
            pathfindingTime = navSystem.pathfindingCooldown;

            if (t is Vector2)
            {
                SetBehavior(Seek.Create(), (Vector2)t);
            } else if (t is Transform)
            {
                SetBehavior(Seek.Create(), (Transform)t);
            } else
            {
                throw new System.Exception("Task target not implemented.");
            }
            
        }

        /// <summary>
        /// Determine if agent is stuck based on how stationary it is
        /// </summary>
        /// <returns></returns>
        public bool IsStuck()
        {
            return Stationary > stationaryLastFrame && Stationary >= navSystem.stationaryPathingThreshold;
        }

        private void OnEnable()
        {
            if (initialized)
                navSystem.AddAgent(this);
        }

        //private void OnDisable()
        //{
        //    if (initialized && !this.IsDestroyed())
        //    {
        //        initialized = false;
        //        navSystem.RemoveAgent(this);
        //    }
                
        //}

        private void OnDestroy()
        {
            if (initialized && !this.IsDestroyed())
            {
                initialized = false;
                navSystem.RemoveAgent(this);
            }
        }

        public void ConsumeJob(object data)
        {
            waitingForPath = false;
            PathfindingResult result = (PathfindingResult)data;

            if (result.path == null)
            {
                Debug.Log("No path!");
                return;
            }

            // Debug
            if (navSystem.debugging)
                navSystem.VisualizePath(result.path);
            //Debug.Log("Path length: " + result.path.Count);
            //Vector2 dir = result.path[1] - result.path[0];
            //Vector2 old = 
            //foreach (Vector2 v in result.path)
            //{
            //    Debug.Log("Path node: " + v);
            //}

            if (steeringTask == null)
            {
                SetTarget(result.goal);
            }

            SetBehavior(navSystem.steeringPath, result.path);
        }

        public void JobFailed()
        {
            waitingForPath = false;
        }
    }
}