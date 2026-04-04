using UnityEngine;

namespace CellularAutomata.UI
{
    public class GenerateCaBtn : MonoBehaviour
    {
        public CaGenerator[] generators;
        public DropdownCa dropdown;
        public CaTerrain terrain;

        public void Generate()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;
            
            switch (dropdown.activeIndex)
            {
                case 0:
                    generators[0].Generate();
                    break;
                case 1:
                    generators[1].Generate();
                    break;
                case 2:
                    generators[2].Generate();
                    break;
                case 3:
                    generators[3].Generate();
                    break;
                case 4:
                    generators[4].Generate();
                    break;
                case 5:
                    generators[5].Generate();
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
                    generators[0].ClearParentContext();
                    break;
                case 1:
                    generators[1].ClearParentContext();
                    break;
                case 2:
                    generators[2].ClearParentContext();
                    break;
                case 3:
                    generators[3].ClearParentContext();
                    break;
                case 4:
                    generators[4].ClearParentContext();
                    break;
                case 5:
                    terrain.ResetTerrain();
                    break;
            }
        }
    }
}