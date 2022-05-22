# Unity auto attach
Auto attach components to serialized fields.

Attaches components in editor when any MonoBehaviour selected, removed or added. 
Attaches only once, so there is almost no performance impact.

Component, components array and component list are supported. For other types you can implement abstract AutoSetter class.

Examples:

```c#
    public class AutoAttachDemo : MonoBehaviour
    {
        [AutoAttach] //Get component on current gameObject
        public NavMeshAgent agent;
        
        [AutoAttach(AutoAttachType.Parent)] //Get component in parent gameObjects
        public Collider colliderInParent;
        
        [AutoAttach(AutoAttachType.Children)] //Get component in children gameObjects
        [SerializeField]
        private Collider colliderInChildren;

        [AutoAttach(AutoAttachType.Children)] //Get components in children gameObjects
        [SerializeField]
        private Renderer[] rendererArray;

        [AutoAttach(AutoAttachType.Parent)] //Get components in parent gameObjects
        [SerializeField]
        private List<Collider> colliderList;
        
        [AutoAttach(AutoAttachType.Children)] //Get components in children gameObjects
        [SerializeField]
        private List<MeshFilter> meshFilterList;
        
        
        
        [AutoAdd] //Get component on current gameObject or add if not exist (similar to RequireComponent)
        public NavMeshAgent agent;
    }
```
