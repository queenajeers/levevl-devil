using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
   [SerializeField] Animator trapAnimator;


   public void Activate()
   {
      trapAnimator.Play("activate", 0, 0);
   }

}
