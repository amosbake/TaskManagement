using System.Collections;
using UnityEngine;

namespace UnityTaskManagement
{
    public class Task
    {
        public delegate void FinishedHandler(bool manual);

        private TaskManager.TaskState _task;
        
        public bool Running
        {
            get
            {
                return _task.Running;
            }
        }

        public bool Paused
        {
            get
            {
                return _task.Paused;
            }
        }

        public Coroutine coroutine
        {
            get;
            private set;
        }
        
        public event FinishedHandler Finished;
        
        public Task(IEnumerator c, bool autoStart = true)
        {
            _task = TaskManager.CreateTask(c);
            _task.Finished += TaskFinished;
            if (autoStart)
            {
                Start();
            }
        }

        public void Start()
        {
            coroutine = _task.Start();
        }

        public void Stop()
        {
            _task.Stop();
        }

        public void Pause()
        {
            _task.Pause();
        }

        public void Unpause()
        {
            _task.Unpause();
        }

        private void TaskFinished(bool manual)
        {
            FinishedHandler finished = this.Finished;
            if (finished != null)
            {
                finished(manual);
            }
        }
    }
}