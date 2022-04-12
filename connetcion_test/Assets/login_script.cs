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
class login_data
{
    public string csrf_token;
    public string pubkey;
}
[Serializable]
class captcha_data
{
    public string captcha_name;
}
[Serializable]
class param
{
    public string username;
    public string password;
    public string captcha;
    public string id;
    public string csrf_token;
}
[Serializable]
class login_result
{
    public string access_token;
    public int id;
}

public class login_script : MonoBehaviour
{
    public GameObject btn,inp1,inp2,inp3,img;

    private string posturl="http://182.92.124.204:5666/api";

    private string usr,pwd,cta;

    private string csrf,key,captcha_url;

    // Start is called before the first frame update
    void Start()
    {
        btn = GameObject.Find("Button");
        btn.GetComponent<Button>().onClick.AddListener(handle_click);

        inp1 = GameObject.Find("username");
        inp1.GetComponent<InputField>().onValueChanged.AddListener (value_change1);
        inp1.GetComponent<InputField>().onEndEdit.AddListener (handle_enter1);
        
        inp2 = GameObject.Find("passwd");
        inp2.GetComponent<InputField>().onValueChanged.AddListener (value_change2);
        inp2.GetComponent<InputField>().onEndEdit.AddListener (handle_enter2);

        inp3 = GameObject.Find("captcha");
        inp3.GetComponent<InputField>().onValueChanged.AddListener (value_change3);
        inp3.GetComponent<InputField>().onEndEdit.AddListener (handle_enter3);

        StartCoroutine(Post());

    }

    IEnumerator Post()
    {
        
        UnityWebRequest request = UnityWebRequest.Get(posturl);
        yield return request.SendWebRequest();
        if(request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string receiveContent = request.downloadHandler.text;
            // byte[] datas = Encoding.UTF8.GetBytes(receiveContent);
            // string resultstr = Encoding.UTF8.GetString(datas);
            // print(resultstr);

            login_data myfile = new login_data();
            JsonUtility.FromJsonOverwrite(receiveContent, myfile);
            
            this.csrf = myfile.csrf_token;
            this.key = myfile.pubkey;
            
            Debug.Log("csrf token:"+this.csrf+"pubkey:"+this.key);

            
            StartCoroutine(getcaptcha());
        }
    }
    IEnumerator getcaptcha(){
        float id = UnityEngine.Random.Range(0f,0.5f);
        print(id.ToString());
        List<IMultipartFormSection> iparams = new List<IMultipartFormSection>();
        iparams.Add(new MultipartFormDataSection("id", id.ToString()));
        iparams.Add(new MultipartFormDataSection("pubkey", this.key));

        // WWWForm iparams = new WWWForm();
        // iparams.AddField("id",id.toString(),Encoding.UTF8);
        // iparams.AddField("pubkey",this.key);

        UnityWebRequest request2 = UnityWebRequest.Post("http://182.92.124.204:5666/captcha",iparams);
        yield return request2.SendWebRequest();
        if(request2.isHttpError || request2.isNetworkError)
        {
            Debug.LogError(request2.error);
        }
        else
        {
            string receiveContent = request2.downloadHandler.text;
            // byte[] datas = request2.downloadHandler.data;
            // string resultstr = Encoding.GetEncoding("GB2312").GetString(datas);
            // print(resultstr);//"static/0.5574529574237279.png"

            captcha_data myfile = new captcha_data();
            JsonUtility.FromJsonOverwrite(receiveContent, myfile);
            
            this.captcha_url = myfile.captcha_name;

            print(this.captcha_url);
    
            StartCoroutine(getcaptchaimg());
        }
    }
    IEnumerator getcaptchaimg(){
        img = GameObject.Find("Image");
        WWW www = new WWW("http://182.92.124.204:5666/" + this.captcha_url);
        yield return www;
        if (www!=null && string.IsNullOrEmpty(www.error)){
            Texture2D texture = www.texture;
            //创建 Sprite
            Sprite sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(0.5f,0.5f) );
            img.GetComponent<Image>().sprite = sprite;
        }
        else{
            print("error");
        }
    }
    public void handle_click_img(){
        StartCoroutine(getcaptcha());
    }
    IEnumerator begin_login(){

        List<IMultipartFormSection> iparams = new List<IMultipartFormSection>();
        iparams.Add(new MultipartFormDataSection("username", this.usr));
        iparams.Add(new MultipartFormDataSection("password", this.pwd));
        iparams.Add(new MultipartFormDataSection("captcha", this.cta));
        iparams.Add(new MultipartFormDataSection("id", this.key));
        iparams.Add(new MultipartFormDataSection("csrf_token", this.csrf));
        UnityWebRequest request2 = UnityWebRequest.Post("http://182.92.124.204:5666/login",iparams);
        yield return request2.SendWebRequest();
        if(request2.isHttpError || request2.isNetworkError)
        {
            Debug.LogError(request2.error);
        }
        else
        {
            try
            {
                string receiveContent = request2.downloadHandler.text;
                login_result myfile = new login_result();
                JsonUtility.FromJsonOverwrite(receiveContent, myfile);
                // print(receiveContent);
                PlayerPrefs.SetInt("userid",myfile.id);
                PlayerPrefs.SetString("name",this.usr);
                PlayerPrefs.SetString("access_token",myfile.access_token);
                SceneManager.LoadScene("self");
            }
            catch (System.Exception)
            {
                print("login error");
            }
        }
    }
    public void handle_click(){
        print("login by "+"username:"+this.usr+"passwd:"+this.pwd+"captcha:"+this.cta);
        StartCoroutine(begin_login());
    }

    public void value_change1(string s){
        this.usr=s;
    }
    public void handle_enter1(string s){
        this.usr=s;
    }
    public void value_change2(string s){
        this.pwd=s;
    }
    public void handle_enter2(string s){
        this.pwd=s;
    }
    public void value_change3(string s){
        this.cta=s;
    }
    public void handle_enter3(string s){
        this.cta=s;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
