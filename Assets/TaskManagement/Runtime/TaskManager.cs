using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTaskManagement
{
    public class TaskManager : TaskGroup
    {
        public class TaskState
        {
            public delegate void FinishedHandler(bool manual);

            private IEnumerator _coroutine;

            private Coroutine _runningCoroutine;

            private bool _running;

            private bool _paused;

            private bool _stopped;

            private TaskGroup _owner;

            public bool Running
            {
                get { return _running; }
            }

            public bool Paused
            {
                get { return _paused; }
            }

            public event FinishedHandler Finished;

            public TaskState(IEnumerator c, TaskGroup coroutineOwner)
            {
                _coroutine = c;
                _owner = coroutineOwner;
            }

            public void Pause()
            {
                _paused = true;
            }

            public void Unpause()
            {
                _paused = false;
            }

            public Coroutine Start()
            {
                if (TaskManager.Paused)
                {
                    return _runningCoroutine;
                }
                _running = true;
                _runningCoroutine = _owner.StartCoroutine(CallWrapper());
                return _runningCoroutine;
            }

            public void Stop()
            {
                if(_owner != null && _runningCoroutine != null)
                    _owner.StopCoroutine(_runningCoroutine);
                _stopped = true;
                _running = false;
            }

            private IEnumerator CallWrapper()
            {
                while (_running)
                {
                    if (_paused)
                    {
                        yield return null;
                    }

                    yield return _coroutine;
                    _running = false;
                }

                if (Finished != null)
                {
                    Finished(_stopped);
                }
            }
        }

        private static TaskGroup _defaultGroup;

        private static Dictionary<string, TaskGroup> _createdGroups = new Dictionary<string, TaskGroup>();

        private readonly static string DEFAULT_GROUP_ID = "DEFAULT_GROUP";

        public readonly static string UI_GROUP_ID = "UI";
        
        /// <summary>
        /// Check default TaskGroup existed,if not create one
        /// </summary>
        private static void CreateDefault()
        {
            if (_defaultGroup == null)
            {
                GameObject gameObject = new GameObject("TaskManager");
                _defaultGroup = gameObject.AddComponent<TaskManager>();
                _defaultGroup.ID = DEFAULT_GROUP_ID;
                
                _createdGroups.Add(DEFAULT_GROUP_ID,_defaultGroup);
                DontDestroyOnLoad(gameObject);
            }
        }

        private static bool _paused;

        public static bool Paused
        {
            get { return _paused; }
        }

        public static void Pause()
        {
            _paused = true;
        }

        public static void Resume()
        {
            _paused = false;
        }

        public static void Reset()
        {
            StopAllTasksInGroup(_defaultGroup.ID);
            foreach (string key in _createdGroups.Keys)
            {
                StopAllTasksInGroup(key);
            }

            Destroy(_defaultGroup.gameObject);
            _defaultGroup = null;
            _createdGroups = new Dictionary<string, TaskGroup>();
        }

        public static TaskState CreateTask(IEnumerator coroutine)
        {
            CreateDefault();
            return new TaskState(coroutine, _defaultGroup);
        }

        public static TaskState CreateTask(IEnumerator coroutine, string groupID)
        {
            CreateDefault();
            TaskGroup value;
            if (!_createdGroups.TryGetValue(groupID, out value))
            {
                value = _defaultGroup.gameObject.AddComponent<TaskGroup>();
                value.ID = groupID;
                _createdGroups.Add(groupID, value);
            }

            return new TaskState(coroutine, value);
        }

        public static void StopAllTasksInGroup(string groupID = "DEFAULT_GROUP")
        {
            TaskGroup value;
            if (_createdGroups.TryGetValue(groupID, out value))
            {
                value.StopAllCoroutines();
            }
        }
    }
}