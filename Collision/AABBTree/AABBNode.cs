//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public abstract class AABBNode : IAABB
//{
//    public Vector2 max;
//    public Vector2 min;

    

    

//    public AABBNode(IAABB element)
//    {
//        this.element = element;
//    }

//    public abstract void Insert(IAABB collider);

//    public bool Envelops(IAABB collider)
//    {
//        return MinPosition.x < collider.MinPosition.x && MaxPosition.x > collider.MaxPosition.x &&
//            MinPosition.y < collider.MinPosition.y && MaxPosition.y > collider.MaxPosition.y;
//    }

//    public Vector2 MinPosition => min;

//    public Vector2 MaxPosition => max;

//    public Vector2 Position => throw new System.NotImplementedException();
//}

//public class AABBBranch: AABBNode
//{
//    public AABBNode leftChild;
//    public AABBNode rightChild;

//    public AABBBranch(AABBNode left, AABBNode right)
//    {
//        leftChild = left;
//        rightChild = right;
//    }

//    public override void Insert(IAABB collider)
//    {
//        if (Envelops(collider))
//        {
//            if (leftChild != null && leftChild.Envelops(collider))
//            {
//                leftChild.Insert(collider);
//            }
//            else if (rightChild != null && rightChild.Envelops(collider))
//            {
//                rightChild.Insert(collider);
//            }
//            else
//            {
//                if ((leftChild.Position - collider.Position).magnitude < (rightChild.Position - collider.Position).magnitude)
//                {
//                    leftChild
//                }
//            }
//        }

//    }
//}

//public class AABBLeaf: AABBNode
//{
//    public IAABB element;

//    public override void Insert(IAABB collider)
//    {
//        throw new System.Exception("Leaf already has an element");
//    }
//}