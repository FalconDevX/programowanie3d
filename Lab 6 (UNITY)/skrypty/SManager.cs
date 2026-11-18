using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SManager : MonoBehaviour
{
   public void IdzDoSceny()
    {
        
        SceneManager.LoadScene (1);
    }
    
    public void Wyjdz()
    {
        Application.Quit();
    }

    public void IdzInstrukca()

    {
         SceneManager.LoadScene (2);  
    }

     public void IdzMenu()

    {
         SceneManager.LoadScene (0);  
    }

}
