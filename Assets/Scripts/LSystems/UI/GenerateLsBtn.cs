using UnityEngine;

namespace LSystems.UI
{
    public class GenerateLsBtn : MonoBehaviour
    {
        public LsTreeGen treeGen;
        public LsTreeRender treeRender;
        public LsGenerator[] generators;
        public DropdownLs dropdown;

        public void Generate()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;
            
            switch (dropdown.activeIndex)
            {
                case 0:
                    treeGen.Generate();
                    break;
                case 1:
                    generators[0].Generate();
                    break;
                case 2:
                    generators[1].Generate();
                    break;
                case 3:
                    generators[2].Generate();
                    break;
                case 4:
                    generators[3].Generate();
                    break;
            }
        }
        
        public void Reset()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;
            
            switch (dropdown.activeIndex)
            {
                case 0:
                    treeRender.ClearTree();
                    break;
                case 1:
                    generators[0].ClearParentContext();
                    break;
                case 2:
                    generators[1].ClearParentContext();
                    break;
                case 3:
                    generators[2].ClearParentContext();
                    break;
                case 4:
                    generators[3].ClearParentContext();
                    break;
            }
        }
    }
}