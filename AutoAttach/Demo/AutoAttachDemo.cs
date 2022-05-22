using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Dythervin.AutoAttach.Demo
{
    public class AutoAttachDemo : MonoBehaviour
    {
        [AutoAttach]
        public NavMeshAgent agent;

        [AutoAttach(AutoAttachType.Children)]
        [SerializeField]
        private Renderer[] rendererArray;

        [AutoAttach(AutoAttachType.Children)]
        [SerializeField]
        private List<Collider> rendererList;
        
        [AutoAttach(AutoAttachType.Children)]
        [SerializeField]
        private List<MeshFilter> meshFilterList;
    }
}