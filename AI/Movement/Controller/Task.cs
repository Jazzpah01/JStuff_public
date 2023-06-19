using JStuff.AI.Steering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using JStuff.Utilities;

public abstract class Task
{
    public abstract bool ShouldFollow();
    public abstract Vector2 GetPosition();
}

public class PositionTask: Task
{
    SteeringAgent agent;
    Vector2 target;

    public PositionTask(SteeringAgent agent, Vector2 target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override Vector2 GetPosition()
    {
        return target;
    }

    public override bool ShouldFollow()
    {
        if ((agent.Position - target).magnitude - agent.radius <= 0)
        {
            return false;
        }
        return true;
    }
}

public class AgentTask : Task
{
    SteeringAgent agent;
    SteeringAgent target;

    public AgentTask(SteeringAgent agent, SteeringAgent target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override Vector2 GetPosition()
    {
        return target.Position;
    }

    public override bool ShouldFollow()
    {
        if (target.IsDestroyed() || (agent.Position - target.Position).magnitude - agent.radius - target.radius <= 0)
        {
            return false;
        }
        return true;
    }
}

public class TransformTask : Task
{
    SteeringAgent agent;
    Transform target;

    public TransformTask(SteeringAgent agent, Transform target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override Vector2 GetPosition()
    {
        return (agent.Up == UpAxis.Z) ? target.position.xy() : target.position.xz();
    }

    public override bool ShouldFollow()
    {
        Vector2 pos = (agent.Up == UpAxis.Z) ? target.position.xy() : target.position.xz();

        if ((agent.Position - pos).magnitude - agent.radius <= 0)
        {
            return false;
        }
        return true;
    }
}

public class PathTask : Task
{
    SteeringAgent agent;
    IList<Vector2> target;

    public PathTask(SteeringAgent agent, IList<Vector2> target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override Vector2 GetPosition()
    {
        return target[target.Count - 1];
    }

    public override bool ShouldFollow()
    {
        Vector2 finalTarget = this.target[this.target.Count - 1];
        if ((agent.Position - finalTarget).magnitude - agent.radius <= 0)
        {
            return false;
        }
        return true;
    }
}