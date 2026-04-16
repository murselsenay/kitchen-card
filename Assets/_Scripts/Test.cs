using Game.Core;
using UnityEngine;

public class Test : MonoBehaviour
{
    async void Start()
    {
        await GameBootstrap.InitializeAsync();
    }
}
