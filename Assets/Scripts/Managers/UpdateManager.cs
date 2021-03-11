using System;
using UnityEngine;
using NaughtyAttributes;

namespace Sjouke
{
    public class UpdateManager : MonoBehaviour
    {
        public enum UpdateType { Update, FixedUpdate }

        public static UpdateManager Instance { get; private set; }

        public UpdateType UpdateLoop = UpdateType.Update;

        [EnableIf(nameof(IsUsingFixedUpdate))]
        public float FixedTimeStep = 1 / 60;

        private bool _useUpdate;
        private bool IsUsingFixedUpdate => UpdateLoop == UpdateType.FixedUpdate;

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(Instance.gameObject);
                Debug.LogWarning("More than one UpdateManager instance found!");
            }
            Instance = this;

            _useUpdate = !IsUsingFixedUpdate;

            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            SetUpdateLoop(!_useUpdate);

            if (!_useUpdate && !Physics.autoSimulation)
                Debug.LogError("Physics.autoSimulation is false while Update Type is set to FixedUpdate!");
        }

        private void Update()
        {
            if (!_useUpdate)
                return;

            Physics.Simulate(Time.deltaTime);
        }

        public void SetUpdateLoop(bool useFixedUpdate)
        {
            UpdateLoop = useFixedUpdate ? UpdateType.FixedUpdate : UpdateType.Update;
            _useUpdate = !IsUsingFixedUpdate;
            Physics.autoSimulation = !_useUpdate;

            // Debug.Log($"Set Update loop to loopType {Convert.ToInt32(IsUsingFixedUpdate) + 1}.");
        }
    }
}