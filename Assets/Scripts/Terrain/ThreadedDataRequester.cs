using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour {

    private static ThreadedDataRequester instance;
    private Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    public static void RequestData(Func<object> i_generateData, Action<object> callBack)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataThread(i_generateData, callBack);
        };

        new Thread(threadStart).Start();
    }

    private void DataThread(Func<object> i_generateData, Action<object> callBack)
    {
        object l_data = i_generateData();
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callBack, l_data));
        }
    }

    private void Update()
    {
        // Multithreading optimization.
        if (dataQueue.Count > 0)
        {
            // If theres something in the queue we want to make sure it gets executed.

            while (dataQueue.Count > 0)
            {
                lock(dataQueue)
                {
                    ThreadInfo threadInfo = dataQueue.Dequeue();
                    // When this process is done we want to invoke any callbacks that have been registered.
                    threadInfo.callBack(threadInfo.paramiter);
                }
            }
        }
    }

    private struct ThreadInfo
    {
        public readonly Action<object> callBack;
        public readonly object paramiter;

        public ThreadInfo(Action<object> callBack, object paramiter)
        {
            this.callBack = callBack;
            this.paramiter = paramiter;
        }
    }
}
