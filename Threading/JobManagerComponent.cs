using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Threading
{
    public class JobManagerComponent : MonoBehaviour
    {
        public JobManagerData managerData;
        public JobManager manager;
        public static JobManagerComponent instance;

        [Min(1)]public int updateDelay = 1;
        private int delayer = 0;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            manager = new JobManager();

            manager.Initialize(managerData);
        }

        // Update is called once per frame
        void Update()
        {
            delayer++;
            if (delayer >= updateDelay)
            {
                delayer = 0;
                manager.Update();
            }
        }

        private void OnDestroy()
        {
            manager.Remove();
        }

        private void OnApplicationQuit()
        {
            manager.Remove();
        }
    }
}