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
class userid_data2{
    // public string created_on;
    public string imdb;
    // public List<float> relevance;
    public string src;
    // public List<string> tags;
}

[Serializable]
class all_data2{
    public List<userid_data2> datas;
}

public class search_script : MonoBehaviour
{
    private string posturl="http://182.92.124.204:5666/search_by_all";
    private string posturl2="http://182.92.124.204:5666/search_by_userid";


    // private Image image;

    public GameObject img,inp,lst,btn,btn1,btn2,lst2;

    private string[] genre_list = new string[] {"","Action","Adventure","Animation","Children","Comedy","Crime",
                                                "Documentary","Drama","Fantasy","Film-Noir","Horror","Musical",
                                                "Mystery","Romance","Sci-Fi","Thriller","War","Western"};

    private string img_url="";

    // private string prefix="https://speechdb-oss-image.oss-cn-beijing.aliyuncs.com/";

    private int gender_select=0;
    private string genre_select="";
    private string keyword="";

    private int nowpage = 0;

    private List<userid_data2> url_lst;

    // Start is called before the first frame update
    void Start()
    {
        btn1 = GameObject.Find("Button1");
        btn1.GetComponent<Button>().onClick.AddListener(handle_click1);

        inp = GameObject.Find("InputField");
        inp.GetComponent<InputField>().onValueChanged.AddListener (value_change);
        inp.GetComponent<InputField>().onEndEdit.AddListener (handle_enter);

        lst = GameObject.Find("Dropdown");
        lst.GetComponent<Dropdown>().onValueChanged.AddListener (handle_select);

        lst2 = GameObject.Find("Dropdown2");
        lst2.GetComponent<Dropdown>().onValueChanged.AddListener (handle_select2);

        btn = GameObject.Find("Button");
        btn.GetComponent<Button>().onClick.AddListener(handle_click);

        btn2 = GameObject.Find("Button2");
        btn2.GetComponent<Button>().onClick.AddListener(handle_click2);
        
        StartCoroutine(Post());

    }
    // unfinished
    public void handle_click(){
        // print("searching...");
        // print("keyword:"+this.keyword+"genre:"+this.genre_select+"gender:"+this.gender_select);
        StartCoroutine(Post());
    }
    public void handle_click1(){
        SceneManager.LoadScene("self");
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
    public void handle_select(int i){
        this.genre_select=this.genre_list[i];
    }
    public void handle_select2(int i){
        this.gender_select=i;
    }
    public void value_change(string s){
        this.keyword=s;
    }
    public void handle_enter(string s){
        this.keyword=s;
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
        // print("searching...");
        // print("keyword:"+this.keyword+"genre:"+genre_list[this.gender_select]+"gender:"+this.gender_select);
        UnityWebRequest request;
        if(this.keyword==""&&this.gender_select==0&&this.genre_select==""){
            print("search by userid");
            int someuserid = PlayerPrefs.GetInt("userid");
            List<IMultipartFormSection> iparams = new List<IMultipartFormSection>();
            iparams.Add(new MultipartFormDataSection("userid",someuserid.ToString()));
            request = UnityWebRequest.Post(posturl2,iparams);
            
        }
        else{
            print("search by all");

            WWWForm myform = new WWWForm();
            myform.AddField("genre",this.genre_select);
            myform.AddField("gender",this.gender_select);
            myform.AddField("keyword",this.keyword);

            request = UnityWebRequest.Post(posturl,myform);
        }
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

            all_data2 myfile = new all_data2();
            JsonUtility.FromJsonOverwrite(new_content, myfile);
            
            this.url_lst = myfile.datas;
            print("data length:"+this.url_lst.Count);

            StartCoroutine(getimg());
        }
    }
}
