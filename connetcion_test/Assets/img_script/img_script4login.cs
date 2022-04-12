using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class img_script4login : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void handle_click(){
        GameObject.Find("Main Camera").GetComponent<login_script>().handle_click_img();
    }
}
