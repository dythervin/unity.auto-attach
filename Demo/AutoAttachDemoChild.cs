using UnityEngine;
using UnityEngine.AI;

namespace Dythervin.AutoAttach.Demo
{
    public class AutoAttachDemoChild : MonoBehaviour
    {
        [Attach(Attach.Parent)]
        public NavMeshAgent agent;
    }
}