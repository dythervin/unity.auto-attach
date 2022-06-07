using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Dythervin.AutoAttach.Demo
{
    public class AutoAttachDemo : MonoBehaviour
    {
        [AttachOrAdd]
        public NavMeshAgent agent;

        [Attach(Attach.Children, false)]
        [SerializeField]
        private Renderer[] rendererArray;

        [Attach(Attach.Children)]
        [SerializeField]
        private List<Collider> rendererList;
        
        [Attach(Attach.Children)]
        [SerializeField]
        private List<MeshFilter> meshFilterList;
    }
}