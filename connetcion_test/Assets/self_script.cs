using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using System.IO;

[Serializable]
class userid_data{
    // public string created_on;
    public string imdb;
    // public List<float> relevance;
    public string src;
    // public List<string> tags;
}

[Serializable]
class all_data{
    public List<userid_data> datas;
}


public class self_script : MonoBehaviour
{
    private string posturl="http://182.92.124.204:5666/search_by_userid";

    // private Image image;

    public GameObject img,btn,btn1,btn2,txt,btn3;

    private string img_url="";

    // private string prefix="https://speechdb-oss-image.oss-cn-beijing.aliyuncs.com/";

    private int nowpage = 0;

    private List<userid_data> url_lst;

    // Start is called before the first frame update
    void Start()
    {
        btn1 = GameObject.Find("tologout");
        btn1.GetComponent<Button>().onClick.AddListener(handle_click1);

        txt = GameObject.Find("hello");
        txt.GetComponent<Text>().text = "Hello!"+PlayerPrefs.GetString("name");

        btn = GameObject.Find("tosearch");
        btn.GetComponent<Button>().onClick.AddListener(handle_click);

        btn2 = GameObject.Find("next10item");
        btn2.GetComponent<Button>().onClick.AddListener(handle_click2);

        btn3 = GameObject.Find("recommend");
        btn3.GetComponent<Button>().onClick.AddListener(recommend);
        
        print("getting data...");
        StartCoroutine(Post());

    }
    public void recommend(){
        StartCoroutine(rec());
    }
    IEnumerator rec()
    {
        int someuserid = PlayerPrefs.GetInt("userid");
        List<IMultipartFormSection> iparams = new List<IMultipartFormSection>();
        iparams.Add(new MultipartFormDataSection("userid",someuserid.ToString()));
        UnityWebRequest request = UnityWebRequest.Post("http://182.92.124.204:5666/rec_by_userid",iparams);
        request.SetRequestHeader("Authorization","Bearer "+PlayerPrefs.GetString("access_token"));
        yield return request.SendWebRequest();
        if(request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string receiveContent = request.downloadHandler.text;
            string new_content = "{\"datas\":"+receiveContent+"}";
            print(new_content);

            all_data myfile = new all_data();
            JsonUtility.FromJsonOverwrite(new_content, myfile);

            this.url_lst = myfile.datas;
            print("data length:"+this.url_lst.Count);

            StartCoroutine(getimg());
        }
    }
    public void handle_click(){
        SceneManager.LoadScene("search");
    }
    public void handle_click1(){
        PlayerPrefs.DeleteKey("userid");
        PlayerPrefs.DeleteKey("name");
        PlayerPrefs.DeleteKey("access_token");
        SceneManager.LoadScene("login");
    }
    public void handle_click2(){
        print("nextpage");
        this.nowpage+=1;
        if(this.nowpage > this.url_lst.Count/10){
            this.nowpage=0;
        }
        StartCoroutine(getimg());
    }
    IEnumerator getimg(){
        print("now page:"+this.nowpage);
        for(int i=1;i<=10;i++){
            int block = this.nowpage*10+i-1;
            if(block == this.url_lst.Count){
                break;
            }
            img = GameObject.Find("Image"+i.ToString());
            WWW www = new WWW(this.url_lst[block].src);
            yield return www;
            if (www!=null && string.IsNullOrEmpty(www.error)){
                Texture2D texture = www.texture;
                //创建 Sprite
                Sprite sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(0.5f,0.5f) );
                img.GetComponent<Image>().sprite = sprite;
            }
        }
    }

    // unfinished
    public void hit_test(int i){
        int block = this.nowpage*10+i;
        if(block>=this.url_lst.Count&&this.nowpage>0){
            block=block-10;
        }
        else if(block>=this.url_lst.Count&&this.nowpage==0){
            return;
        }
        print("block"+block);
        print("page"+this.nowpage);
        string web_url="";
        if(this.url_lst[block].imdb.Length==5){
            web_url="00"+this.url_lst[block].imdb;
        }
        else if(this.url_lst[block].imdb.Length==6){
            web_url="0"+this.url_lst[block].imdb;
        }
        else{
            web_url=this.url_lst[block].imdb;
        }
        print(web_url);
        Application.OpenURL("https://www.imdb.com/title/tt"+web_url);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Post()
    {
        int someuserid = PlayerPrefs.GetInt("userid");
        List<IMultipartFormSection> iparams = new List<IMultipartFormSection>();
        iparams.Add(new MultipartFormDataSection("userid",someuserid.ToString()));
        UnityWebRequest request = UnityWebRequest.Post(posturl,iparams);
        request.SetRequestHeader("Authorization","Bearer "+PlayerPrefs.GetString("access_token"));
        yield return request.SendWebRequest();
        if(request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string receiveContent = request.downloadHandler.text;
            string new_content = "{\"datas\":"+receiveContent+"}";
            print(new_content);

            all_data myfile = new all_data();
            JsonUtility.FromJsonOverwrite(new_content, myfile);

            this.url_lst = myfile.datas;
            print("data length:"+this.url_lst.Count);

            StartCoroutine(getimg());
        }
    }
}
