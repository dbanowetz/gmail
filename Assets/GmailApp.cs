using System.Collections;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.ComponentModel;
using EASendMail;

public class GmailApp : MonoBehaviour {
    
    const string clientID = "39453173011-lmomtla80onjhdn272igqg68fs9n941j.apps.googleusercontent.com";
    const string clientSecret = "L6EPLGeFkk0AlTBROHAoGa35";
    const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
    const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
    const string userID = "dbanowetz1@gmail.com";
    string access_token = "";
    [SerializeField]
    private InputField Body;
    [SerializeField]
    private InputField To;
    [SerializeField]
    private InputField From;
    [SerializeField]
    private InputField Subject;
    [SerializeField]
    private Canvas loginPage;
    [SerializeField]
    private Canvas menuPage;
    [SerializeField]
    private Canvas sendEmailPage;
    [SerializeField]
    private Button SendEmailButton;
    [SerializeField]
    private Button loginButton;
    [SerializeField]
    private Button emailButton;
    [SerializeField]
    private Button messagesButton;
    [SerializeField]
    NewBehaviourScript notifySystem;
    private bool resultEmailSucess;
    private bool triggerResultEmail;

    void Awake()
    {
       
    }
    // Use this for initialization
    void Start () {
        bindEventsandFunctions();
       
    }
    private void bindEventsandFunctions()
    {
        //Bind OAUTH login function to button
        var loginButtonEvent = new Button.ButtonClickedEvent();
        loginButtonEvent.AddListener(login);
        loginButton.onClick = loginButtonEvent;

        //Bind the function to check messages to the message menu button.
        var messagesEvent = new Button.ButtonClickedEvent();
        messagesEvent.AddListener(getMessages);
        messagesButton.onClick = messagesEvent;
        //Bind the function that sends an email to a menu button
        var sendEmailEvent = new Button.ButtonClickedEvent();
        sendEmailEvent.AddListener(switchCanvasForSendingEmail);
        SendEmailButton.onClick = sendEmailEvent;


    }
    //TODO:Find a way to parse through the return string so I can display the information as a notification
    private void getMessages()
    {
        handleMessages(access_token);
    }
    private void handleMessages(string access_token)
    {
        string userinfoRequestURI = string.Format("https://www.googleapis.com/gmail/v1/users/" + userID + "/messages");

        // sends the request
        HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);

