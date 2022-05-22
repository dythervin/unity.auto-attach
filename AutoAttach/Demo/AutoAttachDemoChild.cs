using UnityEngine;
using UnityEngine.AI;

namespace Dythervin.AutoAttach.Demo
{
    public class AutoAttachDemoChild : MonoBehaviour
    {
        [AutoAttach(AutoAttachType.Parent)]
        public NavMeshAgent agent;
    }
}