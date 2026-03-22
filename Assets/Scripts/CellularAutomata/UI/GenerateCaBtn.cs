using UnityEngine;

namespace CellularAutomata.UI
{
    public class GenerateCaBtn : MonoBehaviour
    {
        public CaGenerator[] generators;
        public DropdownCa dropdown;

        public void Generate()
        {
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
    }
}