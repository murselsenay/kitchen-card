using System.Collections.Generic;
using UnityEngine;

namespace Game.Views
{
    public class DifficultyStarsView : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _filledStars = new List<GameObject>();

        public void SetStarCount(int filledStarCount)
        {
            for (int i = 0; i < _filledStars.Count; i++)
            {
                if (_filledStars[i] == null) continue;
                _filledStars[i].SetActive(i < filledStarCount);
            }
        }
    }
}