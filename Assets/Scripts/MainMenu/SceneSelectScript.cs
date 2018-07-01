using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelectScript : MonoBehaviour {
  public void selectScene(){
    switch(this.gameObject.name){
      case "ForestBtn":
        SceneManager.LoadScene("forest_scene");
        break;
      case "DarkBtn":
        SceneManager.LoadScene("dark_scene");
        break;
      default:
        SceneManager.LoadScene("main_menu");
        break;
    }
  }
}
