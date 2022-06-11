using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Dythervin.AutoAttach.Demo
{
    public class AutoAttachDemo : MonoBehaviour
    {
        [AttachOrAdd]
        public NavMeshAgent agent;

        [Attach(Attach.Child)]
        [SerializeField]
        private Renderer[] rendererArray;

        [Attach(Attach.Child, false)]
        [SerializeField]
        private List<Collider> rendererList;

        [Attach(Attach.Child)]
        [SerializeField]
        private List<MeshFilter> meshFilterList;

        [Attach(Attach.Scene)]
        [SerializeField] private Camera anyCamera;

        [Attach(Attach.Scene)]
        [SerializeField] private Light[] allLights;
    }
}