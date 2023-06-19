using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/Combine")]
    public class CombineTerrainObjects : TerrainNode
    {
        public enum CombineStrategy
        {
            None,
            Collision
        }

        public CombineStrategy strategy;


        InputLink<List<TerrainObject>> input1;
        InputLink<List<TerrainObject>> input2;

        OutputLink<List<TerrainObject>> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            input1 = AddInputLink<List<TerrainObject>>();
            input2 = AddInputLink<List<TerrainObject>>();

            output = AddOutputLink(Evaluate, portName: "TerrainObjects");
        }

        public List<TerrainObject> Evaluate()
        {
            List<TerrainObject> in1 = input1.Evaluate();
            List<TerrainObject> in2 = input2.Evaluate();

            List<TerrainObject> retval = new List<TerrainObject>(in1);

            if (in2.Count == 0)
                return in1;

            if (in1.Count == 0)
                return in2;

            switch (strategy)
            {
                case CombineStrategy.None:
                    foreach(TerrainObject obje in in2)
                    {
                        retval.Add(obje);
                    }
                    return retval;
                case CombineStrategy.Collision:
                    foreach (TerrainObject a in in2)
                    {
                        foreach (TerrainObject b in in1)
                        {
                            if ((a.position - b.position).sqrMagnitude >= (a.radius + b.radius) * (a.radius + b.radius))
                            {
                                // No collision!
                                retval.Add(a);
                            }
                        }
                    }
                    return retval;
            }

            

            return retval;
        }

        public override Node Clone()
        {
            CombineTerrainObjects retval = base.Clone() as CombineTerrainObjects;

            retval.strategy = strategy;

            return retval;
        }
    }
}