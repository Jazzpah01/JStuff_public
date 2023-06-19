using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionObserver : MonoBehaviour
{
    public delegate void CollisionObservation(Collider other);
    private CollisionObservation enterObservation;
    private CollisionObservation exitObservation;
    private CollisionObservation stayObservation;

    private Collider collider;

    /// <summary>
    /// Get the collider that this script is observing.
    /// </summary>
    public Collider Collider => collider;

    public enum CollisionType
    {
        Enter,
        Exit,
        Stay
    }

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == transform.parent)
            return;

        if (enterObservation != null) enterObservation(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == transform.parent)
            return;

        if (exitObservation != null) exitObservation(other);
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.transform == transform.parent)
    //        return;

    //    if (stayObservation != null) stayObservation(other);
    //}

    /// <summary>
    /// Subscribe a function to the observer. When this script observes a collision, it will called the subscribed function.
    /// </summary>
    /// <param name="observerMethod"></param>
    /// <param name="type"></param>
    public void Subscribe(CollisionObservation observerMethod, CollisionType type)
    {
        switch (type)
        {
            case CollisionType.Enter:
                enterObservation += observerMethod;
                break;
            case CollisionType.Exit:
                exitObservation += observerMethod;
                break;
            case CollisionType.Stay:
                stayObservation += observerMethod;
                break;
        }
    }

    private void OnDestroy()
    {
        enterObservation = null;
        exitObservation = null;
        stayObservation = null;
    }
}