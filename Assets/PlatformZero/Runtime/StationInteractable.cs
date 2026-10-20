#if PLATFORM_ZERO_YARN
using UnityEngine;

namespace PlatformZero
{
    public sealed class StationInteractable : MonoBehaviour
    {
        [SerializeField] private string node;
        [SerializeField] private string prompt;
        public string Node => node;
        public string Prompt => prompt;
        public void Configure(string yarnNode, string label) { node = yarnNode; prompt = label; }
    }
}

#endif
