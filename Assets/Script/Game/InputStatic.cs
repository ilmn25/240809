using System;
using UnityEngine;

namespace Script.Game
{
    public class InputStatic : MonoBehaviour
    {
        public static event Action Jump; 
        public static event Action up; 
        public static event Action down; 
        public static event Action right; 
        public static event Action left; 
    }
}