        userinfoRequest.Method = "GET";
        userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
        userinfoRequest.ContentType = "application/x-www-form-urlencoded";
        userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        // gets the response
        WebResponse userinfoResponse = userinfoRequest.GetResponse();
        using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
        {
            // reads response body
            string userinfoResponseText = userinfoResponseReader.ReadToEnd();
            //     Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(userinfoResponseText);

            notifySystem.AddNewNotification("userinfoCall", userinfoResponseText);
        }
    }
    #region Login
    private void login()
    {
        string state = randomDataBase64url(32);
        string code_verifier = randomDataBase64url(32);
        string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
        const string code_challenge_method = "S256";

        // Creates a redirect URI using an available port on the loopback address.
        string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
        // output("redirect URI: " + redirectURI);

        // Creates an HttpListener to listen for requests on that redirect URI.
        var http = new HttpListener();
        http.Prefixes.Add(redirectURI);
        // output("Listening..");
        http.Start();

        // Creates the OAuth 2.0 authorization request.
        string authorizationRequest = string.Format("{0}?response_type=code&scope=https://mail.google.com&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
            authorizationEndpoint,
            System.Uri.EscapeDataString(redirectURI),
            clientID,
            state,
            code_challenge,
            code_challenge_method);

        // Opens request in the browser.
        System.Diagnostics.Process.Start(authorizationRequest);

        // Waits for the OAuth authorization response.
        var context = http.GetContext();

        #region MyRegion
        // Brings this app back to the foreground.


        // Sends an HTTP response to the browser.
        var response = context.Response;
        string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        responseOutput.Write(buffer, 0, buffer.Length);
        //responseOutput.WriteAsync(buffer, 0, buffer.Length)
       
            responseOutput.Close();
            http.Stop();


        // Checks for errors.
        if (context.Request.QueryString.Get("error") != null)
        {
            return;
        }
        if (context.Request.QueryString.Get("code") == null
            || context.Request.QueryString.Get("state") == null)
        {
            return;
        }

        //// extracts the code
        var code = context.Request.QueryString.Get("code");
        var incoming_state = context.Request.QueryString.Get("state");

        //// Compares the receieved state to the expected value, to ensure that
        //// this app made the request which resulted in authorization.
        if (incoming_state != state)
        {
            return;
        }

        //// Starts the code exchange at the Token Endpoint.
        performCodeExchange(code, code_verifier, redirectURI);
        #endregion

    }
    void performCodeExchange(string code, string code_verifier, string redirectURI)
    {

        // builds the  request
        string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
        string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
            code,
            System.Uri.EscapeDataString(redirectURI),
            clientID,
            code_verifier,
            clientSecret
            );
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        // sends the request
        HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
        tokenRequest.Method = "POST";
        tokenRequest.ContentType = "application/x-www-form-urlencoded";
        tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
        tokenRequest.ContentLength = _byteVersion.Length;
        Stream stream = tokenRequest.GetRequestStream();
         stream.Write(_byteVersion, 0, _byteVersion.Length);
        stream.Close();

        try
        {
            // gets the response
            WebResponse tokenResponse =  tokenRequest.GetResponse();
            using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
            {
                // reads response body
                string responseText =  reader.ReadToEnd();

                // converts to dictionary
                Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                access_token = tokenEndpointDecoded["access_token"];
                loginPage.gameObject.SetActive(false);
                menuPage.gameObject.SetActive(true);
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // reads response body
                        string responseText =  reader.ReadToEnd();
                    }
                }

            }
        }
    }
    #endregion
  
    //TODO: Construct the email message to be sent
    void constructEmailMessage(string to, string from, string subject, string bodyText,string _Access)
    {
        SmtpMail msg = new SmtpMail("TryIt");
        EASendMail.SmtpClient osmtp = new EASendMail.SmtpClient();
        try
        {
            SmtpServer oServer = new SmtpServer("smtp.gmail.com");
            oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;
            oServer.Port = 587;
            oServer.AuthType = SmtpAuthType.XOAUTH2;
            oServer.Password = _Access;
            oServer.User = userID;
            msg.From = from;
            msg.To = to;
            msg.Subject = subject;
            msg.TextBody = bodyText;
            msg.ReplyTo = from;
            osmtp.SendMail(oServer, msg);
        }catch(Exception exp)
        {
            Debug.LogWarning(exp.Message);
        }
         //This way takes the users email and password. Not as safe as you would have to handle their info securely.
        //SmtpClient _oSmtp = new SmtpClient();
        //_oSmtp.Host = "smtp.gmail.com";
        //_oSmtp.Port = 587;
        //_oSmtp.Credentials = new System.Net.NetworkCredential(from, _Access) as ICredentialsByHost;
        //_oSmtp.EnableSsl = true;
        //MailMessage temp = new MailMessage(from, to, subject, bodyText);
        //temp.ReplyTo = temp.From;
        //_oSmtp.SendAsync(temp, "RUbella10");


    }
    private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Cancelled || e.Error != null)
        {
            print("Email not sent: " + e.Error.ToString());

            resultEmailSucess = false;
            triggerResultEmail = true;
        }
        else
        {
            print("Email successfully sent.");

            resultEmailSucess = true;
            triggerResultEmail = true;
        }
    }
    //Function to turn on the email canvas
    private void switchCanvasForSendingEmail()
    {
        menuPage.gameObject.SetActive(false);
        sendEmailPage.gameObject.SetActive(true);
    }
    //TODO:Construct a message and UI so you are able to send an Email
    private void sendEmail()
    {
        if (access_token.CompareTo("") != 0)
        {
            bool missingField = false;
            if (To.text.CompareTo("") == 0)
            {
                missingField = true;
            }
            if (From.text.CompareTo("") == 0)
            {
                missingField = true;
            }

            if (!missingField)
            {
                constructEmailMessage(To.text, From.text, Subject.text, Body.text, access_token);
            }
            else
            {
                Debug.Log("Invalid field");
            }
        }else
        {
            Debug.Log("Invalid Access Token");
        }

    }
    
	// Update is called once per frame
	void Update () {
		
	}

    private static string randomDataBase64url(uint length)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] bytes = new byte[length];
        rng.GetBytes(bytes);
        return base64urlencodeNoPadding(bytes);
    }

    /// <summary>
    /// Returns the SHA256 hash of the input string.
    /// </summary>
    /// <param name="inputStirng"></param>
    /// <returns></returns>
    private static byte[] sha256(string inputStirng)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
        SHA256Managed sha256 = new SHA256Managed();
        return sha256.ComputeHash(bytes);
    }

    /// <summary>
    /// Base64url no-padding encodes the given input buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private static string base64urlencodeNoPadding(byte[] buffer)
    {
        string base64 = System.Convert.ToBase64String(buffer);

        // Converts base64 to base64url.
        base64 = base64.Replace("+", "-");
        base64 = base64.Replace("/", "_");
        // Strips padding.
        base64 = base64.Replace("=", "");

        return base64;
    }

    private static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
    private bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
     bool isOk = true;
     // If there are errors in the certificate chain, look at each error to determine the cause.
     if (sslPolicyErrors != SslPolicyErrors.None) {
         for (int i=0; i<chain.ChainStatus.Length; i++) {
             if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
                 chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                 chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                 chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                 chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                 bool chainIsValid = chain.Build ((X509Certificate2)certificate);
                 if (!chainIsValid) {
                     isOk = false;
                 }
             }
         }
     }
     return isOk;
 }

}
