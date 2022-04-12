using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class all_img_script : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void handle_click(int i){
        GameObject.Find("Main Camera").GetComponent<search_script>().hit_test(i);
    }
}
