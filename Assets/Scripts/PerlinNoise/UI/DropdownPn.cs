using UnityEngine;

namespace PerlinNoise.UI
{
    public class DropdownPn : MonoBehaviour
    {
        public GameObject[] generators;
        public int activeIndex;

        public void Dropdown(int index)
        {
            activeIndex = index;
            switch (index)
            {
                case 0:
                    foreach (GameObject gen in generators)
                    {
                        gen.SetActive(false);
                    }

                    generators[0].SetActive(true);
                    break;
                case 1:
                    foreach (GameObject gen in generators)
                    {
                        gen.SetActive(false);
                    }

                    generators[1].SetActive(true);
                    break;
                case 2:
                    foreach (GameObject gen in generators)
                    {
                        gen.SetActive(false);
                    }

                    generators[2].SetActive(true);
                    break;
                case 3:
                    foreach (GameObject gen in generators)
                    {
                        gen.SetActive(false);
                    }

                    generators[3].SetActive(true);
                    break;
                case 4:
                    foreach (GameObject gen in generators)
                    {
                        gen.SetActive(false);
                    }

                    generators[4].SetActive(true);
                    break;
                case 5:
                    foreach (GameObject gen in generators)
                    {
                        gen.SetActive(false);
                    }

                    generators[5].SetActive(true);
                    break;
            }
        }
    }
}